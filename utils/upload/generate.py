# generates a package file with customizable file count and size.
import os
import shutil
import random
from random import randrange
import glob
import shutil


package_count = 3
package_file_count_min = 1
package_file_count_max = 100
file_size_min = 1
file_size_max = 10000000
file_reuse_chance = 50
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