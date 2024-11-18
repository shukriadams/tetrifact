# continously uploads packages. Use to stress test, prune test etc
# Requires Python >= 3.4
import subprocess
import uuid
import time
import glob
import os
from pathlib import Path

pause=1 # seconds
zipPath = './content.zip'
packages = glob.glob(f'./packages/*.zip')

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
            f'http://localhost:5000/v1/packages/{packageName}?IsArchive=true'
        ],
        stderr=subprocess.PIPE,
        stdout=subprocess.PIPE).stdout.decode('utf8')

    print(f'Upload result : {uploadResult}')
    time.sleep(pause)
