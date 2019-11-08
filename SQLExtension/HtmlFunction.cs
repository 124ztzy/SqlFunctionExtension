using HtmlAgilityPack;
using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

//Html函数
public partial class Function
{
    //Html解析
    public static HtmlDocument HtmlParse(string text)
    {
        //text传空，加载上次保存的解析对象
        HtmlDocument document = null;
        if(string.IsNullOrEmpty(text))
        {
            document = (HtmlDocument)Variable("系统解析缓存");
            if(document == null)
                throw new Exception("无法解析空Html字符串，无法加载系统解析缓存");
        }
        else
        {
            document = new HtmlDocument();
            document.LoadHtml(text);
            VariableAssign("系统解析缓存", document);
        }
        return document;
    }


    //Html路径提取函数
    [SqlFunction(TableDefinition = CellTableDefinition, FillRowMethodName = CellTableFillRowMethod)]
    public static IEnumerable HtmlPath(string text, string rowPath, string columnsPath)
    {
        LinkedList<object[]> result = new LinkedList<object[]>();
        HtmlDocument document = HtmlParse(text);
        //定位分组节点
        IEnumerable<HtmlNode> rows = null;
        if(string.IsNullOrEmpty(rowPath))
            rows = new List<HtmlNode>(1) { document.DocumentNode };
        else
            rows = document.DocumentNode.SelectNodes(rowPath);
        //提取子节点
        if(rows != null)
        {
            if(string.IsNullOrEmpty(columnsPath))
            {
                int r = 0;
                foreach(HtmlNode row in rows)
                {
                    result.AddLast(new object[] { null, r, null, GetString(row.InnerText) });
                    r++;
                }
            }
            else
            {
                foreach(string path in columnsPath.Split(','))
                {
                    string xpath = path.Trim();
                    int r = 0;
                    foreach(HtmlNode row in rows)
                    {
                        HtmlNodeCollection nodes = row.SelectNodes(xpath);
                        if(nodes != null)
                        {
                            int c = 0;
                            foreach(HtmlNode node in nodes)
                            {
                                result.AddLast(new object[] { null, r, xpath, GetString(node.InnerText) });
                                c++;
                            }
                        }
                        r++;
                    }
                }
            }
        }
        return result;
    }
    //Html table标签提取函数
    [SqlFunction(TableDefinition = CellTableDefinition, FillRowMethodName = CellTableFillRowMethod)]
    public static IEnumerable HtmlTable(string text, string tablesPath, bool? isTranspose, string rowHeader)
    {
        LinkedList<object[]> result = new LinkedList<object[]>();
        HtmlDocument document = HtmlParse(text);
        //定位表节点
        HtmlNodeCollection tables = document.DocumentNode.SelectNodes(tablesPath);
        if(tables != null)
        {
            int t = 0;
            foreach(HtmlNode table in tables)
            {
                HtmlNodeCollection trs = table.SelectNodes(".//tr");
                if(trs != null)
                {
                    int r = 0;
                    foreach(HtmlNode tr in trs)
                    {
                        HtmlNodeCollection tds = tr.SelectNodes("td");
                        if(tds == null)
                            tds = tr.SelectNodes("th");
                        if(tds != null)
                        {
                            int c = 0;
                            foreach(HtmlNode td in tds)
                            {
                                result.AddLast(new object[] { t, r, c, GetString(td.InnerText) });
                                //解析跨咧
                                HtmlAttribute colspan = td.Attributes["colspan"];
                                if(colspan != null)
                                {
                                    int span = int.Parse(colspan.Value, NumberStyles.Float);
                                    for(int i = 1; i < span; i++)
                                    {
                                        c++;
                                        result.AddLast(new object[] { t, r, c, GetString(td.InnerText) });
                                    }
                                }
                                //解析跨行
                                HtmlAttribute rowspan = td.Attributes["rowspan"];
                                if(rowspan != null)
                                {
                                    int span = int.Parse(rowspan.Value, NumberStyles.Float);
                                    for(int i = 1; i < span; i++)
                                    {
                                        //result.Add(new object[] { "调试", trs.Count, c.ToString(), GetString(td.InnerText) });
                                        HtmlNode newtd = HtmlNode.CreateNode(td.OuterHtml);
                                        newtd.Attributes.Remove("rowspan");
                                        HtmlNode nexttr = trs[r + i];
                                        HtmlNode position = nexttr.SelectSingleNode("td[" + (c + 1) + "]");
                                        if(position == null)
                                            nexttr.AppendChild(newtd);
                                        else
                                            nexttr.InsertBefore(newtd, position);
                                    }
                                    td.Attributes.Remove("rowspan");
                                }
                                c++;
                            }
                        }
                        r++;
                    }
                }
                t++;
            }
        }
        //执行行列变换和首行作为列名
        if(isTranspose != null && isTranspose.Value)
            RowColumnTranspose(result);
        if(!string.IsNullOrEmpty(rowHeader))
            RowAsHeader(result, rowHeader);
        return result;
    }

}
