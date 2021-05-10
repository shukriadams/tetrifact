:: single-click uploads test package to local dev server
curl ^
    -X POST ^
    -H "Content-Type: multipart/form-data" ^
    -H "Transfer-Encoding: chunked" ^
    -F "Files=@content.zip" ^
    http://localhost:7313/v1/packages/%RANDOM%?isArchive=true 

pause