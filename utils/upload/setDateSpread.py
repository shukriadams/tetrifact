import os
import sys
import glob
import json
import datetime

work_dir='./../../src/Tetrifact.Web/bin/Debug/net6.0/data/'
packagesPerDay=5

def loadJson(filepath):
    import json
    print (filepath)
    with open(filepath) as f:
        jsonstring = f.read()
        return json.loads(jsonstring)

def writeJson(filepath, dataObject):
    import json
    with open(filepath, 'w') as out:
        json.dump(dataObject, out, indent = 4)


if not os.path.isdir(work_dir):
    print(f'ERROR : could not find root path {work_dir}')
    sys.exit(1)

# get all packages
packages_dir = os.path.join(work_dir, 'packages')
packages = glob.glob(f'{packages_dir}/*')

if len(packages) == 0:
    print(f'No packages found at path {packages_dir}')
    sys.exit(1)

print(f'Found {len(packages)} packages')

daysBack = 0
date = datetime.datetime.now()

for package in packages:
    date = date + datetime.timedelta(days = daysBack)

    for day in range(packagesPerDay):
        manifestPath = os.path.join(package, 'manifest.json')
        manifest = loadJson(manifestPath)
        manifest['CreatedUtc'] = str(date)
        writeJson(manifestPath, manifest)

        manifestPath = os.path.join(package, 'manifest-head.json')
        manifest = loadJson(manifestPath)
        manifest['CreatedUtc'] = str(date)
        writeJson(manifestPath, manifest)

        print(f'Updated package {package}')

    daysBack += 1