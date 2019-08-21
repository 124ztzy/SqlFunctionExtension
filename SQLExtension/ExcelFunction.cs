using ExcelDataReader;
using Microsoft.SqlServer.Server;
using System.Collections;
using System.IO;

//Excel函数
public partial class Function
{
    //Excel读取表函数
    [SqlFunction(TableDefinition = "sheetName nvarchar(max), rowNumber int, columnNumber int, cellValue nvarchar(max)", FillRowMethodName = "FillExcelReadRow")]
    public static IEnumerable ExcelRead(string path)
    {
        ArrayList list = new ArrayList();
        FileStream file = new FileStream(path, FileMode.Open);
        IExcelDataReader reader = ExcelReaderFactory.CreateReader(file);
        do
        {
            int rowNumber = 0;
            while(reader.Read())
            {
                for(int columnNumber = 0; columnNumber < reader.FieldCount; columnNumber++)
                {
                    list.Add(new object[] { reader.Name, rowNumber, columnNumber, reader.GetValue(columnNumber) });
                }
                rowNumber++;
            }
        } while(reader.NextResult());
        reader.Close();
        file.Close();
        return list;
    }
    public static void FillExcelReadRow(object row, out string sheetName, out int rowNumber, out int columnNumber, out string cellValue)
    {
        object[] cells = (object[])row;
        sheetName = (string)cells[0];
        rowNumber = (int)cells[1];
        columnNumber = (int)cells[2];
        cellValue = (string)cells[3];
    }
}
