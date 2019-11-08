using System;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using System.Globalization;


//填充单元格函数
public partial class Function
{
    //单元格表定义语句
    public const string CellTableDefinition = "tableName nvarchar(max), rowName nvarchar(max), columnName nvarchar(max), cellValue nvarchar(max)";
    //单元格表填充函数
    public const string CellTableFillRowMethod = "FillCellRow";
    //单元格表填充函数
    public static void FillCellRow(object row, out string tableName, out string rowName, out string columnName, out string cellValue)
    {
        object[] cells = (object[])row;
        tableName = cells[0]?.ToString();
        rowName = cells[1].ToString();
        columnName = cells[2]?.ToString();
        cellValue = cells[3]?.ToString();
    }


    //表第几行作为列名
    public static void RowAsHeader(LinkedList<object[]> list, string rowName)
    {
        List<string> headers = new List<string>();
        LinkedListNode<object[]> node = list.First;
        while(node != null)
        {
            object[] cells = node.Value;
            if(rowName.Equals(cells[1].ToString()))
            {
                headers.Add(cells[3]?.ToString());
                LinkedListNode<object[]> deleteNode = node;
                node = node.Next;
                //一旦删除node.next就为空
                list.Remove(deleteNode);
            }
            else
            {
                int index = Convert.ToInt32(cells[2]);
                if(index < headers.Count)
                    cells[2] = headers[index];
                node = node.Next;
            }
        }
    }
    //转置，行转列
    public static void RowColumnTranspose(LinkedList<object[]> list)
    {
        foreach(object[] cells in list)
        {
            object rowName = cells[1];
            cells[1] = cells[2];
            cells[2] = rowName;
        }
    }


    //获取规范字符串
    [SqlFunction]
    public static string GetString(object value)
    {
        if(value == null)
        {
            return null;
        }
        else
        {
            //将(char)160空格替换成正常空格
            string text = value.ToString().Replace(' ', ' ').Trim();
            if(text == "" || text == "-" || text == "-%")
            {
                return null;
            }
            else
            {
                return text;
            }
        }
    }
    //获取小数，支持汉字
    [SqlFunction]
    [return: SqlFacet(Precision = 22, Scale = 6)]
    public static decimal? GetNumber(string text)
    {
        if(text == null)
        {
            return null;
        }
        else
        {
            text = text.Trim();
            if(text == "" || text == "-" || text == "-%")
            {
                return null;
            }
            else
            {
                text = text.Replace(",", null);
                decimal multiply = 1;
                if(text.EndsWith("%"))
                {
                    multiply = 0.01M;
                    text = text.Substring(0, text.Length - 1).Trim();
                }
                if(decimal.TryParse(text, NumberStyles.Float, null, out decimal value))
                {
                    return value * multiply;
                }
                else
                {
                    //当前位值
                    int i = 0;
                    //之前值
                    int n = 0;
                    foreach(char c in text)
                    {
                        switch(c)
                        {
                            case '零':
                            case '〇':
                                i = 0;
                                break;
                            case '一':
                                i = 1;
                                break;
                            case '二':
                            case '两':
                                i = 2;
                                break;
                            case '三':
                                i = 3;
                                break;
                            case '四':
                                i = 4;
                                break;
                            case '五':
                                i = 5;
                                break;
                            case '六':
                                i = 6;
                                break;
                            case '七':
                                i = 7;
                                break;
                            case '八':
                                i = 8;
                                break;
                            case '九':
                                i = 9;
                                break;
                            case '十':
                                if(i == 0 && n == 0)
                                    n = 10;
                                else
                                    n = n + i * 10;
                                i = 0;
                                break;
                            case '百':
                                n = n + i * 100;
                                i = 0;
                                break;
                            case '千':
                                n = n + i * 1000;
                                i = 0;
                                break;
                            case '万':
                                if(i == 0)
                                    n = n * 10000;
                                else
                                    n = n + i * 10000;
                                i = 0;
                                break;
                            default:
                                throw new Exception("无法解析的数字“" + c + "”在“" + text + "”");
                        }
                    }
                    return n + i;
                }
            }
        }
    }
    //获取日期
    [SqlFunction]
    public static DateTime? GetDateTime(string text)
    {
        if(text == null)
        {
            return null;
        }
        else
        {
            text = text.Trim().TrimStart('-', '/', '.');
            if(text == "")
            {
                return null;
            }
            else
            {
                if(int.TryParse(text, out int number) && text.Length == 8)
                {
                    return DateTime.ParseExact(text, "yyyyMMdd", null);
                }
                else
                {
                    if(DateTime.TryParse(text, out DateTime value))
                        return value;
                    else
                        throw new Exception("无法解析的时间“" + text + "”");
                }
            }
        }
    }

}