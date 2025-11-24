using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using BlockManager.Abstractions;
using BlockManager.Core;
using System;
using Exception = System.Exception;

namespace BlockManager.Adapter._2010
{
    public class BlockInsertCommands
    {
        private static IBlockLibraryService _blockLibraryService;
        private static ICADCommandService _cadCommandService;

        static BlockInsertCommands()
        {
            // 初始化服务实现
            _blockLibraryService = new Cad2010BlockLibraryService();
            _cadCommandService = new Cad2010CADCommandService();
            
            // 设置 Core 项目的服务依赖
            BlockLibraryCommands.SetBlockLibraryService(_blockLibraryService);
        }

        [CommandMethod("BLOCKVIEWER")]
        public void ShowBlockViewer()
        {
            try
            {
                // 注册向后兼容的事件处理（如果需要）
                BlockLibraryViewer.OnDwgBlockInsertRequested += HandleDwgBlockInsertRequest;
                
                // 通过服务显示块库浏览器
                _blockLibraryService.ShowBlockLibraryViewer();
            }
            catch (Exception ex)
            {
                var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\n启动块库浏览器时发生错误: {ex.Message}");
                
                // 只在出错时取消注册
                BlockLibraryViewer.OnDwgBlockInsertRequested -= HandleDwgBlockInsertRequest;
            }
        }

        private void HandleDwgBlockInsertRequest(string dwgPath, string blockName)
        {
            try
            {
                // 通过服务处理插入请求
                _blockLibraryService.InsertDwgBlock(dwgPath, blockName);
            }
            catch (Exception ex)
            {
                _blockLibraryService.ShowMessage($"处理 DWG 块插入请求时发生错误: {ex.Message}");
            }
        }
    }
}