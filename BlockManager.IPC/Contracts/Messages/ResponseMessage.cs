namespace BlockManager.IPC.Contracts.Messages
{
    /// <summary>
    /// 响应消息
    /// </summary>
    public class ResponseMessage : MessageBase
    {
        /// <summary>
        /// 响应数据载荷
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// 错误信息（可选）
        /// </summary>
        public ErrorInfo? Error { get; set; }

        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool IsSuccess => Error == null;

        public ResponseMessage()
        {
            MessageType = "RESPONSE";
        }
    }

    /// <summary>
    /// 错误信息
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 错误详情
        /// </summary>
        public string? Details { get; set; }
    }
}
