using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Game;



namespace PatherExplorer
{
    public class RenderObjects : RenderGroup
    {
        private static Random Random = new Random(DateTime.Now.Millisecond);
        private static Pen[] LanesPen;
        public RenderObjects(System.Windows.Controls.Image viewport) : base(viewport)
        {
            LanesPen = GeneratePens(500);
        }

        private Pen[] GeneratePens(int i)
        {
            Pen[] ret = new Pen[i];
            for (int j = 0; j < i; j++)
            {
                ret[j] = new Pen(GetRandomColor(), 1);
            }

            return ret;
        }

        private static PatherExplorerData _curData;
        
        private static readonly Pen ActiveMosterPen = new Pen(Brushes.BlueViolet, 3);

        private static readonly Pen UnactiveMosterPen = new Pen(Brushes.Orange, 3);

        private static readonly Pen BlightPathPen = new Pen(Brushes.CornflowerBlue, 1);
        private static readonly Pen BestTargetPen = new Pen(Brushes.Black, 2);

        private static readonly Pen _arrowPen = new Pen(Color.DarkGoldenrod, 0.5f);

        private static Color GetRandomColor()
        {
            return Color.FromArgb(Random.Next(0, 255), Random.Next(0, 255), Random.Next(0, 255));
            // The error is here
        }
        private static List<Lane> CreateBlightLanes(List<CachedObject> blightPaths)
        {
            List<Lane> ret = new List<Lane>();
            CachedObject[] controllList = new CachedObject[blightPaths.Count];
            blightPaths.CopyTo(controllList);
            if (ProtopypeLaneFinder(ref blightPaths, ref ret, controllList.ToList()))
            {
                return ret;
            }

            return new List<Lane>();

        }
        private void CreateVisual()
        {
            try
            {
                if (_curData.IsInGame)
                {

                    int cols = _curData.CachedTerrainData.Cols * 23;
                    int rows = _curData.CachedTerrainData.Rows * 23;
                    Bitmap bitmap = new Bitmap(cols, rows);
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.InterpolationMode = PatherExplorerGui.InterpolationMode;
                        graphics.SmoothingMode = PatherExplorerGui.SmoothingMode;
                        graphics.PixelOffsetMode = PatherExplorerGui.PixelOffsetMode;
                        graphics.CompositingQuality = PatherExplorerGui.CompositingQuality;
                        foreach (var ojbect in _curData.GetObjects())
                        {
                            if (ojbect.IsMonster && !ojbect.IsDead)
                            {
                                if (ojbect.IsActive && PatherExplorerData.ShowActiveMonsters)
                                {
                                    graphics.DrawEllipse(ActiveMosterPen, ojbect.Position.X - 0.66f, ojbect.Position.Y - 0.66f, 2, 2);
                                    if (ojbect.PathfindingPositions.Any())
                                    {
                                        for (int i = 0; i < ojbect.PathfindingPositions.Count - 1; i++)
                                        {
                                            var start = ojbect.PathfindingPositions[i];
                                            var end = ojbect.PathfindingPositions[i + 1];
                                            using (GraphicsPath capPath = new GraphicsPath())
                                            {
                                                // Arrow
                                                var linelength = Math.Sqrt(Math.Pow((end.Y - start.Y), 2) + Math.Pow((end.X - start.X), 2));
                                                var arrowLenght = linelength < 120 ? 5 : 10;
                                                var arrowSpace = linelength < 120 ? 3 : 6;

                                                capPath.AddLine(-arrowSpace, -arrowLenght, 0, 0);
                                                capPath.AddLine(0, 0, arrowSpace, -arrowLenght);

                                                _arrowPen.CustomEndCap = new CustomLineCap(null, capPath);

                                                graphics.DrawLine(_arrowPen, start.X, start.Y, end.X, end.Y);
                                            }
                                        }
                                        foreach (var position in ojbect.PathfindingPositions)
                                        {

                                        }
                                    }
                                }
                                else if (!ojbect.IsActive && PatherExplorerData.ShowUnactiveMonsters)
                                {
                                    graphics.DrawEllipse(UnactiveMosterPen, ojbect.Position.X - 0.66f, ojbect.Position.Y - 0.66f, 2, 2);
                                }
                            }


                            if (ojbect.IsBlightPath)
                            {
                                if (PatherExplorerData.ShowBlightPathsBeams)
                                {
                                    using (GraphicsPath capPath = new GraphicsPath())
                                    {
                                        // Arrow
                                        var linelength = Math.Sqrt(Math.Pow((ojbect.BeamEndMap.Y - ojbect.BeamStartMap.Y), 2) + Math.Pow((ojbect.BeamEndMap.X - ojbect.BeamStartMap.X), 2));
                                        var arrowLenght = linelength < 120 ? 6 : 10;
                                        var arrowSpace = linelength < 120 ? 3 : 6;

                                        capPath.AddLine(-arrowSpace, -arrowLenght, 0, 0);
                                        capPath.AddLine(0, 0, arrowSpace, -arrowLenght);

                                        _arrowPen.CustomEndCap = new CustomLineCap(null, capPath);

                                        graphics.DrawLine(_arrowPen, ojbect.BeamStartMap.X, ojbect.BeamStartMap.Y, ojbect.BeamEndMap.X, ojbect.BeamEndMap.Y);
                                    }
                                }
                                if (PatherExplorerData.ShowBlightPaths)
                                {
                                    graphics.DrawEllipse(BlightPathPen, (float)ojbect.Position.X - 0.66f, (float)ojbect.Position.Y - 0.66f, 2, 2);
                                }


                            }
                        }

                        if (PatherExplorerData.BlightLaneDebug)
                        {
                            var lanes = CreateBlightLanes(_curData.GetObjects().Where(o => o.IsBlightPath).ToList());
                            if (lanes.Any())
                            {
                                foreach (var lane in lanes)
                                {
                                    var lanePath = lane.Path;
                                    var lanePathCount = lanePath.Count;
                                    for (int i = 0; i < lanePathCount; i++)
                                    {
                                        var cachedObject = lanePath[i];
                                        graphics.DrawEllipse(lane.LaneColor, cachedObject.Position.X - 1, cachedObject.Position.Y - 1, 3, 3);
                                        if (i < lanePathCount - 1)
                                        {
                                            var cachedObject2 = lanePath[i + 1];
                                            graphics.DrawLine(lane.LaneColor, cachedObject.Position.X, cachedObject.Position.Y, cachedObject2.Position.X, cachedObject2.Position.Y);
                                        }
                                    }

                                }
                            }
                        }

                        if (true)
                        {
                            foreach (var o in _curData.GetDrawObjects())
                            {
                                if (o.Name.Contains("ChillTower") || o.Name.Contains("StunTower") || o.Name.Contains("EmpowerTower"))
                                {
                                    var range = o.Name.Contains("Empower") ? 55f : 35f;
                                    graphics.FillEllipse(new Pen(Color.FromName(o.DrawPen), o.DrawPenSize).Brush, o.Position.X - 2f, o.Position.Y - 2f, 4f, 4f);
                                    graphics.DrawEllipse(new Pen(Color.FromName(o.DrawPen), o.DrawPenSize), o.Position.X - range, o.Position.Y - range, range * 2, range * 2);
                                }
                                else if (o.Name.Contains("Tower"))
                                {
                                    graphics.DrawEllipse(new Pen(Color.FromName(o.DrawPen), o.DrawPenSize), o.Position.X - 2.5f, o.Position.Y - 2.5f, 5f, 5f);
                                }
                                else
                                {
                                    graphics.FillEllipse(new Pen(Color.FromName(o.DrawPen), o.DrawPenSize).Brush, o.Position.X - 1.5f, o.Position.Y - 1.5f, 3f, 3f);
                                }
                            }
                        }

                        if (true)
                        {
                            if (_curData.BestTargetPosition != Vector2i.Zero)
                            {
                                graphics.DrawLine(BestTargetPen, _curData.MyPos.X, _curData.MyPos.Y, _curData.BestTargetPosition.X, _curData.BestTargetPosition.Y);
                                graphics.FillEllipse(BestTargetPen.Brush, _curData.BestTargetPosition.X - 1.5f, _curData.BestTargetPosition.Y - 1.5f, 3f, 3f);

                            }
                        }
                    }
                    //bitmap.MakeTransparent(Color.Black);
                    BitmapImage bitmapimage = new BitmapImage();
                    using (MemoryStream memory = new MemoryStream())
                    {
                        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                        memory.Position = 0;

                        bitmapimage.BeginInit();
                        bitmapimage.StreamSource = memory;
                        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapimage.EndInit();
                        bitmapimage.Freeze();
                    }

                    LokiPoe.BeginDispatchIfNecessary(View.Dispatcher, () => View.Source = bitmapimage);
                }
                else
                {
                    LokiPoe.BeginDispatchIfNecessary(View.Dispatcher, () => View.Source = null);
                }
            }
            catch (Exception e)
            {

            }
            
        }

		public override void Render(PatherExplorerData data)
        {
            if (!PatherExplorerData.ShowActiveMonsters && 
                !PatherExplorerData.ShowUnactiveMonsters && 
                !PatherExplorerData.ShowBlightPaths && 
                !PatherExplorerData.ShowBlightPathsBeams &&
                !PatherExplorerData.BlightLaneDebug &&
                !PatherExplorerData.ForceObjectRedraw) return;
            var polyPathfinder = ExilePather.PolyPathfinder;
            if (polyPathfinder == null)
                return;
            _curData = data;
            if (polyPathfinder.AreaGenerated)
            {
                CreateVisual();
            }

        }

        public class Lane
        {
            public Pen LaneColor { get; set; }
            public Vector3 StartPoint
            {
                get { return Path.Any() ? Path[0].BeamStart : Vector3.Zero; }
            }
            public Vector3 EndPoint
            {
                get { return Path.Any() ? Path.Last().BeamStart : Vector3.Zero; }
            }
            public List<CachedObject> Path = new List<CachedObject>();

            public Lane(CachedObject node, Pen pen)
            {
                LaneColor = pen;
                Path.Add(node);
            }
        }

        
        private static bool ProtopypeLaneFinder(ref List<CachedObject> nodes, ref List<Lane> lanes, List<CachedObject> controlList)
        {
            //nothing more to evaluate, we can return to unwrap the recursive function.
            if (!nodes.Any()) return true;
            var isLaneStart = false;
            var belongToLane = false;
            var node = nodes[0];
            nodes.RemoveAt(0);
            isLaneStart = IsLaneStart(node, controlList);
            if (isLaneStart)
            {
                var lanecolor = LanesPen[lanes.Count];
                lanes.Add(new Lane(node, lanecolor));
            }
            else
            {
                var laneBelonging = lanes.FirstOrDefault(x => BelongToThisLane(node, x));
                if (laneBelonging != null)
                {
                    belongToLane = true;
                    for (int j = laneBelonging.Path.Count - 1; j >= 0; j--)
                    {
                        var laneNode = laneBelonging.Path[j];
                        if (node.BeamStart == laneNode.BeamEnd)
                        {
                            laneBelonging.Path.Insert(j + 1, node);
                            break;
                        }
                        if (node.BeamEnd == laneNode.BeamStart)
                        {
                            laneBelonging.Path.Insert(j, node);
                            break;
                        }
                    }
                }
            }

            if (!belongToLane && !isLaneStart) nodes.Add(node);

            return ProtopypeLaneFinder(ref nodes, ref lanes, controlList);
        }

        private static bool IsLaneStart(CachedObject testNode, List<CachedObject> nodes)
        {
            foreach (var node in nodes)
            {
                if (IsSameNode(node, testNode))
                {
                    continue;
                }

                if (testNode.BeamStart == node.BeamEnd)
                {
                    if (nodes.Count(x => x.BeamEnd == node.BeamEnd) > 1)
                    {
                        return true;
                    }
                    if (nodes.Count(x => x.BeamEnd == node.BeamEnd) > 1)
                    {
                        return true;
                    }
                    return false;
                }
            }

            return true;
        }

        private static bool IsSameNode(CachedObject node1, CachedObject node2)
        {
            return node1.Id == node2.Id;
        }


        private static bool BelongToThisLane(CachedObject node, Lane lane)
        {
            
            return lane.Path.Any(x =>
                (node.BeamStart.X == x.BeamEnd.X && node.BeamStart.Y == x.BeamEnd.Y) ||
                (node.BeamEnd.X == x.BeamStart.X && node.BeamEnd.Y == x.BeamStart.Y)
            );
        }
    }
}
