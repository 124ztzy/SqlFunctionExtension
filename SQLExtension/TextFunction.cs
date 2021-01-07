using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlTypes;


//文本函数
public partial class Function
{
    //文本格式化
    [SqlFunction(IsDeterministic = true)]
    public static object ConvertType(object value0, string typeName)
    {
        object value1 = value0;
        Type type = value0.GetType();
        switch(type.Name)
        {
            case "SqlBinary":
                value1 = ((SqlBinary)value0).Value;
                break;
            case "SqlBoolean":
                value1 = ((SqlBoolean)value0).Value;
                break;
            case "SqlByte":
                value1 = ((SqlByte)value0).Value;
                break;
            case "SqlDateTime":
                value1 = ((SqlDateTime)value0).Value;
                break;
            case "SqlDecimal":
                value1 = ((SqlDecimal)value0).Value;
                break;
            case "SqlDouble":
                value1 = ((SqlDouble)value0).Value;
                break;
            case "SqlGuid":
                value1 = ((SqlGuid)value0).Value;
                break;
            case "SqlInt16":
                value1 = ((SqlInt16)value0).Value;
                break;
            case "SqlInt32":
                value1 = ((SqlInt32)value0).Value;
                break;
            case "SqlInt64":
                value1 = ((SqlInt64)value0).Value;
                break;
            case "SqlMoney":
                value1 = ((SqlMoney)value0).Value;
                break;
            case "SqlSingle":
                value1 = ((SqlSingle)value0).Value;
                break;
            case "SqlString":
                value1 = ((SqlString)value0).Value;
                break;
            case "DateTime":
            case "TimeSpan":
                value1 = value0;
                break;
            default:
                return type.FullName;
        }
        if(string.IsNullOrEmpty(typeName))
            return value1;
        else
            return Convert.ChangeType(value1, Type.GetType(typeName.StartsWith("System.") ? typeName : "System." + typeName));
    }
    [SqlFunction(IsDeterministic = true)]
    public static string TextFormat(string format, object value1, object value2, object value3, object value4, object value5)
    {
        return string.Format(format, ConvertType(value1, null), ConvertType(value2, null), ConvertType(value3, null), ConvertType(value4, null), ConvertType(value5, null));
    }


    ////将\u十六进制编码转化汉字
    //[SqlFunction]
    //public static string ConvertUnicode(string text)
    //{
    //    StringBuilder builder = new StringBuilder();
    //    int i = 0;
    //    while(i < text.Length)
    //    {
    //        if(text[i] == '\\' && text[i + 1] == 'u')
    //        {
    //            int word = Convert.ToInt32(text.Substring(i + 2, 4), 16);
    //            builder.Append((char)word);
    //            i += 6;
    //        }
    //        else
    //        {
    //            builder.Append(text[i]);
    //            i++;
    //        }
    //    }
    //    return builder.ToString();
    //}

}
