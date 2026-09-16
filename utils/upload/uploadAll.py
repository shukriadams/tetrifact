# continously uploads packages. Use to stress test, prune test etc
# Requires Python >= 3.4
import subprocess
import uuid
import time
import glob
import os
import urllib.request
import sys
import argparse

from pathlib import Path

pause=0 # seconds
zipPath = './content.zip'
packages = glob.glob(f'./packages/**/*.zip')

argParser = argparse.ArgumentParser()
argParser.add_argument('--server_address', default='localhost:5000')
args = vars(argParser.parse_args())

server_address = args['server_address']
server_address=f'http://{server_address}'

# check if tetrifact is running
print(f'attemtping to contact server @ {server_address}')
try :
    response =  urllib.request.urlopen(server_address)
    response_code = response.getcode()
    if response_code != 200:
        print(f'Error contacting tetrifact at {server_address}, got code {response_code}')
        sys.exit(1)

except Exception as e:
    print(f'Error contacting tetrifact at {server_address}')
    sys.exit(1)

if len(packages) == 0 :
    print ('no packages found. Run generate.py to create some')
else:
    print (f'Found {len(packages)} packages to upload.')

for package in packages:
    packageName = uuid.uuid4()
    # packageName = Path(package).stem
    print (f'uploading package {packageName}')
    uploadResult = subprocess.run(
        [
            'curl',
            '-X', 'POST', 
            '-H', 'Transfer-Encoding:chunked', 
            '-H', 'Content-Type:multipart/form-data', 
            '-F', f'Files=@{package}',
            f'{server_address}/v1/packages/{packageName}?IsArchive=true'
        ],
        stderr=subprocess.PIPE,
        stdout=subprocess.PIPE).stdout.decode('utf8')

    print(f'Upload result : {uploadResult}, sleeping {pause} seconds...')
    time.sleep(pause)

print(f'Finished uploading {len(packages)} packages')