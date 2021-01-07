using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;


//序列函数
public partial class Function
{
    //数字序列
    [SqlFunction(IsDeterministic = true, TableDefinition = "rowNumber int, cellValue decimal(22,6)", FillRowMethodName = "FillSequenceDecimalRow", IsPrecise = true)]
    public static IEnumerable SequenceDecimal([SqlFacet(Scale = 6)] decimal start, [SqlFacet(Scale = 6)] decimal end, [SqlFacet(Scale = 6)] decimal step)
    {
        LinkedList<object[]> result = new LinkedList<object[]>();
        if(step > 0)
        {
            int number = 1;
            for(decimal value = start; value <= end; value += step)
            {
                result.AddLast(new object[] { number, value });
                number++;
            }
        }
        return result;
    }
    public static void FillSequenceDecimalRow(object row, out int rowNumber, out decimal cellValue)
    {
        object[] cells = (object[])row;
        rowNumber = (int)cells[0];
        cellValue = (decimal)cells[1];
    }
    //日期序列
    [SqlFunction(IsDeterministic = true, TableDefinition = "rowNumber int, cellValue datetime", FillRowMethodName = "FillSequenceDateRow")]
    public static IEnumerable SequenceDateTime(DateTime start, DateTime end, long ms)
    {
        LinkedList<object[]> result = new LinkedList<object[]>();
        if(ms > 0)
        {
            int number = 1;
            for(DateTime value = start; value <= end; value = value.AddMilliseconds(ms))
            {
                result.AddLast(new object[] { number, value });
                number++;
            }
        }
        return result;
    }
    public static void FillSequenceDateRow(object row, out int rowNumber, out DateTime cellValue)
    {
        object[] cells = (object[])row;
        rowNumber = (int)cells[0];
        cellValue = (DateTime)cells[1];
    }
    
}
