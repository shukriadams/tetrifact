# continously uploads packages. Use to stress test, prune test etc
import subprocess
import uuid
import time

pause=10 # seconds
zipPath = './content.zip'

while True:
    package1Name = uuid.uuid4().hex
    uploadResult = subprocess.run(
        [
            'curl',
            '-X', 'POST', 
            '-H', 'Transfer-Encoding:chunked', 
            '-H', 'Content-Type:multipart/form-data', 
            '-F', f'Files=@{zipPath}',
            f'http://localhost:5000/v1/packages/{package1Name}?IsArchive=true'
        ],
        stderr=subprocess.PIPE,
        stdout=subprocess.PIPE).stdout.decode('utf8')

    print(uploadResult)
    time.sleep(pause)
