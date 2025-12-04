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

                ed.WriteMessage($"\n[2010] 从块库浏览器请求插入 DWG 块: {blockName}");
                
                // 确保路径格式正确
                string normalizedPath = Path.GetFullPath(dwgPath);
                
                // 检查文件是否存在
                if (!File.Exists(normalizedPath))
                {
                    ed.WriteMessage($"\n[2010] 错误：文件不存在 - {normalizedPath}");
                    ShowMessage($"错误：文件不存在 - {normalizedPath}");
                    return;
                }
                
                ed.WriteMessage($"\n[2010] 准备插入文件: {normalizedPath}");
                
                // 使用简单的INSERT命令
                string escapedPath = normalizedPath.Replace("\\", "/");
                
                ed.WriteMessage($"\n[2010] 执行INSERT命令，文件: {escapedPath}");
                
                // 使用最简单的INSERT命令格式
                string commandStr = $"_INSERT \"{escapedPath}\" ";
                ed.WriteMessage($"\n[2010] 发送命令: {commandStr}");
                
                doc.SendStringToExecute(commandStr, true, false, false);
                
                ed.WriteMessage($"\n[2010] ✅ 已启动 INSERT 命令: {blockName}");
                ShowMessage($"已启动插入命令，请在AutoCAD中指定插入点");
            }
            catch (Exception ex)
            {
                var errorMsg = $"执行 DWG 插入命令时发生错误: {ex.Message}";
                ShowMessage(errorMsg);
                
                var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\n[2010] ❌ {errorMsg}");
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
                ed?.WriteMessage($"\n{message}");
            }
            catch
            {
                // 如果无法访问编辑器，忽略错误
            }
        }
    }
}
