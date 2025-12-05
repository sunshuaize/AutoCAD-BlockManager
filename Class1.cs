using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Test
{
    public class Class1
    {
        [CommandMethod("TianJia")]
        public void SayHello()
        {
            // 获取当前的文档对象
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;
            
            string blockPath = @"C:\Users\PC\Desktop\Block\围护结构\700x900支撑配筋断面.dwg";
            
            try
            {
                // 检查文件是否存在
                if (!File.Exists(blockPath))
                {
                    ed.WriteMessage($"\n错误：找不到文件 {blockPath}");
                    return;
                }
                
                Point3d insertionPoint;
                
                // 声明需要在事务外使用的变量
                string blockName = Path.GetFileNameWithoutExtension(blockPath);
                ObjectId blockId;
                double scale = 1.0;
                double rotation = 0.0;
                
                ed.WriteMessage($"\n尝试导入块: {blockName}");
                
                // 开始事务
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    // 获取块表
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    
                    if (bt.Has(blockName))
                    {
                        // 如果块已存在，直接使用
                        blockId = bt[blockName];
                        ed.WriteMessage($"\n使用已存在的块: {blockName}");
                    }
                    else
                    {
                        // 如果块不存在，从外部文件导入
                        using (Database sourceDb = new Database(false, true))
                        {
                            sourceDb.ReadDwgFile(blockPath, FileShare.Read, true, "");
                            
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
                insertionPoint = GetInsertionPointWithOptions(ed, blockId, ref scale, ref rotation);
                
                if (insertionPoint != Point3d.Origin)
                {
                    // 重新开始事务来插入块
                    using (Transaction tr2 = db.TransactionManager.StartTransaction())
                    {
                        // 创建块参照并应用参数
                        BlockReference blockRef = new BlockReference(insertionPoint, blockId);
                        blockRef.ScaleFactors = new Scale3d(scale, scale, scale);
                        blockRef.Rotation = rotation;
                        
                        // 获取模型空间
                        BlockTable bt2 = tr2.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord ms = tr2.GetObject(bt2[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                        
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
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n错误: {ex.Message}");
            }
        }
        
        // 获取插入点并处理选项的方法
        private Point3d GetInsertionPointWithOptions(Editor ed, ObjectId blockId, ref double scale, ref double rotation)
        {
            while (true)
            {
                // 使用Jig进行预览
                SimpleBlockJig jig = new SimpleBlockJig(blockId, scale, rotation);
                PromptResult jigResult = ed.Drag(jig);
                
                if (jigResult.Status == PromptStatus.OK)
                {
                    // 用户选择了插入点，直接返回
                    return jig.InsertionPoint;
                }
                else if (jigResult.Status == PromptStatus.Keyword)
                {
                    // 处理关键字输入
                    string keyword = jig.LastKeyword;
                    if (ProcessKeyword(ed, keyword, ref scale, ref rotation))
                    {
                        // 参数已更新，继续循环显示预览
                        continue;
                    }
                    else
                    {
                        // 用户取消
                        return Point3d.Origin;
                    }
                }
                else
                {
                    // 用户取消或出错
                    return Point3d.Origin;
                }
            }
        }
        
        // 处理关键字的方法
        private bool ProcessKeyword(Editor ed, string keyword, ref double scale, ref double rotation)
        {
            switch (keyword.ToUpper())
            {
                case "S": // 缩放
                    PromptDoubleOptions scaleOpt = new PromptDoubleOptions($"\n指定缩放因子 <{scale}>: ");
                    scaleOpt.DefaultValue = scale;
                    scaleOpt.UseDefaultValue = true;
                    scaleOpt.AllowNegative = false;
                    scaleOpt.AllowZero = false;
                    PromptDoubleResult scaleResult = ed.GetDouble(scaleOpt);
                    if (scaleResult.Status == PromptStatus.OK)
                        scale = scaleResult.Value;
                    return true;
                    
                case "R": // 旋转
                    PromptAngleOptions angleOpt = new PromptAngleOptions($"\n指定旋转角度 <{rotation * 180 / Math.PI:F1}>: ");
                    angleOpt.DefaultValue = rotation;
                    angleOpt.UseDefaultValue = true;
                    PromptDoubleResult angleResult = ed.GetAngle(angleOpt);
                    if (angleResult.Status == PromptStatus.OK)
                        rotation = angleResult.Value;
                    return true;
                    
                case "X": // X轴缩放
                    PromptDoubleOptions xScaleOpt = new PromptDoubleOptions($"\n指定X轴缩放因子 <{scale}>: ");
                    xScaleOpt.DefaultValue = scale;
                    xScaleOpt.UseDefaultValue = true;
                    xScaleOpt.AllowNegative = false;
                    xScaleOpt.AllowZero = false;
                    PromptDoubleResult xScaleResult = ed.GetDouble(xScaleOpt);
                    if (xScaleResult.Status == PromptStatus.OK)
                        scale = xScaleResult.Value;
                    return true;
                    
                case "Y": // Y轴缩放
                    PromptDoubleOptions yScaleOpt = new PromptDoubleOptions($"\n指定Y轴缩放因子 <{scale}>: ");
                    yScaleOpt.DefaultValue = scale;
                    yScaleOpt.UseDefaultValue = true;
                    yScaleOpt.AllowNegative = false;
                    yScaleOpt.AllowZero = false;
                    PromptDoubleResult yScaleResult = ed.GetDouble(yScaleOpt);
                    if (yScaleResult.Status == PromptStatus.OK)
                        scale = yScaleResult.Value;
                    return true;
                    
                case "Z": // Z轴缩放
                    PromptDoubleOptions zScaleOpt = new PromptDoubleOptions($"\n指定Z轴缩放因子 <{scale}>: ");
                    zScaleOpt.DefaultValue = scale;
                    zScaleOpt.UseDefaultValue = true;
                    zScaleOpt.AllowNegative = false;
                    zScaleOpt.AllowZero = false;
                    PromptDoubleResult zScaleResult = ed.GetDouble(zScaleOpt);
                    if (zScaleResult.Status == PromptStatus.OK)
                        scale = zScaleResult.Value;
                    return true;
                    
                case "B": // 基点
                    ed.WriteMessage("\n基点功能暂未实现");
                    return true;
                    
                default:
                    return false;
            }
        }
    }
    
    // 简单的块预览Jig类
    public class SimpleBlockJig : EntityJig
    {
        private Point3d _insertionPoint;
        private double _scale;
        private double _rotation;
        private string _lastKeyword = "";
        
        public Point3d InsertionPoint => _insertionPoint;
        public string LastKeyword => _lastKeyword;
        
        public SimpleBlockJig(ObjectId blockId, double scale, double rotation) : base(new BlockReference(Point3d.Origin, blockId))
        {
            _scale = scale;
            _rotation = rotation;
            
            // 设置块参照的初始属性
            BlockReference blockRef = Entity as BlockReference;
            blockRef.ScaleFactors = new Scale3d(_scale, _scale, _scale);
            blockRef.Rotation = _rotation;
        }
        
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions options = new JigPromptPointOptions("\n指定插入点或 [基点(B)/缩放(S)/X/Y/Z/旋转(R)]: ");
            options.UserInputControls = UserInputControls.Accept3dCoordinates;
            options.Keywords.Add("B", "B", "基点(B)");
            options.Keywords.Add("S", "S", "缩放(S)");
            options.Keywords.Add("X", "X", "X");
            options.Keywords.Add("Y", "Y", "Y");
            options.Keywords.Add("Z", "Z", "Z");
            options.Keywords.Add("R", "R", "旋转(R)");
            
            PromptPointResult result = prompts.AcquirePoint(options);
            
            if (result.Status == PromptStatus.OK)
            {
                Point3d currentPoint = result.Value;
                
                if (_insertionPoint.DistanceTo(currentPoint) < Tolerance.Global.EqualPoint)
                    return SamplerStatus.NoChange;
                
                _insertionPoint = currentPoint;
                return SamplerStatus.OK;
            }
            else if (result.Status == PromptStatus.Keyword)
            {
                _lastKeyword = result.StringResult;
                return SamplerStatus.OK;
            }
            
            return SamplerStatus.Cancel;
        }
        
        protected override bool Update()
        {
            try
            {
                BlockReference blockRef = Entity as BlockReference;
                blockRef.Position = _insertionPoint;
                blockRef.ScaleFactors = new Scale3d(_scale, _scale, _scale);
                blockRef.Rotation = _rotation;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    
}
