using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


//正则表达式函数
public partial class Function
{
    //正则表达式判断
    [SqlFunction]
    public static bool RegexIsMatch(string text, string regex, int? option)
    {
        Regex reg = new Regex(regex, option == null ? RegexOptions.None : (RegexOptions)option);
        return reg.IsMatch(text);
    }
    //正则表达式替换
    [SqlFunction]
    public static string RegexReplace(string text, string regex, string final, int? option)
    {
        return new Regex(regex, option == null ? RegexOptions.None : (RegexOptions)option).Replace(text, final);
    }


    //正则表达式拆分表格
    [SqlFunction(TableDefinition = CellTableDefinition, FillRowMethodName = CellTableFillRowMethod)]
    public static IEnumerable RegexSplit(string text, string rowSplit, string columnSplit, int? option)
    {
        LinkedList<object[]> result = new LinkedList<object[]>();
        Regex rowReg = new Regex(rowSplit, option == null ? RegexOptions.None : (RegexOptions)option);
        Regex columnReg = new Regex(columnSplit, option == null ? RegexOptions.None : (RegexOptions)option);
        int r = 0;
        foreach(string row in rowReg.Split(text))
        {
            int c = 0;
            foreach(string column in columnReg.Split(row))
            {
                result.AddLast(new object[] { null, r, c, GetString(column) });
                c++;
            }
            r++;
        }
        return result;
    }
    //正则表达式匹配
    [SqlFunction(TableDefinition = CellTableDefinition, FillRowMethodName = CellTableFillRowMethod)]
    public static IEnumerable RegexMatch(string text, string regex, int? option)
    {
        LinkedList<object[]> result = new LinkedList<object[]>();
        Regex reg = new Regex(regex, option == null ? RegexOptions.None : (RegexOptions)option);
        Match match = reg.Match(text);
        string[] groupNames = reg.GetGroupNames();
        int m = 0;
        while(match.Success)
        {
            int g = 0;
            foreach(Group group in match.Groups)
            {
                if(group.Success)
                {
                    result.AddLast(new object[] { null, m, groupNames[g], GetString(group.Value) });
                    //int c = 0;
                    //foreach(Capture capture in group.Captures)
                    //{
                    //    result.Add(new object[] { m, groupNames[g], c, GetString(capture.Value) });
                    //    c++;
                    //}
                }
                g++;
            }
            match = match.NextMatch();
            m++;
        }
        return result;
    }
    

    ////Json匹配语句
    //[SqlFunction]
    //public static string JsonRegex(string fields, bool useGroupName)
    //{
    //    StringBuilder builder = new StringBuilder();
    //    foreach(string field in fields.Split(',', ' '))
    //    {
    //        if(!string.IsNullOrEmpty(field))
    //        {
    //            if(builder.Length > 0)
    //                builder.Append(",.*?");
    //            if(useGroupName)
    //                builder.Append("\"" + field + "\"\\s*:\\s*" + "(?:\"(?<" + field + ">[^\"]*?)\"|(?<" + field + ">[\\d\\.\\+\\-eE]+|true|false|null))");
    //            else
    //                builder.Append("\"" + field + "\"\\s*:\\s*" + "(?:\"([^\"]*?)\"|([\\d\\.\\+\\-eE]+|true|false|null))");
    //        }
    //    }
    //    return builder.ToString();
    //}

}
