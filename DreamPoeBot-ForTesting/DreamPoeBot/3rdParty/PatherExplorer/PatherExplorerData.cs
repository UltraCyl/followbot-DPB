using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.Objects;

namespace PatherExplorer
{
    public class DrawObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Vector2i Position { get; set; }
        public string DrawPen { get; set; }
        public float DrawPenSize { get; set; }

        public DrawObject()
        {

        }
        public DrawObject(int id, string name, Vector2i position, string drawPen, float drawPenSize)
        {
            Id = id;
            Name = name;
            Position = position;
            DrawPen = drawPen;
            DrawPenSize = drawPenSize;
        }
    }
    public class CachedObject
    {
        public string Name { get; set; }
        public string Metadata { get; set; }
        public int Id { get; set; }
        public Vector2i Position { get; set; }
        public bool IsMonster { get; set; }
        public bool IsBlightPath { get; set; }
        public Vector3 BeamStart { get; set; } = Vector3.Zero;
        public Vector3 BeamEnd { get; set; } = Vector3.Zero;
        public Vector2i BeamStartMap { get; set; } = Vector2i.Zero;
        public Vector2i BeamEndMap { get; set; } = Vector2i.Zero;
        public int Unknown1 { get; set; } = 0;
        public int Unknown2 { get; set; } = 0;
        public bool IsActive { get; set; }
        public bool IsDead { get; set; }
        public List<Vector2i> PathfindingPositions { get; set; }
        public CachedObject(NetworkObject obj)
        {
            Name = obj.Name;
            Metadata = obj.Metadata;
            Id = obj.Id;
            Position = obj.Position;
            IsMonster = false;
            IsBlightPath = Metadata == "Metadata/Terrain/Leagues/Blight/Objects/BlightPathway";
            IsDead = false;
            IsActive = true;
            var monster = obj as Monster;

            if (monster != null)
            {
                IsMonster = true;
                IsDead = monster.IsDead;
                IsActive = monster.IsActive;
            }
            else if (IsBlightPath)
            {
                BeamStart = obj.Components.BeamComponent.BeamStart;
                BeamStartMap = BeamStart.World3ToMap();
                BeamEnd = obj.Components.BeamComponent.BeamEnd;
                BeamEndMap = BeamEnd.World3ToMap();
                Unknown1 = obj.Components.BeamComponent.Unknown1;
                Unknown2 = obj.Components.BeamComponent.Unknown2;
            }

            if (obj.Components.PathfindingComponent != null)
            {
                PathfindingPositions = obj.Components.PathfindingComponent.PathingNodes.ToList();
            }
            else PathfindingPositions = new List<Vector2i>();
        }
    }
    public class PatherExplorerData
    {
        private static readonly object _locker = new object();
        public bool IsValid { get; set; }
        public bool IsInGame { get; private set; }
        public uint Seed { get; private set; }
        public Vector2i MyPos { get; private set; }
        public float MyHeight { get; private set; }
        public float MapHeight { get; private set; }
        public Vector2 MyWorldPos { get; private set; }
        public bool ForceReload { get; set; }
        public float Zoom { get; set; } = 1;
        public static bool LockCamera { get; set; }
        public static bool ShowFlyMap { get; set; }
        public static bool ShowActiveMonsters { get; set; }
        public static bool ShowUnactiveMonsters { get; set; }
        public static bool ShowBlightPaths { get; set; }
        public static bool ShowBlightPathsBeams { get; set; }
        public static bool BlightLaneDebug { get; set; }
        public static bool ForceObjectRedraw { get; set; } = false;
        public static int BestTargetId { get; set; } = 0;
        public Vector2i BestTargetPosition { get; set; }
        public static LokiPoe.TerrainDataEntry[,] TgtsEntry { get; set; }
        public static LokiPoe.TerrainDataEntry[,] TdtsEntry { get; set; }
        public CachedTerrainData CachedTerrainData { get; private set; }

        private PathfindingCommand _pathfindingCommand = null;
        public static Vector2i _previousSelectedTgtsPos { get; set; }
        public PathfindingCommand PathfindingCommand
        {
            get { lock (_locker) { return _pathfindingCommand; } }
            set
            {
                lock (_locker)
                {
                    _pathfindingCommand = value;
                }
                NextWalkPosition = Vector2i.Zero;
            }
        }

        private Vector2i _nextWalkPosition = Vector2i.Zero;

        public Vector2i NextWalkPosition
        {
            get { lock (_locker) { return _nextWalkPosition; } }
            set { lock (_locker) { _nextWalkPosition = value; } }
        }


        public bool RaycastChanged = false;
        private List<RaycastData> _raycastData = new List<RaycastData>();
        public void AddRaycastData(RaycastData raycastData)
        {
            lock (_locker)
            {
                var ret = _raycastData.FirstOrDefault(x => x.Origin == raycastData.Origin && x.Destination == raycastData.Destination);
                if (ret != null)
                    _raycastData.Remove(ret);
                _raycastData.Add(raycastData);
                RaycastChanged = true;
            }
        }
        public void RemoveRaycastData(RaycastData raycastData)
        {
            lock (_locker)
            {
                var ret = _raycastData.FirstOrDefault(x => x.Origin == raycastData.Origin && x.Destination == raycastData.Destination);
                if (ret != null)
                    _raycastData.Remove(ret);
                RaycastChanged = true;
            }
        }
        public List<RaycastData> GetRaycastData()
        {
            List<RaycastData> data = new List<RaycastData>();
            lock (_locker)
            {
                data = new List<RaycastData>(_raycastData);
            }

            return data;
        }
        public void ClearRaycastData()
        {
            lock (_locker)
            {
                _raycastData = new List<RaycastData>();
                RaycastChanged = true;
            }
        }

        private List<DrawObject> _drawObjects = new List<DrawObject>();
        public void AddDrawObject(DrawObject drawObject)
        {
            lock (_locker)
            {
                var ret = _drawObjects.FirstOrDefault(x => x.Id == drawObject.Id);
                if (ret != null)
                    _drawObjects.Remove(ret);
                _drawObjects.Add(drawObject);
            }
        }
        public void RemoveDrawObject(DrawObject drawObject)
        {
            lock (_locker)
            {
                var ret = _drawObjects.FirstOrDefault(x => x.Id == drawObject.Id);
                if (ret != null)
                    _drawObjects.Remove(ret);
            }
        }
        public List<DrawObject> GetDrawObjects()
        {
            List<DrawObject> data = new List<DrawObject>();
            lock (_locker)
            {
                data = new List<DrawObject>(_drawObjects);
            }

            return data;
        }
        public void ClearDrawObjects()
        {
            lock (_locker)
            {
                _drawObjects.Clear();
            }
        }

        private List<CachedObject> _objects = new List<CachedObject>();
        public void AddObject(CachedObject obj)
        {
            lock (_locker)
            {
                var ret = _objects.FirstOrDefault(x => x.Id == obj.Id);
                if (ret != null)
                    _objects.Remove(ret);
                _objects.Add(obj);
            }
        }
        public void RemoveDrawObject(CachedObject obj)
        {
            lock (_locker)
            {
                var ret = _objects.FirstOrDefault(x => x.Id == obj.Id);
                if (ret != null)
                    _objects.Remove(ret);
            }
        }
        public List<CachedObject> GetObjects()
        {
            List<CachedObject> data = new List<CachedObject>();
            lock (_locker)
            {
                data = new List<CachedObject>(_objects);
            }

            return data;
        }
        public void ClearObjects()
        {
            lock (_locker)
            {
                _objects.Clear();
            }
        }
        public void Update()
        {
            lock (_locker)
            {
                IsInGame = LokiPoe.IsInGame;

                if (IsInGame)
                {
                    if (Seed != LokiPoe.LocalData.AreaHash)
                        ClearDrawObjects();
                    Seed = LokiPoe.LocalData.AreaHash;

                    MyPos = LokiPoe.MyPosition;
                    MyHeight = LokiPoe.Me.Components.RenderComponent.TerrainHeightAt;
                    MapHeight = LokiPoe.ClientFunctions.GetHeightAt(MyPos.X, MyPos.Y);
                    MyWorldPos = LokiPoe.MyWorldPosition;
                    CachedTerrainData = LokiPoe.TerrainData.Cache;
                    TgtsEntry = (LokiPoe.TerrainDataEntry[,])LokiPoe.TerrainData.TgtEntries.Clone();
                    TdtsEntry = (LokiPoe.TerrainDataEntry[,])LokiPoe.TerrainData.TdtEntries.Clone();

                    ClearObjects();
                    try
                    {
                        if (BestTargetId != 0)
                        {
                            var bestTarget = LokiPoe.ObjectManager.GetObjectById(BestTargetId);
                            if (bestTarget != null)
                            {
                                BestTargetPosition = bestTarget.Position;
                            }
                            else
                            {
                                BestTargetPosition = Vector2i.Zero;
                                BestTargetId = 0;
                            }
                        }
                        var obj = LokiPoe.ObjectManager.Objects;
                        foreach (var o in obj)
                        {
                            if (o != null)
                                AddObject(new CachedObject(o));
                        }
                    }
                    catch
                    {

                    }
                }
                else
                {
                    Seed = 0;
                    PathfindingCommand = null;
                    NextWalkPosition = Vector2i.Zero;
                    TgtsEntry = null;
                    TdtsEntry = null;
                }

                IsValid = true;

                //ForceReload = false; // If we set this to false, we'll never be able to process it when set
            }
        }
    }
}
