
--使用C#扩展Sql Server函数，下面是一些常见函数使用示例


--1. 序列表
--数字序列
select *
from dbo.Sequence(0.1, 1, 0.01)
--日期序列，每天
select *
from dbo.SequenceDate('2019-01-01', '2019-12-31', 1)
--时间序列，每隔1小时
select *
from dbo.SequenceDateTime('2019-01-01 00:00:00', '2019-01-02 00:00:00', '01:00:00')



--2. 字符串聚合拼接
select dbo.TextAggregate(f1, ',')
from (
	select 1 as f1
	union all 
	select 2
) as t
--可用for xml path代替
select stuff((
	select ',' + cast(f1 as varchar)
	from (
		select 1 as f1
		union all 
		select 2
	) as t
	for xml path('')
), 1, 1, '')



--3. 下载提取采集
--采集天天基金净值，下载后用正则表达式匹配提取值再行转列
--drop table #jz
select matchNumber, '000001' as fundCode, '华夏成长混合' as fundName, [1] as tradeDate, [3] as jz, [5] as ljjz
into #jz
from dbo.RegexMatch(
	 dbo.DownloadText(
		'http://api.fund.eastmoney.com/f10/lsjz?fundCode=000001&pageIndex=1&pageSize=1000', 
		'http://fundf10.eastmoney.com/jjjz_000001.html', 
		 null, 
		 null),
	 dbo.JsonRegex('FSRQ, DWJZ, LJJZ', 0),
	 0
) as t1
pivot (max(captureValue) for groupName in ([1],[3],[5])) as t2


--下载中证央企指数样本，下载一次后，12小时内调用将不再下载
select dbo.DownloadFileCache(
	'http://www.csindex.com.cn/uploads/file/autofile/cons/000926cons.xls', 
	'http://www.csindex.com.cn/zh-CN/indices/index-detail/000926', 
	null, 
	'\\192.168.1.34\database\中证指数\000926cons.xls',
	'12:00:00')


--读取Excel，可避免OLEDB莫名其妙地报错
--需要添加程序集ICSharpCode.SharpZipLib.dll、ExcelDataReader.dll
select [0] as tradeDate, [1] as indexCode, [2] as indexName, [4] as stockCode, [5] as stockName
from dbo.ExcelRead('\\192.168.1.34\database\中证指数\000926cons2.xls') as t1
pivot (max(cellValue) for columnNumber in ([0],[1],[2],[3],[4],[5],[6],[7])) as t2
where [0] != '日期Date'
--可用OLEDB驱动代替
select * 
from openrowset(
	'Microsoft.ACE.OLEDB.12.0',
	'Excel 8.0;hdr=no;Database=\\192.168.1.34\database\中证指数\000926cons.xls',
	'select * from [2019-08-20$]'
)



									
--4. 全局变量保存赋值
--计算每周一定投1000收益率（不考虑分红），利用全局变量进行累加运算
select dbo.VariableAssign('number', 0)
select dbo.VariableAssign('cost', 0.0)
select dbo.VariableAssign('share', 0.0)
select 
	*, 
	datepart(weekday, tradeDate) - 1 as '周几', 
	case when datepart(weekday, tradeDate) - 1 = 1 
		then dbo.VariableAssign('number', dbo.VariableBigint('number') + 1)
		else dbo.VariableBigint('number')
	end as '第几期',
	case when datepart(weekday, tradeDate) - 1 = 1 
		then dbo.VariableAssign('cost', dbo.VariableDecimal('cost') + 1000)
		else dbo.VariableDecimal('cost')
	end as '总投入',
	--按申购费1.5%计算，实际申购金额 * (1 + 1.5%) = 1000，实际申购金额 = 1000.0 / 1.015
	case when datepart(weekday, tradeDate) - 1 = 1 
		then dbo.VariableAssign('share', dbo.VariableDecimal('share') + cast(1000.0 / 1.015 / jz as decimal(18, 6)))
		else dbo.VariableDecimal('share')
	end as '总份额',
	dbo.VariableDecimal('share') * jz as '总金额',
	case when dbo.VariableDecimal('cost') = 0
		then 0
		else (dbo.VariableDecimal('share') * jz - dbo.VariableDecimal('cost')) / dbo.VariableDecimal('cost')
	 end as '定投收益率'
from #jz
where tradeDate >= '2019-01-01'
order by tradeDate


									      
--5. 调用C#反射函数
--执行反射方法，格式化字符串
exec dbo.ExecuteReflection 
	'System.String', 
	null,
	'Format', 
	'select ''{0:D4}'', 1 union all select ''{0:D4}'', 2',
	0
--可用TextFormat函数代替
select dbo.TextFormat(f1, f2)
from (
	select '{0:D4}' as f1, 1 as f2
	union all 
	select '{0:D4}', 2
) as t1


--执行反射方法，获取文件信息
exec dbo.ExecuteReflection 
	'System.IO.FileInfo', 
	'select ''\\192.168.1.34\database\中证指数\000926cons.xls'' union all select ''\\192.168.1.34\database\国富人寿\998600指数交易数据（20181228）.xlsx''', 
	null, 
	null,
	0

--执行反射方法，获取目录下文件信息
exec dbo.ExecuteReflection 
	'System.IO.DirectoryInfo', 
	'select ''\\192.168.1.34\database\中证指数\'' union all select ''\\192.168.1.34\database\国富人寿\''', 
	'GetFiles', 
	null,
	0
--可用FileTree文件树表函数代替
select *
from dbo.FileTree('\\192.168.1.34\database\中证指数\', 0, null)
union all
select *
from dbo.FileTree('\\192.168.1.34\database\国富人寿\', 0, null)

