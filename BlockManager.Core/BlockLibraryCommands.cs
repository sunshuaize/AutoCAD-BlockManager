using System;
using System.Windows.Forms;
using BlockManager.Abstractions;

namespace BlockManager.Core
{
    public class BlockLibraryCommands
    {
        private static IBlockLibraryService _blockLibraryService;

        /// <summary>
        /// 设置块库服务实现
        /// </summary>
        /// <param name="service">块库服务实现</param>
        public static void SetBlockLibraryService(IBlockLibraryService service)
        {
            _blockLibraryService = service;
        }

        /// <summary>
        /// 显示块库浏览器
        /// </summary>
        public static void ShowBlockLibraryViewer()
        {
            try
            {
                if (_blockLibraryService != null)
                {
                    _blockLibraryService.ShowBlockLibraryViewer();
                }
                else
                {
                    // 如果没有服务实现，直接显示UI
                    var viewer = new BlockLibraryViewer();
                    viewer.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动块库浏览器时发生错误: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 显示块库浏览器（非模态）
        /// </summary>
        public static void ShowBlockLibraryViewerModeless()
        {
            ShowBlockLibraryViewer(); // 统一调用
        }
    }
}
