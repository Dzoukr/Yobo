ALTER TABLE dbo.Lessons ADD	Capacity int NULL;
GO
UPDATE dbo.Lessons SET Capacity = 12;
GO
ALTER TABLE dbo.Lessons ALTER COLUMN Capacity int NOT NULL;
GO
