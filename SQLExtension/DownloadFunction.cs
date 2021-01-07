using Microsoft.SqlServer.Server;
using System;
using System.IO;
using System.Net;
using System.Text;


//下载函数
public static partial class Function
{
    //下载文件
    [SqlFunction(IsDeterministic = true)]
    public static string DownloadFile(string url, string headers, string postParam, string savePath)
    {
        FileInfo file = new FileInfo(savePath);
        if(!file.Directory.Exists)
            file.Directory.Create();
        WebClient webClient = new WebClient();
        if(!string.IsNullOrEmpty(headers))
        {
            foreach(string pair in headers.Split('\r', '\n'))
            {
                int index = -1;
                if(!string.IsNullOrEmpty(pair) && (index = pair.IndexOf(':')) > 0)
                {
                    string key = pair.Substring(0, index);
                    string val = pair.Substring(index + 1).Trim();
                    webClient.Headers[key] = val;
                }
            }
        }
        byte[] data = null;
        if (string.IsNullOrEmpty(postParam))
        {
            data = webClient.DownloadData(url);
        }
        else
        {
            webClient.Headers["Content-Type"] = "application/x-www-form-urlencoded";
            data = webClient.UploadData(url, Encoding.UTF8.GetBytes(postParam));
        }
        File.WriteAllBytes(savePath, data);
        return savePath;
    }
    //下载缓存文件
    [SqlFunction(IsDeterministic = true)]
    public static string DownloadFileCache(string url, string headers, string postParam, string savePath, long cacheMs)
    {
        FileInfo file = new FileInfo(savePath);
        if(file.Exists && (DateTime.Now - file.CreationTime).TotalMilliseconds < cacheMs)
            return savePath;
        else
            return DownloadFile(url, headers, postParam, savePath);
    }


    //下载文本
    [SqlFunction(IsDeterministic = true)]
    public static string DownloadText(string url, string headers, string postParam, string encoding)
    {
        WebClient webClient = new WebClient();
        if(!string.IsNullOrEmpty(headers))
        {
            foreach(string pair in headers.Split('\r', '\n'))
            {
                int index = -1;
                if(!string.IsNullOrEmpty(pair) && (index = pair.IndexOf(':')) > 0)
                {
                    string key = pair.Substring(0, index);
                    string val = pair.Substring(index + 1).Trim();
                    webClient.Headers[key] = val;
                }
            }
        }
        webClient.Encoding = string.IsNullOrEmpty(encoding) ? Encoding.UTF8 : Encoding.GetEncoding(encoding);
        if(string.IsNullOrEmpty(postParam))
        {
            return webClient.DownloadString(url);
        }
        else
        {
            webClient.Headers["Content-Type"] = "application/x-www-form-urlencoded";
            return webClient.UploadString(url, postParam);
        }
    }
    //下载缓存文本
    [SqlFunction(IsDeterministic = true)]
    public static string DownloadTextCache(string url, string headers, string postParam, string encoding, string savePath, long cacheMs)
    {
        Encoding encode = string.IsNullOrEmpty(encoding) ? Encoding.UTF8 : Encoding.GetEncoding(encoding);
        FileInfo file = new FileInfo(savePath);
        if(file.Exists && (DateTime.Now - file.CreationTime).TotalMilliseconds < cacheMs)
            return File.ReadAllText(savePath, encode);
        else
            return File.ReadAllText(DownloadFile(url, headers, postParam, savePath), encode);
    }
}
