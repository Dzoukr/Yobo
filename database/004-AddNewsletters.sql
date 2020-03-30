ALTER TABLE dbo.Users ADD
	Newsletters bit NOT NULL CONSTRAINT DF_Users_Newsletters DEFAULT 0