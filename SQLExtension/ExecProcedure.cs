using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;


//批量执行存储过程
public partial class Procedure
{
    //循环调用执行反射函数
    [SqlProcedure]
    public static void ExecuteReflection(string className, string constructorParamterSql, string methodName, string methodParamterSql, bool isSetResult)
    {
        //查找类
        Type type = Type.GetType(className.StartsWith("System.") ? className : "System." + className);
        if(type == null)
        {
            throw new Exception("找不到类 " + className);
        }
        else
        {
            SqlContext.Pipe.Send("类 " + type.FullName);
            SqlConnection connection = new SqlConnection("context connection=true");
            connection.Open();
            //查找执行函数
            MethodInfo method = null;
            SqlDataReader methodReader = null;
            Type resultType = null;
            if(string.IsNullOrEmpty(methodName))
            {
                resultType = type;
                SqlContext.Pipe.Send("无执行函数");
            }
            else
            {
                Type[] paramTypes = null;
                if(string.IsNullOrEmpty(methodParamterSql))
                {
                    paramTypes = new Type[0];
                    SqlContext.Pipe.Send("执行函数参数 " + "无");
                }
                else
                {
                    methodReader = new SqlCommand(methodParamterSql, connection).ExecuteReader();
                    if(methodReader.Read())
                    {
                        paramTypes = new Type[methodReader.FieldCount];
                        for(int i = 0; i < methodReader.FieldCount; i++)
                        {
                            paramTypes[i] = methodReader.GetFieldType(i);
                            SqlContext.Pipe.Send("执行函数参数 " + paramTypes[i].FullName);
                        }
                    }
                }
                method = type.GetMethod(methodName, paramTypes);
                if(method == null)
                {
                    StringBuilder builder = new StringBuilder();
                    foreach(Type paramType in paramTypes)
                    {
                        if(builder.Length > 0)
                            builder.Append(", ");
                        builder.Append(paramType.FullName);
                    }
                    throw new Exception("找不到执行函数" + methodName + "(" + builder + ")");
                }
                else
                {
                    resultType = method.ReturnType;
                    SqlContext.Pipe.Send("函数 " + method.Name);
                }
            }
            //查找构造函数
            ConstructorInfo ctor = null;
            SqlDataReader ctorReader = null;
            if(method == null || !method.IsStatic)
            {
                Type[] paramTypes = null;
                if(string.IsNullOrEmpty(constructorParamterSql))
                {
                    paramTypes = new Type[0];
                    SqlContext.Pipe.Send("构造函数参数 " + "无");
                }
                else
                {
                    ctorReader = new SqlCommand(constructorParamterSql, connection).ExecuteReader();
                    if(ctorReader.Read())
                    {
                        paramTypes = new Type[ctorReader.FieldCount];
                        for(int i = 0; i < ctorReader.FieldCount; i++)
                        {
                            paramTypes[i] = ctorReader.GetFieldType(i);
                            SqlContext.Pipe.Send("构造函数参数 " + paramTypes[i].FullName);
                        }
                    }
                }
                ctor = type.GetConstructor(paramTypes);
                if(ctor == null)
                {
                    StringBuilder builder = new StringBuilder();
                    foreach(Type paramType in paramTypes)
                    {
                        if(builder.Length > 0)
                            builder.Append(", ");
                        builder.Append(paramType.FullName);
                    }
                    throw new Exception("找不到构造函数" + methodName + "(" + builder + ")");
                }
            }
            //反射返回值
            bool isBasicType = false;
            bool isIteratorType = false;
            SqlDataRecord record = null;
            List<PropertyInfo> properties = new List<PropertyInfo>();
            if(resultType.IsPrimitive || resultType.Name == "String" || resultType.Name == "DateTime" || resultType.Name == "TimeSpan")
            {
                isBasicType = true;
                record = new SqlDataRecord(new SqlMetaData("result", SqlDbType.NText));
                SqlContext.Pipe.Send("返回基础类型 " + resultType.FullName);
            }
            else
            {
                Type realType = null;
                if(resultType.IsArray)
                {
                    isIteratorType = true;
                    realType = resultType.GetElementType();
                    SqlContext.Pipe.Send("返回数组类型 " + realType.FullName);
                }
                else
                {
                    realType = resultType;
                    SqlContext.Pipe.Send("返回类类型 " + realType.FullName);
                }
                List<SqlMetaData> metaDatas = new List<SqlMetaData>();
                foreach(PropertyInfo property in realType.GetProperties())
                {
                    if(property.PropertyType.IsPrimitive || property.PropertyType.Name == "String" || property.PropertyType.Name == "DateTime" || property.PropertyType.Name == "TimeSpan")
                    {
                        properties.Add(property);
                        metaDatas.Add(new SqlMetaData(property.Name, SqlDbType.NText));
                        SqlContext.Pipe.Send("详细结果 " + property.Name + " \t" + property.PropertyType.FullName);
                    }
                }
                record = new SqlDataRecord(metaDatas.ToArray());
            }
            int row = 1;
            if(isSetResult)
                SqlContext.Pipe.SendResultsStart(record);
            do
            {
                //创建实例
                object instance = null;
                if(ctor != null)
                {
                    if(ctorReader == null)
                    {
                        instance = ctor.Invoke(null);
                    }
                    else
                    {
                        object[] paramters = new object[ctorReader.FieldCount];
                        for(int i = 0; i < ctorReader.FieldCount; i++)
                        {
                            paramters[i] = ctorReader.GetValue(i);
                        }
                        instance = ctor.Invoke(paramters);
                    }
                    if(!isSetResult)
                        SqlContext.Pipe.Send("创建实例 " + instance);
                }
                do
                {
                    //执行结果
                    object result = instance;
                    if(method != null)
                    {
                        if(methodReader == null)
                        {
                            result = method.Invoke(instance, null);
                        }
                        else
                        {
                            object[] paramters = new object[methodReader.FieldCount];
                            for(int i = 0; i < methodReader.FieldCount; i++)
                            {
                                paramters[i] = methodReader.GetValue(i);
                            }
                            result = method.Invoke(instance, paramters);
                        }
                    }
                    if(!isSetResult)
                        SqlContext.Pipe.Send("执行结果 " + result);
                    //输出结果
                    if(isBasicType)
                    {
                        if(!isSetResult)
                            SqlContext.Pipe.Send("行 " + row + " \t值 " + result);
                        record.SetValue(0, result);
                        if(isSetResult)
                            SqlContext.Pipe.SendResultsRow(record);
                    }
                    else
                    {
                        foreach(object item in isIteratorType ? (IEnumerable)result : new object[] { result })
                        {
                            if(!isSetResult)
                                SqlContext.Pipe.Send("返回结果 " + item);
                            for(int i = 0; i < properties.Count; i++)
                            {
                                if(!isSetResult)
                                    SqlContext.Pipe.Send("行 " + row + " 列 " + properties[i].Name + " \t值 " + properties[i].GetValue(item, null));
                                record.SetValue(i, properties[i].GetValue(item, null)?.ToString());
                            }
                            if(isSetResult)
                                SqlContext.Pipe.SendResultsRow(record);
                        }
                    }
                    row++;
                } while(methodReader != null && methodReader.Read());
            } while(ctorReader != null && ctorReader.Read());
            if(isSetResult)
                SqlContext.Pipe.SendResultsEnd();
            //清理
            if(ctorReader != null)
                ctorReader.Close();
            if(methodReader != null)
                methodReader.Close();
            connection.Close();
        }
    }
}
