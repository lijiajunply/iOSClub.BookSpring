using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace TencentCos.Lib.Common;

//enum HttpMethod { Get, Post};
/// <summary>
/// 请求调用类
/// </summary>
class Request
{
    [Obsolete("已过时")]
    public string SendRequest(string url, ref Dictionary<string, object> data, HttpMethod requestMethod,
        ref Dictionary<string, string> header, int timeOut, string? localPath = null, long offset = -1,
        int sliceSize = 0)
    {
        try
        {
            ServicePointManager.Expect100Continue = false;
            if (requestMethod == HttpMethod.Get)
            {
                var paramStr = "";
                foreach (var key in data.Keys)
                {
                    paramStr += $"{key}={HttpUtility.UrlEncode(data[key].ToString())}&";
                }

                paramStr = paramStr.TrimEnd('&');
                url += (url.EndsWith('?') ? "&" : "?") + paramStr;
            }

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = CosDefaultValue.ACCEPT;
            request.KeepAlive = true;
            request.UserAgent = CosDefaultValue.USER_AGENT_VERSION;
            request.Timeout = timeOut;
            foreach (var key in header.Keys)
            {
                if (key == "Content-Type")
                {
                    request.ContentType = header[key];
                }
                else
                {
                    request.Headers.Add(key, header[key]);
                }
            }

            if (requestMethod == HttpMethod.Post)
            {
                request.Method = requestMethod.ToString().ToUpper();
                var memStream = new MemoryStream();
                if (header.ContainsKey("Content-Type") && header["Content-Type"] == "application/json")
                {
                    var json = JsonConvert.SerializeObject(data);
                    var jsonByte = Encoding.GetEncoding("utf-8").GetBytes(json);
                    memStream.Write(jsonByte, 0, jsonByte.Length);
                }
                else
                {
                    var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
                    var beginBoundary = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                    var endBoundary = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    request.ContentType = "multipart/form-data; boundary=" + boundary;

                    var strBuf = new StringBuilder();
                    foreach (var key in data.Keys)
                    {
                        strBuf.Append("\r\n--" + boundary + "\r\n");
                        strBuf.Append("Content-Disposition: form-data; name=\"" + key + "\"\r\n\r\n");
                        strBuf.Append(data[key]);
                    }

                    var paramsByte = Encoding.GetEncoding("utf-8").GetBytes(strBuf.ToString());
                    memStream.Write(paramsByte, 0, paramsByte.Length);


                    if (localPath != null)
                    {
                        memStream.Write(beginBoundary, 0, beginBoundary.Length);
                        var fileInfo = new FileInfo(localPath);
                        using var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                        const string filePartHeader =
                            "Content-Disposition: form-data; name=\"fileContent\"; filename=\"{0}\"\r\n" +
                            "Content-Type: application/octet-stream\r\n\r\n";
                        var headerText = string.Format(filePartHeader, fileInfo.Name);
                        var headerBytes = Encoding.UTF8.GetBytes(headerText);
                        memStream.Write(headerBytes, 0, headerBytes.Length);

                        if (offset == -1)
                        {
                            var buffer = new byte[1024];
                            int bytesRead;
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                memStream.Write(buffer, 0, bytesRead);
                            }
                        }
                        else
                        {
                            var buffer = new byte[sliceSize];
                            fileStream.Seek(offset, SeekOrigin.Begin);
                            var bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                            memStream.Write(buffer, 0, bytesRead);
                        }
                    }

                    memStream.Write(endBoundary, 0, endBoundary.Length);
                }

                request.ContentLength = memStream.Length;
                var requestStream = request.GetRequestStream();
                memStream.Position = 0;
                var tempBuffer = new byte[memStream.Length];
                _ = memStream.Read(tempBuffer, 0, tempBuffer.Length);
                memStream.Close();

                requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                requestStream.Close();

                //Console.WriteLine(strBuf.ToString());
            }

            //Console.WriteLine(request.ContentType.ToString());


            var response = request.GetResponse();
            using var s = response.GetResponseStream();
            var reader = new StreamReader(s, Encoding.UTF8);
            var rsp_data = reader.ReadToEnd();
            response.Close();

            return rsp_data;
        }
        catch (WebException we)
        {
            if (we.Status != WebExceptionStatus.ProtocolError) throw;
            using var s = we.Response!.GetResponseStream();
            var reader = new StreamReader(s, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}