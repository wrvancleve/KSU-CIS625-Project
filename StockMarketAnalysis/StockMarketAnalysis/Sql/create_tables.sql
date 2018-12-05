/* Authors: William Van Cleve, Thomas Rolston, Joseph Webster
 * Date: December 7th, 2018
 * Professor: Dr. Daniel Andresen
 * Course: CIS 625 | Concurrent Systems
 * Stock Market Analysis Optimization
 */
DROP table if exists [StockData].[RawData];
DROP table if exists [StockData].[PreFilteredData];
DROP table if exists [StockData].[CurrentAggregateData];
DROP table if exists [StockData].[PreviousAggregateData];
DROP table if exists [StockData].[MaxAggregateData];

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
	--[Date] datetimeoffset null
)

-- holds the current data AFTER aggregation
create table [StockData].[CurrentAggregateData] 
(
	CurrentAggregateId int not null identity(1,1) primary key clustered,
	CriteriaSetId int not null,
	AggregateKey nvarchar(128) not null,
	AggregateSharesHeld float not null,
	AggregatePercentageSharesHeld float not null, 
	AggregateValue float not null	
)

-- holds the previous days data AFTER aggregation
create table [StockData].[PreviousAggregateData]
(
	PreviousAggregateId int not null identity(1,1) primary key clustered,
	CriteriaSetId int not null,
	AggregateKey nvarchar(128) not null,
	AggregateSharesHeld float not null,
	AggregatePercentageSharesHeld float not null, 
	AggregateValue float not null
)

-- stores the results of Max (sharesheld, value of shares, percentage held)
create table [StockData].[MaxAggregateData] 
(
	MaxAggregateId int not null identity(1,1) primary key clustered,
	CriteriaSetId int not null,
	AggregateKey nvarchar(128) not null,
	AggregateSharesHeld float not null,
	AggregatePercentageSharesHeld float not null, 
	AggregateValue float not null
)