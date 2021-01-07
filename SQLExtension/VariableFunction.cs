using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;


//全局变量函数
public partial class Function
{
    //全局变量
    private static Dictionary<string, object> _variables = new Dictionary<string, object>();


    //取值全局变量
    [SqlFunction(IsDeterministic = true)]
    public static object Variable(string name)
    {
        if(_variables.TryGetValue(name, out object value))
            return value;
        else
            return null;
    }


    //返回文本类型
    [SqlFunction(IsDeterministic = true)]
    public static string VariableText(string name)
    {
        if(_variables.TryGetValue(name, out object value))
            return value.ToString();
        else
            return null;
    }
    //返回小数类型
    [SqlFunction(IsDeterministic = true)]
    [return: SqlFacet(Precision = 22, Scale = 6)]
    public static decimal? VariableDecimal(string name)
    {
        if(_variables.TryGetValue(name, out object value))
            return Convert.ToDecimal(ConvertType(value, null));
        else
            return null;
    }
    //返回时间类型
    [SqlFunction(IsDeterministic = true)]
    public static DateTime? VariableDateTime(string name)
    {
        if(_variables.TryGetValue(name, out object value))
            return Convert.ToDateTime(ConvertType(value, null));
        else
            return null;
    }


    //赋值或定义全局变量
    [SqlFunction(IsDeterministic = true)]
    public static object VariableAssign(string name, object value)
    {
        if(_variables.ContainsKey(name))
            _variables[name] = value;
        else
            _variables.Add(name, value);
        return value;
    }
    //清除全局变量，传null将全部清理
    [SqlFunction(IsDeterministic = true)]
    public static bool VariableClear(string name)
    {
        if(name == null)
        {
            _variables.Clear();
            return true;
        }
        else
        {
            return _variables.Remove(name);
        }
    }


    //查看全局变量
    [SqlFunction(IsDeterministic = true, TableDefinition = "variableName nvarchar(max), variableValue nvarchar(max)", FillRowMethodName = "FillVariablesRow")]
    public static IEnumerable Variables()
    {
        return _variables;
    }
    public static void FillVariablesRow(object row, out string variableName, out string variableValue)
    {
        KeyValuePair<string, object> pair = (KeyValuePair<string, object>)row;
        variableName = pair.Key;
        variableValue = pair.Value.ToString();
    }

}
