using System.IO.Compression;
using System.Text;

namespace BookSpring.WebApp.Models;

public static class GZipServer
{
    /// <summary>
    /// 压缩方法
    /// </summary>
    public static string CompressString(string str)
    {
        var compressBeforeByte = Encoding.GetEncoding("UTF-8").GetBytes(str);
        var compressAfterByte = Compress(compressBeforeByte);
        return Convert.ToBase64String(compressAfterByte);
    }

    private static byte[] Compress(byte[] data)
    {
        try
        {
            var ms = new MemoryStream();
            var zip = new GZipStream(ms, CompressionMode.Compress, true);
            zip.Write(data, 0, data.Length);
            zip.Close();
            var buffer = new byte[ms.Length];
            ms.Position = 0;
            _ = ms.Read(buffer, 0, buffer.Length);
            ms.Close();
            return buffer;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// 字符串解压缩
    /// </summary>
    public static string DecompressString(string str)
    {
        return Encoding.GetEncoding("UTF-8").GetString(Decompress(Convert.FromBase64String(str)));
    }

    private static byte[] Decompress(byte[] data)
    {
        try
        {
            var ms = new MemoryStream(data);
            var zip = new GZipStream(ms, CompressionMode.Decompress, true);
            var stream = new MemoryStream();
            var buffer = new byte[0x1000];
            while (true)
            {
                var reader = zip.Read(buffer, 0, buffer.Length);
                if (reader <= 0) break;
                stream.Write(buffer, 0, reader);
            }

            zip.Close();
            ms.Close();
            stream.Position = 0;
            buffer = stream.ToArray();
            stream.Close();
            return buffer;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}