function isWeiXin() {
    const ua = navigator.userAgent
    return !!/MicroMessenger/i.test(ua)
}

function NavigateTo(url, webUrl) {
    const u = navigator.userAgent;
    const isAndroid = u.indexOf('Android') > -1 || u.indexOf('Adr') > -1; //android终端
    const isiOS = !!u.match(/\(i[^;]+;( U;)? CPU.+Mac OS X/); //ios终端
    let result = isAndroid || isiOS;
    if (u.indexOf('MQQBrowser') !== -1 || !result) {
        window.open(webUrl)
    } else {
        window.open(url)
    }
}

function jsSaveAsFile(filename, byteBase64) {
    const link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + byteBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// wwwroot/localStorage.js

window.localStorageHelper = {
    setItem: function (key, value) {
        localStorage.setItem(key, value);
    },
    getItem: function (key) {
        return localStorage.getItem(key);
    },
    removeItem: function (key) {
        localStorage.removeItem(key);
    },
    clear: function () {
        localStorage.clear();
    }
};

async function copyText(content) {
    try {
        await navigator.clipboard.writeText(content);
        return true;
    } catch (err) {
        console.error('复制失败: ', err);
        return false;
    }
}

async function UploadFile() {
    const file = document.getElementById('fileSelector').files[0];
    if (!file) {
        return "";
    }
    const now = Math.floor(Date.now() / 1000);
    const cos = new COS({
        SecretId: window.config.SecretId, // sts服务下发的临时 secretId
        SecretKey: window.config.SecretKey, // sts服务下发的临时 secretKey
        StartTime: now, // 建议传入服务端时间，可避免客户端时间不准导致的签名错误
        ExpiredTime: now + 1800, // 临时密钥过期时间
    });
    cos.uploadFile({
        Bucket: window.config.Bucket, /* 填写自己的 bucket，必须字段 */
        Region: window.config.Region,     /* 存储桶所在地域，必须字段 */
        Key: 'ilib/' + file.name,              /* 存储在桶里的对象键（例如:1.jpg，a/b/test.txt，图片.jpg）支持中文，必须字段 */
        Body: file, // 上传文件对象
        SliceSize: 1024 * 1024 * 5,     /* 触发分块上传的阈值，超过5MB使用分块上传，小于5MB使用简单上传。可自行设置，非必须 */
        onProgress: function (progressData) {
            console.log(JSON.stringify(progressData));
        }
    }, function (err, _) {
        if (err) {
            console.log('上传失败', err);
        } else {
            console.log('上传成功');
        }
        return 'ilib/' + file.name;
    });
    return 'ilib/' + file.name;
}

function fileDownload(url) {
    const now = Math.floor(Date.now() / 1000);
    const cos = new COS({
        SecretId: window.config.SecretId, // sts服务下发的临时 secretId
        SecretKey: window.config.SecretKey, // sts服务下发的临时 secretKey
        StartTime: now, // 建议传入服务端时间，可避免客户端时间不准导致的签名错误
        ExpiredTime: now + 1800, // 临时密钥过期时间
    });
    cos.getObjectUrl({
        Bucket: window.config.Bucket, /* 填写自己的 bucket，必须字段 */
        Region: window.config.Region,     /* 存储桶所在地域，必须字段 */
        Key: url,              /* 存储在桶里的对象键（例如1.jpg，a/b/test.txt），必须字段 */
    }, function (err, data) {
        if (err) return console.log(err);
        const downloadUrl = data.Url + (data.Url.indexOf('?') > -1 ? '&' : '?') + 'response-content-disposition=attachment';
        window.open(downloadUrl);
    });
}

function deleteFile(url) {
    const now = Math.floor(Date.now() / 1000);
    const cos = new COS({
        SecretId: window.config.SecretId, // sts服务下发的临时 secretId
        SecretKey: window.config.SecretKey, // sts服务下发的临时 secretKey
        StartTime: now, // 建议传入服务端时间，可避免客户端时间不准导致的签名错误
        ExpiredTime: now + 1800, // 临时密钥过期时间
    });
    cos.deleteObject({
        Bucket: window.config.Bucket, /* 填写自己的 bucket，必须字段 */
        Region: window.config.Region,     /* 存储桶所在地域，必须字段 */
        Key: url,              /* 存储在桶里的对象键（例如1.jpg，a/b/test.txt），必须字段 */
    }, function(err, data) {
        console.log(err || data);
    });
}

function updateImageDisplay(url) {
    const file = document.getElementById('fileSelector').files[0];
    if (!file) {
        return;
    }
    let fileName = 'ilib/' + file.name;
    deleteFile(url)
    const now = Math.floor(Date.now() / 1000);
    const cos = new COS({
        SecretId: window.config.SecretId, 
        SecretKey: window.config.SecretKey,
        StartTime: now, 
        ExpiredTime: now + 1800, 
    });
    cos.uploadFile({
        Bucket: window.config.Bucket,
        Region: window.config.Region, 
        Key: fileName,      
        Body: file,
        SliceSize: 1024 * 1024 * 5,
        onProgress: function (progressData) {
            console.log(JSON.stringify(progressData));
        }
    }, function (err, _) {
        if (err) {
            console.log('上传失败', err);
        } else {
            console.log('上传成功');
        }
    });
    
    return fileName;
}