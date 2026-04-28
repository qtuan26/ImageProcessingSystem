using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ImageProcessingSystem;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Đăng ký các service bạn đã tạo ở đây
        services.AddSingleton<IImageService, ImageService>();
        services.AddSingleton<ICostMonitorService, CostMonitorService>();
    })
    .Build();

host.Run();