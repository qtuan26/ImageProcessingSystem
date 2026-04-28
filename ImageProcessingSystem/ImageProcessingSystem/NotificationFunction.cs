using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ImageProcessingSystem
{
    public class NotificationFunction
    {
        private readonly ILogger<NotificationFunction> _logger;

        public NotificationFunction(ILogger<NotificationFunction> logger)
        {
            _logger = logger;
        }

        [Function("SendNotification")]
        public void Run([QueueTrigger("notification-queue", Connection = "AzureWebJobsStorage")] string message)
        {
            // Giả lập gửi Email/SMS bằng cách in ra Log
            _logger.LogInformation("--------------------------------------------------");
            _logger.LogInformation($"[EMAIL SERVICE]: Đang gửi thông báo...");
            _logger.LogInformation($"Nội dung: {message}");
            _logger.LogInformation("--------------------------------------------------");
        }
    }
}