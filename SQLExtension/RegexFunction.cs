using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


//正则表达式函数
public partial class Function
{
    //正则表达式判断
    [SqlFunction]
    public static bool RegexIsMatch(string text, string regex, int option)
    {
        Regex reg = new Regex(regex, (RegexOptions)option);
        return reg.IsMatch(text);
    }
    //正则表达式替换
    [SqlFunction]
    public static string RegexReplace(string text, string regex, string final, int option)
    {
        return new Regex(regex, (RegexOptions)option).Replace(text, final);
    }


    //正则表达式拆分表格
    [SqlFunction(TableDefinition = "rowNumber int, columnNumber int, cellValue nvarchar(max)", FillRowMethodName = "FillRegexSplitRow")]
    public static IEnumerable RegexSplit(string text, string rowSplit, string columnSplit, int option)
    {
        List<object[]> list = new List<object[]>(8192);
        Regex rowReg = new Regex(rowSplit, (RegexOptions)option);
        Regex columnReg = new Regex(columnSplit, (RegexOptions)option);
        int r = 0;
        foreach(string row in rowReg.Split(text))
        {
            int c = 0;
            foreach(string column in columnReg.Split(row))
            {
                list.Add(new object[] { r, c, column });
                c++;
            }
            r++;
        }
        return list;
    }
    public static void FillRegexSplitRow(object row, out int rowNumber, out int columnNumber, out string cellValue)
    {
        object[] cells = (object[])row;
        rowNumber = (int)cells[0];
        columnNumber = (int)cells[1];
        cellValue = (string)cells[2];
    }
    //正则表达式匹配
    [SqlFunction(TableDefinition = "matchNumber int, groupName nvarchar(max), captureNumber int, cellValue nvarchar(max)", FillRowMethodName = "FillRegexMatchRow")]
    public static IEnumerable RegexMatch(string text, string regex, int option)
    {
        List<object[]> list = new List<object[]>(8192);
        Regex reg = new Regex(regex, (RegexOptions)option);
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
                    int c = 0;
                    foreach(Capture capture in group.Captures)
                    {
                        list.Add(new object[] { m, groupNames[g], c, capture.Value });
                        c++;
                    }
                }
                g++;
            }
            match = match.NextMatch();
            m++;
        }
        return list;
    }
    public static void FillRegexMatchRow(object row, out int matchNumber, out string groupName, out int captureNumber, out string cellValue)
    {
        object[] cells = (object[])row;
        matchNumber = (int)cells[0];
        groupName = (string)cells[1];
        captureNumber = (int)cells[2];
        cellValue = (string)cells[3];
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
