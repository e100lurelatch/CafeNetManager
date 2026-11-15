DEMO DEMO DEMO, ĐỂ NHÁP THÔI chưa sửa
# ☕ CafeNetManager — Quản Lý Cà Phê

![Framework](https://shields.io)
![Architecture](https://shields.io)
![UI](https://shields.io)
![Database](https://shields.io)

Đồ án môn học: Ứng dụng desktop quản lý quán cà phê — quản lý danh mục món ăn/bàn ăn/khách hàng, gọi món trực quan, tính tiền xuất hóa đơn và báo cáo doanh thu chi tiết.

## Mục lục

* [Tính năng](#-tính-năng)
* [Công nghệ](#-công-nghệ)
* [Cài đặt và chạy](#-cài-đặt-và-chạy)
* [Tài khoản demo](#-tài-khoản-demo)

---

## Tính năng
  
| Nhóm | Nội dung |
| :--- | :--- |
| **Danh mục** | Quản lý loại đồ uống, món ăn, khách hàng, bàn ăn (CRUD + tìm kiếm) |
| **Gọi món** | Chọn món trực quan theo sơ đồ bàn, tùy chỉnh topping, chuyển/tách/gộp hóa đơn |
| **Thanh toán** | Tính tiền tự động, áp dụng mã giảm giá, xuất hóa đơn ra file `.txt` hoặc in tạm tính |
| **Trực quan** | Sơ đồ phòng bàn đổi màu động theo trạng thái trống/đang có khách |
| **Báo cáo** | Thống kê doanh thu, số lượng món bán ra theo khoảng ngày |

**Luồng nghiệp vụ khép kín:** Đăng nhập → Sơ đồ bàn → Gọi món → Vào bếp → Phục vụ → Thanh toán / Hóa đơn → Báo cáo doanh thu.


## Công nghệ

* **Framework:** .NET 10.0 / WPF (Windows Presentation Foundation)
* **Thư viện UI:** WPF-UI / MaterialDesignInXaml
* **Cơ sở dữ liệu:** Microsoft SQL Server

## Cài đặt và chạy

1. Clone repository này về máy cá nhân.
2. Khởi tạo cơ sở dữ liệu bằng cách chạy file script `.sql` đi kèm trong SQL Server.
3. Thay đổi chuỗi kết nối kết nối `ConnectionString` trong file `App.config`.
4. Mở giải pháp (`.sln`) bằng Visual Studio và nhấn `F5` để chạy.

## Tài khoản demo

* **Tài khoản:** `Admin`
* **Mật khẩu:** `123456`

## Thành viên

| Thành viên | Phụ trách |
| :--- | :--- |
| Nguyễn Đăng Dương | • Xây dựng tính năng quản lý nhân viên<br>• Xây dựng tính năng báo cáo thống kê doanh thu |
| Võ Anh Quân       | • Xây dựng tính năng bảng điều khiển admin<br> • Xây dựng tính năng quản lý món ăn<br>• Xây dựng tính năng quản lý nguồn hàng |
| Hà Anh Bằng       | • Xây dựng tính năng quản lý thông tin admin<br>• Xây dựng tính năng quản lý thông tin quy định |
| Lê Đức Bảo        | • Xây dựng tính năng quản lý bàn ăn, thanh toán<br>• Xây dựng tính năng quản lý ưu đãi  |
