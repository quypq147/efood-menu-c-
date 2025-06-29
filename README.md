
[English](https://github.com/quypq147/efood-menu-c-/blob/main/README.md)     [Vietnamese](https://github.com/quypq147/efood-menu-c-/blob/main/README_vi.md)
# 📖 Efood-Menu Project Documentation

## English

Welcome to the Efood-Menu project repository!  
This document provides an overview, setup instructions, and usage guidelines for the restaurant menu and ordering web application.

### Features
- User authentication and authorization
- Manage food categories, items, tables, and orders (Admin area)
- Shopping cart and checkout functionality
- Responsive UI with Bootstrap
- Image upload for food items
- Voucher and reservation management
- Modern, clear project structure

### Getting Started

1. **Clone the repository**
   ```sh
   git clone https://github.com/your-username/Efood-Menu.git
   ```
2. **Install dependencies**
   - Open the solution in Visual Studio.
   - Restore NuGet packages (automatically or via `dotnet restore`).
   - Ensure you have SQL Server or update the connection string in `appsettings.json`.
3. **Apply database migrations**
   - Use the Package Manager Console:
     ```
     Update-Database
     ```
   - Or use CLI:
     ```
     dotnet ef database update
     ```
4. **Run the application**
   - Press F5 in Visual Studio or run:
     ```
     dotnet run
     ```

### Folder Structure

- `Areas/Admin`: Admin controllers and views
- `Controllers`: Main site controllers
- `Models`: Entity and ViewModel classes
- `Repositories`: Data access layer
- `Views`: Razor views for UI
- `wwwroot`: Static files (CSS, JS, images)

### Contribution

Feel free to open issues or submit pull requests to improve the project.

### Đóng góp

Hãy mở issue hoặc gửi pull request nếu bạn muốn đóng góp cho dự án.


