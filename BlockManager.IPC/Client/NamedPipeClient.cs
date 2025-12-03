using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlockManager.IPC.Contracts;
using BlockManager.IPC.Contracts.Messages;
using BlockManager.IPC.DTOs;
using Newtonsoft.Json;

namespace BlockManager.IPC.Client
{
    /// <summary>
    /// 命名管道客户端实现
    /// </summary>
    public class NamedPipeClient : IBlockManagerClient, IDisposable
    {
        private readonly string _pipeName;
        private NamedPipeClientStream? _pipeClient;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;

        public NamedPipeClient(string pipeName = "BlockManager_IPC")
        {
            _pipeName = pipeName;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public event EventHandler<FileChangedEventArgs>? FileChanged;

        public bool IsConnected => _pipeClient?.IsConnected ?? false;

        public async Task ConnectAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NamedPipeClient));

            if (IsConnected)
                return;

            _pipeClient?.Dispose();
            _pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut);

            try
            {
                // 增加重试机制
                int maxRetries = 3;
                int retryDelay = 1000; // 1秒

                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        await _pipeClient.ConnectAsync(10000); // 增加到10秒超时
                        return; // 连接成功，退出重试循环
                    }
                    catch (TimeoutException) when (i < maxRetries - 1)
                    {
                        // 不是最后一次重试，等待后继续
                        await Task.Delay(retryDelay);
                        retryDelay *= 2; // 指数退避
                        
                        // 重新创建管道客户端
                        _pipeClient?.Dispose();
                        _pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut);
                    }
                }
            }
            catch (TimeoutException)
            {
                throw new InvalidOperationException($"无法连接到CAD进程的IPC服务器 (管道名: {_pipeName})。\n请确保：\n1. CAD进程正在运行\n2. 已加载BlockManager插件\n3. 已执行BLOCKVIEWER命令启动IPC服务器");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"连接IPC服务器时发生错误: {ex.Message}", ex);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_pipeClient != null && _pipeClient.IsConnected)
            {
                _pipeClient.Close();
            }
            await Task.CompletedTask;
        }

        public async Task<TreeNodeDto> GetBlockLibraryTreeAsync(string rootPath)
        {
            var request = new RequestMessage
            {
                Action = "GET_BLOCK_LIBRARY_TREE",
                Data = new { rootPath }
            };

            var response = await SendRequestAsync(request);
            
            if (!response.IsSuccess)
            {
                throw new InvalidOperationException($"获取块库文件树失败: {response.Error?.Message}");
            }

            return JsonConvert.DeserializeObject<TreeNodeDto>(JsonConvert.SerializeObject(response.Data))!;
        }

        public async Task<PreviewDto> GetFilePreviewAsync(string filePath)
        {
            var request = new RequestMessage
            {
                Action = "GET_FILE_PREVIEW",
                Data = new { filePath }
            };

            var response = await SendRequestAsync(request);
            
            if (!response.IsSuccess)
            {
                return new PreviewDto
                {
                    FilePath = filePath,
                    IsSuccess = false,
                    ErrorMessage = response.Error?.Message ?? "未知错误"
                };
            }

            return JsonConvert.DeserializeObject<PreviewDto>(JsonConvert.SerializeObject(response.Data))!;
        }

        public async Task<bool> InsertBlockAsync(InsertBlockRequest request)
        {
            var requestMessage = new RequestMessage
            {
                Action = "INSERT_BLOCK",
                Data = request
            };

            var response = await SendRequestAsync(requestMessage);
            return response.IsSuccess;
        }

        private async Task<ResponseMessage> SendRequestAsync(RequestMessage request)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("客户端未连接到服务器");
            }

            try
            {
                // 序列化请求
                var requestJson = JsonConvert.SerializeObject(request);
                var requestBytes = Encoding.UTF8.GetBytes(requestJson);

                // 发送请求长度
                var lengthBytes = BitConverter.GetBytes(requestBytes.Length);
                await _pipeClient!.WriteAsync(lengthBytes, 0, lengthBytes.Length);

                // 发送请求数据
                await _pipeClient.WriteAsync(requestBytes, 0, requestBytes.Length);
                await _pipeClient.FlushAsync();

                // 读取响应长度
                var responseLengthBytes = new byte[4];
                await ReadExactAsync(_pipeClient, responseLengthBytes, 4);
                var responseLength = BitConverter.ToInt32(responseLengthBytes, 0);

                // 读取响应数据
                var responseBytes = new byte[responseLength];
                await ReadExactAsync(_pipeClient, responseBytes, responseLength);

                // 反序列化响应
                var responseJson = Encoding.UTF8.GetString(responseBytes);
                return JsonConvert.DeserializeObject<ResponseMessage>(responseJson)!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"IPC通信错误: {ex.Message}", ex);
            }
        }

        private static async Task ReadExactAsync(Stream stream, byte[] buffer, int count)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = await stream.ReadAsync(buffer, totalRead, count - totalRead);
                if (read == 0)
                {
                    throw new EndOfStreamException("连接已断开");
                }
                totalRead += read;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _cancellationTokenSource.Cancel();
            _pipeClient?.Dispose();
            _cancellationTokenSource.Dispose();
            _disposed = true;
        }
    }
}
