using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;


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
    [SqlFunction(IsDeterministic = true)]
    public static long FileSize(string path)
    {
        FileInfo file = new FileInfo(path);
        if(file.Exists)
            return file.Length;
        else
            return -1;
    }
    //移动文件，返回文件长度
    [SqlFunction(IsDeterministic = true)]
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
    [SqlFunction(IsDeterministic = true)]
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
    [SqlFunction(IsDeterministic = true)]
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


    //读取文本，默认utf-8编码
    [SqlFunction(IsDeterministic = true)]
    public static string FileRead(string path, string encoding)
    {
        return File.ReadAllText(path, string.IsNullOrEmpty(encoding) ? Encoding.UTF8 : Encoding.GetEncoding(encoding));
    }
    //写入文本，默认utf-8编码
    [SqlFunction(IsDeterministic = true)]
    public static long FileWirte(string path, string content, string encoding)
    {
        FileInfo file = new FileInfo(path);
        if(!file.Directory.Exists)
            file.Directory.Create();
        byte[] data = (string.IsNullOrEmpty(encoding) ? Encoding.UTF8 : Encoding.GetEncoding(encoding)).GetBytes(content);
        File.WriteAllBytes(path, data);
        return data.LongLength;
    }


    //文件列表
    [SqlFunction(IsDeterministic = true, TableDefinition = "fullPath nvarchar(max), fileName nvarchar(max), fileExtension nvarchar(max), fileSize bigint, createTime datetime, lastWirteTime datetime", FillRowMethodName = "FillFileTreeRow")]
    public static IEnumerable FileTree(string path, bool isRecurve)
    {
        return FileTreeRecurve(path, isRecurve, new LinkedList<object[]>());
    }
    public static IEnumerable FileTreeRecurve(string path, bool isRecurve, LinkedList<object[]> list)
    {
        DirectoryInfo directory = new DirectoryInfo(path);
        foreach(DirectoryInfo child in directory.GetDirectories())
        {
            list.AddLast(new object[] { child.FullName, child.Name, null, null, child.CreationTime, child.LastWriteTime });
            if(isRecurve)
                FileTreeRecurve(child.FullName, isRecurve, list);
        }
        foreach(FileInfo child in directory.GetFiles())
        {
            list.AddLast(new object[] { child.FullName, child.Name.Substring(0, child.Name.LastIndexOf('.')), child.Extension, child.Length, child.CreationTime, child.LastWriteTime });
        }
        return list;
    }
    public static void FillFileTreeRow(object row, out string fullPath, out string fileName, out string fileExtension, out long? fileSize, out DateTime createTime, out DateTime lastWirteTime)
    {
        object[] cells = (object[])row;
        fullPath = (string)cells[0];
        fileName = (string)cells[1];
        fileExtension = (string)cells[2];
        fileSize = (long?)cells[3];
        createTime = (DateTime)cells[4];
        lastWirteTime = (DateTime)cells[5];
    }
    
}
