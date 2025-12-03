namespace BlockManager.IPC.Contracts.Messages
{
    /// <summary>
    /// 请求消息
    /// </summary>
    public class RequestMessage : MessageBase
    {
        /// <summary>
        /// 请求数据载荷
        /// </summary>
        public object? Data { get; set; }

        public RequestMessage()
        {
            MessageType = "REQUEST";
        }
    }
}
