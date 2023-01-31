function EncryptPwd(val)
{
    var lVal = $('#k-val').val();
    var cc = CryptoJS.enc.Utf8.parse(lVal);
    var encryptedPwd = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(val), cc,
        {
            keySize: 128 / 8,
            iv: cc,
            mode: CryptoJS.mode.CBC,
            padding: CryptoJS.pad.Pkcs7
        });
    return encryptedPwd;
}