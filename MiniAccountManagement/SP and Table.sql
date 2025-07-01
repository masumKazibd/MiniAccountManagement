
----Create Role 
USE MiniAccountManagement
CREATE PROCEDURE CreateRole
    @RoleName NVARCHAR(256)
AS
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName)
    VALUES (NEWID(), @RoleName, UPPER(@RoleName))
END

--- Delete Role
CREATE PROCEDURE DeleteRole
    @RoleId NVARCHAR(450)
AS
BEGIN
    DELETE FROM AspNetRoles WHERE Id = @RoleId
END

--- Update Role

CREATE PROCEDURE UpdateRole
    @RoleId NVARCHAR(450),
    @RoleName NVARCHAR(256)
AS
BEGIN
    UPDATE AspNetRoles
    SET Name = @RoleName
    WHERE Id = @RoleId;
END

--- Assign UserROle

CREATE PROCEDURE AssignUserRole
    @UserId NVARCHAR(450),
    @RoleId NVARCHAR(450)
AS
BEGIN
    DELETE FROM AspNetUserRoles WHERE UserId = @UserId;

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);
END


-- Create the table to hold account information
CREATE TABLE dbo.ChartOfAccounts (
    AccountId INT IDENTITY(1,1) PRIMARY KEY,
    AccountCode NVARCHAR(20) NOT NULL UNIQUE,
    AccountName NVARCHAR(100) NOT NULL,
    ParentAccountId INT NULL, -- This is the foreign key to itself for the hierarchy
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_ChartOfAccounts_Parent FOREIGN KEY (ParentAccountId) REFERENCES dbo.ChartOfAccounts(AccountId)
);
GO

