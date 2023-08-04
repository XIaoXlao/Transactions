CREATE TABLE [dbo].[Transactions](	
	[TransNo] [varchar](50) NOT NULL,
	[Amount] [decimal](18, 0) NOT NULL,
	[CurrencyCode] [char](3) NOT NULL,
	[TransDate] [datetime] NOT NULL,
	[Status] [varchar](20) NOT NULL
) ON [PRIMARY]
GO