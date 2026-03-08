# generates a package file with customizable file count and size. the package is zipped
# and ready to be uploaded to a tetrifact instance


import os
import shutil
import random
from random import randrange
import glob
import shutil
import sys
from importlib.machinery import SourceFileLoader
import argparse
import array
from pathlib import Path

loader = SourceFileLoader('loader', './vars.py').load_module()

argParser = argparse.ArgumentParser()
argParser.add_argument('--package_count', default=3)
argParser.add_argument('--package_file_count_min', default=1)
argParser.add_argument('--package_file_count_max', default=100)
argParser.add_argument('--segment_count_min', default=1)
argParser.add_argument('--segment_count_max', default=10)
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
segment_count_min = args['segment_count_min']
segment_count_max = args['segment_count_max']
file_reuse_chance = args['file_reuse_chance']
output_directory = './packages'
generate_directory = os.path.join(output_directory, 'generate')
assemble_directory = os.path.join(output_directory, 'assemble')

class SegmentStats:
    def __init__(self):
        self.hash = None
        self.length = 0
        self.isReused = False

    def ToString(self):
        return f'hash:{self.hash}, length:{self.length}, isReused:{self.isReused}'    

class FileStats:
    def __init__(self, name):
        self.name = name
        self.isReused = False
        self.segments = []

    def ToString(self):
        string = f'name:{self.name}, isReused:{self.isReused}\n'
        for segment in self.segments:
            string = string + f'{segment.ToString()}\n'

        return string

    def GetLength(self):
        length = 0
        for segment in self.segments:
            length = length + segment.length

        return length

    def GetUniqueLength(self):
        length = 0
        for segment in self.segments:
            if not segment.isReused:
                length = length + segment.length

        return length

class PackageStats:
    def __init__(self, name):
        self.name = name
        self.length = 0
        self.files = []

    def ToString(self):
        string = f'name:{self.name}\n'
        for file in self.files:
            string = string + f'{file.ToString()}\n'

        length = 0
        for file in self.files:
            length = length + file.GetLength()
        
        string = string + f'length: {length}\n'

        lengthUnique = 0
        for file in self.files:
            lengthUnique = lengthUnique + file.GetUniqueLength()
        
        string = string + f'length: {lengthUnique} (unique)\n'
        string = string + f'percent unique: {((lengthUnique / length)*100)}\n'

        return string


print("GENERATING CONTENT")

files_in_package = random.randint(package_file_count_min, package_file_count_max)
segments_in_file = random.randint(segment_count_min, segment_count_max)

# purge and recreate packages dir
if os.path.isdir(output_directory):
    shutil.rmtree(output_directory)

Path(generate_directory).mkdir(parents=True, exist_ok=True)
Path(assemble_directory).mkdir(parents=True, exist_ok=True)

# generate content

for n_package in range(package_count):
    
    packageStats = PackageStats(str(n_package))

    package_directory = os.path.join(generate_directory, str(n_package))
    os.makedirs(package_directory)

    for n_file in range(files_in_package):

        fileStats = FileStats(str(n_file))
        packageStats.files.append(fileStats)

        reuseFile = n_package > 0 and random.randint(0, 100) > file_reuse_chance
        previous_package_directory = os.path.join(generate_directory, str(n_package - 1))

        if reuseFile:
            fileStats.isReused = True
            # copy file from previous package to current
            files = glob.glob(f'{previous_package_directory}/*')
            source_file = files[random.randint(0, len(files) - 1)]
            shutil.copytree(os.path.join(previous_package_directory, str(n_file)), 
                os.path.join(package_directory, str(n_file)))

            print (f'for package {n_package}, copyied file {n_file} from package {(n_package - 1)}')
    
        else:
    
            fileSize = random.randint(file_size_min, file_size_max)
            os.makedirs(os.path.join(package_directory, str(n_file)))

            # A file consists of a series of blocks. Each block can either be reused or newly generated.
            for n_segment in range(segments_in_file):
                segment = SegmentStats()
                fileStats.segments.append(segment)

                reuseSegment = n_package > 0 and n_file > 0 and random.randint(0, 100) > 50
                this_segment_path = os.path.join(package_directory, str(n_file), str(n_segment))

                if reuseSegment:
                    source_segment_path = os.path.join(previous_package_directory, str(n_file), str(n_segment))
                    shutil.copyfile(source_segment_path, this_segment_path)

                    with open(source_segment_path, 'rb') as binary_file:
                        data = binary_file.read()
                        segment.hash = hashlib.sha1(data).hexdigest()
                        segment.length = len(data)

                    segment.isReused = True

                    print(f'copied segment {n_segment}, file {n_file}, package {n_package}')
                else:
                    with open(this_segment_path, 'wb') as binary_file:
                        bytes_array = bytearray(os.urandom(fileSize))
                        binary_file.write(bytes_array)
                    
                    import hashlib

                    segment.hash = hashlib.sha1(bytes_array).hexdigest()

                    segment.length = len(bytes_array)
                    print(f'created new segment {n_segment}, file {n_file}, package {n_package}')
            
            print (f'for package {n_package}, generating file {n_file}/{files_in_package}, size {fileSize}')
            with open(os.path.join(generate_directory, str(n_package), 'stats.txt'), 'w', encoding='utf-8') as f:
                f.write(packageStats.ToString())                
            
# assemble content blocks into zip files
for n_package in range(package_count):
    
    package_directory = os.path.join(generate_directory, str(n_package))
    files = glob.glob(f'{package_directory}/*')

    package_assemble_dir = os.path.join(assemble_directory, str(n_package))    
    Path(package_assemble_dir).mkdir(parents=True, exist_ok=True)

    for file in files:
        segments = glob.glob(f'{file}/*')
        random.shuffle(segments)

        with open(os.path.join(package_assemble_dir, os.path.basename(file)), 'wb') as binary_file:
            for segment in segments:
                with open(segment, 'rb') as f:
                    lines = f.readlines()
                    for line in lines:
                        binary_file.write(line)
            print (f'assembled segments for file {file}')

    shutil.make_archive(os.path.join(assemble_directory,str(n_package)), 'zip', package_assemble_dir)
    print (f'zipped package {package_assemble_dir}')
