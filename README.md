# kurofinance

---

## Stack

- **Backend:** ASP.NET Core MVC (.NET 10)
- **Database:** PostgreSQL (Supabase) via Npgsql + EF Core
- **Auth:** Cookie-based (BCrypt password hashing) + Google OAuth 2.0
- **Frontend:** Razor Views, Bootstrap 5.3, vanilla JS (fetch API)
- **Export:** ClosedXML (Excel)

---

## Project structure

```
kuro-finance/
├── KuroFinance.slnx
├── KuroFinance.Data/
│   ├── AppDbContext.cs
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Category.cs
│   │   └── Transaction.cs
│   ├── Repositories/
│   │   ├── Interfaces/
│   │   │   ├── IUserRepository.cs
│   │   │   ├── ICategoryRepository.cs
│   │   │   └── ITransactionRepository.cs
│   │   ├── UserRepository.cs
│   │   ├── CategoryRepository.cs
│   │   └── TransactionRepository.cs
│   └── Migrations/
└── KuroFinance.Web/
    ├── Program.cs
    ├── appsettings.json
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── DashboardController.cs
    │   ├── TransactionController.cs
    │   ├── CategoryController.cs
    │   └── ExportController.cs
    ├── Models/
    │   ├── LoginViewModel.cs
    │   ├── RegisterViewModel.cs
    │   ├── DashboardViewModel.cs
    │   ├── TransactionFormViewModel.cs
    │   ├── TransactionListViewModel.cs
    │   └── CategoryFormViewModel.cs
    ├── Views/
    │   ├── Auth/         (Login, Register)
    │   ├── Dashboard/    (Index)
    │   ├── Transaction/  (Index, _TransactionTable)
    │   ├── Category/     (Index)
    │   ├── Export/       (Index)
    │   └── Shared/       (_Layout, _AuthLayout)
    └── wwwroot/
        ├── css/app.css
        └── js/
```

---

## Data model

**User**
- Id (Guid), Name, Email, PasswordHash (nullable), GoogleId (nullable), CreatedAt

**Category**
- Id (Guid), Name, Type (Income | Expense), UserId

**Transaction**
- Id (Guid), Description, Amount, Type, Date, CreatedAt, UserId, CategoryId

Categories and transactions are always scoped to the authenticated user.

---

## Running locally

**Requirements:** .NET 10 SDK, accessible PostgreSQL

**1. Set the connection string and Google OAuth credentials via user secrets:**
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string" --project KuroFinance.Web
dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id" --project KuroFinance.Web
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-client-secret" --project KuroFinance.Web
```

**2. Apply migrations:**
```bash
dotnet ef database update --project KuroFinance.Data --startup-project KuroFinance.Web
```

**3. Run:**
```bash
dotnet run --project KuroFinance.Web
```

---

## Deploy (SquareCloud)

**Startup command:**
```
dotnet publish "KuroFinance.Web/KuroFinance.Web.csproj" -c Release -o out && cd out && dotnet KuroFinance.Web.dll
```
