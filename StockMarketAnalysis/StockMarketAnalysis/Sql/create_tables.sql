/* 
 * Authors: William Van Cleve, Thomas Rolston, Joseph Webster
 * Date: December 7th, 2018
 * Professor: Dr. Daniel Andresen
 * Course: CIS 625 | Concurrent Systems
 * Stock Market Analysis Optimization
 */

DROP TABLE IF EXISTS [StockData].[RawData];
DROP TABLE IF EXISTS [StockData].[CurrentPreFilteredData];
DROP TABLE IF EXISTS [StockData].[MaxAggregateData];
DROP TABLE IF EXISTS [StockData].[PreviousAggregateData];
DROP TABLE IF EXISTS [StockData].[CurrentAggregateData];

/*
TRUNCATE TABLE [StockData].[RawData];
TRUNCATE TABLE [StockData].[CurrentAggregateData];
TRUNCATE TABLE [StockData].[CurrentPreFilteredData];
TRUNCATE TABLE [StockData].[PreviousAggregateData];
TRUNCATE TABLE [StockData].[MaxAggregateData];
*/

/* Holds raw data read in from the file */
CREATE TABLE [StockData].[RawData]
(
	DataId INT NOT NULL IDENTITY(1,1) PRIMARY KEY CLUSTERED,
	StockCode NVARCHAR(16) NOT NULL,
	StockType NVARCHAR(16) NOT NULL,
	HolderId NVARCHAR(32) NOT NULL,
	HolderCountry NVARCHAR(64) NOT NULL,
	SharesHeld FLOAT NOT NULL,
	PercentageSharesHeld FLOAT NOT NULL,
	Direction NVARCHAR(16) NOT NULL,
	[Value] FLOAT NOT NULL
)

/* Holds raw data read in from the file */
CREATE TABLE [StockData].[CurrentPreFilteredData]
(
	DataId INT NOT NULL IDENTITY(1,1),
	CriteriaSetId INT NOT NULL,
	StockCode NVARCHAR(16) NOT NULL,
	StockType NVARCHAR(16) NOT NULL,
	HolderId NVARCHAR(32) NOT NULL,
	HolderCountry NVARCHAR(64) NOT NULL,
	SharesHeld FLOAT NOT NULL,
	PercentageSharesHeld FLOAT NOT NULL,
	Direction NVARCHAR(16) NOT NULL,
	[Value] FLOAT NOT NULL

	PRIMARY KEY CLUSTERED
	(
		DataId ASC,
		CriteriaSetId ASC
	)
)

/* Holds max aggregate data from the past */
CREATE TABLE [StockData].[MaxAggregateData] 
(
	CriteriaSetId INT NOT NULL,
	AggregateKey NVARCHAR(144) NOT NULL,
	AggregateSharesHeld FLOAT NOT NULL,
	AggregatePercentageSharesHeld FLOAT NOT NULL, 
	AggregateValue FLOAT NOT NULL
    
	PRIMARY KEY CLUSTERED
	(
		CriteriaSetId ASC,
		AggregateKey ASC
	)
)

/* Holds aggregate data from the previous day */
CREATE TABLE [StockData].[PreviousAggregateData]
(
	CriteriaSetId INT NOT NULL,
	AggregateKey NVARCHAR(144) NOT NULL,
	AggregateSharesHeld FLOAT NOT NULL,
	AggregatePercentageSharesHeld FLOAT NOT NULL, 
	AggregateValue FLOAT NOT NULL
    
	PRIMARY KEY CLUSTERED
	(
		CriteriaSetId ASC,
		AggregateKey ASC
	)
)

/* Holds aggregate data from the current day */
CREATE TABLE [StockData].[CurrentAggregateData] 
(
	CriteriaSetId INT NOT NULL,
	AggregateKey NVARCHAR(144) NOT NULL,
	AggregateSharesHeld FLOAT NOT NULL,
	AggregatePercentageSharesHeld FLOAT NOT NULL, 
	AggregateValue FLOAT NOT NULL	

	PRIMARY KEY CLUSTERED
	(
		CriteriaSetId ASC,
		AggregateKey ASC
	)
)