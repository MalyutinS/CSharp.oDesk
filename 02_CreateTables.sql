USE [ODeskJobsSearch]
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

IF NOT EXISTS ( SELECT  *
                FROM    sys.objects
                WHERE   object_id = OBJECT_ID(N'[dbo].[Jobs]')
                        AND type IN ( N'U' ) ) 
    BEGIN

        CREATE TABLE [dbo].[Jobs]
            (
              [Id] [uniqueidentifier] NOT NULL ,
              [ODeskId] [varchar](50) NOT NULL ,
              [Title] [varchar](500) NOT NULL ,
              [OdeskCategory] [varchar](50) NOT NULL ,
              [OdeskSubcategory] [varchar](50) NOT NULL ,
              [DateCreated] [datetime] NOT NULL ,
              [Budjet] [int] NOT NULL ,
              [ClientCountry] [varchar](50) NOT NULL ,
              [Skill] [varchar](50) NOT NULL ,
              CONSTRAINT [PK_Jobs_1] PRIMARY KEY CLUSTERED ( [Id] ASC )
                WITH ( PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON,
                       ALLOW_PAGE_LOCKS = ON ) ON [PRIMARY]
            )
        ON  [PRIMARY]


    END

GO

SET ANSI_PADDING OFF
GO




USE [ODeskJobsSearch]
GO

/****** Object:  Table [dbo].[Contractors]    Script Date: 07/13/2014 11:59:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF NOT EXISTS ( SELECT  *
                FROM    sys.objects
                WHERE   object_id = OBJECT_ID(N'[dbo].[Contractors]')
                        AND type IN ( N'U' ) ) 
    BEGIN

        CREATE TABLE [dbo].[Contractors]
            (
              [Id] [uniqueidentifier] NOT NULL ,
              [ODeskId] [varchar](50) NOT NULL ,
              [Rate] [float] NOT NULL ,
              [Feedback] [float](50) NOT NULL ,
              [Country] [varchar](50) NOT NULL ,
              [LastActivity] [datetime] NOT NULL ,
              [MemberSince] [datetime] NOT NULL ,
              [PortfolioItemsCount] [int] NOT NULL ,
              [TestPassedCount] [int] NOT NULL ,
              [ProfileType] [varchar](50) NOT NULL ,
              [Skill] [varchar](50) NOT NULL ,
              CONSTRAINT [PK_Contractors_1] PRIMARY KEY CLUSTERED ( [Id] ASC )
                WITH ( PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON,
                       ALLOW_PAGE_LOCKS = ON ) ON [PRIMARY]
            )
        ON  [PRIMARY]


    END

GO

SET ANSI_PADDING OFF
GO



