BEGIN TRANSACTION;
ALTER TABLE [Cases] DROP CONSTRAINT [FK_Cases_Employees_EmployeeId];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cases]') AND [c].[name] = N'CaseDate');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Cases] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Cases] DROP COLUMN [CaseDate];

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cases]') AND [c].[name] = N'EndTime');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Cases] DROP CONSTRAINT ' + @var1 + ';');
UPDATE [Cases] SET [EndTime] = '0001-01-01T00:00:00.0000000' WHERE [EndTime] IS NULL;
ALTER TABLE [Cases] ALTER COLUMN [EndTime] datetime2 NOT NULL;
ALTER TABLE [Cases] ADD DEFAULT '0001-01-01T00:00:00.0000000' FOR [EndTime];

DROP INDEX [IX_Cases_EmployeeId] ON [Cases];
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cases]') AND [c].[name] = N'EmployeeId');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Cases] DROP CONSTRAINT ' + @var2 + ';');
UPDATE [Cases] SET [EmployeeId] = 0 WHERE [EmployeeId] IS NULL;
ALTER TABLE [Cases] ALTER COLUMN [EmployeeId] int NOT NULL;
ALTER TABLE [Cases] ADD DEFAULT 0 FOR [EmployeeId];
CREATE INDEX [IX_Cases_EmployeeId] ON [Cases] ([EmployeeId]);

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cases]') AND [c].[name] = N'CaseName');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Cases] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [Cases] ALTER COLUMN [CaseName] nvarchar(200) NOT NULL;

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cases]') AND [c].[name] = N'CaseDescription');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Cases] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [Cases] ALTER COLUMN [CaseDescription] nvarchar(1000) NOT NULL;

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cases]') AND [c].[name] = N'BeginTime');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Cases] DROP CONSTRAINT ' + @var5 + ';');
UPDATE [Cases] SET [BeginTime] = '0001-01-01T00:00:00.0000000' WHERE [BeginTime] IS NULL;
ALTER TABLE [Cases] ALTER COLUMN [BeginTime] datetime2 NOT NULL;
ALTER TABLE [Cases] ADD DEFAULT '0001-01-01T00:00:00.0000000' FOR [BeginTime];

ALTER TABLE [Cases] ADD CONSTRAINT [FK_Cases_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260505104104_Seeding', N'10.0.7');

COMMIT;
GO

