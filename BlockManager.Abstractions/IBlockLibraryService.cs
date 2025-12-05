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
        /// 检查文件是否存在
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件是否存在</returns>
        bool FileExists(string filePath);

        /// <summary>
        /// 插入DWG文件作为块到当前图纸
        /// </summary>
        /// <param name="dwgFilePath">DWG文件路径</param>
        /// <param name="blockName">块名称（可选，如果为空则使用文件名）</param>
        /// <returns>插入是否成功</returns>
        bool InsertDwgBlock(string dwgFilePath, string blockName = null);

    }
}
