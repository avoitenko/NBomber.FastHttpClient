### NBomber.FastHttpClient

The reason for creating this repository is that using HttpClient for load testing comes with a lot of overhead.

Replacing HttpClient to TCPClient gives a performance boost by several times if your server responses are very short, up to 1 ms.

I started the python http Server and measured the performance. The code of HttpServer localted in **server.py** file. Run by **server.bat**

Also I compared results with JMeter which is taken as a standard.


Test result table (amount of iterations, more is better)


>duration : 10s
server response: ~ 1 ms

|threads   |JMETER          |COMMON          | FAST     |
| -------- | -------------- | -------------- | -------- |
| 1        | 17591          | 7330           | 15310    |
| 2        | 28878          | 9033           | 27484    |
| 3        | 30946          | 9319           | 32447    |
| 4        | 32288          | 9485           | 36370    |

As you can see, with server response less than 2 ms the difference is quite significant to abandon HttpClient in favor of alternative solutions.


>duration : 10s
server response: ~ 15 ms

|threads   |JMETER          |COMMON          | FAST    |
| -------- | -------------- | -------------- | ------- |
| 1        | 650            | 586            | 603     |
| 2        | 1262           | 1180           | 1224    |
| 3        | 1885           | 1886           | 1874    |
| 4        | 2492           | 2451           | 2456    |

With a response more than 15 ms, the results are approximately the same for everyone.


TODO:
- add support of HTTPS for FastHttp.Send().
- add some features to better compatibility with HttpClient and NBomber examples.
