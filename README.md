# 🚀 Trava Backend - Productivity & Team Collaboration Platform

[![Framework](https://img.shields.io/badge/.NET-8.0-512bd4.svg?logo=dotnet)]()
[![Database](https://img.shields.io/badge/PostgreSQL-4169e1.svg?logo=postgresql&logoColor=white)]()
[![Cache](https://img.shields.io/badge/Redis-dc382d.svg?logo=redis&logoColor=white)]()
[![Architecture](https://img.shields.io/badge/Clean-Architecture-blue.svg)]()

**Trava** là một hệ thống backend mạnh mẽ hỗ trợ quản lý công việc và cộng tác nhóm, được xây dựng trên nền tảng .NET hiện đại. Dự án cung cấp các giải pháp tối ưu cho việc tổ chức không gian làm việc (Spaces), quản lý hạng mục công việc (Tasks), và tương tác thời gian thực giữa các thành viên.

---

## 🏛️ Kiến Trúc Hệ Thống (Architecture)

Dự án tuân thủ nghiêm ngặt mô hình **Clean Architecture**, giúp tách biệt rõ ràng các mối quan tâm (Separation of Concerns), đảm bảo hệ thống dễ dàng bảo trì và mở rộng.

| Project Component | Trách nhiệm |
| :--- | :--- |
| **`Trava.Domain`** | Core Entities, Enums, Domain Logic & Interfaces. Không phụ thuộc vào Layer khác. |
| **`Trava.Application`** | Logic ứng dụng, CQRS (MediatR), DTOs, Validation (FluentValidation), Mapping (AutoMapper). |
| **`Trava.Infrastructure`** | Persistence (EF Core, PostgreSQL), Repositories, Redis, Services (JWT, Email). |
| **`Trava.API`** | Presentation Layer, RESTful Endpoints, Auth Middleware, Global Exception Handling. |
| **`Trava.Shared`** | Common Utilities, Constants, Shared Models & Custom Exception Codes. |

---

## 🛠️ Công Nghệ Sử Dụng (Tech Stack)

### Core Frameworks & Patterns
*   **Backend Framework:** .NET 8.0 SDK
*   **Architecture Pattern:** Clean Architecture, CQRS (MediatR), Repository Pattern, Unit of Work.
*   **Validation:** FluentValidation cho việc thực thi các quy tắc nghiệp vụ.
*   **Object Mapping:** AutoMapper để chuyển đổi linh hoạt giữa Entity và DTO.
*   **API Management:** RESTful API, Swagger/OpenAPI, Rate Limiting, CORS.

### Data & Infrastructure
*   **Cơ sở dữ liệu:** PostgreSQL (thông qua Entity Framework Core).
*   **Caching:** Redis (Distributed Caching) giúp tăng tốc độ phản hồi API.
*   **Security:** JWT Authentication (JSON Web Token) bảo mật các endpoint.
*   **Real-time (Planned):** SignalR cho hệ thống đẩy thông báo tức thời.
*   **Background Workers:** Lên lịch công việc định kỳ (Dự kiến: Quartz.NET / Hangfire).

---

## ✨ Tính Năng Chính (Core Features)

*   🔐 **Quản lý Hội viên:** Đăng ký, đăng nhập bảo mật, quản lý thông tin cá nhân và cập nhật bảo mật.
*   🏢 **Quản lý Không gian làm việc (Spaces):** Hỗ trợ mô hình Space **Cá nhân** (Private) và **Đội nhóm** (Team) với cơ chế phân quyền rõ ràng.
*   📩 **Hệ thống Lời mời:** Cơ chế gửi/nhận lời mời tham gia Workspace, quản lý trạng thái lời mời theo thời gian thực.
*   ✅ **Quản lý Công việc (Task Management):** 
    *   Hỗ trợ cấu trúc Task phức tạp (Parent/Sub-tasks).
    *   Quản lý độ ưu tiên (Low to Urgent) và khối lượng công việc (Points).
    *   Tương tác trực tiếp qua hệ thống bình luận (Comments).
*   🔔 **Hệ thống Thông báo:** Cung cấp thông tin cập nhật về thay đổi Space, phân công Task và nhắc nhở thời hạn.

---

## 📂 Cấu Trúc Mã Nguồn (Directory Structure)

```bash
backend/src/
├── Trava.API/            # REST API Endpoints & Configuration
├── Trava.Application/    # Logic ứng dụng, Commands & Queries
├── Trava.Domain/         # Core Entities & Domain Models
├── Trava.Infrastructure/ # DB Context, Repositories & External Services
└── Trava.Shared/         # Utilities, Constants & Shared Models
```

---

## 🚀 Hướng Dẫn Cài Đặt (Getting Started)

### 1. Yêu cầu hệ thống (Prerequisites)
*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) trở lên.
*   [PostgreSQL](https://www.postgresql.org/) server đã cài đặt.
*   [Redis Server](https://redis.io/) (Local hoặc Cloud).

### 2. Cấu hình môi trường (Configuration)
Cập nhật các thông số kết nối trong file `appsettings.json` tại project `Trava.API`:
*   `DefaultConnection`: Chuỗi kết nối tới PostgreSQL.
*   `RedisConnection`: Địa chỉ kết nối tới Redis server.
*   `Jwt:Key`: Khóa bí mật dùng để ký token.

### 3. Khởi tạo Database
Mở terminal và thực thi các lệnh sau để áp dụng các thay đổi cơ sở dữ liệu:
```bash
cd backend/src/Trava.Infrastructure
dotnet ef database update --startup-project ../Trava.API/
```

### 4. Chạy dự án
```bash
cd backend/src/Trava.API
dotnet run
```
Sau khi khởi động thành công, bạn có thể truy cập `/swagger` để xem tài liệu API chi tiết.

---
Developed with ❤️ by Minh Thuan.
