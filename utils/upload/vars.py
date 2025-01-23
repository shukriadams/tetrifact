# Loads variables dictionary from a file. File must be formatted as 
#    name1=value1
#    name2=value2
# empty lines and comments are allowed
#
# filename : string path to file to load
# vars : dictionary object with defaul values. Create with 
#     myvars = dict({
#        'name1': 'a default value',
#        'name2' : 'a default value'
#     })

def mergeFromFile(filename, args):
    import os
    from pathlib import Path
    import argparse
    import ast
    
    if type(args) == argparse.Namespace:
        raise TypeError('args is argparse.Namespace, please convert to dictionary with vars(args) first')

    file = Path(filename)
    if not file.exists():
        return args

    with open(filename, 'r') as fh:
        for line in fh.readlines():
            try:
                if line.startswith('#'):
                    continue

                values = line.replace('\n', '').split('=')
                if len(values) != 2: 
                    continue

                args[values[0].strip()] = values[1].strip() 
            except Exception as e:
                print(f'err {e}, reading value \"{values[1]}\"')


    return args