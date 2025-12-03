namespace BlockManager.IPC.DTOs
{
    /// <summary>
    /// 插入块请求数据传输对象
    /// </summary>
    public class InsertBlockRequest
    {
        /// <summary>
        /// 块文件路径
        /// </summary>
        public string BlockPath { get; set; } = string.Empty;

        /// <summary>
        /// 块名称
        /// </summary>
        public string BlockName { get; set; } = string.Empty;

        /// <summary>
        /// 插入点（可选，用于未来扩展）
        /// </summary>
        public Point3D? InsertionPoint { get; set; }

        /// <summary>
        /// 缩放比例（可选，用于未来扩展）
        /// </summary>
        public Scale3D? Scale { get; set; }

        /// <summary>
        /// 旋转角度（可选，用于未来扩展）
        /// </summary>
        public double? Rotation { get; set; }
    }

    /// <summary>
    /// 3D点坐标
    /// </summary>
    public class Point3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    /// <summary>
    /// 3D缩放比例
    /// </summary>
    public class Scale3D
    {
        public double X { get; set; } = 1.0;
        public double Y { get; set; } = 1.0;
        public double Z { get; set; } = 1.0;
    }
}
