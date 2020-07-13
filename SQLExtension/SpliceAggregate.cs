using Microsoft.SqlServer.Server;
using System;
using System.IO;
using System.Text;


//文本聚合函数
[Serializable]
[SqlUserDefinedAggregate(Format.UserDefined, MaxByteSize = 8000)]
public struct Splice : IBinarySerialize
{
    //初始化
    public void Init()
    {
        _str = new StringBuilder();
    }
    //聚合
    public void Accumulate(string value, string spacing)
    {
        if(_str.Length > 0)
            _str.Append(spacing);
        _str.Append(value);
    }
    //多组合并
    public void Merge(Splice group)
    {
        _str.Append(group._str);
    }
    //输出
    public string Terminate()
    {
        return _str.ToString();
    }


    //序列化
    public void Read(BinaryReader reader)
    {
        _str = new StringBuilder(reader.ReadString());
    }
    public void Write(BinaryWriter writer)
    {
        writer.Write(_str.ToString());
    }


    //文本聚合
    private StringBuilder _str;
}


