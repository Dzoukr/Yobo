ALTER TABLE dbo.Lessons ADD	CancellableBeforeStart time(7) NULL;
GO
UPDATE dbo.Lessons SET CancellableBeforeStart = '03:00:00';
GO
ALTER TABLE dbo.Lessons ALTER COLUMN CancellableBeforeStart time(7) NOT NULL;
GO