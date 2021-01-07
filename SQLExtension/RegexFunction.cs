using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


//正则表达式函数
public partial class Function
{
    //正则表达式判断
    [SqlFunction(IsDeterministic = true)]
    public static bool RegexIsMatch(string text, string regex)
    {
        Regex reg = new Regex(regex);
        return reg.IsMatch(text);
    }
    //正则表达式替换
    [SqlFunction(IsDeterministic = true)]
    public static string RegexReplace(string text, string regex, string final)
    {
        Regex reg = new Regex(regex);
        return reg.Replace(text, final);
    }


    //正则表达式拆分表格
    [SqlFunction(IsDeterministic = true, TableDefinition = CellTableDefinition, FillRowMethodName = CellTableFillRowMethod)]
    public static IEnumerable RegexSplit(string text, string rowSplit, string columnSplit)
    {
        LinkedList<object[]> result = new LinkedList<object[]>();
        Regex rowReg = new Regex(rowSplit);
        Regex columnReg = new Regex(columnSplit);
        int r = 0;
        foreach(string row in rowReg.Split(text))
        {
            int c = 0;
            foreach(string column in columnReg.Split(row))
            {
                result.AddLast(new object[] { null, r, c, GetText(column) });
                c++;
            }
            r++;
        }
        return result;
    }
    //正则表达式匹配
    [SqlFunction(IsDeterministic = true, TableDefinition = CellTableDefinition, FillRowMethodName = CellTableFillRowMethod)]
    public static IEnumerable RegexMatch(string text, string regex)
    {
        LinkedList<object[]> result = new LinkedList<object[]>();
        Regex reg = new Regex(regex);
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
                    result.AddLast(new object[] { null, m, groupNames[g], GetText(group.Value) });
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

}
