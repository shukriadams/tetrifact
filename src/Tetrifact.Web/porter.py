#!/usr/bin/python3
# Proof-of-concept for Porter, a packages-as-source-code manager for C#.
#
# Use : 
# porter --install /some/path
#
# Confirmed working on Python:
# 3.8.0 Linux
# 3.12.0 Windows
import warnings

import argparse
import os
import sys
import json
import re as regex
import shutil
import subprocess
import glob
import codecs
import uuid
import base64
import shutil
import stat
import fnmatch

# still using onerror on shutil.rmtree, ignore
warnings.filterwarnings('ignore', category=DeprecationWarning) 

# files to ignore when transferring porter CS files
CSFileBlacklist = [
    # assembly definition from package will break parent project's properties
    'Properties/CustomAssemblyInfo.cs'
]

class Package:
    def __init__(self, repo, tag):
        self.Source = 'https://github.com' # currently only public github repos supported
        self.Repo = repo.replace('.', '/') # github repos dont have .'s, must be /
        self.Tag = tag # current only only tag is supported

# file-safe encoding (base64)
def encode(str):
    b = str.encode('ascii')
    b64_bytes = base64.b64encode(b)
    s = b64_bytes.decode('ascii')
    return s.replace('/', '_')

# deletes a dir, including readonly files
def deleteDir(dir):
    def on_error( func, file, exc_info):
        os.chmod(file, stat.S_IWRITE)
        os.unlink(file)
    
    # replace onerror with onexc (love how they somehow manage to make a garbage api even more garbage)
    shutil.rmtree( dir, onerror = on_error )

def exec(command, cwd=None):

    # force to run in current dir if none set
    if cwd is None:
        cwd = os.path.dirname(os.path.realpath(__file__))

    result = subprocess.Popen(command,
        cwd=cwd,
        shell=True, 
        stderr=subprocess.PIPE,
        stdout=subprocess.PIPE)

    # wait for command to exit. NOTE : this is extremely likely to hang, rewrite to use .communicate() instead
    result.wait()

    returnCode = result.poll()

    # if fail to get a return code from poll, assume pass, this is probably not a good idea but not 
    # sure how else to handle this now. Ideally wait should always be on
    if returnCode is None:
        returnCode = 0

    stdout = result.stdout.readlines()
    stderr = result.stderr.readlines()

    # convert to
    for i in range(0, len(stdout)):
        stdout[i] = stdout[i].decode('utf-8')

    for i in range(0, len(stderr)):
        stderr[i] = stderr[i].decode('utf-8')

    if returnCode != 0:
        print (f'ERROR : command \"{command}\" failed, return code was {returnCode}, error was :')
        print(stderr)
        sys.exit(1)

    return returnCode


def process_porter(root_dir_path, context=[], require_run_times=None):
    
    # load the porter.json in this directory, we already confirmed it's here
    porter_start_file_path = os.path.join(root_dir_path, 'porter.json')
    porter_conf = {}
    porter_packages_dir = os.path.join(root_dir_path, 'porter')

    try:
        with open(porter_start_file_path) as file:
            porter_conf = json.load(file)
    except Exception as e:
        print(f'Error : loading json from {porter_start_file_path}:{e}')
        sys.exit(1)

    packages_to_install = []
    package_name=porter_conf['name']

    # enforce required structures in conf
    if not 'runtimes' in porter_conf:
        porter_conf['runtimes'] = []

    if not 'packages' in porter_conf:
        porter_conf['packages'] = []
    
    # if runtimes not set, we're at top-level project, use it's runtimes
    if require_run_times == None:
        if len(porter_conf['runtimes']) == 0:
            print('top level project must declare at least one expected runtime')
            sys.exit(1)
        
        require_run_times = porter_conf['runtimes']

    context = context[:] # copy array so we don't end up cascading additions to adjacent recursive calls
    context.append(package_name)

    # generate package to install from conf packages
    for package in porter_conf['packages']:
        # get package source from package string. format is expected to be 
        # source.author+repo@tag
        parts = regex.search('([^.]*).(.*)@(.*)', package)
        if not parts.groups:
            print(f'package {package} is malformed')
            continue

        if not parts.group(1) or not parts.group(2) or not parts.group(3):
            print(f'package {package} is malformed')
            continue
            
        package_source = parts.group(1)
        auth_repo = parts.group(2)
        tag = parts.group(3)

        if package_source != 'github':
            print(f'Error : only github currently supported : {package_source}') 
            sys.exit(1)

        p = Package(auth_repo, tag)
        packages_to_install.append(p)

    for package in packages_to_install:
        occurences = [i for i, x in enumerate(packages_to_install) if x.Repo == package.Repo]
        if len(occurences) > 1:
            print(f'Error : package {package.Repo} occurs more than once')
            sys.exit(1)

    if not os.path.isdir(porter_packages_dir):
        os.makedirs(porter_packages_dir)

    for package in packages_to_install:
        # create temp dir in porter's own work dir, use deterministic path so we always
        # clean up after ourselves. Path is based on package's total namespace depth, so
        # no chance of collision with other packages. make it filesystem safe by safe 
        # base64 encoding. Before cloning always delete the temp path
        package_temp_dir = os.path.join(work_dir, encode('_'.join(context)))

        # convert to abs path for remapping later
        package_temp_dir = os.path.abspath(package_temp_dir)

        if os.path.isdir(package_temp_dir):
            deleteDir(package_temp_dir)
        os.makedirs(package_temp_dir)

        # clone package to temp location, we need to analyse it first
        result = exec(f'git clone --branch {package.Tag} {package.Source}/{package.Repo} {package_temp_dir}')
        if result != 0:
            sys.exit(0)
        
        # package must have a porter.json file in it, else we ignore it
        package_porter_conf = os.path.join(package_temp_dir, 'porter.json')
        isPackagePorter=True
        if not os.path.isfile(package_porter_conf):
            print(f'Warning : package @ path {package_porter_conf} is not a porter package')
            continue

        # load porter.json
        try:
            with open(package_porter_conf) as file:
                this_package_porter_conf = json.load(file)
        except Exception as e:
            print(f'Error : loading json from {package_porter_conf}. Assuming not a valid porter package, will not proces. {e}')
            continue

        this_package_name = this_package_porter_conf['name']

        if not 'ignore' in this_package_porter_conf:
            this_package_porter_conf['ignore'] = []
        ignore_paths = this_package_porter_conf['ignore']

        package_copy_root = package_temp_dir
        if 'export' in this_package_porter_conf:
            package_copy_root = os.path.join(package_temp_dir, this_package_porter_conf['export'])

        # enforce top level runtimes on this
        this_package_runtimes = this_package_porter_conf['runtimes']
        if not any(runtimes in set(this_package_runtimes) for runtimes in require_run_times):
            print(f'{this_package_name} runtimes {this_package_runtimes} do not align with required runtimes {require_run_times}')
            sys.exit(1)

        # destroy then create public directory of this package
        child_package_dir = os.path.join(porter_packages_dir, this_package_name)
        # convert to absolute for remapping
        child_package_dir = os.path.abspath(child_package_dir)

        if os.path.isdir(child_package_dir):
            deleteDir(child_package_dir)   

        if not os.path.isdir(child_package_dir):
            os.makedirs(child_package_dir)

        # find all .cs files in package temp, we want to wrap and copy them
        cs_files = glob.glob(os.path.join(package_copy_root, '**/*.cs'), recursive=True) 
        for cs_file in cs_files:

            # convert to abs path for easier remap
            cs_file = os.path.abspath(cs_file)
            
            # is CS file on blacklist?
            blacklisted=False
            for blacklist in CSFileBlacklist:
                if cs_file.endswith(blacklist):
                    blacklisted = True
            
            if blacklisted:
                print(f'Ignoring blacklisted file {cs_file}')
                continue

            ignore=False
            for ignore_path in ignore_paths:
                if fnmatch.fnmatch(cs_file, ignore_path):
                    ignore=True

            if ignore:
                continue

            with codecs.open(cs_file, encoding='utf-8') as file:
                file_content = file.read()

                # wrap file contents in namespace stack threaded down package stack 
                namespace_lead = '//PORTER-WRAPPER!\n'
                namespace_tail= ''
                for this_context in context:
                    namespace_lead = (namespace_lead+
                        'namespace '+this_context+'.Porter_Packages {\n')

                    namespace_tail = namespace_tail +'}\n'

                namespace_lead = f'{namespace_lead}//PORTER-WRAPPER!\n\n\n'
                namespace_tail = f'\n\n//PORTER-WRAPPER!\n{namespace_tail}//PORTER-WRAPPER!'
                file_content = f'{namespace_lead}{file_content}{namespace_tail}'

                # remap .cs file in temp dir to public child package dir
                remapped_file_path = cs_file.replace(package_copy_root, child_package_dir)

                # create target directory
                remapped_file_dir = os.path.dirname(remapped_file_path)
                if not os.path.isdir(remapped_file_dir):
                    os.makedirs(remapped_file_dir)

                with open(remapped_file_path, 'w') as output:
                    output.write(file_content)

        # copy porter.json over too, for reference. we probably want to add some of our own stuff to this later ala npm
        shutil.copyfile(os.path.join(package_temp_dir, 'porter.json'), 
            os.path.join(child_package_dir, 'porter.json'))

        # clean up temp child package dir
        deleteDir(package_temp_dir)
    
        print(f'Installed package {this_package_name}')

        # finally recurse by running in child package dir,
        process_porter(child_package_dir, context)   


# let's get started - currently the only supported call is  --install some/path
print('Porter, a package manager for C#')

argsParser = argparse.ArgumentParser()
argsParser.add_argument('--install', nargs='?', const='')
args = argsParser.parse_args()

# if no intall dir set, use current dir
run_in_dir = args.install 
if run_in_dir == '':
    run_in_dir = os.getcwd()

print(f'Running in dir : {run_in_dir}')

# the directory we start processing in must have a porter.json file in it.
root_dir_path = os.path.abspath(run_in_dir)
porter_start_file_path = os.path.join(root_dir_path, 'porter.json')
if not os.path.isfile(porter_start_file_path):
    print(f'Error : expected porter.json not found at path {root_dir_path}')
    sys.exit(1)

# create a work dir for porter, it needs this to check incoming git checkouts
# before merging their contents into top-level packages directory
work_dir = os.path.join(os.getcwd(), '.porter_temp')
if not os.path.isdir(work_dir):
    os.makedirs(work_dir)

# recurse through all the packages, all the way down through all the packages
process_porter(root_dir_path)

print('Done!')