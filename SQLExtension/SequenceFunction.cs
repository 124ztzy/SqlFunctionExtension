using Microsoft.SqlServer.Server;
using System;
using System.Collections;


//序列函数
public partial class Function
{
    //数字序列
    [SqlFunction(TableDefinition = "number int, numberValue decimal(12,6)", FillRowMethodName = "FillSequenceRow", IsPrecise = true)]
    public static IEnumerable Sequence([SqlFacet(Scale = 6)] decimal start, [SqlFacet(Scale = 6)] decimal end, [SqlFacet(Scale = 6)] decimal step)
    {
        ArrayList list = new ArrayList();
        if(step > 0)
        {
            int number = 1;
            for(decimal value = start; value <= end; value += step)
            {
                list.Add(new object[] { number, value });
                number++;
            }
        }
        return list;
    }
    public static void FillSequenceRow(object row, out int number, out decimal numberValue)
    {
        object[] cells = (object[])row;
        number = (int)cells[0];
        numberValue = (decimal)cells[1];
    }
    //日期序列
    [SqlFunction(TableDefinition = "number int, dateValue dateTime", FillRowMethodName = "FillSequenceDateRow")]
    public static IEnumerable SequenceDate(DateTime start, DateTime end, [SqlFacet(Scale = 6)] decimal step)
    {
        ArrayList list = new ArrayList();
        if(step > 0)
        {
            int number = 1;
            for(DateTime value = start; value <= end; value = value.AddDays((double)step))
            {
                list.Add(new object[] { number, value });
                number++;
            }
        }
        return list;
    }
    public static void FillSequenceDateRow(object row, out int number, out DateTime dateValue)
    {
        object[] cells = (object[])row;
        number = (int)cells[0];
        dateValue = (DateTime)cells[1];
    }
    //时间序列
    [SqlFunction(TableDefinition = "number int, timeValue dateTime", FillRowMethodName = "FillSequenceDateTimeRow")]
    public static IEnumerable SequenceDateTime(DateTime start, DateTime end, TimeSpan step)
    {
        ArrayList list = new ArrayList();
        if(step > TimeSpan.Zero)
        {
            int number = 1;
            for(DateTime value = start; value <= end; value += step)
            {
                list.Add(new object[] { number, value });
                number++;
            }
        }
        return list;
    }
    public static void FillSequenceDateTimeRow(object row, out int number, out DateTime timeValue)
    {
        object[] cells = (object[])row;
        number = (int)cells[0];
        timeValue = (DateTime)cells[1];
    }
}
