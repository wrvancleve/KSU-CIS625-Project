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
	(@HolderCountries = 'null' OR CHARINDEX(HolderCountry, @HolderCountries) > 0)
	AND (@StockType = 'null' OR RD.StockType = @StockType)
	and (@Direction = 'null' OR RD.Direction = @Direction)
go 

/* Insert Current Aggregate Data */
drop procedure if exists [StockData].[GetCurrentAggregateData]
go
create procedure [StockData].[GetCurrentAggregateData]
	@CriteriaSetId int,
	@AggregateKeys nvarchar(128)
AS
insert [StockData].[CurrentAggregateData](CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
select PFD.CriteriaSetId as [CriteriaSetId],
	REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', PFD.HolderId), 'stockcode', PFD.StockCode), 'stocktype', PFD.StockType), 'direction', PFD.Direction) AS [AggregateKey],
	sum(PFD.SharesHeld) as [AggregateSharesHeld],
	sum(PFD.PercentageSharesHeld) as [AggregatePercentageSharesHeld],
	sum(PFD.[Value]) as [AggregateValue]
from [StockData].[PreFilteredData] as PFD
WHERE PFD.CriteriaSetId = @CriteriaSetId
-- the SQL in the where statement may need to be moved to a HAVING clause
GROUP BY PFD.CriteriaSetId, REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', PFD.HolderId), 'stockcode', PFD.StockCode), 'stocktype', PFD.StockType), 'direction', PFD.Direction)
go

/* Get Previous Aggregate Data */
drop procedure if exists [StockData].[GetPreviousAggregateData]
go
create procedure [StockData].[GetPreviousAggregateData]
	@CriteriaSetId int,
	@AggregateKeys nvarchar(128)
AS
insert [StockData].[PreviousAggregateData](CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
select PFD.CriteriaSetId as [CriteriaSetId],
	REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', PFD.HolderId), 'stockcode', PFD.StockCode), 'stocktype', PFD.StockType), 'direction', PFD.Direction) AS [AggregateKey],
	sum(PFD.SharesHeld) as [AggregateSharesHeld],
	sum(PFD.PercentageSharesHeld) as [AggregatePercentageSharesHeld],
	sum(PFD.[Value]) as [AggregateValue]
from [StockData].[PreFilteredData] as PFD
WHERE PFD.CriteriaSetId = @CriteriaSetId
-- the SQL in the where statement may need to be moved to a HAVING clause
GROUP BY PFD.CriteriaSetId, REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', PFD.HolderId), 'stockcode', PFD.StockCode), 'stocktype', PFD.StockType), 'direction', PFD.Direction)
go

/* Get Max Aggregate Data */
drop procedure if exists [StockData].[GetMaxAggregateData]
go
create procedure [StockData].[GetMaxAggregateData]
	@CriteriaSetId int,
	@AggregateKeys nvarchar(128)
AS
insert [StockData].[MaxAggregateData](CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
select PFD.CriteriaSetId as [CriteriaSetId],
	REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', PFD.HolderId), 'stockcode', PFD.StockCode), 'stocktype', PFD.StockType), 'direction', PFD.Direction) AS [AggregateKey],
	sum(PFD.SharesHeld) as [AggregateSharesHeld],
	sum(PFD.PercentageSharesHeld) as [AggregatePercentageSharesHeld],
	sum(PFD.[Value]) as [AggregateValue]
from [StockData].[PreFilteredData] as PFD
WHERE PFD.CriteriaSetId = @CriteriaSetId
-- the SQL in the where statement may need to be moved to a HAVING clause
GROUP BY PFD.CriteriaSetId, REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', PFD.HolderId), 'stockcode', PFD.StockCode), 'stocktype', PFD.StockType), 'direction', PFD.Direction)
go

/* NOT FINISHED */
/* Trigger to insert Max Aggregate Data */
drop trigger if exists [StockData].[UpdateMaxAggregateData]
go

create trigger [StockData].[UpdateMaxAggregateData] on [StockData].[MaxAggregateData]
-- may need to be after insert
after insert
as
if exists 
	(
		select MAD.CriteriaSetId as [CriteriaSetId],
			MAD.AggregateKey as [AggregateKey]
		from [StockData].[MaxAggregateData] as MAD
		intersect 
			select i.CriteriaSetId as [CriteriaSetId],
				i.AggregateKey as [AggregateKey]
			from inserted as i
	)
begin
	Insert [StockData].[MaxAggregateData]
	select CriteriaSetId, AggregateKey, MaxAggregateSharesHeld, MaxAggregatePercentageSharesHeld, MaxAggregateValue
	from 
	(
		Select I.CriteriaSetId, I.AggregateKey, I.AggregateSharesHeld, I.AggregatePercentageSharesHeld, I.AggregateValue
		from inserted i
		union all
		Select MAD.CriteriaSetId, MAD.AggregateKey, MAD.AggregateSharesHeld, MAD.AggregatePercentageSharesHeld, MAD.AggregateValue
		from [StockData].[MaxAggregateData] MAD
	) as MaxValues(CriteriaSetId, AggregateKey, MaxAggregateSharesHeld, MaxAggregatePercentageSharesHeld, MaxAggregateValue)
end

else
	
begin
	insert [StockData].[MaxAggregateData](CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
	select i.CriteriaSetId as [CriteriaSetId],
		i.AggregateKey as [AggregateKey],
		i.AggregateSharesHeld as [AggregateSharesHeld],
		i.AggregatePercentageSharesHeld as [AggregatePercentageSharesHeld],
		i.AggregateValue as [AggregateValue]
	from inserted as i
end