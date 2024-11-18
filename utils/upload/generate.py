# generates a package file with customizable file count and size.
import os
import shutil
import random
from random import randrange
import glob
import shutil
import sys
from importlib.machinery import SourceFileLoader
import argparse

loader = SourceFileLoader('loader', './vars.py').load_module()

argParser = argparse.ArgumentParser()
argParser.add_argument('--package_count', default=3)
argParser.add_argument('--package_file_count_min', default=1)
argParser.add_argument('--package_file_count_max', default=100)
argParser.add_argument('--file_size_min', default=1)
argParser.add_argument('--file_size_max', default=10000000)
argParser.add_argument('--file_reuse_chance', default=50)

args = vars(argParser.parse_args())
args = loader.mergeFromFile('.generate', args)

package_count = args['package_count']
package_file_count_min = args['package_file_count_min']
package_file_count_max = args['package_file_count_max']
file_size_min = args['file_size_min']
file_size_max = args['file_size_max']
file_reuse_chance = args['file_reuse_chance']
output_directory = './packages'

print("GENERATING CONTENT")

# purge and recreate packages dir
if os.path.isdir(output_directory):
    shutil.rmtree(output_directory)

os.makedirs(output_directory)

for n_package in range(package_count):

    files_in_package = random.randint(package_file_count_min, package_file_count_max)

    package_directory = os.path.join(output_directory, str(n_package))
    os.makedirs(package_directory)

    for n_file in range(files_in_package):

        fileSize = random.randint(file_size_min, file_size_max)
        reuseFile = False
        roll = random.randint(0, 100)
        reuseFile = n_package > 0 and roll > file_reuse_chance
        this_file_name = os.path.join(package_directory, str(n_file))

        if reuseFile:
            previous_package_directory = os.path.join(output_directory, str(n_package - 1))
            files = glob.glob(f'{previous_package_directory}/*')
            source_file = files[random.randint(0, len(files) - 1)]
            shutil.copy(source_file, this_file_name)
            print (f'for package {n_package}, copying {n_file} from {source_file}')
        else:
            with open(this_file_name, 'wb') as binary_file:
                print (f'for package {n_package}, generating {n_file} of {files_in_package}, size {fileSize}')
                bytes_array = bytearray(os.urandom(fileSize))
                binary_file.write(bytes_array)

    print (f'zipping package {n_package}')
    shutil.make_archive(os.path.join(output_directory,str(n_package)), 'zip', package_directory)