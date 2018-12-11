/* 
 * Authors: William Van Cleve, Thomas Rolston, Joseph Webster
 * Date: December 7th, 2018
 * Professor: Dr. Daniel Andresen
 * Course: CIS 625 | Concurrent Systems
 * Stock Market Analysis Optimization
 */

/* Clear raw data in raw data table */
DROP PROCEDURE IF EXISTS [StockData].[ClearData]
GO
CREATE PROCEDURE [StockData].[ClearData]
AS
	TRUNCATE TABLE [StockData].[RawData]
	TRUNCATE TABLE [StockData].[CurrentPreFilteredData]
GO

/* Inserts raw data into raw data table */
DROP PROCEDURE IF EXISTS [StockData].[InsertData]
GO
CREATE PROCEDURE [StockData].[InsertData]
	@StockCode NVARCHAR(16),
	@StockType NVARCHAR(16),
	@HolderId NVARCHAR(32),
	@HolderCountry NVARCHAR(64),
	@SharesHeld FLOAT,
	@PercentageSharesHeld FLOAT,
	@Direction NVARCHAR(16),
	@Value FLOAT
AS
	INSERT INTO [StockData].[RawData]
		--(StockCode, StockType, HolderId, HolderCountry, SharesHeld, PercentageSharesHeld, Direction, [Value])
	VALUES(@StockCode, @StockType, @HolderId, @HolderCountry, @SharesHeld, @PercentageSharesHeld, @Direction, @Value)
GO

/* Obtains max aggregate data for a given criteria set */
DROP PROCEDURE IF EXISTS [StockData].[ObtainMaxAggregateData]
GO
CREATE PROCEDURE [StockData].[ObtainMaxAggregateData]
	@HolderCountries NVARCHAR(128) = NULL,
	@StockType NVARCHAR(16) = NULL,
	@Direction NVARCHAR(16) = NULL,
	@CriteriaSetId INT,
	@AggregateKeys NVARCHAR(144)
AS
	WITH NewAggregateData(CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
	AS
	(
		SELECT @CriteriaSetId,
			REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', RD.HolderId), 'stockcode', RD.StockCode), 'stocktype', RD.StockType), 'direction', RD.Direction) AS [AggregateKey],
			SUM(RD.SharesHeld) AS [AggregateSharesHeld],
			SUM(RD.PercentageSharesHeld) AS [AggregatePercentageSharesHeld],
			SUM(RD.[Value]) AS [AggregateValue]
		FROM [StockData].[RawData] AS RD
		WHERE (CHARINDEX(RD.HolderCountry, @HolderCountries) > 0 OR @HolderCountries IS NULL)
			AND (RD.StockType = @StockType OR @StockType IS NULL)
			AND (RD.Direction = @Direction OR @Direction IS NULL)
		GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', RD.HolderId), 'stockcode', RD.StockCode), 'stocktype', RD.StockType), 'direction', RD.Direction)
	)
	MERGE [StockData].[MaxAggregateData] WITH (TABLOCK) AS MAD 
	USING NewAggregateData AS NAD
	ON NAD.CriteriaSetId = MAD.CriteriaSetId AND NAD.AggregateKey = MAD.AggregateKey
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
					SELECT NAD.AggregateSharesHeld
					WHERE NAD.CriteriaSetId = MAD.CriteriaSetId AND NAD.AggregateKey = MAD.AggregateKey
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
					SELECT NAD.AggregatePercentageSharesHeld
					WHERE NAD.CriteriaSetId = MAD.CriteriaSetId AND NAD.AggregateKey = MAD.AggregateKey
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
					SELECT NAD.AggregateValue
					WHERE NAD.CriteriaSetId = MAD.CriteriaSetId AND NAD.AggregateKey = MAD.AggregateKey
				) AS PercentageSharesHeld
			)
	WHEN NOT MATCHED AND NAD.CriteriaSetId = CriteriaSetId AND NAD.AggregateKey = AggregateKey THEN
		INSERT (CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
		VALUES (NAD.CriteriaSetId, NAD.AggregateKey, NAD.AggregateSharesHeld, NAD.AggregatePercentageSharesHeld, NAD.AggregateValue);
	--OPTION (ORDER GROUP);
GO

/* Obtain previous aggregate data for a given criteria set */
DROP PROCEDURE IF EXISTS [StockData].[ObtainPreviousAggregateData]
GO
CREATE PROCEDURE [StockData].[ObtainPreviousAggregateData]
	@HolderCountries NVARCHAR(128) = NULL,
	@StockType NVARCHAR(16) = NULL,
	@Direction NVARCHAR(16) = NULL,
	@CriteriaSetId INT,
	@AggregateKeys NVARCHAR(144)
AS		
	WITH PreFilteredData(StockCode, StockType, HolderId, HolderCountry, SharesHeld, PercentageSharesHeld, Direction, [Value])
	AS
	(
		SELECT RD.StockCode AS [StockCode],
			RD.StockType AS [StockType],
			RD.HolderId AS [HolderId],
			RD.HolderCountry AS [HolderCountry],
			RD.SharesHeld AS [SharesHeld],
			RD.PercentageSharesHeld AS [PercentageSharesHeld],
			RD.Direction AS [Direction], 
			RD.[Value] AS [Value]
		FROM [StockData].[RawData] AS RD
		WHERE (CHARINDEX(HolderCountry, @HolderCountries) > 0 OR @HolderCountries IS NULL)
			AND (RD.StockType = @StockType OR @StockType IS NULL)
			AND (RD.Direction = @Direction OR @Direction IS NULL)
	)
	INSERT INTO [StockData].[PreviousAggregateData]
			--(CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
	SELECT @CriteriaSetId AS [CriteriaSetId],
		REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', PFD.HolderId), 'stockcode', PFD.StockCode), 'stocktype', PFD.StockType), 'direction', PFD.Direction) AS [AggregateKey],
		Sum(PFD.SharesHeld) AS [AggregateSharesHeld],
		Sum(PFD.PercentageSharesHeld) AS [AggregatePercentageSharesHeld],
		Sum(PFD.[Value]) AS [AggregateValue]
	FROM PreFilteredData PFD
	GROUP BY REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', PFD.HolderId), 'stockcode', PFD.StockCode), 'stocktype', PFD.StockType), 'direction', PFD.Direction)
GO

/* Obtain previous aggregate data for a given criteria set */
DROP PROCEDURE IF EXISTS [StockData].[ObtainCurrentPreFilteredData]
GO
CREATE PROCEDURE [StockData].[ObtainCurrentPreFilteredData]
	@HolderCountries NVARCHAR(128) = NULL,
	@StockType NVARCHAR(16) = NULL,
	@Direction NVARCHAR(16) = NULL,
	@CriteriaSetId INT
AS
	INSERT INTO [StockData].[CurrentPreFilteredData]
	OUTPUT INSERTED.StockCode, INSERTED.StockType, INSERTED.HolderId, INSERTED.HolderCountry, INSERTED.SharesHeld, INSERTED.PercentageSharesHeld, INSERTED.Direction, INSERTED.[Value]
	SELECT @CriteriaSetId,
		RD.StockCode AS [StockCode],
		RD.StockType AS [StockType],
		RD.HolderId AS [HolderId],
		RD.HolderCountry AS [HolderCountry],
		RD.SharesHeld AS [SharesHeld],
		RD.PercentageSharesHeld AS [PercentageSharesHeld],
		RD.Direction AS [Direction], 
		RD.[Value] AS [Value]
	FROM [StockData].[RawData] AS RD
	WHERE (CHARINDEX(RD.HolderCountry, @HolderCountries) > 0 OR @HolderCountries IS NULL)
		AND (RD.StockType = @StockType OR @StockType IS NULL)
		AND (RD.Direction = @Direction OR @Direction IS NULL)
GO

/* Obtain previous aggregate data for a given criteria set */
DROP PROCEDURE IF EXISTS [StockData].[ObtainCurrentAggregateData]
GO
CREATE PROCEDURE [StockData].[ObtainCurrentAggregateData]
	@CriteriaSetId INT,
	@AggregateKeys NVARCHAR(144)
AS
	--(StockCode, StockType, HolderId, HolderCountry, SharesHeld, PercentageSharesHeld, Direction, [Value])
	/* Insert prefitlered data into temp table */
	INSERT INTO [StockData].[CurrentAggregateData]
		--(CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
	OUTPUT INSERTED.AggregateKey, INSERTED.AggregateSharesHeld, INSERTED.AggregatePercentageSharesHeld, INSERTED.AggregateValue
	SELECT PFD.CriteriaSetId,
		REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', PFD.HolderId), 'stockcode', PFD.StockCode), 'stocktype', PFD.StockType), 'direction', PFD.Direction) AS [AggregateKey],
		Sum(PFD.SharesHeld) AS [AggregateSharesHeld],
		Sum(PFD.PercentageSharesHeld) AS [AggregatePercentageSharesHeld],
		Sum(PFD.[Value]) AS [AggregateValue]
	FROM [StockData].[CurrentPreFilteredData] PFD
	WHERE PFD.CriteriaSetId = @CriteriaSetId
	GROUP BY PFD.CriteriaSetId, REPLACE(REPLACE(REPLACE(REPLACE(@AggregateKeys, 'holderid', PFD.HolderId), 'stockcode', PFD.StockCode), 'stocktype', PFD.StockType), 'direction', PFD.Direction)
GO

/* Move current aggregate data into previous aggregate data */
DROP PROCEDURE IF EXISTS [StockData].[MoveAggregateData]
GO
CREATE PROCEDURE [StockData].[MoveAggregateData]
AS
	TRUNCATE TABLE [StockData].[PreviousAggregateData]

	INSERT INTO [StockData].[PreviousAggregateData]
		--(CriteriaSetId, AggregateKey, AggregateSharesHeld, AggregatePercentageSharesHeld, AggregateValue)
	SELECT CAD.CriteriaSetId, CAD.AggregateKey, CAD.AggregateSharesHeld, CAD.AggregatePercentageSharesHeld, CAD.AggregateValue
	FROM [StockData].[CurrentAggregateData] CAD

	TRUNCATE TABLE [StockData].[CurrentAggregateData]
GO