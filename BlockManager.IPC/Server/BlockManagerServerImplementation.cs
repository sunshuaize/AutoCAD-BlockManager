using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlockManager.IPC.Contracts;
using BlockManager.IPC.DTOs;

namespace BlockManager.IPC.Server
{
    /// <summary>
    /// 块管理器服务端的默认实现
    /// </summary>
    public class BlockManagerServerImplementation : IBlockManagerServer
    {
        public BlockManagerServerImplementation()
        {
        }

        public bool IsRunning { get; private set; }

        public async Task StartAsync()
        {
            IsRunning = true;
            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            IsRunning = false;
            await Task.CompletedTask;
        }

        public async Task<TreeNodeDto> GetBlockLibraryTreeAsync(string rootPath)
        {
            await Task.CompletedTask;

            if (!Directory.Exists(rootPath))
            {
                throw new DirectoryNotFoundException($"指定的路径不存在: {rootPath}");
            }

            return BuildTreeNode(rootPath, Path.GetFileName(rootPath) ?? "块库");
        }

        public async Task<PreviewDto> GetFilePreviewAsync(string filePath)
        {
            await Task.CompletedTask;

            try
            {
                if (!File.Exists(filePath))
                {
                    return new PreviewDto
                    {
                        FilePath = filePath,
                        IsSuccess = false,
                        ErrorMessage = "文件不存在"
                    };
                }

                var fileInfo = new FileInfo(filePath);
                var extension = fileInfo.Extension.ToLowerInvariant();

                // 创建文件信息
                var metadata = new FileInfoDto
                {
                    Name = fileInfo.Name,
                    FullPath = fileInfo.FullName,
                    Extension = extension,
                    Size = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime
                };

                string? previewBase64 = null;

                // 处理不同类型的文件预览
                if (IsImageFile(extension))
                {
                    // 直接读取图片文件
                    previewBase64 = Convert.ToBase64String(File.ReadAllBytes(filePath));
                    metadata.HasPreview = true;
                    metadata.PreviewPath = filePath;
                }
                else if (extension == ".dwg")
                {
                    // 查找对应的PNG预览图
                    var previewPath = Path.ChangeExtension(filePath, ".png");
                    if (File.Exists(previewPath))
                    {
                        previewBase64 = Convert.ToBase64String(File.ReadAllBytes(previewPath));
                        metadata.HasPreview = true;
                        metadata.PreviewPath = previewPath;
                    }
                    else
                    {
                        metadata.HasPreview = false;
                    }
                }

                return new PreviewDto
                {
                    FilePath = filePath,
                    PreviewImageBase64 = previewBase64 ?? string.Empty,
                    Metadata = metadata,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new PreviewDto
                {
                    FilePath = filePath,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<CommandExecutionResponse> ExecuteCommandAsync(CommandExecutionRequest request)
        {
            await Task.CompletedTask;
            
            // 基础实现返回未实现错误
            return new CommandExecutionResponse
            {
                IsSuccess = false,
                ErrorMessage = "命令执行功能需要在具体的适配器实现中提供",
                ExecutedAt = DateTime.UtcNow,
                ExecutionTimeMs = 0
            };
        }

     
        private TreeNodeDto BuildTreeNode(string path, string name)
        {
            var node = new TreeNodeDto
            {
                Name = name,
                Path = path
            };

            if (Directory.Exists(path))
            {
                node.Type = "folder";
                node.IconType = "folder";
                node.Children = new List<TreeNodeDto>();

                try
                {
                    // 添加子目录
                    var directories = Directory.GetDirectories(path)
                        .OrderBy(d => Path.GetFileName(d))
                        .ToList();

                    foreach (var directory in directories)
                    {
                        var dirName = Path.GetFileName(directory);
                        if (!string.IsNullOrEmpty(dirName))
                        {
                            node.Children.Add(BuildTreeNode(directory, dirName));
                        }
                    }

                    // 添加文件（只显示DWG文件）
                    var files = Directory.GetFiles(path, "*.dwg")
                        .OrderBy(f => Path.GetFileName(f))
                        .ToList();

                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file);
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            var fileNode = new TreeNodeDto
                            {
                                Name = fileName,
                                Path = file,
                                Type = "file",
                                IconType = GetFileIconType(Path.GetExtension(file)),
                                FileInfo = CreateFileInfo(file)
                            };
                            node.Children.Add(fileNode);
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // 忽略无权限访问的目录
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"读取目录时出错 {path}: {ex.Message}");
                }
            }
            else if (File.Exists(path))
            {
                node.Type = "file";
                node.IconType = GetFileIconType(Path.GetExtension(path));
                node.FileInfo = CreateFileInfo(path);
            }

            return node;
        }

        private FileInfoDto CreateFileInfo(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var extension = fileInfo.Extension.ToLowerInvariant();

                var dto = new FileInfoDto
                {
                    Name = fileInfo.Name,
                    FullPath = fileInfo.FullName,
                    Extension = extension,
                    Size = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime
                };

                // 检查是否有预览图
                if (extension == ".dwg")
                {
                    var previewPath = Path.ChangeExtension(filePath, ".png");
                    dto.HasPreview = File.Exists(previewPath);
                    dto.PreviewPath = previewPath;
                }
                else if (IsImageFile(extension))
                {
                    dto.HasPreview = true;
                    dto.PreviewPath = filePath;
                }

                return dto;
            }
            catch
            {
                return new FileInfoDto
                {
                    Name = Path.GetFileName(filePath),
                    FullPath = filePath,
                    Extension = Path.GetExtension(filePath)
                };
            }
        }

        private string GetFileIconType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".dwg" => "dwg",
                ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".tiff" => "image",
                _ => "file"
            };
        }

        private bool IsImageFile(string extension)
        {
            var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".webp" };
            return imageExtensions.Contains(extension.ToLowerInvariant());
        }
    }
}
