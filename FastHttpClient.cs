using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using NBomber.Contracts.Internal.Cluster;

namespace NBomber.FastHttpClient;


//+------------------------------------------------------------------+
public class FastHttpClient
{
    public int SendTimeout { get; set; }
    public int ReceiveTimeout { get; set; }
    public Version DefaultRequestVersion { get; set; }
    public Dictionary<string, string> DefaultRequestHeaders { get; set; }

    //+------------------------------------------------------------------+    
    public FastHttpClient()
    {
        SendTimeout = 5000;
        ReceiveTimeout = 1000;
        DefaultRequestVersion = new Version(1, 1);
        DefaultRequestHeaders = new Dictionary<string, string>();
    }
}


//+------------------------------------------------------------------+
public class FastResponseMessage
{
    public Dictionary<string, string> Headers { get; set; }
    public string Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }

    //---
    public bool IsError
    {
        get => (int)StatusCode < 200 || (int)StatusCode > 204;
    }

    //---
    public FastResponseMessage()
    {
        Headers = new Dictionary<string, string>();
        Message = String.Empty;
        StatusCode = HttpStatusCode.OK;
    }
}


//+------------------------------------------------------------------+
public class FastHttp
{

    //---
    public static HttpRequestMessage CreateRequest(string method, string url)
    {
        //--- add prefix by default
        if (!url.Contains("https://") && !url.Contains("http://"))
        {
            url = "http://" + url;
        }
        //---
        return new HttpRequestMessage(new HttpMethod(method), new Uri(url, UriKind.RelativeOrAbsolute));
    }


    //---
    public static Task<FastResponseMessage> Send(FastHttpClient client, HttpRequestMessage request)
    {

        FastResponseMessage result = new FastResponseMessage();

        try
        {

            //---
            if (request.RequestUri is null)
            {
                throw new Exception("RequestUri is null");
            }

            //---
            if (request.Content is null)
            {
                throw new Exception("Content is null");
            }

            //---
            using (var tcpClient = new TcpClient(request.RequestUri.Host, request.RequestUri.Port))
            {
                //---
                tcpClient.SendTimeout = client.SendTimeout;
                tcpClient.ReceiveTimeout = client.ReceiveTimeout;

                //---
                using (NetworkStream stream = tcpClient.GetStream())
                using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII, 8096, true))
                using (StreamReader reader = new StreamReader(stream, Encoding.ASCII))
                {
                    
                    string strMethod = request.Method.ToString().ToUpper();
                    string strVersion = request.Version.ToString();
                    
                    //--- construct and send the HTTP request
                    string header = $"{strMethod} {request.RequestUri.PathAndQuery} HTTP/{strVersion}\r\n";

                    //---
                    var requestStream = request.Content.ReadAsStream();

                    //--- deafult headers
                    foreach (var it in client.DefaultRequestHeaders)
                    {
                        header += $"{it.Key} : {it.Value}\r\n";
                    }

                    //---
                    if (request.Headers is not null)
                    {
                        foreach (var it in request.Headers)
                        {
                            header += $"{it.Key} : {it.Value.FirstOrDefault()}\r\n";
                        }

                        //--- add connection
                        if (!request.Headers.Contains("Connection"))
                        {
                            header += "Connection: close\r\n";
                        }

                        //--- add Content-Length


                        if (request.Content is not null && requestStream.Length > 0)
                        {
                            header += $"Content-Length: {requestStream.Length}\r\n";
                        }

                    }

                    //--- add end
                    if (header.Length > 0)
                    {
                        if (!header.Contains("\r\n\r\n"))
                            header += "\r\n";
                    }

                    //---
                    if (requestStream is not null && requestStream.Length > 0)
                    {
                        byte[] buffer = new byte[requestStream.Length];
                        requestStream.Read(buffer, 0, (int)requestStream.Length);
                        header += Encoding.UTF8.GetString(buffer);
                    }


                    //---
                    //byte[] headerBytes = Encoding.UTF8.GetBytes(header.ToString());

                    //---
                    char[] headerBytes = header.ToString().ToCharArray();
                    writer.Write(headerBytes, 0, headerBytes.Length);
                    writer.Flush();

                    //---
                    var response = reader.ReadToEnd();

                    //---
                    string[] lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    for (int i = 0; i < lines.Length; i++)
                    {

                        //--- get result
                        if (i == 0)
                        {
                            string[] items = lines[i].Split(new char[] { ' ' }, 3);
                            if (items.Length == 3)
                            {
                                //--- #1
                                if (items[0].Contains("HTTP/"))
                                {
                                    string strVersion1 = items[0].Substring(5, items[0].Length - 5);
                                    double version;
                                    if (double.TryParse(strVersion1, out version))
                                    {
                                        if (version == 1.0 || version == 1.1 || version == 2.0)
                                        {

                                        }
                                        else
                                        {
                                            throw new Exception($"bad format version {strVersion}");
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception($"bad format version {items[0]}");
                                    }
                                }
                                else
                                {
                                    throw new Exception($"bad format http {items[0]}");
                                }


                                //--- #2
                                /*
                                int ResponseCode;
                                if (!int.TryParse(items[1], out ResponseCode))
                                {
                                    throw new Exception("bad response code");
                                }
                                */

                                //--- #3
                                string strCode = items[2].Replace(" ", "");
                                HttpStatusCode code;
                                if (!Enum.TryParse(strCode, true, out code))
                                {
                                    throw new Exception($"bad response code '{strCode}'");
                                }
                                result.StatusCode = code;

                            }

                            continue;
                        }


                        //---
                        string[] parts = lines[i].Split(new char[] { ':' }, 2);

                        //--- get headers
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();
                            result.Headers.Add(key, value);
                            continue;
                        }


                        //--- get body
                        if (lines[i] == string.Empty && i == lines.Length - 2)
                        {
                            result.Message = lines[i + 1];
                            break;
                        }
                    }
                }
            }
        }
        catch (SocketException ex)
        {
            result.Message = ex.Message;
            result.StatusCode = HttpStatusCode.InternalServerError;
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            result.Message = ex.Message;
            result.StatusCode = HttpStatusCode.InternalServerError;
        }

        return Task.FromResult(result);
    }
}
