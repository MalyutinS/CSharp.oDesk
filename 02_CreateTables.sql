

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
              [Skill] [varchar](100) NOT NULL ,
              CONSTRAINT [PK_Jobs_1] PRIMARY KEY CLUSTERED ( [Id] ASC )
                WITH ( PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON,
                       ALLOW_PAGE_LOCKS = ON ) ON [PRIMARY]
            )
        ON  [PRIMARY]


    END

GO

SET ANSI_PADDING OFF
GO




/****** Object:  Table [dbo].[Contractors_Skills]    Script Date: 07/13/2014 11:59:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF NOT EXISTS ( SELECT  *
                FROM    sys.objects
                WHERE   object_id = OBJECT_ID(N'[dbo].[Contractors_Skills]')
                        AND type IN ( N'U' ) ) 
    BEGIN

        CREATE TABLE [dbo].[Contractors_Skills]
            (
              [Id] [uniqueidentifier] NOT NULL ,
              [ODeskId] [varchar](50) NOT NULL ,
              [Skill] [varchar](100) NOT NULL ,
              CONSTRAINT [PK_Contractors_Skills_1] PRIMARY KEY CLUSTERED ( [Id] ASC )
                WITH ( PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON,
                       ALLOW_PAGE_LOCKS = ON ) ON [PRIMARY]
            )
        ON  [PRIMARY]


    END

GO

SET ANSI_PADDING OFF
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
              TotalHours [float] NOT NULL ,
              EngSkill [int] NOT NULL ,
              Country [varchar](50) NOT NULL ,
              TotalFeedback [float] NOT NULL ,
              IsAffiliated [int] NOT NULL ,
              AdjScore [float] NOT NULL ,
              AdjScoreRecent [float] NOT NULL ,
              LastWorkedTs [bigint] NOT NULL ,
              LastWorked [varchar](50) NOT NULL ,
              PortfolioItemsCount [int] NOT NULL ,
              UiProfileAccess [varchar](50) NOT NULL ,
              BilledAssignments [int] NOT NULL ,
              BillRate [float] NOT NULL ,
              RecNo [int] NOT NULL ,
              City [varchar](50) NOT NULL ,
              LastActivity [varchar](50) NOT NULL ,
              ShortName [varchar](50) NOT NULL ,
              CONSTRAINT [PK_Contractors_1] PRIMARY KEY CLUSTERED ( [Id] ASC )
                WITH ( PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON,
                       ALLOW_PAGE_LOCKS = ON ) ON [PRIMARY]
            )
        ON  [PRIMARY]


    END

GO

SET ANSI_PADDING OFF
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
                WHERE   object_id = OBJECT_ID(N'[dbo].[Ranges]')
                        AND type IN ( N'U' ) ) 
    BEGIN

		CREATE TABLE [dbo].[Ranges]
		(
			[Name] VARCHAR(50) NOT NULL , 
		    [Min] INT NOT NULL, 
		    [Max] INT NOT NULL, 
		    PRIMARY KEY ([Min], [Max])
		)
		ON  [PRIMARY]


    END

GO


SET ANSI_PADDING OFF
GO