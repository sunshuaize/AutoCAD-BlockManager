using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System;

namespace BlockManager.Adapter._2024
{
    /// <summary>
    /// 简单的块预览Jig类
    /// </summary>
    public class SimpleBlockJig : EntityJig
    {
        private Point3d _insertionPoint;
        private double _scale;
        private double _rotation;
        private string _lastKeyword = "";
        
        public Point3d InsertionPoint => _insertionPoint;
        public string LastKeyword => _lastKeyword;
        
        public SimpleBlockJig(ObjectId blockId, double scale, double rotation) : base(new BlockReference(Point3d.Origin, blockId))
        {
            _scale = scale;
            _rotation = rotation;
            
            // 设置块参照的初始属性
            BlockReference blockRef = Entity as BlockReference;
            blockRef.ScaleFactors = new Scale3d(_scale, _scale, _scale);
            blockRef.Rotation = _rotation;
        }
        
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions options = new JigPromptPointOptions("\n指定插入点或 [基点(B)/缩放(S)/X/Y/Z/旋转(R)]: ");
            options.UserInputControls = UserInputControls.Accept3dCoordinates;
            options.Keywords.Add("B", "B", "基点(B)");
            options.Keywords.Add("S", "S", "缩放(S)");
            options.Keywords.Add("X", "X", "X");
            options.Keywords.Add("Y", "Y", "Y");
            options.Keywords.Add("Z", "Z", "Z");
            options.Keywords.Add("R", "R", "旋转(R)");
            
            PromptPointResult result = prompts.AcquirePoint(options);
            
            if (result.Status == PromptStatus.OK)
            {
                Point3d currentPoint = result.Value;
                
                if (_insertionPoint.DistanceTo(currentPoint) < Tolerance.Global.EqualPoint)
                    return SamplerStatus.NoChange;
                
                _insertionPoint = currentPoint;
                return SamplerStatus.OK;
            }
            else if (result.Status == PromptStatus.Keyword)
            {
                _lastKeyword = result.StringResult;
                return SamplerStatus.OK;
            }
            
            return SamplerStatus.Cancel;
        }
        
        protected override bool Update()
        {
            try
            {
                BlockReference blockRef = Entity as BlockReference;
                blockRef.Position = _insertionPoint;
                blockRef.ScaleFactors = new Scale3d(_scale, _scale, _scale);
                blockRef.Rotation = _rotation;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
