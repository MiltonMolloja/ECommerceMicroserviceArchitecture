Sqlcmd: 'Identity\".\"AspNetUsers\" VALUES (''' + REPLACE(Id, '''', '''''') + ''', ' +
    CASE WHEN UserName IS NULL THEN 'NULL' ELSE '''' + REPLACE(UserName, '''', '''''') + '''' END + ', ' +
    CASE WHEN NormalizedUserName IS NULL THEN 'NULL' ELSE '''' + REPLACE(NormalizedUserName, '''', '''''') + '''' END + ', ' +
    CASE WHEN Email IS NULL THEN 'NULL' ELSE '''' + REPLACE(Email, '''', '''''') + '''' END + ', ' +
    CASE WHEN NormalizedEmail IS NULL THEN 'NULL' ELSE '''' + REPLACE(NormalizedEmail, '''', '''''') + '''' END + ', ' +
    CASE WHEN EmailConfirmed = 1 THEN 'true' ELSE 'false' END + ', ' +
    CASE WHEN PasswordHash IS NULL THEN 'NULL' ELSE '''' + REPLACE(PasswordHash, '''', '''''') + '''' END + ', ' +
    CASE WHEN SecurityStamp IS NULL THEN 'NULL' ELSE '''' + REPLACE(SecurityStamp, '''', '''''') + '''' END + ', ' +
    CASE WHEN ConcurrencyStamp IS NULL THEN 'NULL' ELSE '''' + REPLACE(ConcurrencyStamp, '''', '''''') + '''' END + ', ' +
    CASE WHEN PhoneNumber IS NULL THEN 'NULL' ELSE '''' + REPLACE(PhoneNumber, '''', '''''') + '''' END + ', ' +
    CASE WHEN PhoneNumberConfirmed = 1 THEN 'true' ELSE 'false' END + ', ' +
    CASE WHEN TwoFactorEnabled = 1 THEN 'true' ELSE 'false' END + ', ' +
    CASE WHEN LockoutEnd IS NULL THEN 'NULL' ELSE '''' + CONVERT(VARCHAR(30), LockoutEnd, 126) + '''' END + ', ' +
    CASE WHEN LockoutEnabled = 1 THEN 'true' ELSE 'false' END + ', ' +
    CAST(AccessFailedCount AS VARCHAR) + ', ' +
    '''' + REPLACE(FirstName, '''', '''''') + ''', ' +
    '''' + REPLACE(LastName, '''', '''''') + ''', ' +
    CASE WHEN PasswordChangedAt IS NULL THEN 'NULL' ELSE '''' + CONVERT(VARCHAR(30), PasswordChangedAt, 126) + '''' END + ');'
FROM [Identity].AspNetUsers
" -E -C -W -h -1': Unexpected argument. Enter '-?' for help.
