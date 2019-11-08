--通过以下步骤向Sql Server添加C#函数扩展
--1. 开启数据库不信任程序集，ALTER DATABASE [Test] SET TRUSTWORTHY ON 
--2. 在数据库管理器中可编程性》程序集》添加，添加bin\debug\下的所有dll
--3. 执行bin\debug\下SQLExtension_Create.sql挑选需要创建的函数执行创建。

--下面是一些常见函数使用介绍
--1. Html处理，Html处理有两个表函数，都是通过xpath来提取节点
--dbo.HtmlTable 表节点提取函数，可用于提取文档中<table></table>节点内容，支持td中的rowspan，colspan跨行
--dbo.HtmlTable(@text nvarchar(max), @tablesPath nvarchar(max), @isTranspose bit, @rowHeader nvarchar(max))
--  @text        需要解析的Html文本。如果传入null，则代表使用上一次解析的html文档，这样避免重复解析，关于此处的现实，可参看下面缓存变量。
--  @tablesPath  提取<table></table>节点xpath路径，可支持多个table路径提取。
--  @isTranspose 是否发生转置，即行列互换，传null不转置
--  @rowHeader   哪一行作为表头，传null无表头
--表函数返回下述结构的表（由于函数无法返回动态的表结构，因此用下述结构来实现动态表）
create table #temp (
	tableName nvarchar(max) NULL,  --表名，多张表时为表序号或表xpath
	rowName nvarchar(max) NULL,    --行名，一般为行序号
	columnName nvarchar(max) NULL, --列名，列序号，或者指定的行作为列名（由@rowHeader参数指定）
	cellValue nvarchar(max) NULL   --单元格值
)
--上述表仅返回了几行几列值为多少，可进行透视pivot操作以方便观察
select *
from dbo.HtmlTable(
	dbo.DownloadText('http://eid.csrc.gov.cn/xbrl/REPORT/HTML/2019/FB030030/CN_50350000_000003_FB030030_20190006/CN_50350000_000003_FB030030_20190006.html', null, null, null),
	'//a[@name="tabItem1_fundIntro"][1]/following-sibling::table[1]//tr[last()]//table',
	null,
	null
) as t
pivot(max(cellvalue) for columnName in ([0], [1], [2])) as t
--执行转置，并指定表头将更容易查看
select *
from dbo.HtmlTable(
	null,
	'//a[@name="tabItem1_fundIntro"][1]/following-sibling::table[1]//tr[last()]//table',
	'true',
	'0'
) as t
pivot(max(cellvalue) for columnName in ([基金简称], [基金主代码], [下属2级基金的基金简称], [下属2级基金的交易代码])) as t

--未完待续
