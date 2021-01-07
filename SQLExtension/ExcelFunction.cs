using ExcelDataReader;
using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using System.IO;


//Excel函数
public partial class Function
{
    //Excel文件函数
    [SqlFunction(IsDeterministic = true, TableDefinition = CellTableDefinition, FillRowMethodName = CellTableFillRowMethod)]
    public static IEnumerable ExcelFile(string path)
    {
        LinkedList<object[]> result = new LinkedList<object[]>();
        using(FileStream file = new FileStream(path, FileMode.Open))
        {
            IExcelDataReader reader = ExcelReaderFactory.CreateReader(file);
            do
            {
                int r = 0;
                while(reader.Read())
                {
                    for(int c = 0; c < reader.FieldCount; c++)
                    {
                        result.AddLast(new object[] { reader.Name, r, c, GetText(reader.GetValue(c)) });
                    }
                    r++;
                }
            } while(reader.NextResult());
            reader.Close();
            file.Close();
        }
        return result;
    }


    //Csv文件函数
    [SqlFunction(IsDeterministic = true, TableDefinition = CellTableDefinition, FillRowMethodName = CellTableFillRowMethod)]
    public static IEnumerable CsvText(string text)
    {
        LinkedList<object[]> result = new LinkedList<object[]>();
        string[] rows = text.Split('\n');
        int r = 0;
        foreach(string row in rows)
        {
            string[] cols = row.Split(',');
            for(int c = 0; c < cols.Length; c++)
            {
                result.AddLast(new object[] { null, r, c, GetText(cols[c]) });
            }
            r++;
        }
        return result;
    }
}
