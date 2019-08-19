using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using System;


//全局变量函数
public partial class Function
{
    //全局变量
    private static Dictionary<string, object> _variable = new Dictionary<string, object>();


    //取值全局变量
    [SqlFunction]
    public static object Variable(string name)
    {
        if(_variable.TryGetValue(name, out object value))
        {
            return value;
        }
        else
        {
            return null;
        }
    }
    //返回文本类型
    [SqlFunction]
    public static string VariableVarchar(string name)
    {
        if(_variable.TryGetValue(name, out object value))
        {
            return value.ToString();
        }
        else
        {
            return null;
        }
    }
    //返回整数类型
    [SqlFunction]
    public static long? VariableBigint(string name)
    {
        if(_variable.TryGetValue(name, out object value))
        {
            return Convert.ToInt64(ConvertType(value, null));
        }
        else
        {
            return null;
        }
    }
    //返回小数类型
    [SqlFunction]
    [return: SqlFacet(Scale = 6)]
    public static decimal? VariableDecimal(string name)
    {
        if(_variable.TryGetValue(name, out object value))
        {
            return Convert.ToDecimal(ConvertType(value, null));
        }
        else
        {
            return null;
        }
    }


    //赋值定义全局变量
    [SqlFunction]
    public static object VariableAssign(string name, object value)
    {
        if(_variable.ContainsKey(name))
        {
            _variable[name] = value;
            return value;
        }
        else
        {
            _variable.Add(name, value);
            return value;
        }
    }
    //清除全局变量
    [SqlFunction]
    public static bool VariableClear(string name)
    {
        if(string.IsNullOrEmpty(name))
        {
            _variable.Clear();
            return true;
        }
        else
        {
            return _variable.Remove(name);
        }
    }


    //查看全部全局变量
    [SqlFunction(TableDefinition = "variableName nvarchar(max), variableValue nvarchar(max)", FillRowMethodName = "FillVariableViewRow")]
    public static IEnumerable VariableView()
    {
        return _variable;
    }
    public static void FillVariableViewRow(object row, out string variableName, out string variableValue)
    {
        KeyValuePair<string, object> pair = (KeyValuePair<string, object>)row;
        variableName = pair.Key;
        variableValue = pair.Value.ToString();
    }
    
}
