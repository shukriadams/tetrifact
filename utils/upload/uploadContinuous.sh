set -e

while /bin/true; do
    # remove existing
    rm -rf ./packages
    
    # generate
    python3 ./generate.py --package_count 1 --file_size_max 10000

    # upload
    python3 ./uploadAll.py

    #
    echo "Pausing ..."
    sleep 1m
done &