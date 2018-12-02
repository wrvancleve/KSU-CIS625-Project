drop procedure if exists [StockData].[InsertRawData]
go
create procedure [StockData].[InsertRawData]
	-- stockcode, stocktype, holderid, holercountry, sharesheld, percentagesharesheld, direction, value
	@StockCode nvarchar(32),
	@StockType nvarchar(32),
	@HolderId nvarchar(32),
	@HolderCountry nvarchar(32),
	@SharesHeld float,
	@PercentageSharedHeld float,
	@Direction nvarchar(32),
	@Value float
as
insert [StockData].[RawData](StockCode, StockType, HolderId, HolderCountry, SharesHeld, PercentageSharesHeld, Direction, [Value])
values(@StockCode, @StockType, @HolderId, @HolderCountry, @SharesHeld, @PercentageSharedHeld, @Direction, @Value)
go

/* Insert Prefiltered Data */
drop procedure if exists [StockData].[InsertPreFilteredData]
go
create procedure [StockData].[InsertPrefilteredData]
	@CriteriaSetId int,
	@StockCode nvarchar(32),
	@StockType nvarchar(32),
	@HolderId nvarchar(32),
	@HolderCountry nvarchar(32),
	@SharesHeld float,
	@PercentageSharedHeld float,
	@Direction nvarchar(32),
	@Value float
as 
insert [StockData].[PreFilteredData](CriteriaSetId, StockCode, StockType, HolderId, HolderCountry, SharesHeld, PercentageSharesHeld, Direction, [Value])
values(@CriteriaSetId, @StockCode, @StockType, @HolderId, @HolderCountry, @SharesHeld, @PercentageSharedHeld, @Direction, @Value)
go 

/* Insert Previous Aggregate Data */
drop procedure if exists [StockData].[InsertPreviousAggregateData]
go
create procedure [StockData].[InsertPreviousAggregateData]
	@CriteriaSetId int,
	@AggregateKey nvarchar(32),
	@AggregateSharesHeld float,
	@AggregatePercentageSharesHeld float, 
	@AggregateValue float
as 
insert [StockData].[PreviousAggregateData](CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
values (@CriteriaSetId, @AggregateKey, @AggregateSharesheld, @AggregatePercentageSharesHeld, @AggregateValue)
go

