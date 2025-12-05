using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using BlockManager.Abstractions;

namespace BlockManager.Adapter._2010
{
    /// <summary>
    /// AutoCAD 2010 CAD命令服务实现
    /// </summary>
    public class Cad2010CADCommandService : ICADCommandService
    {
        public void ExecuteCommand(string command)
        {
            try
            {
                WriteMessage($"[2010命令服务] 执行命令: {command}");
                
                // 检查是否是插入块命令
                if (command.StartsWith("INSERT_BLOCK"))
                {
                    HandleInsertBlockCommand(command);
                    return;
                }
                
                // 处理其他常规命令
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    doc.SendStringToExecute(command, true, false, true);
                    WriteMessage($"[2010命令服务] 命令已发送");
                }
                else
                {
                    WriteMessage("[2010命令服务] 错误: 无法获取当前活动文档");
                }
            }
            catch (Exception ex)
            {
                WriteMessage($"[2010命令服务] 执行命令时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理插入块命令
        /// </summary>
        /// <param name="command">INSERT_BLOCK命令</param>
        private void HandleInsertBlockCommand(string command)
        {
            try
            {
                WriteMessage($"[2010命令服务] 处理插入块命令: {command}");
                
                // TODO: 解析命令参数，提取文件路径和块名
                // TODO: 调用块库服务的InsertDwgBlock方法
                // TODO: 返回执行结果
                
                WriteMessage($"[2010命令服务] 插入块命令处理完成");
            }
            catch (Exception ex)
            {
                WriteMessage($"[2010命令服务] 处理插入块命令失败: {ex.Message}");
            }
        }

        public void WriteMessage(string message)
        {
            try
            {
                // 尝试使用Editor输出日志
                var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                if (ed != null)
                {
                    ed.WriteMessage($"\n[CAD命令服务] {message}");
                }
                
                // 同时使用Debug输出日志，以防Editor不可用
                System.Diagnostics.Debug.WriteLine($"[CAD命令服务] {message}");
            }
            catch (Exception ex)
            {
                // 如果无法访问编辑器，输出到Debug
                System.Diagnostics.Debug.WriteLine($"[CAD命令服务] 无法写入消息: {message}");
                System.Diagnostics.Debug.WriteLine($"[CAD命令服务] 错误: {ex.Message}");
            }
        }

        public string GetCurrentDirectory()
        {
            return Environment.CurrentDirectory;
        }
    }
}
