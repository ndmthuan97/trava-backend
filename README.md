# Trava Backend

## Tổng quan dự án (Project Overview)
**Trava** là một hệ thống quản lý công việc và cộng tác nhóm mạnh mẽ, được xây dựng trên nền tảng .NET hiện đại. Dự án tập trung vào việc cung cấp các công cụ để quản lý không gian làm việc (Spaces), quản lý task (Task Items), và hỗ trợ tương tác giữa các thành viên trong nhóm thông qua hệ thống lời mời và thông báo.

## Kiến trúc hệ thống (Architecture)
Dự án được thiết kế theo mô hình **Clean Architecture** (Kiến trúc sạch) nhằm đảm bảo tính linh hoạt, dễ bảo trì và dễ dàng mở rộng:

- **Trava.Domain**: Chứa các thực thể lõi (Entities), logic nghiệp vụ cơ bản, các hằng số và kiểu liệt kê (Enums). Đây là trung tâm của hệ thống và không phụ thuộc vào bất kỳ thư viện bên ngoài nào.
- **Trava.Application**: Chứa logic ứng dụng, các Command/Query (sử dụng pattern CQRS), DTOs và các Interfaces. Đây là nơi định nghĩa các quy trình nghiệp vụ ví dụ như tạo task, gán task, hay xác thực người dùng.
- **Trava.Infrastructure**: Triển khai các chi tiết kỹ thuật như truy xuất cơ sở dữ liệu (EF Core), Caching (Redis), và các dịch vụ bên thứ ba (Ví dụ: lưu trữ token).
- **Trava.API**: Lớp hiển thị (Presentation Layer), cung cấp các RESTful API endpoints cho phía Frontend hoặc các ứng dụng bên thứ ba tiêu thụ.
- **Trava.Shared**: Chứa các mã nguồn dùng chung, tiện ích và các model chia sẻ giữa các project.

## Công nghệ sử dụng (Tech Stack)
- **Framework**: .NET 8.0
- **Database**: PostgreSQL (Thông qua Entity Framework Core)
- **Caching**: Redis
- **Security**: JWT Authentication (JSON Web Token)
- **Pattern**: CQRS (MediatR), Repository Pattern, Unit of Work
- **Documentation**: Swagger/OpenAPI
- **API Management**: Rate Limiting, CORS Configuration

## Các tính năng chính (Core Features)
- **Quản lý xác thực (Authentication)**: Đăng nhập, đăng ký và bảo mật bằng JWT.
- **Quản lý không gian làm việc (Spaces)**: Tạo, cập nhật không gian làm việc và quản lý thành viên.
- **Hệ thống lời mời (Space Invitations)**: Gửi và quản lý lời mời tham gia không gian làm việc.
- **Quản lý công việc (Task Management)**:
    - Tạo, cập nhật, xóa các đầu việc (Task Items).
    - Gán người thực hiện công việc (Assignment).
    - Theo dõi trạng thái hoàn thành công việc.
    - Bình luận trên các task (Task Comments).
- **Hệ thống thông báo (Notifications)**: Gửi thông báo đến người dùng về các thay đổi quan trọng hoặc lời mời.

## Cấu trúc thư mục
```text
backend/src/
├── Trava.API/           # REST API Endpoints & Configuration
├── Trava.Application/   # Logic ứng dụng, Commands & Queries
├── Trava.Domain/        # Core Entities & Domain Logic
├── Trava.Infrastructure/# DB Context, Repositories, Redis, Services
└── Trava.Shared/        # Utilities & Shared Models
```

## Hướng dẫn chạy dự án
1. **Yêu cầu**: Cài đặt .NET 8.0 SDK, Redis Server và PostgreSQL.
2. **Cấu hình**: Cập nhật Connection String và Redis Connection trong file `appsettings.json` (hoặc `.env`).
3. **Chạy ứng dụng**:
   ```bash
   cd backend/src/Trava.API
   dotnet run
   ```
4. **Tài liệu API**: Truy cập `/swagger` sau khi chạy ứng dụng để xem tài liệu chi tiết.