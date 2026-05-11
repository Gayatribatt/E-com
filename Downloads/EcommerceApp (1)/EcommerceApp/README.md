# ShopWave - ASP.NET Core E-Commerce Application

A fully-featured e-commerce web application built with ASP.NET Core 8 MVC.

## Features

- **Product Catalog** - Browse products by category, search, sort, and paginate
- **Shopping Cart** - Add/remove items, update quantities (session-based for guests, DB-based for users)
- **User Authentication** - Register, login, logout via ASP.NET Core Identity
- **Checkout Flow** - Full checkout with shipping address collection
- **Order Management** - View order history and details
- **Database Seeding** - Auto-seeded with 5 categories and 13 sample products
- **Stripe Ready** - Payment integration scaffolded (add API keys to activate)
- **Responsive Design** - Bootstrap 5 with custom CSS

## Tech Stack

- ASP.NET Core 8 MVC
- Entity Framework Core + SQLite (easily swappable to SQL Server)
- ASP.NET Core Identity
- Bootstrap 5 + Font Awesome 6
- Stripe.net (payment processing)

## Getting Started

### Prerequisites
- .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0

### Run the Application

```bash
cd EcommerceApp

# Restore packages
dotnet restore

# Apply migrations and run
dotnet run
```

The app will be available at `https://localhost:5001` or `http://localhost:5000`.

The database is auto-created and seeded on first run.

### Configuration

Edit `appsettings.json` to configure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ecommerce.db"
  },
  "Stripe": {
    "PublishableKey": "pk_test_YOUR_KEY",
    "SecretKey": "sk_test_YOUR_KEY"
  }
}
```

** SQL Server:**
 `UseSqlServer` in `Program.cs` and update the connection string.

## Project Structure

```
EcommerceApp/
├── Controllers/
│   └── Controllers.cs          # Home, Products, Cart, Checkout, Orders
├── Data/
│   └── ApplicationDbContext.cs  # EF Core context + DB seeder
├── Models/
│   └── Models.cs               # All models + view models
├── Services/
│   └── Services.cs             # ProductService, CartService, OrderService
├── Views/
│   ├── Home/Index.cshtml        # Landing page with featured products
│   ├── Products/                # Product list + details
│   ├── Cart/Index.cshtml        # Shopping cart
│   ├── Checkout/                # Checkout + confirmation
│   ├── Orders/                  # Order history
│   └── Shared/_Layout.cshtml   # Main layout
├── wwwroot/
│   ├── css/site.css
│   └── js/site.js
├── Program.cs
├── appsettings.json
└── EcommerceApp.csproj
```

## Adding Identity UI Pages

Run this to scaffold full Identity pages (login, register, profile, etc.):

```bash
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
dotnet aspnet-codegenerator identity -dc EcommerceApp.Data.ApplicationDbContext --files "Account.Login;Account.Register"
```

## Extending the Project

- **Admin Panel**: Add an `Admin` area with `[Authorize(Roles = "Admin")]`
- **Product Images**: Integrate image upload with Azure Blob Storage or local storage
- **Wishlist**: Add a `Wishlist` model similar to `Cart`
- **Email Notifications**: Add SendGrid for order confirmations
- **Real Payments**: Configure Stripe webhook to handle `payment_intent.succeeded`
