curl 

curl -X post  \
    -H "Content-Type: multipart/form-data;" \
    -F Files=@files/1.txt \
    -F Files=@files/2.txt \
    http://localhost:3000/v1/packages/myPackage 

    #-H "boundary=-------------------------acebdf13572468" \
