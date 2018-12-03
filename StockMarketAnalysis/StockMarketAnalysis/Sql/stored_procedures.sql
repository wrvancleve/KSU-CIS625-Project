/* Authors: William Van Cleve, Thomas Rolston, Joseph Webster
 * Date: December 7th, 2018
 * Professor: Dr. Daniel Andresen
 * Course: CIS 625 | Concurrent Systems
 * Stock Market Analysis Optimization
 */

drop procedure if exists [StockData].[InsertRawData]
go
create procedure [StockData].[InsertRawData]
	-- stockcode, stocktype, holderid, holercountry, sharesheld, percentagesharesheld, direction, value
	@StockCode nvarchar(32),
	@StockType nvarchar(32),
	@HolderId nvarchar(32),
	@HolderCountry nvarchar(32),
	@SharesHeld float,
	@PercentageSharesHeld float,
	@Direction nvarchar(32),
	@Value float
as
insert [StockData].[RawData](StockCode, StockType, HolderId, HolderCountry, SharesHeld, PercentageSharesHeld, Direction, [Value])
values(@StockCode, @StockType, @HolderId, @HolderCountry, @SharesHeld, @PercentageSharesHeld, @Direction, @Value)
go

/* Get the data from RawData that meets the condition of the pre-filter and put it into PreFilteredData */
drop procedure if exists [StockData].[GetPreFilterData]
go
create procedure [StockData].[GetPreFilterData]
	@CriteriaSetId int,
	@HolderCountries nvarchar(128),
	@StockType nvarchar(32),
	@Direction nvarchar(32)
AS
insert into [StockData].[PreFilteredData](CriteriaSetId, StockCode, StockType, HolderId, HolderCountry, SharesHeld, PercentageSharesHeld, Direction, Value)
select @CriteriaSetId as CriteriaSetId,
	RD.StockCode as [StockCode],
	RD.StockType as [StockType],
	RD.HolderId as [HolderId],
	RD.HolderCountry as [HolderCountry],
	RD.SharesHeld as [SharesHeld],
	RD.PercentageSharesHeld as [PercentageSharesHeld],
	RD.Direction as [Direction], 
	RD.Value as [Value]
from [StockData].[RawData] as RD
where 
	(@HolderCountries = "null" OR CHARINDEX(HolderCountry, @HolderCountries) > 0)
	AND (@StockType = "null" OR RD.StockType = @StockType)
	and (@Direction = "null" OR RD.Direction = @Direction)
go 

/* Insert Current Aggregate Data */
drop procedure if exists [StockData].[GetAggregateData]
go
create procedure [StockData].[GetAggregateData]
	@CriteriaSetId int,
	@AggregateKey nvarchar(32)
as 
insert [StockData].[CurrentAggregateData](CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
select PDF.CriteriaSetId as [CriteriaSetId],
	concat(@AggregateKey) as [AggregateKey], -- concatenates the two strings
	sum(PDF.SharesHeld) as [AggregateSharesHeld],
	sum(PDF.PercentageSharesHeld) as [AggregatePercentageSharesHeld],
	sum(PDF.[Value]) as [AggregateValue]
from [StockData].[PreFilteredData] as PFD
-- the SQL in the where statement may need to be moved to a HAVING clause
where concat(PFD.HolderId, '~', PFD.Direction) = @AggregateKey or
	concat(PFD.HolderId, '~', PFD.StockCode) = @AggregateKey or
	concat(PFD.HolderId, '~', PFD.StockType) = @AggregateKey or

	concat(PFD.Direction, '~', PFD.HolderId) = @AggregateKey or
	concat(PFD.Direction, '~', PFD.StockCode) = @AggregateKey or
	concat(PFD.Direction, '~', PFD.StockType) = @AggregateKey or

	concat(PFD.StockCode, '~', PFD.HolderId) = @AggregateKey or
	concat(PFD.StockCode, '~', PFD.Direction) = @AggregateKey or
	concat(PFD.StockCode, '~', PFD.StockType) = @AggregateKey or

	concat(PFD.StockType, '~', PFD.HolderId) = @AggregateKey or
	concat(PFD.StockType, '~', PFD.Direction) = @AggregateKey or
	concat(PFD.StockType, '~', PFD.StockCode) = @AggregateKey
go

/* Insert Current Aggregate Data */
drop procedure if exists [StockData].[InsertCurrentAggregateData]
go
create procedure [StockData].[InsertCurrentAggregateData]
	@CriteriaSetId int,
	@AggregateKey nvarchar(32),
	@AggregateSharesHeld float,
	@AggregatePercentageSharesHeld float, 
	@AggregateValue float
as 
insert [StockData].[CurrentAggregateData](CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
values (@CriteriaSetId, @AggregateKey, @AggregateSharesheld, @AggregatePercentageSharesHeld, @AggregateValue)
go

/* Insert Max Aggregate Data */
drop procedure if exists [StockData].[InsertMaxAggregateData]
go
create procedure [StockData].[InsertMaxAggregateData]
	@CriteriaSetId int,
	@AggregateKey nvarchar(32),
	@AggregateSharesHeld float,
	@AggregatePercentageSharesHeld float, 
	@AggregateValue float
as 
insert [StockData].[MaxAggregateData](CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
values (@CriteriaSetId, @AggregateKey, @AggregateSharesheld, @AggregatePercentageSharesHeld, @AggregateValue)
go