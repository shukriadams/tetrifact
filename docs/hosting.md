## Nginx Forward Proxy

A common scenario when hosting docker-based sites in production is to expose the container app to the outside world with Nginx. This
works well with Tetrifact, but you will likely need to tweak Nginx for this to work properly. Specfically, make sure you increase the
maximum body size and timeouts so your Tetrifact server can handle larger files. For example



    server {

        listen 80;
        server_name tetrifact.example.com;

        client_max_body_size 10G;
        proxy_read_timeout 900s;

        location / {
            rewrite ^/tetrifact.example.com?(.*) /$1 break;
            proxy_pass http://127.0.0.1:49000;
        }
    }

