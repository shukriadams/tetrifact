# script to test partial uploading of packages. Run against local tetrifact instance

import os
import sys
import subprocess
from subprocess import run
from os import walk
from hashlib import sha256
import json
import shutil
import argparse
import glob

parser = argparse.ArgumentParser()
parser.add_argument('--address', default='localhost:5000')
parser.add_argument('--zip', default='false')
parser.add_argument('--clean', default='') # can be "", "partial" "both"
parser.add_argument('--package1_path', default='./package1')
parser.add_argument('--package2_path', default='./package2')

args = parser.parse_args()
address = f'http://{args.address}'

# ensure package paths exist
if not os.path.exists(args.package1_path):
    print(f'Package 1 path {args.package1_path} does not exist')
    sys.exit(1)

if not os.path.exists(args.package2_path):
    print(f'Package 2 path {args.package2_path} does not exist')
    sys.exit(1)

# delete existing packages 1+2
package1Name='package1'
package2Name='package2'

# remove first package only if
if args.clean == 'both':
    run(['curl', '-X', 'DELETE', f'{address}/v1/packages/{package1Name}'])

if args.clean == 'both' or args.clean == 'partial':
    run(['curl', '-X', 'DELETE', f'{address}/v1/packages/{package2Name}'])

# generate archive of project 1
zipPath = os.path.join(f'1.zip')
if args.zip == 'true':
    try:
        if os.path.exists(zipPath):
            print(f'removing archive {zipPath}')
            os.remove(zipPath)
    except OSError as e:
        print(f'Error removing zip: {zipPath} : {e}')
        sys.exit(1)

    print(f'generating archive for package 1 at {zipPath}')

    run(
        ['7z', 
        'a' ,
        zipPath, 
        os.path.join(args.package1_path, '*')] 
    )

if args.clean == 'both':
    run(
        [
            'curl',
            '-X', 'POST', 
            '-H', 'Transfer-Encoding:chunked', 
            '-H', 'Content-Type:multipart/form-data', 
            '-F', f'Files=@{zipPath}',
            f'{address}/v1/packages/{package1Name}?IsArchive=true'
        ]
    )

# generate a manifest of package2 files
print(f'Looking up files in {args.package2_path}')
files = glob.glob(args.package2_path + '/**/*', recursive=True) 
sorted(files)

def shortenFilePath(path, root):
    path = path.replace('\\', '/')
    clipStart = len(root) + 1 # clip off root + first slash
    path = path[ clipStart : clipStart + len(path)]
    return path

if args.clean == 'both' or args.clean == 'partial':


    manifest = {}
    manifest['files'] = []
    package2HashContent = ''

    print('building manifest for package2')

    count = 0
    files_count = len(files)
    for file in files:
        
        count = count + 1

        fileData = {}
        fileData['path'] = shortenFilePath(file, args.package2_path)

        filePathFull = os.path.join(args.package2_path, file)

        whateverPython = fileData['path']

        print(f'manifest file {count}/{files_count} : {whateverPython}')

        if os.path.isdir(filePathFull):
            continue

        fileContent=''
        with open(filePathFull, mode='rb') as f:
            fileContent = f.read()

        fileData['hash'] = sha256(fileContent).hexdigest()
        manifest['files'].append(fileData)
        package2HashContent += sha256(file.encode('utf-8')).hexdigest() + fileData['hash']

    package2LocalHash = sha256(package2HashContent.encode('utf-8')).hexdigest()

    print('Dumping manifest for package2')
    with open('./2manifest.json', 'w') as out:
        json.dump(manifest, out, indent = 4)


print('reloading manifest2')
with open('./2manifest.json') as f:
    jsonstring = f.read()
    manifest = json.loads(jsonstring)

# use package2 manifest to determine which files in that package are unique vs files that already exist on server
result = subprocess.run([
        'curl',
        '-X', 'POST', 
        '-H', 'Transfer-Encoding:chunked', 
        '-H', 'Content-Type:multipart/form-data', 
        '-F', 'Manifest=@./2manifest.json',
        f'{address}/v1/packages/filterexistingfiles'
    ],
    shell=True, 
    stderr=subprocess.PIPE,
    stdout=subprocess.PIPE).stdout.decode('utf8')

result = json.loads(result)
common = result['success']['manifest']['files']

print(common)
sys.exit(0)

# copy unique files in package2 to dir to zip up
package2Partial = './package2Partial'
if os.path.exists(package2Partial):
    shutil.rmtree(package2Partial)

os.makedirs(package2Partial)

for file in files:
    skip=False
    for exist in common:
        if file == exist['path']:
            skip=True
            break
    
    if skip:
        continue

    shortened_path = shortenFilePath(file, args.package2_path)
    sourcePath = os.path.join(args.package2_path, file)

    if os.path.isdir(sourcePath):
        continue

    targetPath = os.path.join('./package2Partial', shortened_path)
    shutil.copyfile(sourcePath, targetPath)
    
    print (f'copied {targetPath}')
    

# zip and upload package2 unique files, along with list of files to reuse from package1
zipPath = os.path.join(f'package2Partial.zip')

try:
    if os.path.exists(zipPath):
        os.remove(zipPath)
except OSError as e:
    print(f'Error removing zip: {zipPath} : {e}')
    sys.exit(1)

run(['7z', 'a', zipPath, os.path.join('./package2Partial', '*')])

commonFiles = './2diff.json'
with open(commonFiles, 'w') as out:
    json.dump(common, out, indent = 4)

result = subprocess.run(
    [
        'curl',
        '-X', 'POST', 
        '-H', 'Transfer-Encoding:chunked', 
        '-H', 'Content-Type:multipart/form-data', 
        '-F', f'Files=@{zipPath}',
        '-F', f'ExistingFiles=@{commonFiles}',
        f'{address}/v1/packages/{package2Name}?IsArchive=true'
    ],
    shell=True, 
    stderr=subprocess.PIPE,
    stdout=subprocess.PIPE).stdout.decode('utf8')

result = json.loads(result)
assert result['success'] != None
assert result['success']['hash'] == package2LocalHash
# print(f'__{result}__')
# download remote manifest of 