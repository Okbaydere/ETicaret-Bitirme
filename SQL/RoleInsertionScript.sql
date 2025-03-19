-- IDENTITY_INSERT'i açın
SET
IDENTITY_INSERT [ETicaret].[dbo].[AspNetRoles] ON;

-- Rolleri ekleyin
INSERT INTO [ETicaret].[dbo].[AspNetRoles] ([Id],
    [Name],
    [NormalizedName],
[ConcurrencyStamp])
VALUES
    (1, 'Admin', 'ADMIN', 'f8025fee-dec6-4528-9ca2-82cf8fb996b1'), (2, 'User', 'USER', 'c9609fee-b01a-46e5-a5c4-f33cb8340e1c');

-- IDENTITY_INSERT'i kapatın
SET
IDENTITY_INSERT [ETicaret].[dbo].[AspNetRoles] OFF;