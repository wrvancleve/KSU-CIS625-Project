use jwebster7;
--create schema [StockData];

drop table if exists [StockData].[RawData]

-- holds data BEFORE pre-filtering
create table [StockData].[RawData]
(
	DataId int not null identity(1,1) primary key clustered,
	StockCode nvarchar(32) not null,
	StockType nvarchar(32) not null,
	HolderId nvarchar(32) not null,
	HolderCountry nvarchar(32) not null,
	SharesHeld float not null,
	PercentageSharesHeld float not null,
	Direction nvarchar(32) not null,
	[Value] float not null
	--[Date] date not null
)

-- Testing what's in the database
-- select * from [StockData].[RawData]

-- holds data AFTER pre-filtering
create table [StockData].[PreFilteredData]
(
	PreFilterId int not null identity(1,1) primary key clustered,
	CriteriaSetId int not null,
	StockCode nvarchar(32) not null,
	StockType nvarchar(32) not null,
	HolderId nvarchar(32) not null,
	HolderCountry nvarchar(32) not null,
	SharesHeld float not null,
	PercentageSharesHeld float not null,
	Direction nvarchar(32) not null,
	[Value] float not null, 
	[Date] date not null
)

-- holds the previous days data AFTER aggregation
create table [StockData].[PreviousAggregateData]
(
	PreviousAggregateId int not null identity(1,1) primary key clustered,
	CriteriaSetId int not null,
	AggregateKey nvarchar(32) not null,
	AggregatredSharesHeld float not null,
	AggregatedPercentageSharesHeld float not null, 
	AggregatedValue float not null
)

-- holds the current data AFTER aggregation
create table [StockData].[CurrentAggregateData] 
(
	CurrentAggregateId int not null identity(1,1) primary key clustered,
	CriteriaSetId int not null,
	AggregateKey nvarchar(32) not null,
	AggregatredSharesHeld float not null,
	AggregatedPercentageSharesHeld float not null, 
	AggregatedValue float not null	
)

-- stores the results of Max (sharesheld, value of shares, percentage held)
create table [StockData].[MaxAggregateData] 
(
	MaxAggregateId int not null identity(1,1) primary key clustered,
	CriteriaSetId int not null,
	AggregateKey nvarchar(32) not null,
	AggregatredSharesHeld float not null,
	AggregatedPercentageSharesHeld float not null, 
	AggregatedValue float not null
)