
import os
import sys
import shutil
import urllib.request

work_dir='./../../src/Tetrifact.Web/bin/Debug/net6.0/data/'
server_address='http://localhost:5000'

if not os.path.isdir(work_dir):
    print(f'ERROR : could not find root path {work_dir}')
    sys.exit(1)

# check if tetrifact is running
try :
    response =  urllib.request.urlopen(server_address)
    response_code = response.getcode()
    print(f'Error : Tetrifact appears to be running, stop server before wiping')
    sys.exit(1)

except Exception as e:
    if 'No connection could be made because the target machine actively refused' in str(e):
        print(f'Server seem to be down, proceeding ')
    else:
        print(f'Error : Tetrifact appears to be running, stop server before wiping')
        sys.exit(1)
        
print('DOING A FULL WIPE OF LOCAL TETRIFACT DEV INSTANCE')

for filename in os.listdir(work_dir):
    item_path = os.path.join(work_dir, filename)

    try:

        if os.path.isfile(item_path):
            os.unlink(item_path)
        elif os.path.isdir(item_path):
            shutil.rmtree(item_path)

        print(f'Removed item {item_path}')
    except Exception as e:
        print(f'Failed to delete {item_path} : {e}')

print('Wipe complete')