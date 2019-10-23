using ExcelDataReader;
using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using System.IO;


//Excel函数
public partial class Function
{
    //Excel文件函数
    [SqlFunction(TableDefinition = "sheetName nvarchar(max), rowNumber int, columnName nvarchar(max), cellValue nvarchar(max)", FillRowMethodName = "FillExcelFileRow")]
    public static IEnumerable ExcelFile(string path, bool isFirstHeader)
    {
        List<object[]> result = new List<object[]>(8192);
        FileStream file = new FileStream(path, FileMode.Open);
        IExcelDataReader reader = ExcelReaderFactory.CreateReader(file);
        do
        {
            int r = 0;
            //读取第一行表结构
            List<string> headers = null;
            if(isFirstHeader)
            {
                if(reader.Read())
                {
                    headers = new List<string>(reader.FieldCount);
                    for(int c = 0; c < reader.FieldCount; c++)
                    {
                        headers.Add(reader.GetString(c));
                    }
                    r++;
                }
            }
            //读取正文
            while(reader.Read())
            {
                for(int c = 0; c < reader.FieldCount; c++)
                {
                    result.Add(new object[] { reader.Name, r, headers == null ? c.ToString() : headers[c], reader.GetValue(c) });
                }
                r++;
            }
        } while(reader.NextResult());
        reader.Close();
        file.Close();
        return result;
    }
    public static void FillExcelFileRow(object row, out string sheetName, out int rowNumber, out string columnName, out string cellValue)
    {
        object[] cells = (object[])row;
        sheetName = (string)cells[0];
        rowNumber = (int)cells[1];
        columnName = (string)cells[2];
        cellValue = cells[3].ToString();
    }


    //Csv文件函数
    [SqlFunction(TableDefinition = " rowNumber int, columnName nvarchar(max), cellValue nvarchar(max)", FillRowMethodName = "FillCsvTextRow")]
    public static IEnumerable CsvText(string text, bool isFirstHeader)
    {
        List<object[]> result = new List<object[]>(8192);
        string[] rows = text.Split('\n');
        List<string> headers = null;
        int r = 0;
        foreach(string row in rows)
        {
            string[] cols = row.Split(',');
            //第一行表头
            if(r == 0 && isFirstHeader)
            {
                headers = new List<string>(cols.Length);
                foreach(string col in cols)
                {
                    headers.Add(col.Trim());
                }
            }
            //其余行
            for(int c = 0; c < cols.Length; c++)
            {
                result.Add(new object[] { r, headers == null ? c.ToString() : headers[c], cols[c].Trim() });
            }
            r++;
        }
        return result;
    }
    public static void FillCsvTextRow(object row, out int rowNumber, out string columnName, out string cellValue)
    {
        object[] cells = (object[])row;
        rowNumber = (int)cells[0];
        columnName = (string)cells[1];
        cellValue = (string)cells[2];
    }
}
