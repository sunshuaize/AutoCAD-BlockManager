using System;

namespace BlockManager.Abstractions
{
    /// <summary>
    /// 块库服务接口 - 定义块库浏览和插入的核心合同
    /// </summary>
    public interface IBlockLibraryService
    {
        /// <summary>
        /// 显示块库浏览器
        /// </summary>
        void ShowBlockLibraryViewer();

        /// <summary>
        /// 插入DWG文件作为块
        /// </summary>
        /// <param name="dwgPath">DWG文件路径</param>
        /// <param name="blockName">块名称</param>
        void InsertDwgBlock(string dwgPath, string blockName);

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件是否存在</returns>
        bool FileExists(string filePath);

        /// <summary>
        /// 向用户显示消息
        /// </summary>
        /// <param name="message">消息内容</param>
        void ShowMessage(string message);
    }
}
