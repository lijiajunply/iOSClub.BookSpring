# iOSClub.BookSpring

## 腾讯COS支持

本项目的 `Js/cos-js-sdk-v5.min.js` 是腾讯云的。

本来是想用腾讯云cos写的dotnet版的sdk的，但是我转到.net8环境之后发现根本就跑不通。于是就摆烂了。

## 开发环境注意事项

需自行补一个文件: wwwroot/config.dev.js

内容如下:

```js
window.config = {
    SecretId: '',
    SecretKey: '',
    Bucket: '',
    Region: ''
};
```