using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using BlockManager.Abstractions;

namespace BlockManager.Adapter._2024
{
    /// <summary>
    /// AutoCAD 2024 CAD命令服务实现
    /// </summary>
    public class Cad2024CADCommandService : ICADCommandService
    {
        public void ExecuteCommand(string command)
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                doc.SendStringToExecute(command, false, false, false);
            }
            catch (Exception ex)
            {
                WriteMessage($"执行命令时发生错误: {ex.Message}");
            }
        }

        public void WriteMessage(string message)
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

        public string GetCurrentDirectory()
        {
            return Environment.CurrentDirectory;
        }
    }
}
