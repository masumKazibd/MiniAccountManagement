
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

