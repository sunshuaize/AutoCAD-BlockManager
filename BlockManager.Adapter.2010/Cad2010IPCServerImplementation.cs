using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using BlockManager.Abstractions;

namespace BlockManager.Adapter._2010
{
    /// <summary>
    /// AutoCAD 2010 IPC服务端实现，兼容.NET Framework 3.5
    /// </summary>
    public class Cad2010IPCServerImplementation : IDisposable
    {
        private readonly string _pipeName;
        private readonly IBlockLibraryService _blockLibraryService;
        private readonly ICADCommandService _cadCommandService;
        private Thread _serverThread;
        private volatile bool _isRunning;
        private volatile bool _disposed;

        public Cad2010IPCServerImplementation(IBlockLibraryService blockLibraryService, ICADCommandService cadCommandService, string pipeName = "BlockManager_IPC")
        {
            _blockLibraryService = blockLibraryService ?? throw new ArgumentNullException("blockLibraryService");
            _cadCommandService = cadCommandService ?? throw new ArgumentNullException("cadCommandService");
            _pipeName = pipeName;
        }

        public bool IsRunning
        {
            get { return _isRunning; }
        }

        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException("Cad2010IPCServerImplementation");

            if (_isRunning)
            {
                LogDebug("服务器已在运行，跳过启动");
                return;
            }

            LogDebug("[2010 IPC] 开始启动IPC服务器...");
            _isRunning = true;
            _serverThread = new Thread(RunServer) { IsBackground = true };
            _serverThread.Start();
            LogDebug("[2010 IPC] IPC服务器线程已启动");
        }

        public void Stop()
        {
            if (!_isRunning)
                return;

            LogDebug("[2010 IPC] 正在停止IPC服务器...");
            _isRunning = false;

            if (_serverThread != null && _serverThread.IsAlive)
            {
                _serverThread.Join(2000); // 等待2秒让服务器线程正常退出
            }
            
            LogDebug("[2010 IPC] IPC服务器已停止");
        }

        private void RunServer()
        {
            LogDebug("[2010 IPC] IPC服务器线程启动");
            
            while (_isRunning)
            {
                NamedPipeServerStream pipeServer = null;
                try
                {
                    pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte);
                    
                    LogDebug("[2010 IPC] IPC服务器正在等待连接... (管道名: " + _pipeName + ")");
                    
                    // 等待客户端连接
                    pipeServer.WaitForConnection();
                    
                    LogDebug("[2010 IPC] 客户端已连接到IPC服务器");
                    
                    // 处理客户端请求
                    HandleClient(pipeServer);
                }
                catch (Exception ex)
                {
                    LogDebug("[2010 IPC] IPC服务器错误: " + ex.Message);
                    LogDebug("[2010 IPC] 错误详情: " + ex.ToString());
                }
                finally
                {
                    // 确保管道被正确关闭和释放
                    try
                    {
                        if (pipeServer != null)
                        {
                            if (pipeServer.IsConnected)
                            {
                                pipeServer.Disconnect();
                            }
                            pipeServer.Close();
                            pipeServer.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogDebug("[2010 IPC] 关闭管道时出错: " + ex.Message);
                    }
                    
                    // 短暂等待后重试
                    if (_isRunning)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        private void HandleClient(NamedPipeServerStream pipeServer)
        {
            LogDebug("[2010 IPC] 客户端连接处理开始");
            
            while (pipeServer.IsConnected && _isRunning)
            {
                try
                {
                    // 读取请求长度
                    var lengthBytes = new byte[4];
                    ReadExact(pipeServer, lengthBytes, 4);
                    var requestLength = BitConverter.ToInt32(lengthBytes, 0);

                    LogDebug("[2010 IPC] 收到请求，长度: " + requestLength);

                    // 读取请求数据
                    var requestBytes = new byte[requestLength];
                    ReadExact(pipeServer, requestBytes, requestLength);

                    // 处理请求
                    var requestJson = Encoding.UTF8.GetString(requestBytes);
                    LogDebug("[2010 IPC] 请求内容: " + requestJson);
                    
                    var response = ProcessRequest(requestJson);
                    LogDebug("[2010 IPC] 响应内容: " + response);

                    // 发送响应
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    
                    var responseLengthBytes = BitConverter.GetBytes(responseBytes.Length);
                    pipeServer.Write(responseLengthBytes, 0, responseLengthBytes.Length);
                    pipeServer.Write(responseBytes, 0, responseBytes.Length);
                    pipeServer.Flush();
                    
                    LogDebug("[2010 IPC] 响应已发送，长度: " + responseBytes.Length);
                }
                catch (Exception ex)
                {
                    LogDebug("[2010 IPC] 处理客户端请求时出错: " + ex.Message);
                    break;
                }
            }
            
            LogDebug("[2010 IPC] 客户端连接处理结束");
        }

        private string ProcessRequest(string requestJson)
        {
            try
            {
                LogDebug("[2010 IPC] ===== 收到新请求 =====");
                LogDebug("[2010 IPC] 请求JSON: " + requestJson);
                
                var action = ExtractJsonValue(requestJson, "Action");
                
                if (string.IsNullOrEmpty(action))
                {
                    LogDebug("[2010 IPC] 错误: 请求中缺少Action字段");
                    return CreateErrorResponse("INVALID_REQUEST", "请求中缺少Action字段");
                }
                
                LogDebug($"[2010 IPC] 处理操作: {action}");
                
                string result;
                try
                {
                    // 使用if-else替代switch表达式，兼容C# 7.3
                    if (action == "GET_BLOCK_LIBRARY_TREE")
                    {
                        result = HandleGetBlockLibraryTree(requestJson);
                    }
                    else if (action == "GET_FILE_PREVIEW")
                    {
                        result = HandleGetFilePreview(requestJson);
                    }
                    else if (action == "EXECUTE_COMMAND")
                    {
                        result = HandleExecuteCommand(requestJson);
                    }
                    else
                    {
                        result = CreateErrorResponse("UNKNOWN_ACTION", "未知的操作: " + action);
                    }
                    
                    LogDebug($"[2010 IPC] 操作 {action} 处理完成");
                }
                catch (Exception actionEx)
                {
                    LogDebug($"[2010 IPC] 处理操作 {action} 时出错: {actionEx.Message}");
                    if (actionEx.InnerException != null)
                    {
                        LogDebug($"[2010 IPC] 内部异常: {actionEx.InnerException.Message}");
                    }
                    
                    // 捕获并记录异常堆栈
                    LogDebug($"[2010 IPC] 异常堆栈: {actionEx.StackTrace}");
                    
                    result = CreateErrorResponse("ACTION_ERROR", $"处理 {action} 操作时出错: {actionEx.Message}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                try
                {
                    LogDebug("[2010 IPC] 处理请求时发生严重错误: " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        LogDebug("[2010 IPC] 内部异常: " + ex.InnerException.Message);
                    }
                    LogDebug("[2010 IPC] 异常堆栈: " + ex.StackTrace);
                }
                catch
                {
                    // 忽略日志错误
                }
                
                return CreateErrorResponse("PROCESSING_ERROR", "处理请求时出错: " + ex.Message);
            }
        }

        private string HandleGetBlockLibraryTree(string requestJson)
        {
            try
            {
                var rootPath = @"c:\Users\PC\Desktop\BlockManager\Block";
                
                // 构建简单的文件树JSON
                var treeJson = BuildFileTreeJson(rootPath, "块库");
                
                return "{\"IsSuccess\":true,\"Data\":" + treeJson + "}";
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("GET_TREE_ERROR", ex.Message);
            }
        }

    
        
        private string HandleGetFilePreview(string requestJson)
        {
            try
            {
                var filePath = ExtractJsonValue(requestJson, "filePath");
                
                if (!File.Exists(filePath))
                {
                    return "{\"IsSuccess\":false,\"ErrorMessage\":\"文件不存在\"}";
                }

                var fileInfo = new FileInfo(filePath);
                var extension = fileInfo.Extension.ToLowerInvariant();
                
                string previewBase64 = "";
                bool hasPreview = false;
                
                if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
                {
                    previewBase64 = Convert.ToBase64String(File.ReadAllBytes(filePath));
                    hasPreview = true;
                }
                else if (extension == ".dwg")
                {
                    var previewPath = Path.ChangeExtension(filePath, ".png");
                    if (File.Exists(previewPath))
                    {
                        previewBase64 = Convert.ToBase64String(File.ReadAllBytes(previewPath));
                        hasPreview = true;
                    }
                }

                var previewJson = string.Format(
                    "{{\"FilePath\":\"{0}\",\"PreviewImageBase64\":\"{1}\",\"IsSuccess\":true,\"Metadata\":{{\"Name\":\"{2}\",\"Size\":{3},\"HasPreview\":{4}}}}}",
                    EscapeJsonString(filePath),
                    previewBase64,
                    EscapeJsonString(fileInfo.Name),
                    fileInfo.Length,
                    hasPreview.ToString().ToLower()
                );

                return previewJson;
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("GET_PREVIEW_ERROR", ex.Message);
            }
        }

        private string HandleExecuteCommand(string requestJson)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                LogDebug("[2010 IPC] ===== 收到ExecuteCommand请求 =====");
                LogDebug("[2010 IPC] 请求JSON: " + requestJson);
                
                // 从请求中提取命令
                var command = ExtractJsonValue(requestJson, "Command");
                
                if (string.IsNullOrEmpty(command))
                {
                    LogDebug("[2010 IPC] 错误: 命令为空");
                    return CreateErrorResponse("INVALID_COMMAND", "命令不能为空");
                }
                
                LogDebug("[2010 IPC] 提取的命令: [" + command + "]");
                LogDebug("[2010 IPC] 命令长度: " + command.Length);
                
                // 检查CAD服务
                if (_cadCommandService == null)
                {
                    LogDebug("[2010 IPC] 错误: CAD命令服务为空");
                    return CreateErrorResponse("SERVICE_ERROR", "CAD命令服务不可用");
                }
                
                // 尝试获取当前文档
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    LogDebug("[2010 IPC] 错误: 无法获取当前活动文档");
                }
                else
                {
                    LogDebug("[2010 IPC] 当前文档: " + doc.Name);
                }
                
                // 执行CAD命令
                LogDebug("[2010 IPC] 开始执行CAD命令...");
                try
                {
                    _cadCommandService.ExecuteCommand(command);
                    LogDebug("[2010 IPC] CAD命令执行完成");
                }
                catch (Exception cmdEx)
                {
                    LogDebug("[2010 IPC] 执行命令时出错: " + cmdEx.Message);
                    if (cmdEx.InnerException != null)
                    {
                        LogDebug("[2010 IPC] 内部异常: " + cmdEx.InnerException.Message);
                    }
                    throw; // 重新抛出异常以便返回错误响应
                }
                
                stopwatch.Stop();
                
                // 构建成功响应JSON
                var responseJson = string.Format(
                    "{{\"IsSuccess\":true,\"Data\":{{\"IsSuccess\":true,\"Result\":\"命令 '{0}' 执行完成\",\"ExecutedAt\":\"{1}\",\"ExecutionTimeMs\":{2}}}}}",
                    EscapeJsonString(command),
                    DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    stopwatch.ElapsedMilliseconds
                );
                
                LogDebug("[2010 IPC] 命令执行成功，耗时: " + stopwatch.ElapsedMilliseconds + "ms");
                return responseJson;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                LogDebug("[2010 IPC] 命令执行失败: " + ex.Message);
                
                // 构建错误响应JSON
                var errorResponseJson = string.Format(
                    "{{\"IsSuccess\":true,\"Data\":{{\"IsSuccess\":false,\"ErrorMessage\":\"{0}\",\"ExecutedAt\":\"{1}\",\"ExecutionTimeMs\":{2}}}}}",
                    EscapeJsonString(ex.Message),
                    DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    stopwatch.ElapsedMilliseconds
                );
                
                return errorResponseJson;
            }
        }

        private string BuildFileTreeJson(string path, string name)
        {
            var isDirectory = Directory.Exists(path);
            var type = isDirectory ? "folder" : "file";
            var iconType = isDirectory ? "folder" : GetFileIconType(Path.GetExtension(path));
            
            var json = string.Format(
                "{{\"Name\":\"{0}\",\"Path\":\"{1}\",\"Type\":\"{2}\",\"IconType\":\"{3}\"",
                EscapeJsonString(name),
                EscapeJsonString(path),
                type,
                iconType
            );

            if (isDirectory)
            {
                json += ",\"Children\":[";
                
                try
                {
                    var directories = Directory.GetDirectories(path);
                    var files = Directory.GetFiles(path);
                    var first = true;

                    foreach (var directory in directories)
                    {
                        if (!first) json += ",";
                        json += BuildFileTreeJson(directory, Path.GetFileName(directory));
                        first = false;
                    }

                    foreach (var file in files)
                    {
                        if (!first) json += ",";
                        json += BuildFileTreeJson(file, Path.GetFileName(file));
                        first = false;
                    }
                }
                catch { }

                json += "]";
            }

            json += "}";
            return json;
        }

        private string GetFileIconType(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".dwg": return "dwg";
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".gif": return "image";
                default: return "file";
            }
        }

        private string ExtractJsonValue(string json, string key)
        {
            var keyPattern = "\"" + key + "\":\"";
            var startIndex = json.IndexOf(keyPattern);
            if (startIndex == -1) return "";
            
            startIndex += keyPattern.Length;
            var endIndex = json.IndexOf("\"", startIndex);
            if (endIndex == -1) return "";
            
            return json.Substring(startIndex, endIndex - startIndex);
        }

        private string EscapeJsonString(string str)
        {
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private string CreateErrorResponse(string errorCode, string errorMessage)
        {
            return string.Format(
                "{{\"IsSuccess\":false,\"Error\":{{\"Code\":\"{0}\",\"Message\":\"{1}\"}}}}",
                errorCode,
                EscapeJsonString(errorMessage)
            );
        }

        private void ReadExact(Stream stream, byte[] buffer, int count)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = stream.Read(buffer, totalRead, count - totalRead);
                if (read == 0)
                {
                    throw new EndOfStreamException("连接已断开");
                }
                totalRead += read;
            }
        }

        /// <summary>
        /// 调试日志方法
        /// </summary>
        /// <param name="message">日志消息</param>
        private void LogDebug(string message)
        {
            try
            {
                var logMessage = DateTime.Now.ToString("HH:mm:ss.fff") + " " + message;
                
                // 输出到控制台
                Console.WriteLine(logMessage);
                
                // 输出到调试窗口
                System.Diagnostics.Debug.WriteLine(logMessage);
                
                // 确保目录存在
                var logDir = @"c:\temp";
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                
                // 写入文件
                var logPath = Path.Combine(logDir, "blockmgr_ipc_debug.log");
                File.AppendAllText(logPath, logMessage + Environment.NewLine);
                
                // 同时输出到AutoCAD命令行
                try
                {
                    var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                    ed?.WriteMessage("\n" + logMessage);
                }
                catch
                {
                    // AutoCAD编辑器可能不可用
                }
            }
            catch (Exception ex)
            {
                // 如果日志失败，至少尝试输出到调试窗口
                System.Diagnostics.Debug.WriteLine("日志错误: " + ex.Message);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Stop();
            _disposed = true;
        }
    }
}
