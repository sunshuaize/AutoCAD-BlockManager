using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlockManager.IPC.Contracts;
using BlockManager.IPC.Client;
using BlockManager.UI.ViewModels;

namespace BlockManager.UI
{
    /// <summary>
    /// 应用程序主类
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            // 创建主机和依赖注入容器
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();

            await _host.StartAsync();

            // 创建并显示主窗口
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            base.OnExit(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // 根据命令行参数或环境检测管道名称
            string pipeName = DetectPipeName();
            
            // 注册IPC客户端
            services.AddSingleton<IBlockManagerClient>(provider => new NamedPipeClient(pipeName));

            // 注册ViewModels
            services.AddTransient<MainWindowViewModel>();

            // 注册Views
            services.AddTransient<MainWindow>();
        }

        /// <summary>
        /// 检测应该使用的管道名称
        /// </summary>
        /// <returns>管道名称</returns>
        private string DetectPipeName()
        {
            var args = Environment.GetCommandLineArgs();
            
            // 检查命令行参数
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals("--pipe", StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }
            
            // 默认尝试2024版本的管道，如果不存在则使用2010版本
            return "BlockManager_IPC_2024";
        }
    }
}
