using HtmlAgilityPack;
using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;

//Html函数
public partial class Function
{
    //Html路径提取函数
    [SqlFunction(TableDefinition = "rowNumber int, columnName nvarchar(max), cellValue nvarchar(max)", FillRowMethodName = "FillHtmlPathRow")]
    public static IEnumerable HtmlPath(string text, string locationPath, string extractPaths)
    {
        List<object[]> result = new List<object[]>(8192);
        HtmlDocument document = new HtmlDocument();
        document.LoadHtml(text);
        HtmlNode root = document.DocumentNode;
        if(!string.IsNullOrEmpty(locationPath))
            root = root.SelectSingleNode(locationPath);
        foreach(string path in extractPaths.Split(','))
        {
            string xpath = path.Trim();
            HtmlNodeCollection nodes = root.SelectNodes(xpath);
            int r = 0;
            foreach(HtmlNode node in nodes)
            {
                result.Add(new object[] { r, xpath, node.InnerHtml.Trim() });
                r++;
            }
        }
        return result;
    }
    public static void FillHtmlPathRow(object row, out int rowNumber, out string columnName, out string cellValue)
    {
        object[] cells = (object[])row;
        rowNumber = (int)cells[0];
        columnName = (string)cells[1];
        cellValue = (string)cells[2];
    }

}
