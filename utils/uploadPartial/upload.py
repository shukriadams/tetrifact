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

parser = argparse.ArgumentParser()
parser.add_argument('--address', default='localhost:5000')
parser.add_argument('--zip', default=False)
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

run(['curl', '-X', 'DELETE', f'{address}/v1/packages/{package2Name}'])

# generate archive of project 1
zipPath = os.path.join(f'1.zip')
if args.zip == True:
    try:
        if os.path.exists(zipPath):
            os.remove(zipPath)
    except OSError as e:
        print(f'Error removing zip: {zipPath} : {e}')
        sys.exit(1)

    run(
        ['7z', 
        'a' ,
        zipPath, 
        os.path.join('./package1', '*')] 
    )

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
files = os.listdir('./package2')
sorted(files)

manifest = {}
manifest['files'] = []
package2HashContent = ''

for file in files:
    fileData = {}
    fileData['path'] = file
    filePathFull = os.path.join('./package2', file)

    with open(filePathFull) as f:
        fileContent = f.read()

    fileData['hash'] = sha256(fileContent.encode('utf-8')).hexdigest()
    manifest['files'].append(fileData)
    package2HashContent += sha256(file.encode('utf-8')).hexdigest() + fileData['hash']

package2LocalHash = sha256(package2HashContent.encode('utf-8')).hexdigest()

# use package2 manifest to determine which files in that package are unique vs files that already exist on server
result = subprocess.run([
        'curl',
        '-X', 'POST', 
        '-H', 'Accept: application/json',
        '-H', 'Content-Type: application/json', 
        '-d', json.dumps(manifest),
        f'{address}/v1/packages/filterexistingfiles'
    ],
    shell=True, 
    stderr=subprocess.PIPE,
    stdout=subprocess.PIPE).stdout.decode('utf8')

result = json.loads(result)
common = result['success']['manifest']['files']

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

    shutil.copyfile(os.path.join('./package2', file), os.path.join('./package2Partial', file))

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