using Microsoft.SqlServer.Server;
using System;
using System.IO;
using System.Net;
using System.Text;


//下载函数
public static partial class Function
{
    //下载文件
    [SqlFunction]
    public static long DownloadFile(string url, string headers, string postParam, string savePath)
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
        if(string.IsNullOrEmpty(postParam))
            data = webClient.DownloadData(url);
        else
            data = webClient.UploadData(url, Encoding.UTF8.GetBytes(postParam));
        File.WriteAllBytes(savePath, data);
        return data.LongLength;
    }
    //下载文件并缓存，文件写入时间超出过期时间将重新下载
    [SqlFunction]
    public static long DownloadFileCache(string url, string headers, string postParam, string savePath, TimeSpan dueTime)
    {
        FileInfo file = new FileInfo(savePath);
        if(file.Exists && (DateTime.Now - file.LastWriteTime) < dueTime)
            return file.Length;
        else
            return DownloadFile(url, headers, postParam, savePath);
    }


    //下载文本
    [SqlFunction]
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
            return webClient.DownloadString(url);
        else
            return webClient.UploadString(url, postParam);
    }
    //下载文本并缓存，文件写入时间超出过期时间将重新下载
    [SqlFunction]
    public static string DownloadTextCache(string url, string headers, string postParam, string encoding, string savePath, TimeSpan dueTime)
    {
        FileInfo file = new FileInfo(savePath);
        if(file.Exists && (DateTime.Now - file.LastWriteTime) < dueTime)
        {
            return File.ReadAllText(savePath);
        }
        else
        {
            string content = DownloadText(url, headers, postParam, encoding);
            if(!file.Directory.Exists)
                file.Directory.Create();
            File.WriteAllText(savePath, content);
            return content;
        }
    }
}
