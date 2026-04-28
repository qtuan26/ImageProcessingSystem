# 🖼️ Image Processing System — Azure Functions + ASP.NET Core

Hệ thống xử lý ảnh Serverless sử dụng **Azure Functions (.NET 8 Isolated Worker)** kết hợp giao diện web HTML/JS. Pipeline tự động: Upload → BlobTrigger → Resize + Watermark → Thumbnails → Log → Notification.

---

## 📁 Cấu trúc repo

```
ImageProcessingSystem/
├── backend/
│   ├── ImageProcessingSystem.csproj
│   ├── Program.cs
│   ├── host.json
│   ├── local.settings.json          ← Tự tạo (xem hướng dẫn bên dưới)
│   ├── Functions/
│   │   ├── ImageFunctions.cs
│   │   └── NotificationFunction.cs
│   ├── Services/
│   │   ├── ImageService.cs
│   │   └── CostMonitorService.cs
│   └── Models/
│       ├── ImageProcessingResponse.cs
│       └── ImageLogEntity.cs
└── frontend/
    └── index.html
```

---

## ✅ Yêu cầu cài đặt (Windows)

| Công cụ | Phiên bản | Link tải |
|---|---|---|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Azure Functions Core Tools | v4 | https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local |
| Node.js | 18+ (cần cho func CLI) | https://nodejs.org |
| Azurite (Storage Emulator) | 3.x | Cài qua npm hoặc VS Code extension |
| Azure Storage Explorer | Mới nhất | https://azure.microsoft.com/en-us/products/storage/storage-explorer |
| Visual Studio Code *(tuỳ chọn)* | Mới nhất | https://code.visualstudio.com |

---

## ⚙️ Cài đặt từng bước

### Bước 1 — Clone repo

```bash
git clone https://github.com/<your-username>/ImageProcessingSystem.git
cd ImageProcessingSystem
```

---

### Bước 2 — Cài Azure Functions Core Tools

Mở **Command Prompt** hoặc **PowerShell** với quyền Admin:

```powershell
npm install -g azure-functions-core-tools@4 --unsafe-perm true
```

Kiểm tra cài đặt thành công:

```powershell
func --version
# Kết quả mong đợi: 4.x.x
```

---

### Bước 3 — Cài và khởi động Azurite (Storage Emulator)

**Cách 1: Dùng npm (khuyến nghị)**

```powershell
npm install -g azurite
```

Tạo thư mục lưu dữ liệu và khởi động:

```powershell
mkdir C:\azurite-data
azurite --location C:\azurite-data --debug C:\azurite-data\debug.log
```

> ⚠️ Giữ cửa sổ này **mở trong suốt quá trình chạy**. Azurite sẽ lắng nghe trên các cổng:
> - Blob: `10000`
> - Queue: `10001`  
> - Table: `10002`

**Cách 2: Dùng VS Code Extension**

Cài extension **Azurite** trong VS Code → `Ctrl+Shift+P` → gõ `Azurite: Start`.

---

### Bước 4 — Tạo file `local.settings.json`

Trong thư mục `backend/`, tạo file `local.settings.json` với nội dung sau:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

> ℹ️ File này **không được commit** lên GitHub (đã có trong `.gitignore`). Chuỗi `UseDevelopmentStorage=true` sẽ tự kết nối với Azurite đang chạy ở Bước 3.

---

### Bước 5 — Restore dependencies và build

```powershell
cd backend
dotnet restore
dotnet build
```

Kết quả mong đợi: `Build succeeded.`

---

### Bước 6 — Chạy Azure Functions

```powershell
# Vẫn trong thư mục backend/
func start
```

Kết quả mong đợi — bạn sẽ thấy 3 functions được load:

```
Functions:
    HandleBlobUpload: blobTrigger
    ProcessImageFromQueue: queueTrigger
    SendNotification: queueTrigger

For detailed output, run func with --verbose flag.
```

> ⚠️ Giữ cửa sổ này **mở**. Functions đang chạy trên `http://localhost:7071`.

---

### Bước 7 — Mở giao diện web

Mở file `frontend/index.html` trực tiếp trên trình duyệt:

```
Nhấp đôi vào file  →  frontend/index.html
```

Hoặc dùng **Live Server** (VS Code extension) để tải lại tự động khi chỉnh sửa.

---

## 🚀 Cách sử dụng

1. Mở `frontend/index.html` trên trình duyệt
2. Kéo thả hoặc chọn file ảnh (PNG, JPG, WebP, GIF — tối đa 10MB)
3. Nhấn **Upload** → theo dõi pipeline chạy theo thời gian thực
4. Xem ảnh gốc và ảnh đã xử lý (resize + watermark) side-by-side
5. Kiểm tra log trong cửa sổ terminal đang chạy `func start`

---

## 🔍 Kiểm tra kết quả bằng Azure Storage Explorer

Sau khi upload ảnh thành công, mở **Azure Storage Explorer**:

1. Kết nối tới **Azurite** (Local & Attached → Storage Accounts → Emulator)
2. Kiểm tra các container:

| Nơi kiểm tra | Nội dung |
|---|---|
| Blob → `uploads` | Ảnh gốc vừa upload |
| Blob → `thumbnails` | Ảnh đã resize + watermark |
| Tables → `ImageLogs` | Log metadata từng lần xử lý |
| Queues → `image-process-queue` | (Thường rỗng sau khi xử lý xong) |
| Queues → `notification-queue` | (Thường rỗng sau khi xử lý xong) |

---

## ⚡ Chạy nhanh (tóm tắt)

Mỗi lần muốn chạy lại dự án, mở **3 cửa sổ terminal** theo thứ tự:

```powershell
# Terminal 1 — Khởi động Azurite
azurite --location C:\azurite-data

# Terminal 2 — Chạy Azure Functions
cd backend
func start

# Terminal 3 (tuỳ chọn) — Mở frontend bằng Live Server
# Hoặc nhấp đôi vào frontend/index.html
```

---

## 🐛 Xử lý lỗi thường gặp

| Lỗi | Nguyên nhân | Cách sửa |
|---|---|---|
| `Cannot connect to storage` | Azurite chưa chạy | Chạy lại Bước 3 |
| `func: command not found` | Chưa cài Core Tools | Chạy lại Bước 2 |
| `Build failed` — thiếu package | Chưa restore | Chạy `dotnet restore` |
| Functions không trigger | Container chưa tồn tại | Azurite tự tạo khi function chạy lần đầu |
| Port 7071 bị chiếm | Có app khác dùng cổng | Đổi port: `func start --port 7072` |
| Watermark không hiện | Không có font hệ thống | Cài thêm font hoặc dùng font mặc định |

---

## 📦 Thư viện sử dụng

```xml
<!-- backend/ImageProcessingSystem.csproj -->
<PackageReference Include="Microsoft.Azure.Functions.Worker" Version="1.*" />
<PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Storage.Blobs" Version="*" />
<PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Storage.Queues" Version="*" />
<PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Tables" Version="*" />
<PackageReference Include="SixLabors.ImageSharp" Version="3.*" />
<PackageReference Include="SixLabors.Fonts" Version="1.*" />
```

---

## 📝 Ghi chú

- Hệ thống chạy hoàn toàn **local** với Azurite — không cần tài khoản Azure thực
- Quota mặc định: **100 ảnh/ngày** (reset khi restart `func start`)
- File `local.settings.json` đã được thêm vào `.gitignore`, **không commit** file này
- Để deploy lên Azure Cloud thực tế, thay `UseDevelopmentStorage=true` bằng connection string thật từ Azure Portal

---

## 📄 Báo cáo

Xem file `BaoCao_Azure_Functions_ImageProcessing.docx` để đọc chi tiết kiến trúc, thiết kế và kết quả demo.