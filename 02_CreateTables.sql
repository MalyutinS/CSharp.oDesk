USE [ODeskJobsSearch]
GO

/****** Object:  Table [dbo].[SearchDetails]    Script Date: 07/13/2014 11:59:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[SearchDetails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Category] [varchar](50) NULL,
	[SubCategory] [varchar](50) NULL,
	[Name] [varchar](50) NOT NULL,
	[Keywords] [varchar](250) NOT NULL,
 CONSTRAINT [PK_SearchDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


USE [ODeskJobsSearch]
GO

/****** Object:  Table [dbo].[Jobs]    Script Date: 07/13/2014 11:59:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Jobs](
	[Id] [uniqueidentifier] NOT NULL,
	[ODeskId] [varchar](50) NOT NULL,
	[Title] [varchar](250) NOT NULL,
	[OdeskCategory] [varchar](50) NOT NULL,
	[OdeskSubcategory] [varchar](50) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[Budjet] [int] NOT NULL,
	[ClientCountry] [varchar](50) NOT NULL,
	[SearchCategory] [varchar](50) NOT NULL,
	[SearchSubCategory] [varchar](50) NOT NULL,
	[SearchName] [varchar](50) NOT NULL,
	[SearchKeyword] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Jobs_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


