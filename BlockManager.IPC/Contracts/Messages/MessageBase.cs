using System;

namespace BlockManager.IPC.Contracts.Messages
{
    /// <summary>
    /// 消息基类
    /// </summary>
    public abstract class MessageBase
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 消息类型
        /// </summary>
        public string MessageType { get; set; } = string.Empty;

        /// <summary>
        /// 操作类型
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
