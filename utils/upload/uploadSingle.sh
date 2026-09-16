# creates and uploads a single randomly generated packaged
set -e

SERVER_ADDRESS="localhost:5000"

while [ -n "$1" ]; do 
    case "$1" in
    --server_address) SERVER_ADDRESS="${2#*=}" ;;
    esac 
    shift
done

# remove existing packages
rm -rf ./packages

# generate a new randomized package
python3 generate.py --package_count 1 --file_size_max 10000

# upload
python3 uploadAll.py --server_address $SERVER_ADDRESS

echo "Done uploading!"

