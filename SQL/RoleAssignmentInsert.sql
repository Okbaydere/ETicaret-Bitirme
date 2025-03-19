-- Kullanıcılara rol atama
INSERT INTO [ETicaret].[dbo].[AspNetUserRoles] ([UserId],
[RoleId])
VALUES
    (1, 1), -- 1 ID'li kullanıcıya Admin rolü (ID=1)
    (2, 2), -- 2 ID'li kullanıcıya User rolü (ID=2)
    (3, 2), -- 3 ID'li kullanıcıya User rolü
    (4, 2), -- 4 ID'li kullanıcıya User rolü
    (5, 2); -- 5 ID'li kullanıcıya User rolü