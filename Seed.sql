/****** Object:  Table [dbo].[FultonDocuments]    Script Date: 2020-04-11 2:53:46 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FultonDocuments](
	[ID] [int] NOT NULL,
	[IndexID] [int] NOT NULL,
	[Norm] [float] NOT NULL,
	[Checksum] binary(32) NOT NULL
) ON [PRIMARY]
GO

/****** Object:  Index [FultonDocuments_IndexID_ID]    Script Date: 2020-04-20 9:24:19 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [FultonDocuments_IndexID_ID] ON [dbo].[FultonDocuments]
(
	[IndexID] ASC,
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[FultonTermFrequencies]    Script Date: 2020-04-11 2:54:07 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FultonTermFrequencies](
	[IndexID] [int] NOT NULL,
	[DocumentID] [int] NOT NULL,
	[Token] [varchar](255) NOT NULL,
	[Frequency] [float] NOT NULL
) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [FultonTermFrequencies_Clustered]    Script Date: 2020-04-20 9:24:44 PM ******/
CREATE UNIQUE CLUSTERED INDEX [FultonTermFrequencies_Clustered] ON [dbo].[FultonTermFrequencies]
(
	[IndexID] ASC,
	[DocumentID] ASC,
	[Token] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
