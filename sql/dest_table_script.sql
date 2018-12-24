USE [DESTINATION_DATABASE_NAME]


/****** Object:  Table [dbo].[DESTINATION_TABLE_NAME]    Script Date: 12/23/2018 21:23:34 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DESTINATION_TABLE_NAME]') AND type in (N'U')) DROP TABLE [dbo].[DESTINATION_TABLE_NAME]


USE [DESTINATION_DATABASE_NAME]


/****** Object:  Table [dbo].[DESTINATION_TABLE_NAME]    Script Date: 12/23/2018 21:23:34 ******/
SET ANSI_NULLS ON


SET QUOTED_IDENTIFIER ON


SET ANSI_PADDING ON


CREATE TABLE [dbo].[DESTINATION_TABLE_NAME](
	[Created By] [varchar](255) NULL,
	[Employee Name] [varchar](255) NULL,
	[Employee Number] [varchar](255) NULL,
	[Department] [varchar](255) NULL,
	[Group] [varchar](255) NULL,
	[Total Score] [varchar](255) NULL,
	[Total Weight] [varchar](255) NULL,
	[Bsc Goal Setting Workflow] [varchar](255) NULL,
	[BSC Appraisal Workflow] [varchar](255) NULL,
	[Created] [varchar](255) NULL,
	[Customer Total Score] [varchar](255) NULL,
	[Financial Total Score] [varchar](255) NULL,
	[Internal Process TotalScore] [varchar](255) NULL,
	[Learning and Growth TotalScore] [varchar](255) NULL,
	[Workflow Status] [varchar](255) NULL,
	[InternalProcess_Objective] [varchar](max) NULL,
	[Customer_Objective] [varchar](max) NULL,
	[Financial_Objective] [varchar](max) NULL,
	[Learning and Growth_Objective] [varchar](max) NULL,
	[Job Title] [varchar](255) NULL,
	[Item Type] [varchar](255) NULL,
	[Path] [varchar](255) NULL
) ON [PRIMARY]



SET ANSI_PADDING OFF



