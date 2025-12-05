using System;

namespace BlockManager.IPC.DTOs
{
    /// <summary>
    /// 命令执行响应DTO
    /// </summary>
    public class CommandExecutionResponse
    {
        /// <summary>
        /// 执行是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 执行结果（可选）
        /// </summary>
        public string? Result { get; set; }

        /// <summary>
        /// 错误消息（当IsSuccess为false时）
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 命令执行时间戳
        /// </summary>
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 执行耗时（毫秒）
        /// </summary>
        public long ExecutionTimeMs { get; set; }
    }
}
