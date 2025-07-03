# single-file shim server for Tetrifact for Python3

from http.server import BaseHTTPRequestHandler, HTTPServer, SimpleHTTPRequestHandler
from pathlib import Path
import pathlib
import os
import shutil
import sys
import json
import re as regex
import uuid
import yaml
from random import randrange
from datetime import datetime, timedelta

number_of_packages_to_create=20

class GetHandler(SimpleHTTPRequestHandler):

    def do_GET(self):
        if self.path == '/v1/packages' or self.path.startswith('/v1/packages?'):
            self.list_packages()
        elif regex.match('\/v1\/archives\/(.*)\/status', self.path):
            self.archive_status()
        elif self.path.startswith('/v1/archives'):
            self.archive_get()
        elif self.path.startswith('/v1/packages/'):
            self.return_package()            
        else:
            self.default_get()

    def do_POST(self):
        if regex.match('\/v1\/tickets\/(.*)', self.path):
            self.ticket_create()
        else:
            self.default_post()

    def default_get(self):
        self.return_html('Tetrifact shim server')

    def default_post(self):
        self.return_html('Unsupported POST')

    def archive_get(self):
        allow = False

        if allow:
            data = 'test-data'.encode('ascii')
            self.wfile.write(data)
        else:
            error = {}
            error['queue_position'] = 1
            error['queue_length'] = 10
            self.return_json(json.dumps(error), 432)

    def ticket_create(self):
        response = {}
        allowTicket = True
        state = ''

        if state == '':
            success = {}
            clientIdentifier = self.path.split("tickets/",1)[1] 
            success['ticket'] = 'somet-ticket-guid'
            success['clientIdentifier'] = clientIdentifier
            success['required'] = True
            response['success'] = success
            print(f'ticket generated : {response}')
        elif state == 'full':
            error = {}
            error['code'] = 1
            error['message'] = 'Queue full'
            response['error'] = error
            print(f'ticket rejected')
        else:
            success = {}
            success['required'] = True
            success['ticket'] = ''
            success['message'] = 'Ticket not required'
            response['success'] = success
            print(f'ticket not needed')

        self.return_json(json.dumps(response), 200)

    def return_package(self):
        packageName = self.path.split("packages/",1)[1] 
        with open(f'./tetrifact/v1/packages/{packageName}.json', 'rb') as f:
            self.send_response(200)
            self.send_header('Content-Type', 'application/octet-stream')
            self.send_header('Content-Disposition', 'attachment; filename="{file}"'.format(file=os.path.basename(packageName)))
            fs = os.fstat(f.fileno())
            self.send_header('Content-Length', str(fs.st_size))
            self.end_headers()
            shutil.copyfileobj(f, self.wfile)

    # /v1/archives/{packageId}/status
    def archive_status(self):
        packageName = regex.search('/v1/archives/(.*)/status', self.path).group(1)
        archive_status_path = f'./tetrifact/v1/packages/{packageName}_archive_status.json'
        print(f'handling archive {packageName}')

        if not os.path.isfile(archive_status_path):
            self.send_response(200)
            print(f'archive request for {packageName} not found')
            return

        status = Path(archive_status_path).read_text()
        print(self.path)
        self.send_response(200)
        self.send_header('Content-type','text/text')
        self.end_headers()
        self.wfile.write(str.encode(html))

# generate packages if none exist
def random_date(start, end):
    delta = end - start
    int_delta = (delta.days * 24 * 60 * 60) + delta.seconds
    random_second = randrange(int_delta)
    return start + timedelta(seconds=random_second)

if not os.path.isfile('./v1/packages.json'):

    shutil.rmtree('./v1/packages')
    pathlib.Path('./v1/packages').mkdir(parents=True, exist_ok=True) 


    if os.path.isfile('./.package'):

        packageContent = None

        with open('./.package') as stream:
            try:
                packageContent = yaml.safe_load(stream)
            except yaml.YAMLError as exc:
                print(exc)
                raise exec

        pathlib.Path('./v1/packages/.package_content').mkdir(parents=True, exist_ok=True) 

        for item in packageContent:
            if item['file'] is None:
                print(f'item {item} missing expected value "file"')
                continue

            if item['content'] is None:
                print(f'item {item} missing expected value "content"')
                continue

            file_name = item['file']
            dirName = os.path.dirname(f'./v1/packages/.package_content/{file_name}')
            pathlib.Path(dirName).mkdir(parents=True, exist_ok=True) 

            with open(f'./v1/packages/.package_content/{file_name}', 'w') as file_stream:
                file_stream.write(item['content'])

        shutil.make_archive('./v1/packages/.package_content', 'zip', './v1/packages/.package_content')

    packages = {}
    packages['success'] = {}
    packages['success']['packages'] = []
    manifests = {}

    for i in range(0, number_of_packages_to_create):
        package = {}
        package['id'] = str(uuid.uuid4()).replace('-', '')[0:6]
        package['hash'] = str(uuid.uuid4()).replace('-', '')
        package['createdUtc'] = str(random_date(datetime.today() - timedelta(10), datetime.today()))
        package['encrypted'] = False
        package['description'] = 'random text here'
        package['tags'] = [
            "stream:master",
            "platform:win64",
            "auto",
            f'revision:{i + 1}',
            "platform:Win64"
        ]
        packages['success']['packages'].append(package)

        manifest = {}
        manifest['success'] = {}
        manifest['success']['package'] = {}
        manifest['success']['package']['Id'] = package['id']
        manifest['success']['package']['Files'] = []
        manifest['success']['package']['Size'] = 543456
        manifest['success']['package']['SizeOnDisk'] = 2124
        manifest['success']['package']['IsCompressed'] = False
        manifest['success']['package']['Hash'] = package['hash']
        manifest['success']['package']['CreatedUtc'] = package['createdUtc']
        manifest['success']['package']['Tags'] = package['tags']
        manifest['success']['package']['Description'] = 'random text'
        
        packageId = package['id']

        with open(f'./v1/packages/{packageId}.json', 'w') as out_file:
            out_file.write(json.dumps(manifest, indent=4))

        if os.path.isfile('./v1/packages/.package_content.zip'):
            shutil.copyfile('./v1/packages/.package_content.zip', f'./v1/packages/{packageId}.zip')

    with open('./v1/packages.json', 'w') as out_file:
        out_file.write(json.dumps(packages, indent=4))

    print('generated packages')    

port=8000
print(f'starting local tetrifact clone server on port {port}...')
httpd=HTTPServer(('localhost', port), GetHandler)
httpd.serve_forever()
