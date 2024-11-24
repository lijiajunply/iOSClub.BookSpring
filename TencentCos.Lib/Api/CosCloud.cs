using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TencentCos.Lib.Common;
using HttpMethod = System.Net.Http.HttpMethod;

namespace TencentCos.Lib.Api;

class CosCloud
{
    //文件大于8M时采用分片上传,小于等于8M时采用单文件上传
    private const int SLICE_UPLOAD_FILE_SIZE = 8 * 1024 * 1024;

    //用户计算用户签名超时时间
    private const int SIGN_EXPIRED_TIME = 180;

    //HTTP请求超时时间
    const int HTTP_TIMEOUT_TIME = 60;
    private readonly int appId;
    private readonly string secretId;
    private readonly string secretKey;
    private readonly int timeOut;
    private readonly Request httpRequest;
    private string region = "sh"; //默认上海,可以通过SetRegion方法修改

    /// <summary>
    /// CosCloud 构造方法
    /// </summary>
    /// <param name="appId">授权appid</param>
    /// <param name="secretId">授权secret id</param>
    /// <param name="secretKey">授权secret key</param>
    /// <param name="timeOut">网络超时,默认60秒</param>
    public CosCloud(int appId, string secretId, string secretKey, int timeOut = HTTP_TIMEOUT_TIME)
    {
        this.appId = appId;
        this.secretId = secretId;
        this.secretKey = secretKey;
        this.timeOut = timeOut * 1000;
        httpRequest = new Request();
    }

    /// <summary>
    /// 设置Bucket所在的Region,例如上海sh
    /// </summary>
    /// <param name="stringRegion">地域region</param>
    public void SetRegion(string stringRegion)
    {
        region = stringRegion;
    }

    /// <summary>
    /// 更新文件夹信息
    /// </summary>
    /// <param name="bucketName"> bucket名称</param>
    /// <param name="remotePath">远程文件夹路径</param>
    /// <param name="parameterDic">可选参数Dictionary</param>
    /// 包含如下可选参数：biz_attr:目录属性
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public string UpdateFolder(string bucketName, string remotePath,
        Dictionary<string, string>? parameterDic = null)
    {
        remotePath = HttpUtils.StandardizationRemotePath(remotePath);
        if (string.IsNullOrEmpty(remotePath))
        {
            return constructResult(ERRORCode.ERROR_CODE_PARAMETER_ERROE,
                "ERROR_CODE_PARAMETER_ERROE,remotePath illegal");
        }

        return parameterDic != null ? UpdateFile(bucketName, remotePath, parameterDic) : "";
    }

    /// <summary>
    /// 更新文件信息
    /// </summary>
    /// <param name="bucketName"> bucket名称</param>
    /// <param name="remotePath">远程文件路径</param>
    /// <param name="parameterDic">参数Dictionary</param>
    /// 包含如下可选参数：
    /// forbid:0允许访问 0x01进制访问 0x02进制写访问
    /// biz_attr:文件属性
    /// authority: eInvalid（继承bucket的权限）、eWRPrivate(私有读写)、eWPrivateRPublic(公有读私有写)
    /// 以下参数会打包到custom_headers对象中,携带到cos系统
    /// Cache-Control:
    /// Content-Type:
    /// Content-Disposition:
    /// Content-Language:
    /// x-cos-meta-: 以"x-cos-meta-"为前缀的参数
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public string UpdateFile(string bucketName, string remotePath, Dictionary<string, string>? parameterDic = null)
    {
        var url = generateURL(bucketName, remotePath);
        var data = new Dictionary<string, object>();
        var customerHeaders = new Dictionary<string, object>();
        parameterDic ??= new Dictionary<string, string>();

        data.Add("op", "update");

        //接口中的flag统一cgi设置

        //将forbid设置到data中，这个不用设置flag
        AddParameter(CosParameters.PARA_FORBID, ref data, ref parameterDic);

        //将biz_attr设置到data中
        AddParameter(CosParameters.PARA_BIZ_ATTR, ref data, ref parameterDic);

        //将authority设置到data中
        addAuthority(ref data, ref parameterDic);

        //将customer_headers设置到data["custom_headers"]中
        if (parameterDic.Count != 0 && setCustomerHeaders(ref customerHeaders, ref parameterDic))
        {
            data.Add(CosParameters.PARA_CUSTOM_HEADERS, customerHeaders);
        }

        var sign = Sign.SignatureOnce(appId, secretId, secretKey,
            (remotePath.StartsWith('/') ? "" : "/") + remotePath, bucketName);
        var header = new Dictionary<string, string>
        {
            { "Authorization", sign },
            { "Content-Type", "application/json" }
        };
        return httpRequest.SendRequest(url, ref data, HttpMethod.Post, ref header, timeOut);
    }

    /// <summary>
    /// 删除文件夹
    /// </summary>
    /// <param name="bucketName">bucket名称</param>
    /// <param name="remotePath">远程文件夹路径</param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public string DeleteFolder(string bucketName, string remotePath)
    {
        remotePath = HttpUtils.StandardizationRemotePath(remotePath);
        if (String.IsNullOrEmpty(remotePath))
        {
            return constructResult(ERRORCode.ERROR_CODE_PARAMETER_ERROE,
                "ERROR_CODE_PARAMETER_ERROE,remotePath illegal");
        }

        return DeleteFile(bucketName, remotePath);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="bucketName">bucket名称</param>
    /// <param name="remotePath">远程文件路径</param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public string DeleteFile(string bucketName, string remotePath)
    {
        var url = generateURL(bucketName, remotePath);
        var data = new Dictionary<string, object> { { "op", "delete" } };
        var sign = Sign.SignatureOnce(appId, secretId, secretKey,
            (remotePath.StartsWith("/") ? "" : "/") + remotePath, bucketName);
        var header = new Dictionary<string, string>
        {
            { "Authorization", sign },
            { "Content-Type", "application/json" }
        };

        return httpRequest.SendRequest(url, ref data, HttpMethod.Post, ref header, timeOut);
    }

    /// <summary>
    /// 获取文件夹信息
    /// </summary>
    /// <param name="bucketName">bucket名称</param>
    /// <param name="remotePath">远程文件夹路径</param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public string GetFolderStat(string bucketName, string remotePath)
    {
        remotePath = HttpUtils.StandardizationRemotePath(remotePath);
        if (String.IsNullOrEmpty(remotePath))
        {
            return constructResult(ERRORCode.ERROR_CODE_PARAMETER_ERROE,
                "ERROR_CODE_PARAMETER_ERROE,remotePath illegal");
        }

        return GetFileStat(bucketName, remotePath);
    }

    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="bucketName">bucket名称</param>
    /// <param name="remotePath">远程文件路径</param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public string GetFileStat(string bucketName, string remotePath)
    {
        var url = generateURL(bucketName, remotePath);
        var data = new Dictionary<string, object> { { "op", "stat" } };
        var expired = getExpiredTime();
        var sign = Sign.Signature(appId, secretId, secretKey, expired, bucketName);
        var header = new Dictionary<string, string> { { "Authorization", sign } };
        return httpRequest.SendRequest(url, ref data, HttpMethod.Get, ref header, timeOut);
    }

    /// <summary>
    /// 创建文件夹
    /// </summary>
    /// <param name="bucketName">bucket名称</param>
    /// <param name="remotePath">远程文件夹路径</param>
    /// <param name="parameterDic">参数Dictionary</param>
    /// 包含如下可选参数：biz_attr:目录属性
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public string CreateFolder(string bucketName, string remotePath,
        Dictionary<string, string>? parameterDic = null)
    {
        remotePath = HttpUtils.StandardizationRemotePath(remotePath);
        if (string.IsNullOrEmpty(remotePath))
        {
            return constructResult(ERRORCode.ERROR_CODE_PARAMETER_ERROE,
                "ERROR_CODE_PARAMETER_ERROE,remotePath illegal");
        }

        var url = generateURL(bucketName, remotePath);
        var data = new Dictionary<string, object> { { "op", "create" } };
        AddParameter(CosParameters.PARA_BIZ_ATTR, ref data, ref parameterDic);
        var expired = getExpiredTime();
        var sign = Sign.Signature(appId, secretId, secretKey, expired, bucketName);
        var header = new Dictionary<string, string>
        {
            { "Authorization", sign },
            { "Content-Type", "application/json" }
        };
        return httpRequest.SendRequest(url, ref data, HttpMethod.Post, ref header, timeOut);
    }

    /// <summary>
    /// 目录列表查询所有目录和文件,前缀搜索
    /// </summary>
    /// <param name="bucketName">bucket名称</param>
    /// <param name="remotePath">远程文件夹路径</param>
    /// <param name="parameterDic">参数Dictionary</param>
    /// 包含如下可选参数：
    /// num:拉取的总数,最大1000,如果不带,则默认num=1000
    /// listFlag:大于0返回全部数据，否则返回部分数据
    /// context:透传字段,用于翻页,前端不需理解,需要往后翻页则透传回来
    /// prefix:读取文件/文件夹前缀
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public string GetFolderList(string bucketName, string remotePath, Dictionary<string, string> parameterDic)
    {
        ArgumentNullException.ThrowIfNull(parameterDic);
        var prefix = "";

        if (parameterDic.TryGetValue(CosParameters.PARA_PREFIX, out var value))
            prefix = value;

        remotePath = HttpUtils.StandardizationRemotePath(remotePath);
        if (string.IsNullOrEmpty(remotePath))
        {
            return constructResult(ERRORCode.ERROR_CODE_PARAMETER_ERROE,
                "ERROR_CODE_PARAMETER_ERROE,remotePath illegal");
        }

        var url = generateURL(bucketName, remotePath, prefix);
        var data = new Dictionary<string, object> { { "op", "list" } };

        //num是必选参数
        if (!addFolderListNum(ref data, ref parameterDic))
        {
            return constructResult(ERRORCode.ERROR_CODE_PARAMETER_ERROE, "parameter num value invalidate");
        }

        AddParameter(CosParameters.PARA_CONTEXT, ref data, ref parameterDic);
        AddParameter(CosParameters.PARA_LIST_FLAG, ref data, ref parameterDic);

        var expired = getExpiredTime();
        var sign = Sign.Signature(appId, secretId, secretKey, expired, bucketName);
        var header = new Dictionary<string, string> { { "Authorization", sign } };
        return httpRequest.SendRequest(url, ref data, HttpMethod.Get, ref header, timeOut);
    }

    /// <summary>
    /// 文件上传
    /// 说明: 根据文件大小判断使用单文件上传还是分片上传,当文件大于8M时,内部会进行分片上传,可以携带分片大小sliceSize
    /// 其中分片上传使用SliceUploadInit SliceUploadData SliceUploadFinihs
    /// </summary>
    /// <param name="bucketName">bucket名称</param>
    /// <param name="remotePath">远程文件路径</param>
    /// <param name="localPath">本地文件路径</param>
    /// <param name="parameterDic">参数Dictionary</param>
    /// <param name="isParallel">是否启用线程池并发上传</param>
    /// <param name="minConcurrency">启用并发上传，可以这是最小的并发度，0为系统决定</param>
    /// 包含如下可选参数
    /// bizAttribute：文件属性
    /// insertOnly： 0:同名文件覆盖, 1:同名文件不覆盖,默认1
    /// sliceSize: 分片大小，可选取值为:64*1024 512*1024，1*1024*1024，2*1024*1024，3*1024*1024
    /// <returns></returns>
    public string UploadFile(string bucketName, string remotePath, string localPath,
        Dictionary<string, string> parameterDic = null!, bool isParallel = false, int minConcurrency = 0)
    {
        if (!File.Exists(localPath))
        {
            return constructResult(ERRORCode.ERROR_CODE_FILE_NOT_EXIST, "local file not exist");
        }

        if (remotePath.EndsWith('/'))
        {
            return constructResult(ERRORCode.ERROR_CODE_PARAMETER_ERROE, "file path can not end with '/'");
        }

        var bizAttribute = "";
        if (parameterDic != null! && parameterDic.TryGetValue(CosParameters.PARA_BIZ_ATTR, out var value))
            bizAttribute = value;

        var insertOnly = 1;
        if (parameterDic != null && parameterDic.TryGetValue(CosParameters.PARA_INSERT_ONLY, value: out var value1))
        {
            try
            {
                insertOnly = int.Parse(value1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return constructResult(ERRORCode.ERROR_CODE_PARAMETER_ERROE,
                    "parameter insertOnly value invalidate");
            }
        }

        var fileSize = new FileInfo(localPath).Length;
        if (fileSize <= SLICE_UPLOAD_FILE_SIZE)
        {
            return Upload(bucketName, remotePath, localPath, bizAttribute, insertOnly);
        }

        //分片上传
        var sliceSize = SLICE_SIZE.SLIZE_SIZE_1M;
        if (parameterDic != null && parameterDic.TryGetValue(CosParameters.PARA_SLICE_SIZE, out var value2))
        {
            sliceSize = int.Parse(value2);
            Console.WriteLine("slice size:" + sliceSize);
        }

        var slice_size = getSliceSize(sliceSize);
        return SliceUploadFile(bucketName, remotePath, localPath, bizAttribute, slice_size, insertOnly,
            isParallel, minConcurrency);
    }


    /// <summary>
    /// 分片上传的list请求
    /// </summary>
    /// <param name="bucketName">bucket名称</param>
    /// <param name="remotePath">目标路径</param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public string UploadSliceList(string bucketName, string remotePath)
    {
        var url = generateURL(bucketName, remotePath);
        var data = new Dictionary<string, object> { { "op", "upload_slice_list" } };

        var header = new Dictionary<string, string>();
        var sign = Sign.Signature(appId, secretId, secretKey, getExpiredTime(), bucketName);
        header.Add("Authorization", sign);

        return httpRequest.SendRequest(url, ref data, HttpMethod.Post, ref header, timeOut);
    }

    /// <summary>
    /// 单个文件上传
    /// </summary>
    /// <param name="bucketName">bucket名称</param>
    /// <param name="remotePath">远程文件路径</param>
    /// <param name="localPath">本地文件路径</param>
    /// <param name="bizAttribute">biz_attr属性</param>
    /// <param name="insertOnly">同名文件是否覆盖</param>
    /// <returns></returns>
    public string Upload(string bucketName, string remotePath, string localPath,
        string bizAttribute = "", int insertOnly = 1)
    {
        var url = generateURL(bucketName, remotePath);
        var sha1 = SHA1.GetFileSHA1(localPath);
        var data = new Dictionary<string, object>
        {
            { "op", "upload" },
            { "sha", sha1 },
            { "biz_attr", bizAttribute },
            { "insertOnly", insertOnly }
        };

        var expired = getExpiredTime();
        var sign = Sign.Signature(appId, secretId, secretKey, expired, bucketName);
        var header = new Dictionary<string, string> { { "Authorization", sign } };
        return httpRequest.SendRequest(url, ref data, HttpMethod.Post, ref header, timeOut, localPath);
    }

    /// <summary>
    /// 分片上传 init,如果上一次分片上传未完成，会返回{"code":-4019,"message":"_ERROR_FILE_NOT_FINISH_UPLOAD"}
    /// </summary>
    /// <param name="bucketName">bucket名称</param>
    /// <param name="remotePath">远程文件路径</param>
    /// <param name="localPath">本地文件路径</param>
    /// <param name="fileSha">文件SHA内容</param>
    /// <param name="bizAttribute">biz属性</param>
    /// <param name="sliceSize">切片大小（字节）,默认为1M,目前只支持1M</param>
    /// <param name="insertOnly">是否覆盖同名文件</param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public string SliceUploadInit(string bucketName, string remotePath, string localPath, string fileSha,
        string bizAttribute = "", int sliceSize = SLICE_SIZE.SLIZE_SIZE_1M, int insertOnly = 1)
    {
        sliceSize = SLICE_SIZE.SLIZE_SIZE_1M;
        var url = generateURL(bucketName, remotePath);

        var fileSize = new FileInfo(localPath).Length;

        var data = new Dictionary<string, object>
        {
            { "op", "upload_slice_init" },
            { "filesize", fileSize },
            { "sha", fileSha },
            { "biz_attr", bizAttribute },
            { "slice_size", sliceSize },
            { "insertOnly", insertOnly }
        };

        var uploadParts = ComputeUploadParts(localPath, sliceSize);
        data.Add("uploadparts", uploadParts);


        var expired = getExpiredTime();
        var header = new Dictionary<string, string>();
        var sign = Sign.Signature(appId, secretId, secretKey, expired, bucketName);
        header.Add("Authorization", sign);

        return httpRequest.SendRequest(url, ref data, HttpMethod.Post, ref header, timeOut);
    }

    /// <summary>
    /// 分片上传 
    /// </summary>
    /// <param name="bucketName">bucket名称</param>
    /// <param name="remotePath">远程文件路径</param>
    /// <param name="localPath">本地文件路径</param>
    /// <param name="fileSha">文件的sha1</param>
    /// <param name="session">init请求返回的session</param>
    /// <param name="offset">分片的偏移量</param>
    /// <param name="sliceSize">切片大小（字节）,默认为1M</param>
    /// <param name="sign">签名</param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public string SliceUploadData(string bucketName, string remotePath, string localPath, string fileSha,
        string session, long offset, int sliceSise, string sign)
    {
        var url = generateURL(bucketName, remotePath);
        var data = new Dictionary<string, object>
        {
            { "op", "upload_slice_data" },
            { "session", session },
            { "offset", offset },
            { "sha", fileSha }
        };

        var header = new Dictionary<string, string> { { "Authorization", sign } };
        return httpRequest.SendRequest(url, ref data, HttpMethod.Post, ref header, timeOut, localPath,
            offset, sliceSise);
    }

    /// <summary>
    /// 分片上传 finish
    /// </summary>
    /// <param name="bucketName">bucket名</param>
    /// <param name="remotePath">目标路径</param>
    /// <param name="localPath">本地路径</param>
    /// <param name="fileSha">文件的sha1</param>
    /// <param name="session">init请求返回的session</param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public string SliceUploadFinish(string bucketName, string remotePath, string localPath, string fileSha,
        string session)
    {
        var url = generateURL(bucketName, remotePath);
        var fileSize = new FileInfo(localPath).Length;
        var data = new Dictionary<string, object>
        {
            { "op", "upload_slice_finish" },
            { "session", session },
            { "fileSize", fileSize },
            { "sha", fileSha }
        };

        var header = new Dictionary<string, string>();
        var sign = Sign.Signature(appId, secretId, secretKey, getExpiredTime(), bucketName);
        header.Add("Authorization", sign);
        return httpRequest.SendRequest(url, ref data, HttpMethod.Post, ref header, timeOut);
    }


    private string ComputeUploadParts(string localPath, int sliceSize)
    {
        var buffer = new byte[sliceSize + 1];
        long offset = 0; //文件的偏移
        long totalLen = 0; //总共读取的字节数
        var fileInfo = new FileInfo(localPath);

        var sha = new CosSha1Pure();
        var jsonStr = new StringBuilder();

        jsonStr.Append('[');

        using (var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
        {
            int readLen;
            for (var i = 0; offset < fileInfo.Length; ++i, offset += readLen)
            {
                fileStream.Seek(offset, SeekOrigin.Begin);
                readLen = fileStream.Read(buffer, 0, sliceSize);
                totalLen += readLen;
                string dataSha;

                sha.HashCore(buffer, 0, readLen);
                if ((readLen < sliceSize) || (readLen == sliceSize && totalLen == fileInfo.Length))
                {
                    //最后一片
                    dataSha = sha.FinalHex();
                }
                else
                {
                    //中间的分片
                    dataSha = sha.GetDigest();
                }

                if (i != 0)
                {
                    jsonStr.Append(",{\"offset\":" + offset + "," +
                                   "\"datalen\":" + readLen + "," +
                                   "\"datasha\":\"" + dataSha + "\"}");
                }
                else
                {
                    jsonStr.Append("{\"offset\":" + offset + "," +
                                   "\"datalen\":" + readLen + "," +
                                   "\"datasha\":\"" + dataSha + "\"}");
                }
            }
        }


        jsonStr.Append(']');
        return jsonStr.ToString();
    }

    /// <summary>
    /// 分片上传
    /// </summary>
    /// <param name="bucketName">bucket名称</param>
    /// <param name="remotePath">远程文件路径</param>
    /// <param name="localPath">本地文件路径</param>
    /// <param name="bizAttribute">biz属性</param>
    /// <param name="sliceSize">切片大小（字节）,默认为1M,目前只支持1M</param>
    /// <param name="insertOnly">是否覆盖同名文件</param>
    /// <param name="isParallel">是否启用线程池并发上传</param>
    /// <param name="minConcurrency">启用并发上传，可以这是最小的并发度，0为系统决定</param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public string SliceUploadFile(string bucketName, string remotePath, string localPath,
        string bizAttribute = "", int sliceSize = SLICE_SIZE.SLIZE_SIZE_1M, int insertOnly = 1,
        bool isParallel = false, int minConcurrency = 0)
    {
        var fileSha = SHA1.GetFileSHA1(localPath);
        var fileSize = new FileInfo(localPath).Length;
        var result = SliceUploadInit(bucketName, remotePath, localPath, fileSha, bizAttribute, sliceSize,
            insertOnly);
        var obj = JObject.Parse(result);
        if ((int)obj["code"]! != 0)
        {
            return result;
        }

        var data = obj["data"];
        if (data!["access_url"] != null)
        {
            var accessUrl = data["access_url"];
            Console.WriteLine("命中秒传" + data);
            return result;
        }


        var session = data["session"]?.ToString();
        sliceSize = (int)data["slice_size"];

        var sign = Sign.Signature(appId, secretId, secretKey, getExpiredTime(), bucketName);

        if (isParallel)
        {
            if (minConcurrency > 0)
            {
                ThreadPool.SetMinThreads(minConcurrency, minConcurrency);
            }

            var tasks = new List<Task<string>>();
            for (long offset = 0; offset < fileSize; offset += sliceSize)
            {
                long localOffset = offset;
                tasks.Add(Task.Factory.StartNew(() => SliceUploadData(bucketName, remotePath, localPath,
                    fileSha, session, localOffset,
                    sliceSize, sign)));
            }

            Task.WaitAll(tasks.ToArray());

            foreach (var item in tasks)
            {
                obj =JObject.Parse(item.Result);
                if (obj["code"]?.ToObject<int>() != 0)
                {
                    return item.Result;
                }
            }
        }
        else
        {
            int retryCount = 0;
            for (long offset = 0; offset < fileSize; offset += sliceSize)
            {
                result = SliceUploadData(bucketName, remotePath, localPath, fileSha, session, offset, sliceSize,
                    sign);
                obj = JObject.Parse(result);
                if ((int)obj["code"] != 0)
                {
                    //总共重试10次
                    if (retryCount < 10)
                    {
                        ++retryCount;
                        offset -= sliceSize;
                        //Console.WriteLine("重试...");
                        continue;
                    }

                    //Console.WriteLine("upload fail");
                    return result;
                }
            }
        }

        return SliceUploadFinish(bucketName, remotePath, localPath, fileSha, session);
    }


    /// <summary>
    /// 内部方法：增加参数insertOnly到data中
    /// </summary>
    /// <returns></returns>
    private bool AddInsertOnly(ref Dictionary<string, object> data, ref Dictionary<string, string> paras)
    {
        if (data == null! || paras == null!)
        {
            return false;
        }

        if (!paras.TryGetValue(CosParameters.PARA_INSERT_ONLY, out var para)) return true;
        if (string.IsNullOrEmpty(para)) return false;
        para = para.Trim();
        if (!para.Equals("1") && !para.Equals("0"))
        {
            Console.WriteLine("insertOnly value error, please refer to the RestfullAPI, it will be ignored");
        }
        else
        {
            data.Add(CosParameters.PARA_INSERT_ONLY, para);
        }

        return true;
    }

    /// <summary>
    /// 内部方法：增加参数到data中
    /// </summary>
    /// <returns></returns>
    private bool AddParameter(string paraName, ref Dictionary<string, object> data,
        ref Dictionary<string, string> paras)
    {
        if (data == null! || paras == null! || paraName == null!)
        {
            return false;
        }

        if (!paras.TryGetValue(paraName, value: out var para)) return false;
        data.Add(paraName, para);
        return true;
    }

    /// <summary>
    /// 内部方法：增加参数Pattern到data中
    /// </summary>
    /// <returns></returns>
    private bool AddPattern(ref Dictionary<string, object> data, ref Dictionary<string, string> paras)
    {
        if (data == null! || paras == null!)
        {
            return false;
        }

        if (paras.TryGetValue(CosParameters.PARA_PATTERN, value: out var para))
        {
            var val = para;
            if (val.Equals(FolderPattern.PATTERN_FILE) || val.Equals(FolderPattern.PATTERN_DIR) ||
                val.Equals(FolderPattern.PATTERN_BOTH))
            {
                data.Add(CosParameters.PARA_PATTERN, val);
            }
            else
            {
                return false;
            }
        }
        else
        {
            data.Add(CosParameters.PARA_PATTERN, FolderPattern.PATTERN_BOTH);
        }

        return true;
    }

    /// <summary>
    /// 内部方法：增加参数order到data中
    /// </summary>
    /// <returns></returns>
    private bool addOrder(ref Dictionary<string, object> data, ref Dictionary<string, string> paras)
    {
        if (data == null! || paras == null!)
        {
            return false;
        }

        if (paras.TryGetValue(CosParameters.PARA_ORDER, out var para))
        {
            int val;
            try
            {
                val = int.Parse(para);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            if (val == 0 || val == 1)
            {
                data.Add(CosParameters.PARA_ORDER, val);
            }
            else
            {
                return false;
            }
        }
        else
        {
            data.Add(CosParameters.PARA_ORDER, 0);
        }

        return true;
    }

    /// <summary>
    /// 内部方法：增加参数num到data中
    /// </summary>
    /// <returns></returns>
    private bool addFolderListNum(ref Dictionary<string, object> data, ref Dictionary<string, string> paras)
    {
        if (data == null! || paras == null!)
        {
            return false;
        }

        if (paras.TryGetValue(CosParameters.PARA_NUM, out var value))
        {
            int listnum;
            try
            {
                listnum = int.Parse(value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            if (listnum is <= 0 or >= 1001)
                return false;
            data.Add(CosParameters.PARA_NUM, listnum);
        }
        else
        {
            data.Add(CosParameters.PARA_NUM, 1000);
        }

        return true;
    }

    /// <summary>
    /// 内部方法：增加参数authority到data中
    /// </summary>
    /// <returns></returns>		
    private bool addAuthority(ref Dictionary<string, object> data, ref Dictionary<string, string> paras)
    {
        if (data == null! || paras == null!)
        {
            return false;
        }

        if (!paras.TryGetValue(CosParameters.PARA_AUTHORITY, out var val)) return false;
        if (!isAuthorityValid(val))
        {
            Console.WriteLine("authority value error, please refer to the RestfullAPI");
            return false;
        }

        data.Add(CosParameters.PARA_AUTHORITY, val);
        return true;
    }

    /// <summary>
    /// 内部方法：增加用户自定义参数到data中
    /// </summary>
    /// <returns></returns>	
    private bool addCustomerHeaders(ref Dictionary<string, object> data, ref Dictionary<string, string> paras)
    {
        if (data == null! || paras == null!)
        {
            return false;
        }

        foreach (var dic in paras.Where(dic => isCustomerHeader(dic.Key)))
        {
            if (!data.ContainsKey(dic.Key))
            {
                data.Add(dic.Key, dic.Value);
            }
        }

        return true;
    }

    /// <summary>
    /// 内部方法：增加用户自定义参数到data中
    /// </summary>
    /// <returns></returns>			
    private bool setCustomerHeaders(ref Dictionary<string, object> data,
        ref Dictionary<string, string> parameterDic)
    {
        var flag = false;
        if (parameterDic.Count == 0)
        {
            return flag;
        }

        foreach (var dic in parameterDic)
        {
            if (!isCustomerHeader(dic.Key) || data.ContainsKey(dic.Key)) continue;
            data.Add(dic.Key, dic.Value);
            flag = true;
        }

        return flag;
    }

    /// <summary>
    /// 内部方法：判断是否为用户自定义参数
    /// </summary>
    /// <returns></returns>				
    private bool isCustomerHeader(string key)
    {
        return key.Equals(CosParameters.PARA_CACHE_CONTROL)
               || key.Equals(CosParameters.PARA_CONTENT_TYPE)
               || key.Equals(CosParameters.PARA_CONTENT_DISPOSITION)
               || key.Equals(CosParameters.PARA_CONTENT_LANGUAGE)
               || key.StartsWith(CosParameters.PARA_X_COS_META_PREFIX);
    }

    /// <summary>
    /// 内部方法：判断参数authority参数值是否合法
    /// </summary>
    /// <returns></returns>				
    private bool isAuthorityValid(string authority)
    {
        if (string.IsNullOrEmpty(authority))
            return false;

        return authority.Equals("eInvalid") || authority.Equals("eWRPrivate") ||
               authority.Equals("eWPrivateRPublic");
    }

    /// <summary>
    /// 内部方法：超时时间(当前系统时间+300秒)
    /// </summary>
    /// <returns></returns>	
    private long getExpiredTime()
    {
        return DateTime.UtcNow.ToUnixTime() / 1000 + SIGN_EXPIRED_TIME;
    }

    /// <summary>
    /// 内部方法：用户传入的slice_size进行规范,[64k,3m],大于1m必须是1m的整数倍
    /// </summary>
    /// <returns></returns>	
    private int getSliceSize(int sliceSize)
    {
        if (sliceSize < SLICE_SIZE.SLIZE_SIZE_64K)
        {
            return SLICE_SIZE.SLIZE_SIZE_64K;
        }

        if (sliceSize < SLICE_SIZE.SLIZE_SIZE_1M)
        {
            //size = SLICE_SIZE.SLIZE_SIZE_512K;
            return sliceSize;
        }

        if (sliceSize < SLICE_SIZE.SLIZE_SIZE_2M)
        {
            return SLICE_SIZE.SLIZE_SIZE_1M;
        }

        return sliceSize < SLICE_SIZE.SLIZE_SIZE_3M ? SLICE_SIZE.SLIZE_SIZE_2M : SLICE_SIZE.SLIZE_SIZE_3M;
    }

    /// <summary>
    /// 内部方法：构造URL
    /// </summary>
    /// <returns></returns>
    private string generateURL(string bucketName, string remotePath)
        => "http://" + region + ".file.myqcloud.com/files/v2/" + appId + "/" + bucketName +
           HttpUtils.EncodeRemotePath(remotePath);

    /// <summary>
    /// 内部方法：构造URL
    /// </summary>
    /// <returns></returns>
    private string generateURL(string bucketName, string remotePath, string prefix)
        => "http://" + region + ".file.myqcloud.com/files/v2/" + appId + "/" + bucketName +
           HttpUtils.EncodeRemotePath(remotePath) + HttpUtility.UrlEncode(prefix);

    /// <summary>
    /// 内部方法：构造结果响应
    /// </summary>
    /// <returns></returns>
    private static string constructResult(int code, string message)
    {
        var result = new Dictionary<string, object>
        {
            { "code", code },
            { "message", message }
        };
        return JsonConvert.SerializeObject(result);
    }
}