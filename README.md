# Mini Account Management System

This project is a comprehensive Mini Account Management System developed as a task for Qtec Solution Limited. It is built using ASP.NET Core Razor Pages with a database-first approach, emphasizing the use of raw SQL and stored procedures for all data manipulation, as per the project requirements.

## Technologies Used
- **Backend:** ASP.NET Core 8 with Razor Pages
- **Database:** MS SQL Server
- **Data Access:** Dapper (for calling stored procedures) & ADO.NET
- **Authentication:** ASP.NET Core Identity
- **Authorization:** Custom Role-Based & Policy-Based Authorization
- **Frontend:** Bootstrap 5, HTML5, CSS3, JavaScript
- **Excel Export:** ClosedXML

## Core Features

### 1. Authentication & Authorization
A robust security system built on ASP.NET Core Identity.

- **User Roles:** Three distinct roles (Admin, Accountant, Viewer) with different levels of access.
- **Module Permissions:** Admins can assign access rights for specific application modules (e.g., "ChartOfAccounts", "VoucherEntry") to each role.
- **Custom Authorization Policy:** A custom policy (HasModulePermission) enforces these rights, ensuring users can only access the modules they are permitted to.

**Screenshots:**
- Login Page:
- Role Management Page:
- Permissions Management Page:

### 2. Chart of Accounts
A hierarchical system for managing financial accounts.

- **CRUD Operations:** Full Create, Read, Update, and Delete functionality for all accounts.
- **Hierarchical Tree View:** Accounts are displayed in a collapsible parent-child tree structure for easy navigation.
- **Excel Export:** The entire Chart of Accounts can be exported to an Excel file with a single click.

**Screenshot:**

### 3. Voucher Entry Module
A dynamic form for creating Journal, Payment, and Receipt vouchers.

- **Multi-Line Entries:** Users can dynamically add multiple debit and credit rows to a single voucher.
- **Real-time Totals:** The form automatically calculates and displays the total debits and credits as the user types.
- **Validation:** The system ensures that total debits must equal total credits before a voucher can be saved.
- **Efficient Database Saving:** All voucher data (header and multiple detail lines) is saved in a single, atomic transaction using a stored procedure with a Table-Valued Parameter (TVP).

**Screenshot:**

## Setup and Installation
Follow these steps to run the project locally.

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022
- MS SQL Server (or SQL Server Express)

### 1. Clone the Repository
```bash
git clone https://github.com/your-username/your-repository-name.git
cd your-repository-name
```
### 2. Database Setup
- Open MS SQL Server Management Studio (SSMS).
- Create a new, empty database (e.g., MiniAccountManagementDB).
- Open the appsettings.json file in the project and update the DefaultConnection connection string to point to your new database.
- Run all the SQL scripts located in the /DatabaseScripts folder against your database. This will create the necessary tables, types, and stored procedures.

### 3. Run the Application
- Open the solution file (.sln) in Visual Studio 2022.
- The first time you run the application, the ASP.NET Identity tables will be automatically created in your database by Entity Framework Core.
- The application will also seed the database with default roles ("Admin", "Accountant", "Viewer") and a default admin user.

### Default Admin User
You can log in with the following credentials to access all features:

- Email: ```admin@example.com```
- Password: ```admin123```
