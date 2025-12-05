using System;

namespace BlockManager.Abstractions
{
    /// <summary>
    /// CAD命令服务接口 - 定义CAD命令执行的合同
    /// </summary>
    public interface ICADCommandService
    {
        /// <summary>
        /// 执行CAD命令
        /// </summary>
        /// <param name="command">命令字符串</param>
        void ExecuteCommand(string command);

        /// <summary>
        /// 获取当前工作目录
        /// </summary>
        /// <returns>当前工作目录路径</returns>
        string GetCurrentDirectory();
    }
}
