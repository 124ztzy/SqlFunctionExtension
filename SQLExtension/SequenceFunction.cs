using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;


//序列函数
public partial class Function
{
    //数字序列
    [SqlFunction(TableDefinition = "rowNumber int, cellValue decimal(12,6)", FillRowMethodName = "FillSequenceRow", IsPrecise = true)]
    public static IEnumerable Sequence([SqlFacet(Scale = 6)] decimal start, [SqlFacet(Scale = 6)] decimal end, [SqlFacet(Scale = 6)] decimal step)
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
    public static void FillSequenceRow(object row, out int rowNumber, out decimal cellValue)
    {
        object[] cells = (object[])row;
        rowNumber = (int)cells[0];
        cellValue = (decimal)cells[1];
    }
    //日期序列
    [SqlFunction(TableDefinition = "rowNumber int, cellValue dateTime", FillRowMethodName = "FillSequenceDateRow")]
    public static IEnumerable SequenceDate(DateTime start, DateTime end, [SqlFacet(Scale = 6)] decimal step)
    {
        LinkedList<object[]> result = new LinkedList<object[]>();
        if(step > 0)
        {
            int number = 1;
            for(DateTime value = start; value <= end; value = value.AddDays((double)step))
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
    //时间序列
    [SqlFunction(TableDefinition = "rowNumber int, cellValue dateTime", FillRowMethodName = "FillSequenceDateTimeRow")]
    public static IEnumerable SequenceDateTime(DateTime start, DateTime end, TimeSpan step)
    {
        LinkedList<object[]> result = new LinkedList<object[]>();
        if(step > TimeSpan.Zero)
        {
            int number = 1;
            for(DateTime value = start; value <= end; value += step)
            {
                result.AddLast(new object[] { number, value });
                number++;
            }
        }
        return result;
    }
    public static void FillSequenceDateTimeRow(object row, out int rowNumber, out DateTime cellValue)
    {
        object[] cells = (object[])row;
        rowNumber = (int)cells[0];
        cellValue = (DateTime)cells[1];
    }
}
