# script to test partial uploading of packages. Run against local tetrifact instance
# requires python3.

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
parser.add_argument('--clean', default='false')
parser.add_argument('--package1_path', default='./package1')
parser.add_argument('--package2_path', default='./package2')
parser.add_argument('--manifest', default='false')

args = parser.parse_args()
address = f'http://{args.address}'


def packageExistsOnServer(package):
    lookup = run(
        [
            'curl',
            '-X',
            'GET',
            '-s',
            '-I',
            f'{address}/v1/packages/{package}'
        ],
        stderr=subprocess.PIPE,
        stdout=subprocess.PIPE).stdout.decode('utf8')    

    return 'HTTP/1.1 404 Not Found' not in lookup

def shortenFilePath(path, root):
    path = path.replace('\\', '/')
    clipStart = len(root) + 1 # clip off root + first slash
    return path[ clipStart : clipStart + len(path)]


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

if args.clean == 'true':
    run(['curl', '-X', 'DELETE', f'{address}/v1/packages/{package1Name}'])
    run(['curl', '-X', 'DELETE', f'{address}/v1/packages/{package2Name}'])

# generate archive of project 1
zipPath = os.path.join(f'1.zip')

if not os.path.exists(zipPath):
    print(f'generating archive for package 1 at {zipPath}')
    run(
        ['7z', 
        'a' ,
        zipPath, 
        os.path.join(args.package1_path, '*')] 
    )

if not packageExistsOnServer(package1Name):
    print('uploading package 1')
    uploadResult = run(
        [
            'curl',
            '-X', 'POST', 
            '-H', 'Transfer-Encoding:chunked', 
            '-H', 'Content-Type:multipart/form-data', 
            '-F', f'Files=@{zipPath}',
            f'{address}/v1/packages/{package1Name}?IsArchive=true'
        ],
        stderr=subprocess.PIPE,
        stdout=subprocess.PIPE).stdout.decode('utf8')    

# generate a manifest of package2 files
print(f'Looking up files in {args.package2_path}')
package2_allFiles = glob.glob(args.package2_path + '/**/*', recursive=True) 
sorted(package2_allFiles)


count = 0
package2LocalHash=''

known_dirs=[]

manifest = {}
manifest['files'] = []
package2HashContent = ''

print('building manifest for package2')

files_count = len(package2_allFiles)

if args.manifest == 'true':
    for file in package2_allFiles:

        # on windows lookup returns relative paths, in linux full, remove absolute root on linux to standardize path handling        
        #if file.startswith(args.package2_path):
        #    file = file[len(args.package2_path) + 1:len(file)] 



        count = count + 1

        fileData = {}
        fileData['path'] = shortenFilePath(file, args.package2_path)

        filePathFull = file

        print(fileData['path'])

        if filePathFull in known_dirs:
            continue

        if os.path.isdir(filePathFull):
            known_dirs.append(filePathFull)
            continue

        fileContent=''
        with open(filePathFull, mode='rb') as f:
            fileContent = f.read()

        fileData['hash'] = sha256(fileContent).hexdigest()
        manifest['files'].append(fileData)
        package2HashContent += sha256(file.encode('utf-8')).hexdigest() + fileData['hash']
        print(f'{count}/{files_count} in manifest : {file}')

    manifest['hash'] = sha256(package2HashContent.encode('utf-8')).hexdigest()

    print('Dumping manifest for package2')
    with open('./2manifest.json', 'w') as out:
        json.dump(manifest, out, indent = 4)

# reload incase create phase was disabled
if not os.path.isfile('./2manifest.json'):
    print('2manifest.json not found, be sure run script with "--manifest true" arg')
    sys.exit(0)


print('reloading manifest2')
with open('./2manifest.json') as f:
    jsonstring = f.read()
    manifest = json.loads(jsonstring)


package2LocalHash = manifest['hash']
# use package2 manifest to determine which files in that package are unique vs files that already exist on server
result = subprocess.run([
        'curl',
        '-X', 'POST', 
        '-H', 'Transfer-Encoding:chunked', 
        '-H', 'Content-Type:multipart/form-data', 
        '-F', 'Manifest=@./2manifest.json',
        f'{address}/v1/packages/filterexistingfiles'
    ],
    stderr=subprocess.PIPE,
    stdout=subprocess.PIPE).stdout.decode('utf8')

result = json.loads(result)
common = result['success']['manifest']['files']

# copy unique files in package2 to dir to zip up
package2Partial = './package2Partial'

if args.clean == 'true' and os.path.exists(package2Partial):
    shutil.rmtree(package2Partial)

if not os.path.exists(package2Partial):
    os.makedirs(package2Partial)

# convert common objects to string array, we don't need other data, and string array makes lookups easier
common_simplified = []
for c in common:
    # this should not happen, but we're getting nulls in json, not sure from where
    if c is None: 
        continue

    common_simplified.append(c['path'])

files_in_common_count = len(common)
diff_count = len(manifest['files']) - files_in_common_count

print(f'found {files_in_common_count} files in common, {diff_count} need to be uploaded')

count = 0
total = len(package2_allFiles)
known_dirs = []

for package2File in package2_allFiles:

    count += 1 

    # ignore directories
    if package2File in known_dirs:
        continue

    if os.path.isdir(package2File):
        known_dirs.append(package2File)
        continue

    sourcePathShortened = shortenFilePath(package2File, args.package2_path)

    if sourcePathShortened in common_simplified:
        continue

    targetPath = os.path.join('./package2Partial', sourcePathShortened)

    # we already copied file
    if os.path.exists(targetPath):
        print(f'{targetPath} already copied, skipping')
        continue

    os.makedirs(os.path.dirname(targetPath), exist_ok=True)
    shutil.copyfile(package2File, targetPath)
    print (f'processed {count}/{total} : {targetPath}')


# zip and upload package2 unique files, along with list of files to reuse from package1
zipPath = os.path.join(f'package2Partial.zip')
commonFiles = './2diff.json'

if args.clean == 'true' and os.path.exists(zipPath):
    os.remove(zipPath)

if not os.path.exists(zipPath):
    run(['7z', 'a', zipPath, os.path.join('./package2Partial', '*')])

# write out common files to file, this must be posted along with diffs to create full package
with open(commonFiles, 'w') as out:
    json.dump(common, out, indent = 4)

if packageExistsOnServer(package1Name):
    print('skipping package2 upload, already exists')
else:
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
        stderr=subprocess.PIPE,
        stdout=subprocess.PIPE).stdout.decode('utf8')

    result = json.loads(result)
    package2RemoteHash = result['success']['hash']

    assert result['success'] != None
    print (f'remote hash : {package2RemoteHash}  localhash : {package2LocalHash}')

remoteManifest = run(['curl', f'{address}/v1/packages/{package2Name}'],
    stderr=subprocess.PIPE,
    stdout=subprocess.PIPE).stdout.decode('utf8')    


print('Dumping remote manifest for package2')
with open('./remoteManifest.json', 'w') as out:
    json.dump(remoteManifest, out, indent = 4)


print('reloading remote manifest')
with open('./remoteManifest.json') as f:
    jsonstring = f.read()
    remoteManifest = json.loads(jsonstring)

remoteManifest = json.loads(remoteManifest)
remoteFiles = remoteManifest['success']['package']['files']

localFiles = manifest['files']
count = 0
total = len(localFiles)

for localFile in localFiles:
    remoteFile = next(filter(lambda x: x['path'] == localFile['path'] and x['hash'] == localFile['hash'],  remoteFiles))    
    localFilePath = localFile['path']
    if remoteFile is None:
        print (f'expected remotefile {localFilePath} not found')
        sys.exit(1) 
    count += 1
    print (f'passed {count}/{total} : {localFilePath}')

"""
assert result['success']['hash'] == package2LocalHash
# download remote manifest of 
"""