# 🚗 Driving License Training System

**Driving License Training System** là ứng dụng web xây dựng trên nền tảng **ASP.NET Core MVC**, phục vụ quản lý đào tạo giấy phép lái xe (GPLX). Hệ thống hỗ trợ đầy đủ các nghiệp vụ cốt lõi gồm quản lý học viên, khóa học, kỳ thi, thanh toán và báo cáo.

Ứng dụng sử dụng **Entity Framework Core** với **SQL Server** làm cơ sở dữ liệu, cơ chế **cookie-based authentication** để xác thực người dùng, đồng thời tích hợp các dịch vụ hỗ trợ như:
- ✉️ Gửi email thông báo qua SMTP  
- 📄 Xuất báo cáo PDF  
- 🧑‍💻 Xác thực ảnh khuôn mặt bằng OpenCV  

🔗 **Repository**: https://github.com/ngoctrinh564/driving-license-training-system

---
## Table of Contents

1. [Overview](#-overview)
2. [Features](#-features)
3. [Project Structure](#%EF%B8%8F-project-structure)
4. [Technology Stack](#-technology-stack)
5. [Prerequisites](#%EF%B8%8F-prerequisites)
6. [Configuration](#-configuration)
7. [Database Setup](#%EF%B8%8F-database-setup)
8. [Getting Started](#-getting-started)
9. [Running the Application](#%EF%B8%8F-running-the-application)
10. [Note](#-note)
11. [Roadmap](#%EF%B8%8F-roadmap)
12. [The Dev Team](#-the-dev-team)
13. [Acknowledgements](#-acknowledgements)

---

## 📘 Overview

Hệ thống được thiết kế nhằm hỗ trợ **trung tâm đào tạo lái xe** quản lý toàn bộ vòng đời học viên, bao gồm các giai đoạn: **đăng ký**, **học tập**, **thi cử**, **cấp chứng chỉ** và **theo dõi tài chính**.

Ứng dụng cung cấp giao diện web xây dựng bằng **Razor Views**, backend triển khai theo mô hình **ASP.NET Core MVC**, đảm bảo **phân quyền rõ ràng** giữa người dùng thông thường và **quản trị viên**, giúp kiểm soát truy cập và vận hành hệ thống hiệu quả.

---

## ✨ Features

- 👤 **Quản lý học viên**: đăng ký tài khoản, quản lý hồ sơ cá nhân và theo dõi thông tin sức khỏe.  
- 📚 **Quản lý khóa học & hạng GPLX**: thiết lập lịch học, quản lý học phí và trạng thái thanh toán.  
- 📝 **Quản lý kỳ thi**: tạo và lên lịch kỳ thi, phân công giám thị, chấm điểm phần **lý thuyết** và **thực hành**.  
- 📊 **Dashboard quản trị**: tổng hợp dữ liệu và xuất **báo cáo PDF** về doanh thu, học viên, khóa học và kỳ thi.  
- ✉️ **Email thông báo**: gửi thông báo tự động cho học viên và quản trị viên thông qua SMTP.  
- 🧑‍💻 **Xác thực ảnh chân dung**: kiểm tra ảnh học viên bằng **OpenCV (Haar Cascade)** để đảm bảo tính hợp lệ.  
- 🔐 **Xác thực & phân quyền**: đăng nhập bằng cookie session, phân quyền truy cập theo **role** người dùng.  

---

## 🗂️ Project Structure

Cấu trúc thư mục chính của dự án được tổ chức theo chuẩn **ASP.NET Core MVC**, tách biệt rõ ràng giữa controller, model, view, service và tài nguyên tĩnh:

<pre>
driving-license-training-system/
├── dacn-gplx.sln
├── dacn-gplx/
│ ├── Controllers/
│ ├── Models/
│ ├── Services/
│ ├── Views/
│ ├── wwwroot/
│ │ ├── css/
│ │ ├── js/
│ │ ├── images/
│ │ ├── database/
│ │ │ └── QL_GPLX.sql
│ │ └── haarcascade/
│ │ └── haarcascade_frontalface_default.xml
│ ├── appsettings.json
│ ├── appsettings.Development.json
└─└── Program.cs
</pre>

---

## 🧰 Technology Stack

- ⚙️ **.NET 9 / ASP.NET Core MVC**: nền tảng phát triển ứng dụng web theo mô hình MVC.  
- 🗄️ **Entity Framework Core 9 (SQL Server)**: ORM quản lý truy cập và thao tác dữ liệu.  
- 📄 **QuestPDF**: tạo và xuất báo cáo PDF.  
- ✉️ **SMTP (MailKit / System.Net.Mail)**: gửi email thông báo tự động.  
- 🧑‍💻 **OpenCvSharp4**: xử lý ảnh và xác thực khuôn mặt bằng OpenCV.  
- 🎨 **Bootstrap, jQuery**: xây dựng giao diện người dùng và xử lý tương tác phía client.  

---

## ⚙️ Prerequisites

- 🧩 **.NET 9 SDK**: môi trường build và chạy ứng dụng ASP.NET Core.  
- 🗄️ **SQL Server**: hệ quản trị cơ sở dữ liệu cho hệ thống.  
- ✉️ **SMTP Account**: tài khoản email dùng để gửi thông báo từ ứng dụng.  

---

## 🔧 Configuration

Cấu hình ứng dụng được khai báo trong `appsettings.Development.json` (môi trường phát triển) hoặc `appsettings.json` (môi trường production), bao gồm:

- 🗄️ **Chuỗi kết nối SQL Server**: đảm bảo thông tin server, database và credentials hợp lệ.  
- ✉️ **Cấu hình SMTP**: email gửi đi, mật khẩu, máy chủ SMTP và cổng kết nối.  
- 🧑‍💻 **Haar Cascade file**: bắt buộc tồn tại tại  
  `wwwroot/haarcascade/haarcascade_frontalface_default.xml` để chức năng xác thực khuôn mặt hoạt động chính xác.  

---

## 🗄️ Database Setup

1. ▶️ **Chạy script SQL**: thực thi file `wwwroot/database/QL_GPLX.sql` để khởi tạo cấu trúc dữ liệu.  
2. 🏗️ **Tạo database**: đảm bảo database có tên **`QuanLyGPLX`** được tạo thành công trên SQL Server.  
3. 🔗 **Cập nhật connection string**: cấu hình lại chuỗi kết nối trong `appsettings.Development.json` hoặc `appsettings.json` để trỏ đúng tới database vừa tạo.   

---

## 🚀 Getting Started

Thực hiện các bước sau để khởi chạy dự án trên môi trường local:

### 1️⃣ Clone Repository

    git clone https://github.com/haihttt974/driving-license-training-system.git
    cd driving-license-training-system

### 2️⃣ Cấu hình Database & SMTP

- Mở SQL Server và chạy script:

    wwwroot/database/QL_GPLX.sql

- Đảm bảo database **QuanLyGPLX** được tạo thành công.
- Cập nhật file `appsettings.Development.json`:
  - Chuỗi kết nối SQL Server
  - Thông tin SMTP (email, password, server, port)
- Kiểm tra file Haar Cascade tồn tại tại:

    wwwroot/haarcascade/haarcascade_frontalface_default.xml

### 3️⃣ Build & Run Ứng Dụng

    dotnet restore dacn-gplx.sln
    dotnet build dacn-gplx.sln
    dotnet run --project dacn-gplx/dacn-gplx.csproj

---

## ▶️ Running the Application

Ứng dụng được chạy bằng **Kestrel Web Server** (mặc định của ASP.NET Core).  
Sau khi khởi động thành công, có thể truy cập hệ thống thông qua trình duyệt tại:

- 🌐 `http://localhost:5000`  
- 🔐 `https://localhost:5001`  

Cổng và giao thức (HTTP/HTTPS) có thể thay đổi tùy theo cấu hình trong môi trường chạy và file thiết lập của ASP.NET Core.

---

## 📝 Note

- ⚠️ **Không sử dụng EF Core Migrations**: cấu trúc database được quản lý hoàn toàn bằng script SQL đi kèm.  
- 🔐 **Phân quyền người dùng**: được kiểm soát thông qua kiểm tra **role** trong middleware của ứng dụng.  
- 🧑‍💻 **Xác thực khuôn mặt**: độ chính xác phụ thuộc vào chất lượng ảnh đầu vào (ánh sáng, góc chụp, độ rõ nét).  

---

## 🛣️ Roadmap

- 🗄️ **EF Core Migrations & Seeding**: tự động hóa quản lý schema và dữ liệu khởi tạo.  
- 🔐 **Policy-based Authorization**: nâng cấp cơ chế phân quyền linh hoạt và bảo mật hơn.  
- 🔌 **REST API**: mở rộng hệ thống để tích hợp mobile app hoặc frontend độc lập.  
- 📊 **Advanced Dashboard**: bổ sung biểu đồ, thống kê và phân tích dữ liệu nâng cao.   

---

## 👥 The Dev Team

<div align="center">
	<table>
		<tr>
			<td align="center" valign="top">
					<img src="https://github.com/haihttt974.png?s=150" loading="lazy" width="150" height="150">
	        <br>
	        <a href="https://github.com/haihttt974">Duy Hải</a>
	        <p>
	        </p>
			</td>
			<td align="center" valign="top">
					<img src="https://github.com/ngoctrinh564.png?s=150" loading="lazy" width="150" height="150">
	        <br>
	        <a href="https://github.com/ngoctrinh564">Ngọc Trinh</a>
	        <p>
	        </p>
			</td>
      <td align="center" valign="top">
					<img src="https://github.com/VuMinhThien5.png?s=150" loading="lazy" width="150" height="150">
	        <br>
	        <a href="https://github.com/VuMinhThien5">Minh Thiện</a>
	        <p>
	        </p>
			</td>
		</tr>
	</table>
</div>


---

## 🙏 Acknowledgements

- 🧩 **Microsoft ASP.NET Core**: nền tảng phát triển web hiện đại và ổn định.  
- 🗄️ **Entity Framework Core**: ORM hỗ trợ truy cập và quản lý dữ liệu hiệu quả.  
- 📄 **QuestPDF**: thư viện tạo và xuất báo cáo PDF.  
- 🧑‍💻 **OpenCV / OpenCvSharp**: công cụ xử lý ảnh và nhận diện khuôn mặt.  
