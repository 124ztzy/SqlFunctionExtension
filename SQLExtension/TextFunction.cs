using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Text;
using System.Text.RegularExpressions;


//文本函数
public partial class Function
{
    //文本格式化
    [SqlFunction]
    public static object ConvertType(object value0, string className)
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
        if(string.IsNullOrEmpty(className))
            return value1;
        else
            return Convert.ChangeType(value1, Type.GetType(className.StartsWith("System.") ? className : "System." + className));
    }
    [SqlFunction]
    public static string TextFormat(string format, object value0)
    {
        return string.Format(format, ConvertType(value0, null));
    }
    [SqlFunction]
    public static string TextFormat2(string format, object value0, object value1)
    {
        return string.Format(format, ConvertType(value0, null), ConvertType(value1, null));
    }
    [SqlFunction]
    public static string TextFormat3(string format, object value0, object value1, object value2)
    {
        return string.Format(format, ConvertType(value0, null), ConvertType(value1, null), ConvertType(value2, null));
    }



    //正则表达式判断
    [SqlFunction]
    public static bool RegexIsMatch(string text, string regex, int option)
    {
        return new Regex(regex, (RegexOptions)option).IsMatch(text);
    }
    //正则表达式匹配
    [SqlFunction(TableDefinition = "matchNumber int, groupName nvarchar(max), captureNumber int, captureValue nvarchar(max)", FillRowMethodName = "FillRegexMatchRow")]
    public static IEnumerable RegexMatch(string text, string regex, int option)
    {
        ArrayList list = new ArrayList();
        Regex reg = new Regex(regex, (RegexOptions)option);
        string[] groupNames = reg.GetGroupNames();
        Match match = reg.Match(text);
        int matchNumber = 0;
        while(match.Success)
        {
            int groupNumber = 0;
            foreach(Group group in match.Groups)
            {
                if(group.Success)
                {
                    int captureNumber = 0;
                    foreach(Capture capture in group.Captures)
                    {
                        list.Add(new object[] { matchNumber, groupNames[groupNumber], captureNumber, capture.Value });
                        captureNumber++;
                    }
                }
                groupNumber++;
            }
            match = match.NextMatch();
            matchNumber++;
        }
        return list;
    }
    public static void FillRegexMatchRow(object row, out int matchNumber, out string groupName, out int captureNumber, out string captureValue)
    {
        object[] cells = (object[])row;
        matchNumber = (int)cells[0];
        groupName = (string)cells[1];
        captureNumber = (int)cells[2];
        captureValue = (string)cells[3];
    }
    //正则表达式替换
    [SqlFunction]
    public static string RegexReplace(string text, string regex, string final, int option)
    {
        return new Regex(regex, (RegexOptions)option).Replace(text, final);
    }
    //正则表达式拆分
    [SqlFunction(TableDefinition = "rowNumber int, splitValue nvarchar(max)", FillRowMethodName = "FillRegexSplitRow")]
    public static IEnumerable RegexSplit(string text, string rowSplit, int option)
    {
        ArrayList list = new ArrayList();
        Regex rowReg = new Regex(rowSplit, (RegexOptions)option);
        int lineNumber = 0;
        foreach(string item in rowReg.Split(text))
        {
            list.Add(new object[] { lineNumber, item });
            lineNumber++;
        }
        return list;
    }
    public static void FillRegexSplitRow(object row, out int rowNumber, out string splitValue)
    {
        object[] cells = (object[])row;
        rowNumber = (int)cells[0];
        splitValue = (string)cells[1];
    }
    //正则表达式拆分表格
    [SqlFunction(TableDefinition = "rowNumber int, columnNumber int, splitValue nvarchar(max)", FillRowMethodName = "FillRegexSplitTableRow")]
    public static IEnumerable RegexSplitTable(string text, string rowSplit, string columnSplit, int option)
    {
        ArrayList list = new ArrayList();
        Regex rowReg = new Regex(rowSplit, (RegexOptions)option);
        Regex columnReg = new Regex(columnSplit, (RegexOptions)option);
        int rowNumber = 0;
        foreach(string row in rowReg.Split(text))
        {
            int columnNumber = 0;
            foreach(string column in columnReg.Split(row))
            {
                list.Add(new object[] { rowNumber, columnNumber, column });
                columnNumber++;
            }
            rowNumber++;
        }
        return list;
    }
    public static void FillRegexSplitTableRow(object row, out int rowNumber, out int columnNumber, out string splitValue)
    {
        object[] cells = (object[])row;
        rowNumber = (int)cells[0];
        columnNumber = (int)cells[1];
        splitValue = (string)cells[2];
    }


    //Json匹配语句
    [SqlFunction]
    public static string JsonRegex(string fields, bool useGroupName)
    {
        StringBuilder builder = new StringBuilder();
        foreach(string field in fields.Split(',', ' '))
        {
            if(!string.IsNullOrEmpty(field))
            {
                if(builder.Length > 0)
                    builder.Append(",.*?");
                if(useGroupName)
                    builder.Append("\"?" + field + "\"?\\s*:\\s*" + "(?:\"(?<" + field + ">\\S*?)\"|(?<" + field + ">[\\d\\.\\+\\-eE]+|true|false|null))");
                else
                    builder.Append("\"?" + field + "\"?\\s*:\\s*" + "(?:\"(\\S*?)\"|([\\d\\.\\+\\-eE]+|true|false|null))");
            }
        }
        return builder.ToString();
    }

}
