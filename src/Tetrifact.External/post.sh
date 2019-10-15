curl -X post  \
    -H "Content-Type: multipart/form-data;" \
    -H "Transfer-Encoding: chunked" \
    -F "Files=@files/1.txt;filename=anotherPath/1.txt" \
    -F "Files=@files/2.txt;filename=anotherPath/2.txt" \
    http://localhost:3000/v1/packages/myPackage 
