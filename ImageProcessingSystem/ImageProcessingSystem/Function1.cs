using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;

namespace ImageProcessingSystem
{
    public class ImageFunctions
    {
        private readonly ILogger<ImageFunctions> _logger;
        private readonly IImageService _imageService;
        private readonly ICostMonitorService _costService;

        public ImageFunctions(
            ILogger<ImageFunctions> logger,
            IImageService imageService,
            ICostMonitorService costService)
        {
            _logger = logger;
            _imageService = imageService;
            _costService = costService;
        }

        // 1. Hàm nhận diện ảnh mới từ folder 'uploads' và gửi tên file vào hàng đợi
        [Function("HandleBlobUpload")]
        [QueueOutput("image-process-queue", Connection = "AzureWebJobsStorage")]
        public string HandleBlobUpload(
            [BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] Stream stream,
            string name)
        {
            _logger.LogInformation($"[Producer] Đã nhận ảnh {name}. Đang đẩy vào hàng đợi xử lý...");
            return name;
        }

        // 2. Hàm xử lý ảnh chính lấy từ hàng đợi
        [Function("ProcessImageFromQueue")]
        public async Task<ImageProcessingResponse> ProcessImageFromQueue(
            // Tên tham số là 'fileName'
            [QueueTrigger("image-process-queue", Connection = "AzureWebJobsStorage")] string fileName,
            // Sử dụng đúng biến {fileName} để lấy ảnh từ Blob
            [BlobInput("uploads/{fileName}", Connection = "AzureWebJobsStorage")] Stream inputStream)
        {
            // Kiểm tra hạn mức chi phí (Quota)
            if (await _costService.CheckQuotaAsync())
            {
                _logger.LogWarning($"[Cost Alert] Hạn mức xử lý đã hết cho file: {fileName}");
                return null;
            }

            _logger.LogInformation($"[Consumer] Đang xử lý file: {fileName}");

            // Gọi ImageService để xử lý ảnh (Resize, Filter, v.v.)
            var processedData = await _imageService.ProcessAndEnrichAsync(inputStream, fileName);

            _costService.LogExecution();

            return new ImageProcessingResponse
            {
                LogEntity = new ImageLogEntity
                {
                    PartitionKey = "Images",
                    RowKey = Guid.NewGuid().ToString(),
                    FileName = fileName,
                    Status = "Processed_Successfully"
                },
                ResizedBlob = processedData,
                BlobName = fileName, // Gán để dùng làm tên file đầu ra
                QueueMessage = $"Hoàn tất: {fileName} đã được xử lý thành công!"
            };
        }
    }

    // Class quản lý các đầu ra (Output Binding)
    public class ImageProcessingResponse
    {
        [TableOutput("ImageLogs", Connection = "AzureWebJobsStorage")]
        public ImageLogEntity LogEntity { get; set; }

        // Đầu ra lưu vào folder thumbnails với tên khớp với thuộc tính BlobName
        [BlobOutput("thumbnails/{BlobName}", Connection = "AzureWebJobsStorage")]
        public byte[] ResizedBlob { get; set; }

        public string BlobName { get; set; }

        [QueueOutput("notification-queue", Connection = "AzureWebJobsStorage")]
        public string QueueMessage { get; set; }
    }

    // Class định nghĩa cấu trúc bảng log
    public class ImageLogEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
    }
}