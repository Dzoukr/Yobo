GO
/****** Object:  Table [dbo].[LessonReservations]    Script Date: 25.01.2019 14:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LessonReservations](
	[LessonId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Count] [int] NOT NULL,
	[Created] [datetimeoffset](7) NOT NULL,
	[UseCredits] [bit] NOT NULL,
 CONSTRAINT [PK_LessonReservations_1] PRIMARY KEY CLUSTERED 
(
	[LessonId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Lessons]    Script Date: 25.01.2019 14:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Lessons](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[StartDate] [datetimeoffset](7) NOT NULL,
	[EndDate] [datetimeoffset](7) NOT NULL,
	[Created] [datetimeoffset](7) NOT NULL,
	[IsCancelled] [bit] NOT NULL,
 CONSTRAINT [PK_Lessons] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 25.01.2019 14:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [uniqueidentifier] NOT NULL,
	[Email] [nvarchar](500) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](80) NOT NULL,
	[ActivationKey] [uniqueidentifier] NOT NULL,
	[Registered] [datetimeoffset](7) NOT NULL,
	[Activated] [datetimeoffset](7) NULL,
	[Credits] [int] NOT NULL,
	[CreditsExpiration] [datetimeoffset](7) NULL,
	[CashReservationBlockedUntil] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_Users]    Script Date: 25.01.2019 14:37:14 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users] ON [dbo].[Users]
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[LessonReservations]  WITH CHECK ADD  CONSTRAINT [FK_LessonReservations_Lessons] FOREIGN KEY([LessonId])
REFERENCES [dbo].[Lessons] ([Id])
GO
ALTER TABLE [dbo].[LessonReservations] CHECK CONSTRAINT [FK_LessonReservations_Lessons]
GO
ALTER TABLE [dbo].[LessonReservations]  WITH CHECK ADD  CONSTRAINT [FK_LessonReservations_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[LessonReservations] CHECK CONSTRAINT [FK_LessonReservations_Users]
GO
