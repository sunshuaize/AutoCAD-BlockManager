using System;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using BlockManager.Abstractions;

namespace BlockManager.Adapter._2010
{
    /// <summary>
    /// AutoCAD 2010环境下的块库服务实现
    /// </summary>
    public class Cad2010BlockLibraryService : IBlockLibraryService
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
        /// AutoCAD 2010 插入DWG块的实现
        /// 使用传统的Database.Insert方法
        /// </summary>
        /// <param name="dwgFilePath">DWG文件路径</param>
        /// <param name="blockName">块名称</param>
        /// <returns>插入是否成功</returns>
        public bool InsertDwgBlock(string dwgFilePath, string blockName = null)
        {
            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage($"\n[2010块服务] TODO: 插入DWG块 - {dwgFilePath}");
            
            // TODO: 实现2010版本的块插入逻辑
            // TODO: 使用Database.Insert方法
            
            return false;
        }
    }
}
