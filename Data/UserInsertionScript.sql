-- IDENTITY_INSERT'i açın
SET
IDENTITY_INSERT [ETicaret].[dbo].[AspNetUsers] ON;

-- Kullanıcıları ekleyin
INSERT INTO [ETicaret].[dbo].[AspNetUsers] ([Id],
    [FirstName],
    [LastName],
    [UserName],
    [NormalizedUserName],
    [Email],
    [NormalizedEmail],
    [EmailConfirmed],
    [PasswordHash],
    [SecurityStamp],
    [ConcurrencyStamp],
    [PhoneNumber],
    [PhoneNumberConfirmed],
    [TwoFactorEnabled],
    [LockoutEnabled],
[AccessFailedCount])
VALUES
    (
    1,                                                                                      -- Id (INT)
    'Ahmet',                                                                                -- FirstName
    'Yılmaz',                                                                               -- LastName
    'ahmetyilmaz',                                                                          -- UserName
    'AHMETYILMAZ',                                                                          -- NormalizedUserName
    'ahmet.yilmaz@example.com',                                                             -- Email
    'AHMET.YILMAZ@EXAMPLE.COM',                                                             -- NormalizedEmail
    1,                                                                                      -- EmailConfirmed
    'AQAAAAEAACcQAAAAEHxRvTAVwGMUwz4GkGP7EUiQJh0YtGXnW0vh7SQG+HSobOT4WLzWJxFexZ7VuS8FgQ==', -- PasswordHash
    'UCKPDS5R4XEOANLRWYEBI5YTLCUX35RD',                                                     -- SecurityStamp
    'c9609fee-b01a-46e5-a5c4-f33cb8340e1c',                                                 -- ConcurrencyStamp
    '5551234567',                                                                           -- PhoneNumber
    0,                                                                                      -- PhoneNumberConfirmed
    0,                                                                                      -- TwoFactorEnabled
    1,                                                                                      -- LockoutEnabled
    0                                                                                       -- AccessFailedCount
    ), (
    2,                                                                                      -- Id (INT)
    'Ayşe',                                                                                 -- FirstName
    'Kara',                                                                                 -- LastName
    'aysekarauser',                                                                         -- UserName
    'AYSEKARAUSER',                                                                         -- NormalizedUserName
    'ayse.kara@example.com',                                                                -- Email
    'AYSE.KARA@EXAMPLE.COM',                                                                -- NormalizedEmail
    1,                                                                                      -- EmailConfirmed
    'AQAAAAEAACcQAAAAEJ/DTCAwOJ+9Q7H/l19KI38XMNewZOeO95qRh61D1vl/v1tgI9vXxZAhX7Ri133ytw==', -- PasswordHash
    'DMXF6LPOFKYSOGVS5UWPPFCXWMVTRKL2',                                                     -- SecurityStamp
    'b8f90a05-be01-4b3f-8700-893123cdef01',                                                 -- ConcurrencyStamp
    '5559876543',                                                                           -- PhoneNumber
    0,                                                                                      -- PhoneNumberConfirmed
    0,                                                                                      -- TwoFactorEnabled
    1,                                                                                      -- LockoutEnabled
    0                                                                                       -- AccessFailedCount
    ), (
    3,                                                                                      -- Id (INT)
    'Mehmet',                                                                               -- FirstName
    'Demir',                                                                                -- LastName
    'mehmetdemir',                                                                          -- UserName
    'MEHMETDEMIR',                                                                          -- NormalizedUserName
    'mehmet.demir@example.com',                                                             -- Email
    'MEHMET.DEMIR@EXAMPLE.COM',                                                             -- NormalizedEmail
    1,                                                                                      -- EmailConfirmed
    'AQAAAAEAACcQAAAAECL8fFqqGa4Bj3xpIu2BOWyMTIDktXyMiU9u/4OB+2MYYDFrD1vf+lkiGB/VkBZb7g==', -- PasswordHash
    'RLPIC6H3YVPUDKRYCEFCERJ6QP42THOB',                                                     -- SecurityStamp
    'd5e123f7-a456-4b7c-8d90-123456abcdef',                                                 -- ConcurrencyStamp
    '5553456789',                                                                           -- PhoneNumber
    0,                                                                                      -- PhoneNumberConfirmed
    0,                                                                                      -- TwoFactorEnabled
    1,                                                                                      -- LockoutEnabled
    0                                                                                       -- AccessFailedCount
    );

-- IDENTITY_INSERT'i kapatın
SET
IDENTITY_INSERT [ETicaret].[dbo].[AspNetUsers] OFF;