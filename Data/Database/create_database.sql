USE [master]
GO

CREATE DATABASE DotnetRPG
	CONTAINMENT = NONE
	ON PRIMARY 
		( NAME = N'InitialDatabase', FILENAME = N'/var/opt/mssql/data/InitialDatabase.mdf' , SIZE = 8MB , MAXSIZE = UNLIMITED, FILEGROWTH = 8MB ), 
	FILEGROUP [DATA] DEFAULT
		( NAME = N'InitialDatabase_DATA_1', FILENAME = N'/var/opt/mssql/data/InitialDatabase_DATA_1.ndf' , SIZE = 32MB , MAXSIZE = UNLIMITED, FILEGROWTH = 16MB ),
		( NAME = N'InitialDatabase_DATA_2', FILENAME = N'/var/opt/mssql/data/InitialDatabase_DATA_2.ndf' , SIZE = 32MB , MAXSIZE = UNLIMITED, FILEGROWTH = 16MB ),
		( NAME = N'InitialDatabase_DATA_3', FILENAME = N'/var/opt/mssql/data/InitialDatabase_DATA_3.ndf' , SIZE = 32MB , MAXSIZE = UNLIMITED, FILEGROWTH = 16MB ),
		( NAME = N'InitialDatabase_DATA_4', FILENAME = N'/var/opt/mssql/data/InitialDatabase_DATA_4.ndf' , SIZE = 32MB , MAXSIZE = UNLIMITED, FILEGROWTH = 16MB )
	LOG ON 
		( NAME = N'InitialDatabase_log', FILENAME = N'/var/opt/mssql/data/InitialDatabase_log.ldf' , SIZE = 32MB , MAXSIZE = 2048GB , FILEGROWTH = 32MB )
	COLLATE Czech_100_CS_AS_KS_SC_UTF8
	WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO

ALTER DATABASE [DotnetRPG] MODIFY FILEGROUP [DATA] AUTOGROW_ALL_FILES
GO

