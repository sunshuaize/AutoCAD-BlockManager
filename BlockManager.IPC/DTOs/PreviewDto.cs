using System;

namespace BlockManager.IPC.DTOs
{
    /// <summary>
    /// 预览数据传输对象
    /// </summary>
    public class PreviewDto
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 预览图片的Base64编码
        /// </summary>
        public string PreviewImageBase64 { get; set; } = string.Empty;

        /// <summary>
        /// 文件元数据
        /// </summary>
        public FileInfoDto? Metadata { get; set; }

        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 错误消息（当IsSuccess为false时）
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 预览图片路径
        /// </summary>
        public string PreviewImagePath { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastModified { get; set; }
    }
}
