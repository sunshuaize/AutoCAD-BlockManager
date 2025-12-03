using System;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using BlockManager.Abstractions;

namespace BlockManager.Adapter._2010
{
    /// <summary>
    /// AutoCAD 2010 块库服务实现
    /// </summary>
    public class Cad2010BlockLibraryService : IBlockLibraryService
    {
        public void ShowBlockLibraryViewer()
        {
            // 此方法已不再使用，现在通过IPC和现代化UI实现
            ShowMessage("请使用 BLOCKVIEWER 命令启动现代化块库浏览器");
        }

        public void InsertDwgBlock(string dwgPath, string blockName)
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                
                // 确保路径格式正确
                string normalizedPath = Path.GetFullPath(dwgPath);
                
                // 检查文件是否存在
                if (!File.Exists(normalizedPath))
                {
                                        return;
                }
                
                // 使用 LISP 命令格式来执行 INSERT 命令
                string escapedPath = normalizedPath.Replace("\\", "\\\\");
                string commandStr = $"(command \"_INSERT\" \"{escapedPath}\" pause \"\" \"\" \"\") \n";
                
                doc.SendStringToExecute(commandStr, false, false, false);
                
                            }
            catch (Exception ex)
            {
                ShowMessage($"执行 DWG 插入命令时发生错误: {ex.Message}");
            }
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public void ShowMessage(string message)
        {
            try
            {
                var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                            }
            catch
            {
                // 如果无法访问编辑器，忽略错误
            }
        }
    }
}
