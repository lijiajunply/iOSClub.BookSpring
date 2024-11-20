function isWeiXin() {
    const ua = navigator.userAgent
    return !!/MicroMessenger/i.test(ua)
}

function NavigateTo(url, webUrl) {
    const u = navigator.userAgent;
    const isAndroid = u.indexOf('Android') > -1 || u.indexOf('Adr') > -1; //android终端
    const isiOS = !!u.match(/\(i[^;]+;( U;)? CPU.+Mac OS X/); //ios终端
    let result = isAndroid || isiOS;
    if(u.indexOf('MQQBrowser') !== -1 || !result){
        window.open(webUrl)
    }else{
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

function initializeDrag() {
    let isDragging = false;
    let offsetX, offsetY;

    let ref = document.getElementById('header');
    let element = document.getElementById('draggable');

    ref.onmousedown = (event) => {
        isDragging = true;
        offsetX = event.clientX - element.getBoundingClientRect().left;
        offsetY = event.clientY - element.getBoundingClientRect().top;
        document.addEventListener('mousemove', mouseMoveHandler);
        document.addEventListener('mouseup', mouseUpHandler);
    };

    const mouseMoveHandler = (event) => {
        // let a = Number.parseFloat(element.style.top.slice(0,-2))
        if (isDragging) {
            element.style.left = (event.clientX - offsetX) + 'px';
            element.style.top = (event.clientY - offsetY) + 'px';
        }
    };

    const mouseUpHandler = () => {
        isDragging = false;
        document.removeEventListener('mousemove', mouseMoveHandler);
        document.removeEventListener('mouseup', mouseUpHandler);
    };


}

function playVideo() {
    const video = document.getElementById("videoPlayer");
    video.play();
}

function pauseVideo() {
    const video = document.getElementById("videoPlayer");
    video.pause();
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