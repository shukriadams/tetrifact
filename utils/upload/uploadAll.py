# continously uploads packages. Use to stress test, prune test etc
# Requires Python >= 3.4
import subprocess
import uuid
import time
import glob
import os
import urllib.request
import sys
from pathlib import Path

pause=1 # seconds
zipPath = './content.zip'
packages = glob.glob(f'./packages/*.zip')
server_address='http://localhost:5000'

# check if tetrifact is running
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
    print ('no pacakges found. Run generate.py to create some')
else:
    print (f'Found {len(packages)} packages to upload.')

for package in packages:
    # packageName = os.path.basename(package)
    packageName = Path(package).stem
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

    print(f'Upload result : {uploadResult}')
    time.sleep(pause)
