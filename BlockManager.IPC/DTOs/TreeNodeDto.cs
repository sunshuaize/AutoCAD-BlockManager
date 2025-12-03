using System;
using System.Collections.Generic;

namespace BlockManager.IPC.DTOs
{
    /// <summary>
    /// 文件树节点数据传输对象
    /// </summary>
    public class TreeNodeDto
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 节点路径
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 节点类型：folder 或 file
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 子节点集合
        /// </summary>
        public List<TreeNodeDto> Children { get; set; } = new List<TreeNodeDto>();

        /// <summary>
        /// 文件信息（仅当Type为file时有效）
        /// </summary>
        public FileInfoDto? FileInfo { get; set; }

        /// <summary>
        /// 图标类型：folder、dwg、image、file
        /// </summary>
        public string IconType { get; set; } = string.Empty;
    }
}
