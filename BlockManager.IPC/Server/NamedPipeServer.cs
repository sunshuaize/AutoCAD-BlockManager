using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlockManager.IPC.Contracts;
using BlockManager.IPC.Contracts.Messages;
using BlockManager.IPC.DTOs;
using Newtonsoft.Json;

namespace BlockManager.IPC.Server
{
    /// <summary>
    /// 命名管道服务端实现
    /// </summary>
    public class NamedPipeServer : IBlockManagerServer, IDisposable
    {
        private readonly string _pipeName;
        private readonly IBlockManagerServer _implementation;
        private NamedPipeServerStream? _pipeServer;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task? _serverTask;
        private bool _disposed;

        public NamedPipeServer(IBlockManagerServer implementation, string pipeName = "BlockManager_IPC")
        {
            _implementation = implementation ?? throw new ArgumentNullException(nameof(implementation));
            _pipeName = pipeName;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public bool IsRunning { get; private set; }

        public async Task StartAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NamedPipeServer));

            if (IsRunning)
                return;

            IsRunning = true;
            _serverTask = Task.Run(async () => await RunServerAsync(_cancellationTokenSource.Token));
            
            // 等待服务器真正开始监听
            await Task.Delay(100); // 给服务器一点时间来创建命名管道
            LogToAutoCAD($"IPC服务器已启动并开始监听管道: {_pipeName}");
        }

        public async Task StopAsync()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            _cancellationTokenSource.Cancel();

            if (_serverTask != null)
            {
                try
                {
                    await _serverTask;
                }
                catch (OperationCanceledException)
                {
                    // 预期的取消异常
                }
            }

            _pipeServer?.Dispose();
            _pipeServer = null;
        }

        private async Task RunServerAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte);
                    
                    LogToAutoCAD($"IPC服务器正在等待连接... (管道名: {_pipeName})");
                    
                    // 等待客户端连接
                    await _pipeServer.WaitForConnectionAsync(cancellationToken);
                    
                    LogToAutoCAD("客户端已连接到IPC服务器");
                    
                    // 处理客户端请求
                    await HandleClientAsync(_pipeServer, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    LogToAutoCAD("IPC服务器已停止");
                    break;
                }
                catch (Exception ex)
                {
                    // 记录错误但继续运行
                    LogToAutoCAD($"IPC服务器错误: {ex.Message}");
                    LogToAutoCAD($"错误详情: {ex}");
                    
                    // 短暂等待后重试
                    await Task.Delay(1000, cancellationToken);
                }
                finally
                {
                    try
                    {
                        _pipeServer?.Dispose();
                    }
                    catch { }
                    _pipeServer = null;
                }
            }
        }

        private async Task HandleClientAsync(NamedPipeServerStream pipeServer, CancellationToken cancellationToken)
        {
            while (pipeServer.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 读取请求长度
                    var lengthBytes = new byte[4];
                    await ReadExactAsync(pipeServer, lengthBytes, 4, cancellationToken);
                    var requestLength = BitConverter.ToInt32(lengthBytes, 0);

                    // 读取请求数据
                    var requestBytes = new byte[requestLength];
                    await ReadExactAsync(pipeServer, requestBytes, requestLength, cancellationToken);

                    // 处理请求
                    var requestJson = Encoding.UTF8.GetString(requestBytes);
                    var response = await ProcessRequestAsync(requestJson);

                    // 发送响应
                    var responseJson = JsonConvert.SerializeObject(response);
                    var responseBytes = Encoding.UTF8.GetBytes(responseJson);
                    
                    var responseLengthBytes = BitConverter.GetBytes(responseBytes.Length);
                    await pipeServer.WriteAsync(responseLengthBytes, 0, responseLengthBytes.Length, cancellationToken);
                    await pipeServer.WriteAsync(responseBytes, 0, responseBytes.Length, cancellationToken);
                    await pipeServer.FlushAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理客户端请求时出错: {ex.Message}");
                    break;
                }
            }
        }

        private async Task<ResponseMessage> ProcessRequestAsync(string requestJson)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<RequestMessage>(requestJson);
                if (request == null)
                {
                    return CreateErrorResponse("", "INVALID_REQUEST", "无效的请求格式");
                }

                // 使用if-else替代switch表达式，兼容C# 7.3
                if (request.Action == "GET_BLOCK_LIBRARY_TREE")
                {
                    return await HandleGetBlockLibraryTreeAsync(request);
                }
                else if (request.Action == "GET_FILE_PREVIEW")
                {
                    return await HandleGetFilePreviewAsync(request);
                }
                else if (request.Action == "EXECUTE_COMMAND")
                {
                    return await HandleExecuteCommandAsync(request);
                }
                else
                {
                    return CreateErrorResponse(request.MessageId, "UNKNOWN_ACTION", $"未知的操作: {request.Action}");
                }
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("", "PROCESSING_ERROR", $"处理请求时出错: {ex.Message}");
            }
        }

        private async Task<ResponseMessage> HandleGetBlockLibraryTreeAsync(RequestMessage request)
        {
            try
            {
                var dataJson = JsonConvert.SerializeObject(request.Data);
                var dataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson);
                var rootPath = dataDict?["rootPath"]?.ToString() ?? "";
                
                var result = await _implementation.GetBlockLibraryTreeAsync(rootPath);
                
                return new ResponseMessage
                {
                    MessageId = request.MessageId,
                    Action = request.Action,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(request.MessageId, "GET_TREE_ERROR", ex.Message);
            }
        }

        private async Task<ResponseMessage> HandleGetFilePreviewAsync(RequestMessage request)
        {
            try
            {
                var dataJson = JsonConvert.SerializeObject(request.Data);
                var dataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson);
                var filePath = dataDict?["filePath"]?.ToString() ?? "";
                
                var result = await _implementation.GetFilePreviewAsync(filePath);
                
                return new ResponseMessage
                {
                    MessageId = request.MessageId,
                    Action = request.Action,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(request.MessageId, "GET_PREVIEW_ERROR", ex.Message);
            }
        }

        private async Task<ResponseMessage> HandleExecuteCommandAsync(RequestMessage request)
        {
            try
            {
                var dataJson = JsonConvert.SerializeObject(request.Data);
                var commandRequest = JsonConvert.DeserializeObject<CommandExecutionRequest>(dataJson);
                
                if (commandRequest == null)
                {
                    return CreateErrorResponse(request.MessageId, "INVALID_COMMAND_REQUEST", "无效的命令请求格式");
                }
                
                var result = await _implementation.ExecuteCommandAsync(commandRequest);
                
                return new ResponseMessage
                {
                    MessageId = request.MessageId,
                    Action = request.Action,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(request.MessageId, "EXECUTE_COMMAND_ERROR", ex.Message);
            }
        }

    

        private static ResponseMessage CreateErrorResponse(string messageId, string errorCode, string errorMessage)
        {
            return new ResponseMessage
            {
                MessageId = messageId,
                Error = new ErrorInfo
                {
                    Code = errorCode,
                    Message = errorMessage
                }
            };
        }

        private static async Task ReadExactAsync(Stream stream, byte[] buffer, int count, CancellationToken cancellationToken)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = await stream.ReadAsync(buffer, totalRead, count - totalRead, cancellationToken);
                if (read == 0)
                {
                    throw new EndOfStreamException("连接已断开");
                }
                totalRead += read;
            }
        }

        // 这些方法委托给实际的实现
        public Task<TreeNodeDto> GetBlockLibraryTreeAsync(string rootPath) => _implementation.GetBlockLibraryTreeAsync(rootPath);
        public Task<PreviewDto> GetFilePreviewAsync(string filePath) => _implementation.GetFilePreviewAsync(filePath);
        public Task<CommandExecutionResponse> ExecuteCommandAsync(CommandExecutionRequest request) => _implementation.ExecuteCommandAsync(request);

        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="message">日志消息</param>
        private void LogToAutoCAD(string message)
        {
            // 输出到调试窗口和控制台
            System.Diagnostics.Debug.WriteLine(message);
            Console.WriteLine(message);
            
            // 写入日志文件
            try
            {
                var logPath = @"c:\temp\blockmgr_namedpipe_debug.log";
                var logDir = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss.fff} {message}\n");
            }
            catch
            {
                // 忽略日志文件错误
            }
        }


        public void Dispose()
        {
            if (_disposed)
                return;

            StopAsync().Wait();
            _cancellationTokenSource.Dispose();
            _disposed = true;
        }
    }
}
