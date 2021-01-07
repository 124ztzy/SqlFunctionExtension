--通过以下步骤向Sql Server添加C#函数扩展，主要扩展的表函数、标量函数、聚合函数，以扩充Sql语句的功能。
--添加的功能有文件操作、网络下载、Excel读取、Html路径提取、Json路径提取、正则表达式、全局变量等。


--一、准备步骤
--1. 开启数据库不信任程序集，ALTER DATABASE [Test] SET TRUSTWORTHY ON 
--2. 在数据库管理器中可编程性》程序集》添加，添加bin\debug\下的所有dll，注意有添加顺序，按报错的顺序添加。
--3. 执行bin\debug\下SQLExtension_Create.sql挑选需要创建的函数执行创建。


--二、文件操作相关
--1. 文件大小，文件移动、复制、删除，返回文件大小
select dbo.FileSize('\\192.168.1.34\database\test\测试.xlsx')
select dbo.FileMove('\\192.168.1.34\database\test\测试.xlsx', '\\192.168.1.34\database\test\测试2.xlsx')
select dbo.FileCopy('\\192.168.1.34\database\test\测试.xlsx', '\\192.168.1.34\database\test\测试2.xlsx')
select dbo.FileDelete('\\192.168.1.34\database\test\测试.xlsx')
--2. 文本读取、文本写入，最后参数为编码格式，null为默认utf8
select dbo.FileRead('\\192.168.1.34\database\test\测试.txt', 'gbk')
--这里换行为了故意写入换行符
select dbo.FileWirte('\\192.168.1.34\database\test\测试.txt', 'f1,f2
123,abc
456,def', 'gbk')
--3. 目录树读取，第二个参数是否递归读取子目录
select * from dbo.FileTree('\\192.168.1.34\database\test\', 'false')
--返回表结构
create table #temp1 (
	fullPath nvarchar(max), --完整路径
	fileName nvarchar(max), --文件名或目录名
	fileExtension nvarchar(max), --文件扩展名
	fileSize bigint, --文件大小
	createTime datetime, --创建时间
	lastWirteTime datetime -- 最后写入时间
)
--4. 目录压缩、解压（已放弃）


--三、网络下载
--1. 下载文本，返回文本
-- @url			请求地址
-- @headers		http请求头，如referer、cookies等，多个参数以换行符分隔
-- @postParam	post提交的文本数据，类似key1=value1&key2=value2，传null则为get请求
-- @encoding	读取的文本编码方式，null为utf-8
select dbo.DownloadText('http://www.baidu.com', null, null, null)
--2. 下载文本且缓存一段时间，避免重复下载
-- 前4个参数同上
-- @savePath	保存文件地址
-- @dueTime		过期时间，如果文件最后修改时间在过期时间内，则直接读取该问题，超过过期时间则重新下载文件
select dbo.DownloadTextCache('http://www.baidu.com', null, null, null, '\\192.168.1.34\database\test\baidu.html', '12:00')
--3. 下载文件，返回文件地址（旧版返回文件大小）
-- @url			请求地址
-- @headers		http请求头，如referer、cookies等，多个参数以换行符分隔
-- @postParam	post提交的文本数据，类似key1=value1&key2=value2，传null则为get请求
-- @savePath	保存文件地址
select dbo.DownloadFile('http://www.baidu.com', null, null, '\\192.168.1.34\database\test\baidu.html')
--4. 下载文件且缓存一段时间
--	参数同上
select dbo.DownloadFileCache('http://www.baidu.com', null, null, '\\192.168.1.34\database\test\baidu.html', '12:00')


--四、Csv、Excel读取
--1. 读取Csv文本
--注意传入文本，而不是文件路径
--由于函数无法返回动态的表结构，因此用下述结构来实现动态表，可以通过数据透视进行行转列，以方面查看
create table #temp2 (
	tableName nvarchar(max) NULL,  --表名，多张表时为表序号或表xpath
	rowName nvarchar(max) NULL,    --行名，一般为行序号
	columnName nvarchar(max) NULL, --列名，列序号，或者指定的行作为列名
	cellValue nvarchar(max) NULL   --单元格值
)
--示例
select * from dbo.CsvText(dbo.FileRead('\\192.168.1.34\database\test\测试.txt', null))
--数据透视
select * from dbo.CsvText(dbo.FileRead('\\192.168.1.34\database\test\测试.txt', null))
pivot (max(cellValue) for columnName in ([0], [1])) as t
--2. 读取excel文件
select [0], [1] from dbo.ExcelFile('\\192.168.1.34\database\test\测试.xlsx')
pivot (max(cellValue) for columnName in ([0], [1])) as t


--五、Html处理
--1. xpath路径提取，返回动态表结构，同上面的#temp2
-- @text	html文本，如果传入null，则代表使用上一次解析的html文档，这样避免重复解析，关于此处的实现，可参看下面变量。
-- @rowPath 行提取的xpath路径
-- @columnsPath 从行提取列的xpath路径，多个路径用,分隔
select [th1|td1], [th2|td2]
from (
	select tableName, rowName, replace(replace(columnName, '[', ''), ']', '') columnName, cellValue
	from dbo.HtmlPath(
		dbo.FileRead('\\192.168.1.34\database\test\测试.html', null),
		'//tr',
		'th[1]|td[1], th[2]|td[2]'
	)
) as t pivot (max(cellValue) for columnName in ([th1|td1], [th2|td2])) as t
--2. xpath提取table节点，可用于提取文档中<table></table>节点内容，支持td中的rowspan，colspan跨行，返回动态表结构，同上面的#temp2
--  @text        需要解析的Html文本。null，则代表使用上一次解析的html文档
--  @tablesPath  提取<table></table>节点xpath路径，可支持多个table路径提取。
--  @isTranspose 是否发生转置，即行列互换，传null不转置
--  @rowHeader   哪一行作为表头，传null无表头
--下面是证监会xbrl网页表格的提取基金基本信息的演示
select [0], [1], [2]
from dbo.HtmlTable(
	dbo.DownloadText('http://eid.csrc.gov.cn/xbrl/REPORT/HTML/2019/FB030030/CN_50350000_000003_FB030030_20190006/CN_50350000_000003_FB030030_20190006.html', null, null, null),
	'//a[@name="tabItem1_fundIntro"][1]/following-sibling::table[1]//tr[last()]//table',
	null,
	null
) as t
pivot(max(cellvalue) for columnName in ([0], [1], [2])) as t
--添加转置，并指定表头，将更容易查看，仅提取了部分内容
select [基金简称], [基金主代码], [下属2级基金的基金简称], [下属2级基金的交易代码]
from dbo.HtmlTable(
	null,
	'//a[@name="tabItem1_fundIntro"][1]/following-sibling::table[1]//tr[last()]//table',
	'true',
	'0'
) as t
pivot(max(cellvalue) for columnName in ([基金简称], [基金主代码], [下属2级基金的基金简称], [下属2级基金的交易代码])) as t


--六、Json处理
--1. 通过jsonpath进行提取，传入参数与HtmlPath参数类似。
select f1, f2 from dbo.JsonPath(
	'[{f1:123, f2:''abc''}, {f1:456, f2:''def''}]',
	'[*]',
	'f1, f2'
) pivot(max(cellvalue) for columnName in (f1, f2)) as t


--七、正则表达式
--1. 正则表达式是否匹配，返回1或0
-- @text	待匹配文本
-- @regex	正则表达式
-- @option	匹配选项，null为默认值0（新版已废除）
select dbo.RegexIsMatch('123', '\d[3]', null)
select dbo.RegexIsMatch('123@qq.', '\S+@\S+\.\S+', null)
--2. 正则表达式替换
--替换所有数字
select dbo.RegexReplace('abc123def', '\d', '', null)
--3. 正则表达式匹配，每一次匹配作为一行，返回动态表结构，同上面的#temp2
select [0] from RegexMatch('abc123def', '[a-z]+', null)
pivot(max(cellvalue) for columnName in ([0])) as t
--正则中的分组将作为列返回，如果有列名将返回列名，没有列名将返回列序号，下面是复杂点的示例
select [2] as f1, [value] as f2
from RegexMatch(
	'[{f1:123, f2:''abc''}, {f1:456, f2:''def''}]', 
	'\{([^:]+):([^,]+), (?<key>[^:]+):(?<value>[^,]+)\}', 
	null
) pivot(max(cellvalue) for columnName in ([2],[value])) as t
--4. 正则表达式分割字符串，返回动态表结构，同上面的#temp2
select [0], [1] from RegexSplit('f1,f2
123,abc
456,def', '\r\n', ',', null) 
pivot(max(cellvalue) for columnName in ([0], [1])) as t


--八、序列表
--1. 返回数字序列
-- @start	起始值
-- @end		终止值
-- @step	步长，每次增长值，禁止为负
select * from dbo.Sequence(1, 10, 1)
--2. 日期序列，新版将有更好的函数来替代
--	@step	天数，支持小数
select * from dbo.SequenceDate('2019-01-01', '2019-01-07', 0.25)
--	@step	时间类型
select * from dbo.SequenceDateTime('2019-01-01', '2019-01-02', '1:00')


--九、类型转化
--1. 非常强大的数字转化函数
select dbo.GetNumber('-123e-3')
select dbo.GetNumber('二零零九')
select dbo.GetNumber('一千二百三十')
--2. 转化时间
select dbo.GetDateTime('20190101')
--3. 格式化输出，等同C# String.Fromat，最多支持5个变量
--小数1.23 两位百分数23.40% 六位十进制数字000123 科学计数法1.234568E+008 十六进制FF
select dbo.TextFormat('小数{0:F2} 两位百分数{1:P2} 六位十进制数字{2:D6} 科学计数法{3:E} 十六进制{4:X}', 1.234, 0.234, 123, 123456789, 255)
--逗号分隔123,456,789.00 货币¥0.23 分隔数字12345-6789
select dbo.TextFormat('逗号分隔{0:N2} 货币{1:C2} 分隔数字{2:0000-0000}', 123456789, 0.234, 123456789, null, null)
--19年1月1日0时0分0秒 002019年一月01日星期二 十二小时制12:00.00.0000
select dbo.TextFormat('{0:y年M月d日H时m分s秒} {0:yyyyyy年MMMMdd日dddd 十二小时制hh:mm.ss.ffff}', cast('2019-01-01' as date), null, null, null, null)


--十、变量
--为了解决select查询时，输出且赋值的需要，引入变量
--1. 变量赋值
select dbo.VariableAssign('a', 1.0)
select dbo.VariableAssign('b', getdate())
--2. 变量取值，返回不同类型
select dbo.Variable('a')
select dbo.VariableBigint('a')
select dbo.VariableDecimal('a')
select dbo.VariableDateTime('b')
select dbo.VariableVarchar('b')
--3. 清理变量，成功返回1，传null清理所有变量
select dbo.VariableClear('a')
select dbo.VariableClear(null)
--4. 查看所有变量
select * from dbo.VariableView()
--5. 综合示例，计算1-10累乘
select dbo.VariableAssign('a', 1.0)
select *, dbo.VariableAssign('a', dbo.VariableDecimal('a') * cellValue)
from dbo.Sequence(1, 10, 1)


