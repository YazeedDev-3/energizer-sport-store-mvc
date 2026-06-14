# Energizer Sport

A full-stack sports supplement e-commerce app built with ASP.NET Core MVC. Includes a customer storefront and an admin panel.

## Tech Stack

| Layer     | Technology                              |
| --------- | --------------------------------------- |
| Framework | ASP.NET Core MVC 8.0 (.NET 8)           |
| ORM       | Entity Framework Core 8.0.11            |
| Database  | SQL Server LocalDB (MDF file)           |
| Auth      | Session-based, BCrypt.Net-Next 4.2.0    |
| UI        | Bootstrap 5.1.0, Bootstrap Icons 1.11.3 |
| JS        | jQuery, Bootstrap Bundle                |

## Features

### Customer

- Browse and search products
- Add to cart with quantity control
- Checkout and view order history
- Special offers page
- Login / Register (guests see only catalogue)

### Admin

- Dashboard with live stats and category chart
- Full product CRUD with image upload
- Full user CRUD with search by username and role
- View any customer's orders
- Send email via Gmail SMTP

## Default Accounts

| Username | Password | Role     |
| -------- | -------- | -------- |
| admin    | 1234     | Admin    |
| customer | 1234     | Customer |

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server LocalDB

### Run

```bash
git clone https://github.com/YazeedDev-3/energizer-sport-store-mvc.git
cd project-web2
dotnet run
```

Go to http://localhost:5249. The database is included in `App_Data/` and seeds automatically on first run.

### Email (optional)

Fill in `appsettings.json` to enable the Send Email feature:

```json
"Email": {
  "SenderAddress": "your@gmail.com",
  "Password": "your-app-password"
}
```

Use a Gmail App Password, not your account password.

## Project Structure

```
project-web2/
├── App_Data/               # SQL Server database files
├── Controllers/
│   ├── BaseController.cs
│   ├── HomeController.cs
│   ├── itemsController.cs
│   ├── ordersController.cs
│   └── useraccountsController.cs
├── Data/
│   ├── DbInitializer.cs
│   └── project_web2Context.cs
├── Models/
├── Views/
│   ├── items/
│   ├── orders/
│   ├── useraccounts/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   ├── images/
│   ├── js/
│   └── lib/
├── appsettings.json
├── Program.cs
└── project-web2.csproj
```
