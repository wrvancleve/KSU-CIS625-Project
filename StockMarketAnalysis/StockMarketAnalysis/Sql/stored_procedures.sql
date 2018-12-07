/* Authors: William Van Cleve, Thomas Rolston, Joseph Webster
 * Date: December 7th, 2018
 * Professor: Dr. Daniel Andresen
 * Course: CIS 625 | Concurrent Systems
 * Stock Market Analysis Optimization
 */

/* (Done) Clears Raw Data Table : Must be done before work */
DROP PROCEDURE IF EXISTS [StockData].[ClearRawData]
GO
CREATE PROCEDURE [StockData].[ClearRawData]
AS
	TRUNCATE TABLE [StockData].[RawData]
GO

/* (Done) Inserts Raw Data into TABLE */
drop procedure if exists [StockData].[InsertRawData]
go
create procedure [StockData].[InsertRawData]
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

/* (Done) Get the data from RawData that meets the condition of the pre-filter and put it into PreFilteredData */
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
GO 

/* (Done) Move current aggregate data into previous aggregate data */
DROP PROCEDURE IF EXISTS [StockData].[MoveAggregateData]
GO
CREATE PROCEDURE [StockData].[MoveAggregateData]
	@CriteriaSetId INT
AS
	DELETE FROM [StockData].[PreviousAggregateData]
	WHERE CriteriaSetId = @CriteriaSetId

	INSERT [StockData].[PreviousAggregateData]
	SELECT *
	FROM [StockData].[CurrentAggregateData] CAD
	WHERE CriteriaSetId = @CriteriaSetId
Go

/* (Done) Get Current Aggregate Data */
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

/* (Done) Get Get Aggregate Data */
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

/* (Done) Get Max Aggregate Data */
drop procedure if exists [StockData].[GetMaxAggregateData]
go
create procedure [StockData].[GetMaxAggregateData]
	@CriteriaSetId int,
	@AggregateKeys nvarchar(128)
AS

WITH SourceCte(CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
AS
(
select PFD.CriteriaSetId as [CriteriaSetId],
	REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', PFD.HolderId), 'stockcode', PFD.StockCode), 'stocktype', PFD.StockType), 'direction', PFD.Direction) AS [AggregateKey],
	sum(PFD.SharesHeld) as [AggregateSharesHeld],
	sum(PFD.PercentageSharesHeld) as [AggregatePercentageSharesHeld],
	sum(PFD.[Value]) as [AggregateValue]
from [StockData].[PreFilteredData] as PFD
WHERE PFD.CriteriaSetId = @CriteriaSetId
GROUP BY PFD.CriteriaSetId, REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', PFD.HolderId), 'stockcode', PFD.StockCode), 'stocktype', PFD.StockType), 'direction', PFD.Direction)
)

MERGE [StockData].[MaxAggregateData] MAD
USING SourceCte SCTE ON SCTE.CriteriaSetId = MAD.CriteriaSetId AND SCTE.AggregateKey = MAD.AggregateKey
WHEN MATCHED THEN
	UPDATE
	SET
		AggregateSharesHeld = 
		(
			SELECT MAX(AggregateSharesHeld)
			FROM
			(
				SELECT MAD2.AggregateSharesHeld
				FROM [StockData].[MaxAggregateData] MAD2
				WHERE MAD2.CriteriaSetId = MAD.CriteriaSetId AND MAD2.AggregateKey = MAD.AggregateKey
				UNION ALL
				SELECT SCTE.AggregateSharesHeld
				FROM SCTE
				WHERE SCTE.CriteriaSetId = MAD.CriteriaSetId AND SCTE.AggregateKey = MAD.AggregateKey
			) AS SharesHeld
		),
		AggregatePercentageSharesHeld = 
		(
			SELECT MAX(AggregatePercentageSharesHeld)
			FROM
			(
				SELECT MAD2.AggregatePercentageSharesHeld
				FROM [StockData].[MaxAggregateData] MAD2
				WHERE MAD2.CriteriaSetId = MAD.CriteriaSetId AND MAD2.AggregateKey = MAD.AggregateKey
				UNION ALL
				SELECT SCTE.AggregatePercentageSharesHeld
				FROM SCTE
				WHERE SCTE.CriteriaSetId = MAD.CriteriaSetId AND SCTE.AggregateKey = MAD.AggregateKey
			) AS PercentageSharesHeld
		),
		AggregateValue = 
		(
			SELECT MAX(AggregateValue)
			FROM
			(
				SELECT MAD2.AggregateValue
				FROM [StockData].[MaxAggregateData] MAD2
				WHERE MAD2.CriteriaSetId = MAD.CriteriaSetId AND MAD2.AggregateKey = MAD.AggregateKey
				UNION ALL
				SELECT SCTE.AggregateValue
				FROM SCTE
				WHERE SCTE.CriteriaSetId = MAD.CriteriaSetId AND SCTE.AggregateKey = MAD.AggregateKey
			) AS PercentageSharesHeld
		)
WHEN NOT MATCHED THEN
	INSERT (CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
	VALUES (SourceCte.CriteriaSetId, SourceCte.AggregateKey, SourceCte.AggregateSharesHeld, SourceCte.AggregatePercentageSharesHeld, SourceCte.AggregateValue);
GO



drop procedure if exists [StockData].[GetPostFilterData]
go
create procedure [StockData].[GetPostFilterData]
	@CriteriaSetId int,
	@ColumnName NVARCHAR(128),
	@Comparison NVARCHAR(128),
	@Value nvarchar(128)
AS

IF OBJECT_ID('[StockData].#PostFilterData') IS NULL
	CREATE TABLE [StockData].#PostFilterData
	(
		PostFilterId int not null identity(1,1) primary key clustered,
		CriteriaSetId int not null,
		AggregateKey nvarchar(128) not null,
		AggregateSharesHeld float not null,
		AggregatePercentageSharesHeld float not null, 
		AggregateValue float not null
	)

	/*
SET @ColumnName = CONCAT(@ColumnName, ' ', @Comparison, ' ', @Value)
SET @NewColumn = CONCAT('ALTER TABLE [StockData].#PostFilterData ADD COLUMN `' , @ColumnName, '`NVARCHAR(128)');
PREPARE stmt FROM @NewColumn;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
*/

IF @Comparison = 'MAX'
BEGIN
	UPDATE [StockData].#PostFilterData
	SET NewColumn = 'true'
	WHERE EXISTS
	(
		SELECT CAD.@ColumnName
		FROM [StockData].[CurrentAggregateData] CAD, [StockData].[MaxAggregateData] MAD
		WHERE MAD.@ColumnName < @Value AND CAD.@ColumnName > @Value
	)
END

ELSE IF @Comparison = 'CROSSES'
BEGIN
	UPDATE [StockData].#PostFilterData
	SET NewColumn = 'true'
	WHERE EXISTS
	(
		SELECT CAD.@ColumnName
		FROM [StockData].[CurrentAggregateData] CAD, [StockData].[PreviousAggregateData] PAD
		WHERE PAD.@ColumnName < @Value AND CAD.@ColumnName > @Value
	)
END

ELSE IF @Comparison = '<'
BEGIN
	UPDATE [StockData].#PostFilterData
	SET NewColumn = 'true'
	WHERE EXISTS
	(
		SELECT CAD.@ColumnName
		FROM [StockData].[CurrentAggregateData] CAD
		WHERE CAD.@ColumnName < @Value
	)
END

ELSE IF @Comparison = '<='
BEGIN
	UPDATE [StockData].#PostFilterData
	SET
		NewColumn = 'true'
	WHERE EXISTS
	(
		SELECT CAD.@ColumnName
		FROM [StockData].[CurrentAggregateData] CAD
		WHERE CAD.@ColumnName <= @Value
	)
END

ELSE IF @Comparison = '>'
BEGIN
	UPDATE [StockData].#PostFilterData
	SET
		NewColumn = 'true'
	WHERE EXISTS
	(
		SELECT CAD.@ColumnName
		FROM [StockData].[CurrentAggregateData] CAD
		WHERE CAD.@ColumnName > @Value
	)
END

ELSE IF @Comparison = '>='
BEGIN
	UPDATE [StockData].#PostFilterData
	SET
		NewColumn = 'true'
	WHERE EXISTS
	(
		SELECT CAD.@ColumnName
		FROM [StockData].[CurrentAggregateData] CAD
		WHERE CAD.@ColumnName >= @Value
	)
END

SELECT *
FROM [StockData].#PostFilterData

GO