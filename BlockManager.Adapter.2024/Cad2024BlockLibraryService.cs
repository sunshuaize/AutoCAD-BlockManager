using System;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using BlockManager.Abstractions;

namespace BlockManager.Adapter._2024
{
    /// <summary>
    /// AutoCAD 2024环境下的块库服务实现
    /// </summary>
    public class Cad2024BlockLibraryService : IBlockLibraryService
    {
        /// <summary>
        /// 显示块库浏览器（已不再使用，由BLOCKVIEWER命令处理）
        /// </summary>
        public void ShowBlockLibraryViewer()
        {
            // 此方法已不再使用，块库浏览器通过BLOCKVIEWER命令启动
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件是否存在</returns>
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// AutoCAD 2024 插入DWG块的实现
        /// 使用现代的BlockTableRecord和Entity方法
        /// </summary>
        /// <param name="dwgFilePath">DWG文件路径</param>
        /// <param name="blockName">块名称</param>
        /// <returns>插入是否成功</returns>
        public bool InsertDwgBlock(string dwgFilePath, string blockName = null)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc?.Database;
            var ed = doc?.Editor;
            
            if (doc == null || db == null || ed == null)
            {
                return false;
            }
            
            try
            {
                // 检查文件是否存在
                if (!File.Exists(dwgFilePath))
                {
                    ed.WriteMessage($"\n[2024块服务] 错误：找不到文件 {dwgFilePath}");
                    return false;
                }
                
                // 如果没有指定块名，使用文件名
                if (string.IsNullOrEmpty(blockName))
                {
                    blockName = Path.GetFileNameWithoutExtension(dwgFilePath);
                }
                
                ed.WriteMessage($"\n[2024块服务] 尝试导入块: {blockName}");
                
                // 使用你的demo实现方式
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    // 获取块表
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    
                    // 检查块是否已存在
                    if (!bt.Has(blockName))
                    {
                        // 创建外部数据库对象
                        using (Database sourceDb = new Database(false, true))
                        {
                            // 读取外部DWG文件
                            sourceDb.ReadDwgFile(dwgFilePath, FileShare.Read, true, "");
                            
                            // 使用Insert方法插入整个DWG文件
                            bt.UpgradeOpen();
                            ObjectId blockId = db.Insert(blockName, sourceDb, false);
                            bt.DowngradeOpen();
                            ed.WriteMessage($"\n[2024块服务] 成功导入块定义: {blockName}");
                        }
                    }
                    else
                    {
                        ed.WriteMessage($"\n[2024块服务] 块 '{blockName}' 已存在，跳过导入");
                    }
                    
                    tr.Commit();
                    ed.WriteMessage($"\n[2024块服务] 块 '{blockName}' 准备完成，可使用INSERT命令插入");
                    return true;
                }
            }
            catch (Exception ex)
            {
                ed?.WriteMessage($"\n[2024块服务] 插入块失败: {ex.Message}");
                return false;
            }
        }
    }
}
