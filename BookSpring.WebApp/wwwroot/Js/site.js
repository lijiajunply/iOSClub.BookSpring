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

function setLocalStorage(key, value) {
    localStorage.setItem(key, value);
}

function getLocalStorage(key) {
    return localStorage.getItem(key);
}

function removeLocalStorage(key) {
    localStorage.removeItem(key);
}

function clearLocalStorage(){
    localStorage.clear();
}


async function copyText(content) {
    try {
        await navigator.clipboard.writeText(content);
        return true;
    } catch (err) {
        console.error('复制失败: ', err);
        return false;
    }
}

function GetCos() {
    const now = Math.floor(Date.now() / 1000);
    return new COS({
        SecretId: window.config.SecretId,
        SecretKey: window.config.SecretKey,
        StartTime: now,
        ExpiredTime: now + 1800,
    });
}

async function UploadFile() {
    const file = document.getElementById('fileSelector').files[0];
    if (!file) {
        return "";
    }
    const meter = document.getElementById('meter')
    const cos = GetCos()
    cos.uploadFile({
        Bucket: window.config.Bucket,
        Region: window.config.Region,
        Key: 'ilib/' + file.name,
        Body: file,
        SliceSize: 1024 * 1024 * 5,
        onProgress: function (progressData) {
            console.log(JSON.stringify(progressData));
            meter.value = progressData.percent;
            if (progressData.percent === 1) {
                meter.style.display = 'none';
            }
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

function UploadFiles() {
    const files = document.getElementById('fileSelector').files;
    const cos = GetCos()
    let arr = [];
    const meter = document.getElementById('meter')
    for (let i = 0; i < files.length; i++) {
        const file = files[i];
        cos.uploadFile({
            Bucket: window.config.Bucket,
            Region: window.config.Region,
            Key: 'ilib/' + file.name,
            Body: file,
            SliceSize: 1024 * 1024 * 5,
            onProgress: function (progressData) {
                console.log(JSON.stringify(progressData));
                meter.value = progressData.percent;
                if (progressData.percent === 1) {
                    meter.value = 0
                }
            }
        })
        arr.push(file.name);
    }
    return JSON.stringify(arr);
}

function fileDownload(url) {
    const cos = GetCos()
    cos.getObjectUrl({
        Bucket: window.config.Bucket,
        Region: window.config.Region,
        Key: url,
    }, function (err, data) {
        if (err) return console.log(err);
        const downloadUrl = data.Url + (data.Url.indexOf('?') > -1 ? '&' : '?') + 'response-content-disposition=attachment';
        window.open(downloadUrl);
    });
}

function deleteFile(url) {
    const cos = GetCos()
    cos.deleteObject({
        Bucket: window.config.Bucket,
        Region: window.config.Region,
        Key: url,
    }, function (err, data) {
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
    const cos = GetCos()
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