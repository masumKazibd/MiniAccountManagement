
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



-- Stored Procedure to GET all active accounts
CREATE PROCEDURE dbo.sp_GetChartOfAccounts
AS
BEGIN
    SELECT AccountId, AccountCode, AccountName, ParentAccountId
    FROM dbo.ChartOfAccounts
    WHERE IsActive = 1;
END
GO


-- Stored Procedure to MANAGE (Create, Update, Delete) accounts
CREATE PROCEDURE dbo.sp_ManageChartOfAccounts
    @Action NVARCHAR(10), -- 'CREATE', 'UPDATE', 'DELETE'
    @AccountId INT = NULL,
    @AccountCode NVARCHAR(20) = NULL,
    @AccountName NVARCHAR(100) = NULL,
    @ParentAccountId INT = NULL
AS
BEGIN
    IF @Action = 'CREATE'
    BEGIN
        INSERT INTO dbo.ChartOfAccounts (AccountCode, AccountName, ParentAccountId)
        VALUES (@AccountCode, @AccountName, @ParentAccountId);
    END
    ELSE IF @Action = 'UPDATE'
    BEGIN
        UPDATE dbo.ChartOfAccounts
        SET AccountCode = @AccountCode,
            AccountName = @AccountName,
            ParentAccountId = @ParentAccountId
        WHERE AccountId = @AccountId;
    END
    ELSE IF @Action = 'DELETE'
    BEGIN
        UPDATE dbo.ChartOfAccounts
        SET IsActive = 0
        WHERE AccountId = @AccountId;
    END
END
GO


-- Create the AccessRights table to link Roles to Modules
CREATE TABLE dbo.AccessRights (
    AccessRightId INT IDENTITY(1,1) PRIMARY KEY,
    RoleId NVARCHAR(450) NOT NULL,
    ModuleName NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_AccessRights_Roles FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_AccessRights UNIQUE (RoleId, ModuleName)
);
GO

-- Create a Table-Valued Parameter (TVP) type to pass a list of modules
CREATE TYPE dbo.ModuleListType AS TABLE (
    ModuleName NVARCHAR(100)
);
GO

-- Stored Procedure to GET all permissions for a specific role
CREATE PROCEDURE dbo.sp_GetRolePermissions
    @RoleId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ModuleName 
    FROM dbo.AccessRights 
    WHERE RoleId = @RoleId;
END
GO

-- Stored Procedure to UPDATE all permissions for a specific role
CREATE PROCEDURE dbo.sp_UpdateRolePermissions
    @RoleId NVARCHAR(450),
    @Modules dbo.ModuleListType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    -- First, delete all existing permissions for this role
    DELETE FROM dbo.AccessRights 
    WHERE RoleId = @RoleId;

    -- Then, insert the new set of permissions from the list provided
    INSERT INTO dbo.AccessRights (RoleId, ModuleName)
    SELECT @RoleId, ModuleName 
    FROM @Modules;

    COMMIT TRANSACTION;
END
GO
---- CHeck user permission

CREATE PROCEDURE dbo.sp_CheckUserPermission
    @UserId NVARCHAR(450),
    @ModuleName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if any of the user's roles have an entry in AccessRights for the given module.
    IF EXISTS (
        SELECT 1
        FROM dbo.AspNetUserRoles ur
        JOIN dbo.AccessRights ar ON ur.RoleId = ar.RoleId
        WHERE ur.UserId = @UserId AND ar.ModuleName = @ModuleName
    )
        SELECT 1; -- User has permission
    ELSE
        SELECT 0; -- User does not have permission
END
GO