### NBomber.FastHttpClient

The reason for creating this repository is that using HttpClient for load testing comes with a lot of overhead.

Replacing HttpClient to TCPClient gives a performance boost by several times if your responses are very short, up to 1 ms.

I started the Http Server and measured the performance. The code of HttpServer localted in server.py file.

Also I compared results with JMeter which is taken as a standard.

duration : 10s
server response: < 1ms

Test result table (amount of iterations, more is better)

|threads   |JMETER          |COMMON           | FAST     |
| -------- | -------------- | --------------- | -------- |
| 1        | 15000          | 15000           | 15000    |
| 2        | 15000          | 15000           | 15000    |
| 3        | 15000          | 15000           | 15000    |
| 4        | 15000          | 15000           | 15000    |

As you can see, the difference is quite significant to abandon HttpClient in favor of alternative solutions.

TODO:
- add support of HTTPS for FastHttp.Send().
- add some features to better compatibility with HttpClient and NBomber examples.
