# Requires Python >= 3.4
from importlib.machinery import SourceFileLoader
import subprocess
import argparse
import sys

loader = SourceFileLoader('loader', './vars.py').load_module()
server_address='http://localhost:5000'

argParser = argparse.ArgumentParser()
argParser.add_argument('--tag', default=None)
argParser.add_argument('--package', default=None)
args = vars(argParser.parse_args())
args = loader.mergeFromFile('.tag', args)

if args['package'] is None:
    print ('package not set')
    sys.exit(1)

if args['tag'] is None:
    print ('tag not set')
    sys.exit(1)

result = subprocess.run(
    [
        'curl',
        '-X', 'POST', 
        f'{server_address}/v1/tags/{args['tag']}/{args['package']}'
    ],
    stderr=subprocess.PIPE,
    stdout=subprocess.PIPE).stdout.decode('utf8')

print(f'tag result : {result}')
