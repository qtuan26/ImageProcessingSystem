using System.Threading.Tasks;

namespace ImageProcessingSystem
{
    public interface ICostMonitorService
    {
        // Kiểm tra xem đã vượt hạn mức chi phí chưa
        Task<bool> CheckQuotaAsync();
        // Ghi nhận một lần thực thi để tính tiền
        void LogExecution();
    }

    public class CostMonitorService : ICostMonitorService
    {
        private static int _dailyCount = 0;
        private const int MaxQuota = 100; // Giới hạn 100 ảnh/ngày để quản lý chi phí

        public Task<bool> CheckQuotaAsync()
        {
            return Task.FromResult(_dailyCount >= MaxQuota);
        }

        public void LogExecution()
        {
            _dailyCount++;
        }
    }
}