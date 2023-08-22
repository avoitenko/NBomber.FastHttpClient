# https://stackoverflow.com/a/76662276

from http.server import ThreadingHTTPServer, BaseHTTPRequestHandler
import time

sleepMsc = lambda x: time.sleep(x/1000.0)

class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        #sleepMsc(1)
        self.send_response(200)
        self.end_headers()
    def do_POST(self):
        #sleepMsc(1)
        self.send_response(200)
        self.end_headers()

PORT = 9090

def run():
    print("start server on port %d" % PORT)
    server = ThreadingHTTPServer(('0.0.0.0', PORT), Handler)
    server.serve_forever()

if __name__ == '__main__':
    run()