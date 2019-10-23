using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

//Json函数
public partial class Function
{
    //Json路径提取函数
    [SqlFunction(TableDefinition = "rowNumber int, columnName nvarchar(max), cellValue nvarchar(max)", FillRowMethodName = "FillJsonPathRow")]
    public static IEnumerable JsonPath(string text, string locationPath, string extractPaths)
    {
        List<object[]> result = new List<object[]>(8192);
        JToken root = JToken.Parse(text);
        if(!string.IsNullOrEmpty(locationPath))
            root = root.SelectToken(locationPath);
        foreach(string path in extractPaths.Split(','))
        {
            string jpath = path.Trim();
            int r = 0;
            foreach(JToken token in root.SelectTokens(jpath))
            {
                result.Add(new object[] { r, jpath, token.ToString() });
                r++;
            }
        }
        return result;
    }
    public static void FillJsonPathRow(object row, out int rowNumber, out string columnName, out string cellValue)
    {
        object[] cells = (object[])row;
        rowNumber = (int)cells[0];
        columnName = (string)cells[1];
        cellValue = (string)cells[2];
    }

}
