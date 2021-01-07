using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

//Json函数
public partial class Function
{
    //Json路径提取函数
    [SqlFunction(IsDeterministic = true, TableDefinition = CellTableDefinition, FillRowMethodName = CellTableFillRowMethod)]
    public static IEnumerable JsonPath(string text, string rowPath, string columnPaths)
    {
        LinkedList<object[]> result = new LinkedList<object[]>();
        JToken json = JToken.Parse(text);
        //定位分组节点
        IEnumerable<JToken> rows = null;
        if(string.IsNullOrEmpty(rowPath))
        {
            rows = new List<JToken>(1) { json };
        }
        else
        {
            rows = json.SelectTokens(rowPath);
        }
        //提取子节点
        if(string.IsNullOrEmpty(columnPaths))
        {
            int r = 0;
            foreach(JToken row in rows)
            {
                result.AddLast(new object[] { null, r, null, GetText(row) });
                r++;
            }
        }
        else
        {
            foreach(string path in columnPaths.Split(','))
            {
                string jpath = path.Trim();
                int r = 0;
                foreach(JToken row in rows)
                {
                    int c = 0;
                    foreach(JToken token in row.SelectTokens(jpath))
                    {
                        result.AddLast(new object[] { null, r, jpath, GetText(token) });
                        c++;
                    }
                    r++;
                }   
            }
        }
        return result;
    }

}
