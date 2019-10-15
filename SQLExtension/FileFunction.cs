using Microsoft.SqlServer.Server;
using System.IO;
using System.Net;
using System.Text;
using System;
using System.Collections;
using System.Diagnostics;


//文件函数
public static partial class Function
{
    ////连接共享目录
    //[SqlFunction]
    //public static string ConnectSharedDirectory(string path, string user, string password)
    //{
    //    string errormsg = null;
    //    Process proc = new Process();
    //    try
    //    {
    //        proc.StartInfo.FileName = "cmd.exe";
    //        proc.StartInfo.UseShellExecute = false;
    //        proc.StartInfo.RedirectStandardInput = true;
    //        proc.StartInfo.RedirectStandardOutput = true;
    //        proc.StartInfo.RedirectStandardError = true;
    //        proc.StartInfo.CreateNoWindow = true;
    //        proc.Start();
    //        proc.StandardInput.WriteLine("net use " + path + " /del");
    //        proc.StandardInput.WriteLine("net use " + path + " " + password + " /user:" + user);
    //        proc.StandardInput.WriteLine("exit");
    //        while(!proc.HasExited)
    //        {
    //            proc.WaitForExit(1000);
    //        }
    //        errormsg = proc.StandardError.ReadToEnd();
    //        proc.StandardError.Close();
    //    }
    //    catch(Exception ex)
    //    {
    //        throw ex;
    //    }
    //    finally
    //    {
    //        proc.Close();
    //        proc.Dispose();
    //    }
    //    return errormsg;
    //}


    //文件长度，文件不存在返回-1
    [SqlFunction]
    public static bool FileExist(string path)
    {
        return File.Exists(path);
    }
    //文件长度，文件不存在返回-1
    [SqlFunction]
    public static long FileSize(string path)
    {
        FileInfo file = new FileInfo(path);
        if(file.Exists)
            return file.Length;
        else
            return -1;
    }
    //移动文件，返回文件长度
    [SqlFunction]
    public static long FileMove(string path1, string path2)
    {
        FileInfo file1 = new FileInfo(path1);
        if(file1.Exists)
        {
            FileInfo file2 = new FileInfo(path2);
            if(!file2.Directory.Exists)
                file2.Directory.Create();
            file1.MoveTo(path2);
            return file1.Length;
        }
        else
        {
            return -1;
        }
    }
    //复制文件，返回文件长度
    [SqlFunction]
    public static long FileCopy(string path1, string path2)
    {
        FileInfo file1 = new FileInfo(path1);
        if(file1.Exists)
        {
            FileInfo file2 = new FileInfo(path2);
            if(!file2.Directory.Exists)
                file2.Directory.Create();
            file1.CopyTo(path2);
            return file1.Length;
        }
        else
        {
            return -1;
        }
    }
    //删除文件，返回文件长度
    [SqlFunction]
    public static long FileDelete(string path)
    {
        FileInfo file = new FileInfo(path);
        if(file.Exists)
        {
            file.Delete();
            return file.Length;
        }
        else
        {
            return -1;
        }
    }


    //文件列表，lastList传空即可
    [SqlFunction(TableDefinition = "fullPath nvarchar(max), fileName nvarchar(max), isDirectory bit, fileSize bigint, lastWirteTime datetime", FillRowMethodName = "FillFileTreeRow")]
    public static IEnumerable FileTree(string path, bool isRecurve, object lastList)
    {
        ArrayList list = lastList == null || lastList == DBNull.Value ? new ArrayList() : (ArrayList)lastList;
        DirectoryInfo directory = new DirectoryInfo(path);
        foreach(DirectoryInfo child in directory.GetDirectories())
        {
            list.Add(new object[] { child.FullName, child.Name, true, 0L, child.LastWriteTime });
            if(isRecurve)
                FileTree(child.FullName, isRecurve, list);
        }
        foreach(FileInfo child in directory.GetFiles())
        {
            list.Add(new object[] { child.FullName, child.Name, false, child.Length, child.LastWriteTime });
        }
        return list;
    }
    public static void FillFileTreeRow(object row, out string fullPath, out string fileName, out bool isDirectory, out long fileSize, out DateTime lastWirteTime)
    {
        object[] cells = (object[])row;
        fullPath = (string)cells[0];
        fileName = (string)cells[1];
        isDirectory = (bool)cells[2];
        fileSize = (long)cells[3];
        lastWirteTime = (DateTime)cells[4];
    }


    //读取文本，默认utf-8编码
    [SqlFunction]
    public static string FileRead(string path, string encoding)
    {
        return File.ReadAllText(path, string.IsNullOrEmpty(encoding) ? Encoding.UTF8 : Encoding.GetEncoding(encoding));
    }
    //写入文本，默认utf-8编码
    [SqlFunction]
    public static long FileWirte(string path, string content, string encoding)
    {
        FileInfo file = new FileInfo(path);
        if(!file.Directory.Exists)
            file.Directory.Create();
        byte[] data = string.IsNullOrEmpty(encoding) ? Encoding.UTF8.GetBytes(content) : Encoding.GetEncoding(encoding).GetBytes(content);
        File.WriteAllBytes(path, data);
        return data.LongLength;
    }


    //下载文件
    [SqlFunction]
    public static long DownloadFile(string url, string referer, string postParam, string savePath)
    {
        FileInfo file = new FileInfo(savePath);
        if(!file.Directory.Exists)
            file.Directory.Create();
        WebClient webClient = new WebClient();
        webClient.Headers["Referer"] = referer;
        byte[] data = string.IsNullOrEmpty(postParam) ? webClient.DownloadData(url) : webClient.UploadData(url, Encoding.UTF8.GetBytes(postParam));
        File.WriteAllBytes(savePath, data);
        return data.LongLength;
    }
    //下载文件并缓存，文件写入时间超出过期时间将重新下载
    [SqlFunction]
    public static long DownloadFileCache(string url, string referer, string postParam, string savePath, TimeSpan dueTime)
    {
        FileInfo file = new FileInfo(savePath);
        if(file.Exists && (DateTime.Now - file.LastWriteTime) < dueTime)
            return file.Length;
        else
            return DownloadFile(url, referer, postParam, savePath);
    }
    //下载文本
    [SqlFunction]
    public static string DownloadText(string url, string referer, string postParam, string encoding)
    {
        WebClient webClient = new WebClient();
        webClient.Headers["Referer"] = referer;
        webClient.Encoding = string.IsNullOrEmpty(encoding) ? Encoding.UTF8 : Encoding.GetEncoding(encoding);
        if(string.IsNullOrEmpty(postParam))
            return webClient.DownloadString(url);
        else
            return webClient.UploadString(url, postParam);
    }
    //下载文本并缓存，文件写入时间超出过期时间将重新下载
    [SqlFunction]
    public static string DownloadTextCache(string url, string referer, string postParam, string encoding, string savePath, TimeSpan dueTime)
    {
        FileInfo file = new FileInfo(savePath);
        if(file.Exists && (DateTime.Now - file.LastWriteTime) < dueTime)
        {
            return File.ReadAllText(savePath);
        }
        else
        {
            string content = DownloadText(url, referer, postParam, encoding);
            if(!file.Directory.Exists)
                file.Directory.Create();
            File.WriteAllText(savePath, content);
            return content;
        }
    }
}
