using System;

namespace BlockManager.IPC.DTOs
{
    /// <summary>
    /// 文件信息数据传输对象
    /// </summary>
    public class FileInfoDto
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 完整路径
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// 是否有预览图
        /// </summary>
        public bool HasPreview { get; set; }

        /// <summary>
        /// 预览图路径（DWG对应的PNG路径）
        /// </summary>
        public string PreviewPath { get; set; } = string.Empty;
    }
}
