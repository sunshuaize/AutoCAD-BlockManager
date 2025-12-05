using System.Collections.Generic;

namespace BlockManager.IPC.DTOs
{
    /// <summary>
    /// 命令执行请求DTO
    /// </summary>
    public class CommandExecutionRequest
    {
        /// <summary>
        /// 要执行的命令
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// 命令参数（可选）
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }

        /// <summary>
        /// 是否等待命令完成
        /// </summary>
        public bool WaitForCompletion { get; set; } = true;

        /// <summary>
        /// 命令超时时间（毫秒）
        /// </summary>
        public int TimeoutMs { get; set; } = 30000;
    }
}
