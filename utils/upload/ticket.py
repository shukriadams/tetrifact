# Requires Python >= 3.4
from importlib.machinery import SourceFileLoader
import subprocess
import argparse
import sys
import json

loader = SourceFileLoader('loader', './vars.py').load_module()
server_address='http://localhost:5000'

argParser = argparse.ArgumentParser()
argParser.add_argument('--client', default=None)
argParser.add_argument('--package', default=None)
args = vars(argParser.parse_args())
args = loader.mergeFromFile('.ticket', args)

if args['client'] is None:
    print ('client not set')
    sys.exit(1)

if args['package'] is None:
    print ('package not set')
    sys.exit(1)

result = subprocess.run(
    [
        'curl',
        '-X', 'POST', 
        f'{server_address}/v1/tickets/{args['client']}'
    ],
    stderr=subprocess.PIPE,
    stdout=subprocess.PIPE).stdout.decode('utf8')

ticketData = json.loads(result)
ticketId = ticketData['success']['ticket']

print(f'ticket generated : {ticketId}')


# download package with ticket
uploadResult = subprocess.run(
    [
        'curl',
        "--write-out", "%{http_code}",
        '--verbose',
        '--output', '.package.zip',
        f'{server_address}/v1/archives/{args['package']}?ticket={ticketId}'
    ],
    stderr=subprocess.PIPE,
    stdout=subprocess.PIPE).stdout.decode('utf8')

if uploadResult != "200":
    print (f"download failed with code {uploadResult}")
else:
    print (f"download succeeded with {uploadResult}")
