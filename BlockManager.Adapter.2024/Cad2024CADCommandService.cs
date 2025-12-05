using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using BlockManager.Abstractions;
using System;

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
                WriteMessage($"[2024命令服务] 执行命令: {command}");

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
                    doc.SendStringToExecute(command, false, false, false);
                    WriteMessage($"[2024命令服务] 命令已发送");
                }
                else
                {
                    WriteMessage("[2024命令服务] 错误: 无法获取当前活动文档");
                }
            }
            catch (Exception ex)
            {
                WriteMessage($"[2024命令服务] 执行命令时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理插入块命令
        /// </summary>
        /// <param name="command">INSERT_BLOCK命令</param>
        public void HandleInsertBlockCommand(string command)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            var db = doc.Database;
            var ed = doc.Editor;

            try
            {
                // 移除命令前缀并分割参数
                string args = command.Substring("INSERT_BLOCK".Length).Trim();

                // 使用正则表达式匹配带引号的参数
                var matches = System.Text.RegularExpressions.Regex.Matches(args, @"\""(.*?)\""");
                if (matches.Count < 2)
                {
                    ed.WriteMessage("\n错误: 参数格式不正确。使用格式: INSERT_BLOCK \"文件路径\" \"块名\"");
                    return;
                }

                string filePath = matches[0].Groups[1].Value;
                string blockName = matches[1].Groups[1].Value;

                // 检查文件是否存在
                if (!System.IO.File.Exists(filePath))
                {
                    ed.WriteMessage($"\n错误: 文件不存在: {filePath}");
                    return;
                }

                // 使用文档锁定来调用插入块方法（关键：IPC调用需要文档锁）
                using (DocumentLock docLock = doc.LockDocument())
                {
                    InsertBlockFromPath(filePath, blockName);
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n处理命令时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 从路径插入块（复刻Class1的实现）
        /// </summary>
        private void InsertBlockFromPath(string blockPath, string blockName = null)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc?.Database;
            var ed = doc?.Editor;

            if (doc == null || db == null || ed == null)
            {
                WriteMessage("[2024命令服务] 错误: 无法获取当前文档");
                return;
            }

            try
            {
                // 检查文件是否存在
                if (!System.IO.File.Exists(blockPath))
                {
                    ed.WriteMessage($"\n错误：找不到文件 {blockPath}");
                    return;
                }

                // 如果没有指定块名，使用文件名
                if (string.IsNullOrEmpty(blockName))
                {
                    blockName = System.IO.Path.GetFileNameWithoutExtension(blockPath);
                }

                ed.WriteMessage($"\n尝试导入块: {blockName}");

                // 声明需要在事务外使用的变量
                Autodesk.AutoCAD.DatabaseServices.ObjectId blockId = Autodesk.AutoCAD.DatabaseServices.ObjectId.Null;

                // 开始事务
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    // 获取块表
                    var bt = tr.GetObject(db.BlockTableId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.BlockTable;
                    
                    if (bt.Has(blockName))
                    {
                        // 如果块已存在，直接使用
                        blockId = bt[blockName];
                        ed.WriteMessage($"\n使用已存在的块: {blockName}");
                    }
                    else
                    {
                        // 如果块不存在，从外部文件导入
                        using (var sourceDb = new Autodesk.AutoCAD.DatabaseServices.Database(false, true))
                        {
                            sourceDb.ReadDwgFile(blockPath, System.IO.FileShare.Read, true, "");
                            
                            // 使用Insert方法插入整个DWG文件
                            bt.UpgradeOpen();
                            blockId = db.Insert(blockName, sourceDb, false);
                            bt.DowngradeOpen();
                            ed.WriteMessage($"\n成功导入块定义: {blockName}");
                        }
                    }
                    
                    // 提交当前事务以确保块定义已导入
                    tr.Commit();
                }

                // 使用命令行方式交互，类似原生INSERT命令
                double scale = 1.0;
                double rotation = 0.0;
                var insertionPoint = GetInsertionPointWithOptions(ed, blockId, ref scale, ref rotation);

                if (insertionPoint != Autodesk.AutoCAD.Geometry.Point3d.Origin)
                {
                    // 重新开始事务来插入块
                    using (Autodesk.AutoCAD.DatabaseServices.Transaction tr2 = db.TransactionManager.StartTransaction())
                    {
                        // 创建块参照并应用参数
                        Autodesk.AutoCAD.DatabaseServices.BlockReference blockRef = new Autodesk.AutoCAD.DatabaseServices.BlockReference(insertionPoint, blockId);
                        blockRef.ScaleFactors = new Autodesk.AutoCAD.Geometry.Scale3d(scale, scale, scale);
                        blockRef.Rotation = rotation;

                        // 获取模型空间
                        Autodesk.AutoCAD.DatabaseServices.BlockTable bt2 = tr2.GetObject(db.BlockTableId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.BlockTable;
                        Autodesk.AutoCAD.DatabaseServices.BlockTableRecord ms = tr2.GetObject(bt2[Autodesk.AutoCAD.DatabaseServices.BlockTableRecord.ModelSpace], Autodesk.AutoCAD.DatabaseServices.OpenMode.ForWrite) as Autodesk.AutoCAD.DatabaseServices.BlockTableRecord;

                        // 添加块参照到模型空间
                        ms.AppendEntity(blockRef);
                        tr2.AddNewlyCreatedDBObject(blockRef, true);

                        // 提交事务
                        tr2.Commit();

                        ed.WriteMessage($"\n成功插入块: {blockName}，缩放: {scale:F2}，旋转: {rotation * 180 / Math.PI:F1}°");
                    }
                }
                else
                {
                    ed.WriteMessage("\n操作已取消");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"插入块失败: {ex.ToString()}");
                ed?.WriteMessage($"\n插入块失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取插入点并处理选项（复刻Class1的实现）
        /// </summary>
        private Autodesk.AutoCAD.Geometry.Point3d GetInsertionPointWithOptions(Autodesk.AutoCAD.EditorInput.Editor ed, Autodesk.AutoCAD.DatabaseServices.ObjectId blockId, ref double scale, ref double rotation)
        {
            while (true)
            {
                try
                {
                    // 使用Jig进行预览（每次循环都重新创建）
                    SimpleBlockJig jig = new SimpleBlockJig(blockId, scale, rotation);
                    var jigResult = ed.Drag(jig);

                    if (jigResult.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                    {
                        // 用户选择了插入点，直接返回
                        return jig.InsertionPoint;
                    }
                    else if (jigResult.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.Keyword)
                    {
                        // 处理关键字输入
                        string keyword = jig.LastKeyword;
                        if (HandleKeyword(ed, keyword, ref scale, ref rotation))
                        {
                            // 参数已更新，继续循环显示预览
                            continue;
                        }
                        else
                        {
                            // 用户取消
                            return Autodesk.AutoCAD.Geometry.Point3d.Origin;
                        }
                    }
                    else
                    {
                        // 用户取消或出错
                        return Autodesk.AutoCAD.Geometry.Point3d.Origin;
                    }
                }
                catch (Exception ex)
                {
                    ed.WriteMessage($"\n交互式插入出错: {ex.Message}");
                    return Autodesk.AutoCAD.Geometry.Point3d.Origin;
                }
            }
        }

        /// <summary>
        /// 处理关键字选项（完全复刻Class1的ProcessKeyword实现）
        /// </summary>
        private bool HandleKeyword(Autodesk.AutoCAD.EditorInput.Editor ed, string keyword, ref double scale, ref double rotation)
        {
            switch (keyword.ToUpper())
            {
                case "S": // 缩放
                    var scaleOpt = new Autodesk.AutoCAD.EditorInput.PromptDoubleOptions($"\n指定缩放因子 <{scale}>: ");
                    scaleOpt.DefaultValue = scale;
                    scaleOpt.UseDefaultValue = true;
                    scaleOpt.AllowNegative = false;
                    scaleOpt.AllowZero = false;
                    var scaleResult = ed.GetDouble(scaleOpt);
                    if (scaleResult.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                        scale = scaleResult.Value;
                    return true;

                case "R": // 旋转
                    var angleOpt = new Autodesk.AutoCAD.EditorInput.PromptAngleOptions($"\n指定旋转角度 <{rotation * 180 / Math.PI:F1}>: ");
                    angleOpt.DefaultValue = rotation;
                    angleOpt.UseDefaultValue = true;
                    var angleResult = ed.GetAngle(angleOpt);
                    if (angleResult.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                        rotation = angleResult.Value;
                    return true;

                case "X": // X轴缩放
                    var xScaleOpt = new Autodesk.AutoCAD.EditorInput.PromptDoubleOptions($"\n指定X轴缩放因子 <{scale}>: ");
                    xScaleOpt.DefaultValue = scale;
                    xScaleOpt.UseDefaultValue = true;
                    xScaleOpt.AllowNegative = false;
                    xScaleOpt.AllowZero = false;
                    var xScaleResult = ed.GetDouble(xScaleOpt);
                    if (xScaleResult.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                        scale = xScaleResult.Value;
                    return true;

                case "Y": // Y轴缩放
                    var yScaleOpt = new Autodesk.AutoCAD.EditorInput.PromptDoubleOptions($"\n指定Y轴缩放因子 <{scale}>: ");
                    yScaleOpt.DefaultValue = scale;
                    yScaleOpt.UseDefaultValue = true;
                    yScaleOpt.AllowNegative = false;
                    yScaleOpt.AllowZero = false;
                    var yScaleResult = ed.GetDouble(yScaleOpt);
                    if (yScaleResult.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                        scale = yScaleResult.Value;
                    return true;

                case "Z": // Z轴缩放
                    var zScaleOpt = new Autodesk.AutoCAD.EditorInput.PromptDoubleOptions($"\n指定Z轴缩放因子 <{scale}>: ");
                    zScaleOpt.DefaultValue = scale;
                    zScaleOpt.UseDefaultValue = true;
                    zScaleOpt.AllowNegative = false;
                    zScaleOpt.AllowZero = false;
                    var zScaleResult = ed.GetDouble(zScaleOpt);
                    if (zScaleResult.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                        scale = zScaleResult.Value;
                    return true;

                case "B": // 基点
                    ed.WriteMessage("\n基点功能暂未实现");
                    return true;

                default:
                    return false;
            }
        }

        public void WriteMessage(string message)
        {
            try
            {
                var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                if (ed != null)
                {
                    ed.WriteMessage($"\n[CAD命令服务] {message}");
                }
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