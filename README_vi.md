
# 📖 Tài liệu Dự án Efood-Menu

Chào mừng bạn đến với kho lưu trữ dự án Efood-Menu!  
Tài liệu này cung cấp tổng quan, hướng dẫn cài đặt và sử dụng ứng dụng web quản lý thực đơn và đặt món cho nhà hàng.

## Tính năng
- Đăng nhập, phân quyền người dùng
- Quản lý danh mục, món ăn, bàn, đơn hàng (khu vực Admin)
- Giỏ hàng và thanh toán
- Giao diện hiện đại, responsive với Bootstrap
- Tải ảnh cho món ăn
- Quản lý voucher và đặt bàn
- Cấu trúc dự án rõ ràng, dễ mở rộng

## Bắt đầu

1. **Sao chép kho lưu trữ**
   ```sh
   git clone https://github.com/your-username/Efood-Menu.git
   ```
2. **Cài đặt các phụ thuộc**
   - Mở solution bằng Visual Studio.
   - Khôi phục các gói NuGet (tự động hoặc dùng `dotnet restore`).
   - Đảm bảo có SQL Server hoặc cập nhật chuỗi kết nối trong `appsettings.json`.
3. **Cập nhật cơ sở dữ liệu**
   - Dùng Package Manager Console:
     ```
     Update-Database
     ```
   - Hoặc dùng CLI:
     ```
     dotnet ef database update
     ```
4. **Chạy ứng dụng**
   - Nhấn F5 trong Visual Studio hoặc chạy:
     ```
     dotnet run
     ```

## Cấu trúc thư mục

- `Areas/Admin`: Controller và view cho Admin
- `Controllers`: Controller cho trang chính
- `Models`: Các lớp Entity và ViewModel
- `Repositories`: Lớp truy cập dữ liệu
- `Views`: Razor view cho giao diện
- `wwwroot`: File tĩnh (CSS, JS, hình ảnh)

## Đóng góp

Hãy mở issue hoặc gửi pull request nếu bạn muốn đóng góp cho dự án.
