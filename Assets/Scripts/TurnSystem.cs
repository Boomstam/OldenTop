using System;
using System.Collections.Generic;
using UnityEngine;

namespace OldenTop
{
    public enum Resource
    {
        Grains,
        Aurochs,
        Wood,
        Mushrooms,
        Flintstone,
        Firestone,
        Fish,
        Reeds
    }

    public static class ResourceCatalog
    {
        private static readonly Resource[][] Options =
        {
            new[] { Resource.Grains, Resource.Aurochs },
            new[] { Resource.Wood, Resource.Mushrooms },
            new[] { Resource.Flintstone, Resource.Firestone },
            new[] { Resource.Fish, Resource.Reeds }
        };

        private static readonly string[] IconResourcePaths =
        {
            "ResourceIcons/grains",
            "ResourceIcons/aurochs",
            "ResourceIcons/wood",
            "ResourceIcons/mushrooms",
            "ResourceIcons/flintstone",
            "ResourceIcons/firestone",
            "ResourceIcons/fish",
            "ResourceIcons/reeds"
        };

        private static readonly Texture2D[] Icons = new Texture2D[IconResourcePaths.Length];

        public static Resource[] GetOptions(Terrain terrain)
        {
            return Options[(int)terrain];
        }

        public static bool IsAllowed(Terrain terrain, Resource resource)
        {
            Resource[] options = GetOptions(terrain);
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == resource)
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetLabel(Resource resource)
        {
            return resource.ToString();
        }

        public static string GetIconResourcePath(Resource resource)
        {
            int index = (int)resource;
            return index >= 0 && index < IconResourcePaths.Length ? IconResourcePaths[index] : string.Empty;
        }

        public static Texture2D GetIcon(Resource resource)
        {
            int index = (int)resource;
            if (index < 0 || index >= Icons.Length)
            {
                return null;
            }

            if (Icons[index] == null)
            {
                Icons[index] = Resources.Load<Texture2D>(IconResourcePaths[index]);
            }

            return Icons[index];
        }
    }

    public static class WorkerIconCatalog
    {
        private static readonly string[] IconResourcePaths =
        {
            "WorkerIcons/player-1",
            "WorkerIcons/player-2"
        };

        private static readonly Texture2D[] Icons = new Texture2D[IconResourcePaths.Length];

        public static string GetIconResourcePath(int player)
        {
            return player >= 0 && player < IconResourcePaths.Length
                ? IconResourcePaths[player]
                : string.Empty;
        }

        public static Texture2D GetIcon(int player)
        {
            if (player < 0 || player >= Icons.Length)
            {
                return null;
            }

            if (Icons[player] == null)
            {
                Icons[player] = Resources.Load<Texture2D>(IconResourcePaths[player]);
            }

            return Icons[player];
        }
    }

    public static class HearthIconCatalog
    {
        private const string IconResourcePath = "ResourceIcons/hearth";
        private static Texture2D icon;

        public static Texture2D GetIcon()
        {
            if (icon == null)
            {
                icon = Resources.Load<Texture2D>(IconResourcePath);
            }

            return icon;
        }
    }

    public static class MapSeedUtility
    {
        public static int ToInt32(string seed)
        {
            string value = seed ?? string.Empty;
            if (int.TryParse(value, out int numericSeed))
            {
                return numericSeed;
            }

            unchecked
            {
                const uint offsetBasis = 2166136261;
                const uint prime = 16777619;
                uint hash = offsetBasis;
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= prime;
                }

                return (int)hash;
            }
        }
    }

    public static class ResourceSave
    {
        private const int CurrentVersion = 3;
        private const string LayoutKey = "OldenTop.TileResourceLayout";

        [Serializable]
        private sealed class TileResourceLayout
        {
            public int version;
            public string mapSeed;
            public int[] resources;
        }

        public static Resource[] LoadOrCreate(string mapSeed, Terrain[] terrain)
        {
            if (TryLoad(mapSeed, terrain, out Resource[] choices))
            {
                return choices;
            }

            choices = CreateBalancedLayout(mapSeed, terrain);
            Save(mapSeed, choices);
            return choices;
        }

        public static bool TryLoad(string mapSeed, Terrain[] terrain, out Resource[] choices)
        {
            choices = null;
            string json = PlayerPrefs.GetString(LayoutKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            TileResourceLayout data;
            try
            {
                data = JsonUtility.FromJson<TileResourceLayout>(json);
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (data == null || data.version != CurrentVersion ||
                !string.Equals(data.mapSeed, mapSeed, StringComparison.Ordinal) ||
                data.resources == null || data.resources.Length != terrain.Length)
            {
                return false;
            }

            choices = new Resource[terrain.Length];
            for (int tile = 0; tile < terrain.Length; tile++)
            {
                Resource resource = (Resource)data.resources[tile];
                if (!Enum.IsDefined(typeof(Resource), resource) ||
                    !ResourceCatalog.IsAllowed(terrain[tile], resource))
                {
                    choices = null;
                    return false;
                }

                choices[tile] = resource;
            }

            return true;
        }

        private static Resource[] CreateBalancedLayout(string mapSeed, Terrain[] terrain)
        {
            Resource[] choices = new Resource[terrain.Length];
            int numericSeed = MapSeedUtility.ToInt32(mapSeed);
            System.Random resourceRandom = new System.Random(unchecked(numericSeed ^ 0x5A17C9E3));

            for (int terrainIndex = 0; terrainIndex < 4; terrainIndex++)
            {
                Terrain terrainType = (Terrain)terrainIndex;
                List<int> tiles = new List<int>();
                for (int tile = 0; tile < terrain.Length; tile++)
                {
                    if (terrain[tile] == terrainType)
                    {
                        tiles.Add(tile);
                    }
                }

                for (int i = tiles.Count - 1; i > 0; i--)
                {
                    int swapIndex = resourceRandom.Next(i + 1);
                    int temporary = tiles[i];
                    tiles[i] = tiles[swapIndex];
                    tiles[swapIndex] = temporary;
                }

                Resource[] options = ResourceCatalog.GetOptions(terrainType);
                for (int i = 0; i < tiles.Count; i++)
                {
                    choices[tiles[i]] = options[i % options.Length];
                }
            }

            return choices;
        }

        private static void Save(string mapSeed, Resource[] choices)
        {
            int[] resourceIds = new int[choices.Length];
            for (int i = 0; i < choices.Length; i++)
            {
                resourceIds[i] = (int)choices[i];
            }

            TileResourceLayout data = new TileResourceLayout
            {
                version = CurrentVersion,
                mapSeed = mapSeed,
                resources = resourceIds
            };
            PlayerPrefs.SetString(LayoutKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }

    public sealed class TurnSystem : MonoBehaviour
    {
        private const int PlayerCount = 2;
        private const int WorkersPerPlayer = 4;
        private const float PanelFraction = 0.25f;
        private const float DragThreshold = 6f;
        private const float ResourceIconSize = 42f;
        private const float InventoryWorkerIconSize = 48f;
        private const float MapWorkerIconSize = 32f;
        private const float SelectedMapWorkerIconSize = 40f;
        private const float HearthMarkerSize = 38f;
        private const int WorkerMoveRange = 1;
        private const int TileContentSlotCount = 6;
        private const float HexEdgeNormalProjection = 0.8660254f;
        private const float MaximumZoomedResourceIconScale = 2f;
        private const float ZoomStepMultiplier = 0.85f;
        private const float MinimumZoomFraction = 0.35f;
        private const float MaximumZoomFraction = 1.35f;
        private const float ScrollZoomSensitivity = 0.25f;

        private static readonly string[] SeasonNames = { "Spring", "Summer", "Autumn", "Winter" };
        private static readonly Color[] PlayerColors =
        {
            new Color32(210, 62, 62, 255),
            new Color32(62, 122, 218, 255)
        };

        private readonly int[,] assignments = new int[PlayerCount, WorkersPerPlayer];
        private readonly int[,] assignmentSlots = new int[PlayerCount, WorkersPerPlayer];
        private readonly int[,] turnStartTiles = new int[PlayerCount, WorkersPerPlayer];
        private readonly int[,] turnStartSlots = new int[PlayerCount, WorkersPerPlayer];
        private readonly bool[,] workerPlacedThisTurn = new bool[PlayerCount, WorkersPerPlayer];
        private readonly bool[] completedFirstTurn = new bool[PlayerCount];
        private readonly int[] hearthTiles = { -1, -1 };
        private readonly List<int> highlightedTiles = new List<int>(7);
        private readonly List<int>[] occupiedTilesByPlayer =
        {
            new List<int>(),
            new List<int>()
        };

        private HexMap map;
        private Camera mapCamera;
        private int activePlayer;
        private int seasonIndex;
        private int year = 1;
        private int selectedWorker = -1;
        private int pressedWorker = -1;
        private int draggingWorker = -1;
        private Vector2 workerPressPosition;
        private float fittedCameraSize = 1f;
        private bool resolutionPhase;
        private string statusMessage = "Player 1: place your hearth on any non-water hex.";

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle headingStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallBodyStyle;
        private GUIStyle zoomButtonStyle;
        private GUIStyle resourceFallbackStyle;
        private GUIStyle tooltipStyle;
        private Texture2D iconOutlineTexture;
        private string hoveredTooltip;

        public int ActivePlayer => activePlayer;
        public int Year => year;
        public string Season => SeasonNames[seasonIndex];
        public bool IsResolutionPhase => resolutionPhase;
        public bool IsPlacingHearth => !resolutionPhase && hearthTiles[activePlayer] < 0;
        private bool CanRecallActiveHearth => !completedFirstTurn[activePlayer] && hearthTiles[activePlayer] >= 0;
        public int SelectedWorker => selectedWorker;
        public float MapCameraSize => mapCamera != null ? mapCamera.orthographicSize : 0f;
        public float MinimumMapCameraSize => fittedCameraSize * MinimumZoomFraction;
        public float MaximumMapCameraSize => fittedCameraSize * MaximumZoomFraction;
        public float MapResourceIconSize
        {
            get
            {
                if (mapCamera == null)
                {
                    return ResourceIconSize;
                }

                float zoomInProgress = Mathf.InverseLerp(fittedCameraSize,
                    MinimumMapCameraSize, mapCamera.orthographicSize);
                return Mathf.Lerp(ResourceIconSize,
                    ResourceIconSize * MaximumZoomedResourceIconScale, zoomInProgress);
            }
        }

        private float OccupiedTileIconScale
        {
            get
            {
                if (mapCamera == null)
                {
                    return 1f;
                }

                float cameraSize = Mathf.Max(0.001f, mapCamera.orthographicSize);
                return MaximumZoomedResourceIconScale * MinimumMapCameraSize / cameraSize;
            }
        }

        public void Initialize(HexMap hexMap, Camera cameraOverride = null)
        {
            map = hexMap;
            mapCamera = cameraOverride != null
                ? cameraOverride
                : Camera.main != null
                    ? Camera.main
                    : FindFirstObjectByType<Camera>();
            fittedCameraSize = mapCamera != null ? mapCamera.orthographicSize : 1f;
            ClearHearths();
            ClearAssignments();
            ClearWorkerInteraction();
            map?.ClearTileHighlights();
            map?.ClearTileOccupancyOutlines();
            statusMessage = "Player 1: place your hearth on any non-water hex.";
        }

        private void Awake()
        {
            if (map == null)
            {
                map = GetComponent<HexMap>();
            }
        }

        private void OnGUI()
        {
            if (map == null)
            {
                return;
            }

            EnsureStyles();
            hoveredTooltip = null;
            DrawTileResourceIcons();
            DrawHearthsOnMap();
            DrawPlacementSlotsOnMap();
            DrawAssignmentsOnMap();
            DrawMapHeader();
            DrawInventoryPanel();
            HandleZoomInput(Event.current);
            HandlePointerInteraction(Event.current);

            if (draggingWorker >= 0)
            {
                DrawDragGhost(Event.current.mousePosition);
            }

            DrawTooltip(Event.current.mousePosition);
        }

        private void DrawInventoryPanel()
        {
            float width = Screen.width * PanelFraction;
            Rect panel = new Rect(0f, 0f, width, Screen.height);
            Color previousGuiColor = GUI.color;
            GUI.color = new Color32(12, 13, 12, 255);
            GUI.DrawTexture(panel, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previousGuiColor;
            GUI.Box(panel, GUIContent.none, panelStyle);

            float x = 18f;
            float contentWidth = width - 36f;
            float y = 18f;

            GUI.Label(new Rect(x, y, contentWidth, 34f), "OLDEN TOP", titleStyle);
            y += 38f;
            GUI.Label(new Rect(x, y, contentWidth, 24f), $"Year {year}  •  {SeasonNames[seasonIndex]}", headingStyle);
            y += 30f;

            string phaseText = resolutionPhase
                ? "COMMITMENTS READY"
                : IsPlacingHearth
                    ? $"PLAYER {activePlayer + 1} PLACES HEARTH"
                    : $"PLAYER {activePlayer + 1} ASSIGNS";
            Color previousColor = GUI.color;
            GUI.color = resolutionPhase ? Color.white : PlayerColors[activePlayer];
            GUI.Label(new Rect(x, y, contentWidth, 30f), phaseText, headingStyle);
            GUI.color = previousColor;
            y += 38f;

            GUI.Label(new Rect(x, y, contentWidth, 42f), statusMessage, bodyStyle);
            y += 49f;

            GUI.Label(new Rect(x, y, contentWidth, 24f), "TILE RESOURCE OPTIONS", headingStyle);
            y += 26f;
            for (int terrainIndex = 0; terrainIndex < 4; terrainIndex++)
            {
                Terrain terrain = (Terrain)terrainIndex;
                Resource[] options = ResourceCatalog.GetOptions(terrain);
                GUI.Label(new Rect(x + 8f, y, 86f, 45f), $"{terrain}:", smallBodyStyle);
                for (int option = 0; option < options.Length; option++)
                {
                    DrawResourceIcon(new Rect(x + 94f + option * 49f, y + 1f,
                        ResourceIconSize, ResourceIconSize), options[option]);
                }

                y += 46f;
            }

            y += 8f;
            GUI.Label(new Rect(x, y, contentWidth, 24f),
                IsPlacingHearth ? "STARTING HEARTH" : "WORKERS", headingStyle);
            y += 29f;

            if (IsPlacingHearth)
            {
                GUI.Label(new Rect(x, y, contentWidth, 88f),
                    "Click any non-water hex to place your permanent hearth. All workers begin there.",
                    bodyStyle);
            }
            else if (!resolutionPhase)
            {
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    int column = worker % 2;
                    int row = worker / 2;
                    DrawWorkerCard(new Rect(x + column * (InventoryWorkerIconSize + 10f),
                        y + row * (InventoryWorkerIconSize + 8f),
                        InventoryWorkerIconSize, InventoryWorkerIconSize), worker);
                }

                y += InventoryWorkerIconSize * 2f + 16f;

                string recallLabel = CanRecallActiveHearth
                    ? "Recall workers and hearth"
                    : "Reset this player's worker moves";
                if (GUI.Button(new Rect(x, Screen.height - 94f, contentWidth, 30f), recallLabel))
                {
                    RecallActivePlayerPieces();
                }

                GUI.backgroundColor = PlayerColors[activePlayer];
                if (GUI.Button(new Rect(x, Screen.height - 54f, contentWidth, 36f), "End assignments"))
                {
                    EndAssignments();
                }

                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.Label(new Rect(x, y, contentWidth, 65f),
                    "Both players have committed. Workers remain on the map until the next season begins.", bodyStyle);
                string nextSeason = SeasonNames[(seasonIndex + 1) % SeasonNames.Length];
                GUI.backgroundColor = new Color32(139, 187, 114, 255);
                if (GUI.Button(new Rect(x, Screen.height - 60f, contentWidth, 42f), $"Begin {nextSeason}"))
                {
                    AdvanceSeason();
                }

                GUI.backgroundColor = Color.white;
            }
        }

        private void DrawWorkerCard(Rect rect, int worker)
        {
            bool placedThisTurn = workerPlacedThisTurn[activePlayer, worker];
            bool selected = selectedWorker == worker;

            float selectionExpansion = selected ? 3f : 0f;
            Rect iconRect = new Rect(rect.x - selectionExpansion, rect.y - selectionExpansion,
                rect.width + selectionExpansion * 2f, rect.height + selectionExpansion * 2f);
            DrawWorkerIcon(iconRect, activePlayer, placedThisTurn,
                placedThisTurn ? "placed this turn" : "ready to move");

            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0 && rect.Contains(current.mousePosition))
            {
                BeginWorkerPress(worker, current);
            }
        }

        private void BeginWorkerPress(int worker, Event current)
        {
            if (!SelectActiveWorker(worker))
            {
                return;
            }

            pressedWorker = worker;
            workerPressPosition = current.mousePosition;
            current.Use();
        }

        private void HandlePointerInteraction(Event current)
        {
            if (IsPlacingHearth)
            {
                if (current.type == EventType.MouseDown && current.button == 0 &&
                    TryGetTileAtGuiPosition(current.mousePosition, out int hearthTile))
                {
                    TryPlaceActiveHearth(hearthTile);
                    current.Use();
                }

                return;
            }

            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape &&
                (pressedWorker >= 0 || draggingWorker >= 0))
            {
                ClearPointerState();
                statusMessage = "Worker placement cancelled.";
                current.Use();
                return;
            }

            if (current.type == EventType.MouseDrag && pressedWorker >= 0 &&
                Vector2.Distance(workerPressPosition, current.mousePosition) >= DragThreshold)
            {
                draggingWorker = pressedWorker;
                statusMessage = "Drop the worker onto an available slot, or elsewhere to cancel the move.";
                current.Use();
                return;
            }

            if (current.type == EventType.MouseUp && current.button == 0 && draggingWorker >= 0)
            {
                int worker = draggingWorker;
                if (TryGetPlacementSlotAtGuiPosition(current.mousePosition, out int tile, out int slot))
                {
                    TryAssignActiveWorker(worker, tile, slot);
                }
                else
                {
                    ResetActiveWorkerMove(worker);
                }

                ClearPointerState();
                current.Use();
                return;
            }

            if (current.type == EventType.MouseUp && current.button == 0 && pressedWorker >= 0)
            {
                ClearPointerState();
                current.Use();
                return;
            }

            if (current.type == EventType.MouseDown && current.button == 0 && selectedWorker >= 0 &&
                TryGetPlacementSlotAtGuiPosition(current.mousePosition, out int clickedTile, out int clickedSlot))
            {
                TryAssignActiveWorker(selectedWorker, clickedTile, clickedSlot);
                current.Use();
            }
        }

        private bool TryGetTileAtGuiPosition(Vector2 guiPoint, out int tile)
        {
            tile = -1;
            if (mapCamera == null)
            {
                return false;
            }

            Vector2 screenPoint = new Vector2(guiPoint.x, Screen.height - guiPoint.y);
            if (!mapCamera.pixelRect.Contains(screenPoint))
            {
                return false;
            }

            Vector3 world = mapCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 0f));
            return map.TryGetTileAtWorldPosition(world, out tile);
        }

        private void DrawTileResourceIcons()
        {
            if (mapCamera == null)
            {
                return;
            }

            Rect mapRect = mapCamera.pixelRect;
            for (int tile = 0; tile < map.GeneratedTileCount; tile++)
            {
                if (!map.HasResource(tile))
                {
                    continue;
                }

                Vector3 screen = mapCamera.WorldToScreenPoint(map.GetTileWorldPosition(tile));
                if (screen.z < 0f || !mapRect.Contains(new Vector2(screen.x, screen.y)))
                {
                    continue;
                }

                Resource resource = map.GetSelectedResource(tile);
                float iconSize = IsTileOccupied(tile)
                    ? ResourceIconSize * OccupiedTileIconScale
                    : MapResourceIconSize;
                Rect iconRect = new Rect(screen.x - iconSize * 0.5f,
                    Screen.height - screen.y - iconSize * 0.5f, iconSize, iconSize);
                DrawResourceIcon(iconRect, resource);
            }
        }

        private void DrawResourceIcon(Rect rect, Resource resource)
        {
            Texture2D icon = ResourceCatalog.GetIcon(resource);
            if (icon != null)
            {
                GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.Label(rect, "?", resourceFallbackStyle);
            }

            if (rect.Contains(Event.current.mousePosition))
            {
                hoveredTooltip = ResourceCatalog.GetLabel(resource);
            }
        }

        private void DrawTooltip(Vector2 mousePosition)
        {
            if (string.IsNullOrEmpty(hoveredTooltip))
            {
                return;
            }

            Vector2 size = tooltipStyle.CalcSize(new GUIContent(hoveredTooltip));
            Rect tooltip = new Rect(mousePosition.x + 14f, mousePosition.y + 14f,
                size.x + 16f, size.y + 8f);
            tooltip.x = Mathf.Min(tooltip.x, Screen.width - tooltip.width - 4f);
            tooltip.y = Mathf.Min(tooltip.y, Screen.height - tooltip.height - 4f);
            GUI.Box(tooltip, hoveredTooltip, tooltipStyle);
        }

        private void DrawPlacementSlotsOnMap()
        {
            if (mapCamera == null || selectedWorker < 0 || resolutionPhase || IsPlacingHearth)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = new Color(0.28f, 0.28f, 0.28f, 0.38f);
            for (int tileIndex = 0; tileIndex < highlightedTiles.Count; tileIndex++)
            {
                int tile = highlightedTiles[tileIndex];
                for (int slot = 0; slot < TileContentSlotCount; slot++)
                {
                    if (IsTileSlotOccupied(tile, slot))
                    {
                        continue;
                    }

                    Rect slotRect = GetPlacementSlotGuiRect(tile, slot);
                    GUI.DrawTexture(slotRect, iconOutlineTexture, ScaleMode.ScaleToFit, true);
                    if (slotRect.Contains(Event.current.mousePosition))
                    {
                        hoveredTooltip = "Available worker slot";
                    }
                }
            }

            GUI.color = previousColor;
        }

        private bool TryGetPlacementSlotAtGuiPosition(Vector2 guiPoint, out int tile, out int slot)
        {
            tile = -1;
            slot = -1;
            if (selectedWorker < 0 || resolutionPhase || IsPlacingHearth)
            {
                return false;
            }

            for (int tileIndex = 0; tileIndex < highlightedTiles.Count; tileIndex++)
            {
                int candidateTile = highlightedTiles[tileIndex];
                for (int candidateSlot = 0; candidateSlot < TileContentSlotCount; candidateSlot++)
                {
                    if (IsTileSlotOccupied(candidateTile, candidateSlot) ||
                        !GetPlacementSlotGuiRect(candidateTile, candidateSlot).Contains(guiPoint))
                    {
                        continue;
                    }

                    tile = candidateTile;
                    slot = candidateSlot;
                    return true;
                }
            }

            return false;
        }

        private Rect GetPlacementSlotGuiRect(int tile, int slot)
        {
            Vector3 screen = GetWorkerSlotScreenPosition(tile, slot);
            float markerSize = MapWorkerIconSize * OccupiedTileIconScale;
            return new Rect(screen.x - markerSize * 0.5f,
                Screen.height - screen.y - markerSize * 0.5f, markerSize, markerSize);
        }

        private void DrawAssignmentsOnMap()
        {
            if (mapCamera == null)
            {
                return;
            }

            for (int drawPass = 0; drawPass < 2; drawPass++)
            {
                bool drawSelectedWorker = drawPass == 1;
                for (int player = 0; player < PlayerCount; player++)
                {
                    for (int worker = 0; worker < WorkersPerPlayer; worker++)
                    {
                        bool selected = player == activePlayer && worker == selectedWorker;
                        if (selected != drawSelectedWorker)
                        {
                            continue;
                        }

                        int tile = assignments[player, worker];
                        if (tile < 0 || (player == activePlayer && worker == draggingWorker))
                        {
                            continue;
                        }

                        int slot = assignmentSlots[player, worker];
                        if (slot < 0 || slot >= TileContentSlotCount)
                        {
                            continue;
                        }

                        Vector3 screen = GetWorkerSlotScreenPosition(tile, slot);
                        if (screen.z < 0f)
                        {
                            continue;
                        }

                        float markerSize = (selected ? SelectedMapWorkerIconSize : MapWorkerIconSize) *
                                           OccupiedTileIconScale;
                        Rect marker = new Rect(screen.x - markerSize * 0.5f,
                            Screen.height - screen.y - markerSize * 0.5f, markerSize, markerSize);
                        DrawWorkerIcon(marker, player, false, "placed");

                        Event current = Event.current;
                        if (!resolutionPhase && player == activePlayer && current.type == EventType.MouseDown &&
                            current.button == 0 && marker.Contains(current.mousePosition))
                        {
                            BeginWorkerPress(worker, current);
                        }
                    }
                }
            }
        }

        private Vector3 GetWorkerSlotScreenPosition(int tile, int slot)
        {
            Vector3 center = mapCamera.WorldToScreenPoint(map.GetTileWorldPosition(tile));
            Vector3 corner = mapCamera.WorldToScreenPoint(map.GetTileVertexWorldPosition(tile, slot));
            float centerToCorner = Vector2.Distance(center, corner);
            if (centerToCorner <= 0.001f)
            {
                return center;
            }

            float centerIconRadius = GetTileCenterIconSize(tile) * 0.5f;
            float centerToWorker = (HexEdgeNormalProjection * centerToCorner + centerIconRadius) /
                                   (1f + HexEdgeNormalProjection);
            return Vector3.Lerp(center, corner, Mathf.Clamp01(centerToWorker / centerToCorner));
        }

        private float GetTileCenterIconSize(int tile)
        {
            for (int player = 0; player < PlayerCount; player++)
            {
                if (hearthTiles[player] == tile)
                {
                    return HearthMarkerSize * OccupiedTileIconScale;
                }
            }

            return map.HasResource(tile) ? ResourceIconSize * OccupiedTileIconScale : 0f;
        }

        private void DrawHearthsOnMap()
        {
            if (mapCamera == null)
            {
                return;
            }

            float markerSize = HearthMarkerSize * OccupiedTileIconScale;
            for (int player = 0; player < PlayerCount; player++)
            {
                int tile = hearthTiles[player];
                if (tile < 0)
                {
                    continue;
                }

                Vector3 screen = mapCamera.WorldToScreenPoint(map.GetTileWorldPosition(tile));
                if (screen.z < 0f)
                {
                    continue;
                }

                Rect marker = new Rect(screen.x - markerSize * 0.5f,
                    Screen.height - screen.y - markerSize * 0.5f, markerSize, markerSize);
                DrawIconOutline(marker);
                Texture2D hearthIcon = HearthIconCatalog.GetIcon();
                if (hearthIcon != null)
                {
                    GUI.DrawTexture(marker, hearthIcon, ScaleMode.ScaleToFit, true);
                }
                else
                {
                    GUI.Label(marker, "?", resourceFallbackStyle);
                }

                if (marker.Contains(Event.current.mousePosition))
                {
                    hoveredTooltip = $"Player {player + 1} hearth";
                }
            }
        }

        private void DrawMapHeader()
        {
            float panelWidth = Screen.width * PanelFraction;
            const float buttonSize = 38f;
            const float buttonGap = 6f;
            float controlsWidth = buttonSize * 2f + buttonGap;
            float controlsX = Screen.width - controlsWidth - 18f;
            Rect header = new Rect(panelWidth + 18f, 16f,
                controlsX - panelWidth - 26f, buttonSize);
            GUI.Box(header, resolutionPhase
                ? "All commitments are visible — advance when ready"
                : IsPlacingHearth
                    ? $"Player {activePlayer + 1}: place your hearth on a non-water hex"
                    : $"Player {activePlayer + 1}: each worker may stay or move one tile",
                headingStyle);

            if (GUI.Button(new Rect(controlsX, 16f, buttonSize, buttonSize), "−", zoomButtonStyle))
            {
                AdjustZoom(-1f);
            }

            if (GUI.Button(new Rect(controlsX + buttonSize + buttonGap, 16f, buttonSize, buttonSize),
                    "+", zoomButtonStyle))
            {
                AdjustZoom(1f);
            }
        }

        private void HandleZoomInput(Event current)
        {
            if (current.type != EventType.ScrollWheel || mapCamera == null)
            {
                return;
            }

            Vector2 screenPoint = new Vector2(current.mousePosition.x, Screen.height - current.mousePosition.y);
            if (!mapCamera.pixelRect.Contains(screenPoint))
            {
                return;
            }

            AdjustZoom(-current.delta.y * ScrollZoomSensitivity);
            current.Use();
        }

        public void AdjustZoom(float steps)
        {
            if (mapCamera == null || Mathf.Approximately(steps, 0f))
            {
                return;
            }

            float requestedSize = mapCamera.orthographicSize * Mathf.Pow(ZoomStepMultiplier, steps);
            mapCamera.orthographicSize = Mathf.Clamp(requestedSize,
                MinimumMapCameraSize, MaximumMapCameraSize);
        }

        private void DrawDragGhost(Vector2 mousePosition)
        {
            const float size = 52f;
            Rect ghost = new Rect(mousePosition.x - size * 0.5f, mousePosition.y - size * 0.5f, size, size);
            DrawWorkerIcon(ghost, activePlayer, false, "dragging");
        }

        private void DrawWorkerIcon(Rect rect, int player, bool dimmed, string state)
        {
            DrawIconOutline(rect, dimmed);
            Color previousColor = GUI.color;
            if (dimmed)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.48f);
            }

            Texture2D icon = WorkerIconCatalog.GetIcon(player);
            if (icon != null)
            {
                GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.Label(rect, "?", resourceFallbackStyle);
            }

            GUI.color = previousColor;

            if (rect.Contains(Event.current.mousePosition))
            {
                hoveredTooltip = $"Player {player + 1} • Worker • {state}";
            }
        }

        private void DrawIconOutline(Rect iconRect, bool dimmed = false)
        {
            if (iconOutlineTexture == null)
            {
                return;
            }

            float padding = Mathf.Max(3f, iconRect.width * 0.1f);
            Rect ringRect = new Rect(iconRect.x - padding, iconRect.y - padding,
                iconRect.width + padding * 2f, iconRect.height + padding * 2f);
            Color previousColor = GUI.color;
            GUI.color = dimmed
                ? new Color(0f, 0f, 0f, 0.12f)
                : new Color(0f, 0f, 0f, 0.25f);
            GUI.DrawTexture(ringRect, iconOutlineTexture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;
        }

        public bool TryAssignActiveWorker(int worker, int tile)
        {
            int slot = FindFirstAvailableTileSlot(tile);
            return slot >= 0 && TryAssignActiveWorker(worker, tile, slot);
        }

        public bool TryAssignActiveWorker(int worker, int tile, int slot)
        {
            if (resolutionPhase || map == null || worker < 0 || worker >= WorkersPerPlayer ||
                tile < 0 || tile >= map.GeneratedTileCount ||
                slot < 0 || slot >= TileContentSlotCount || IsPlacingHearth)
            {
                return false;
            }

            int turnStartTile = turnStartTiles[activePlayer, worker];
            int distance = map.GetHexDistance(turnStartTile, tile);
            if (turnStartTile < 0 || distance > WorkerMoveRange)
            {
                statusMessage = "That hex is not highlighted. A worker may stay or move one tile.";
                return false;
            }

            if (IsTileOccupiedByOtherPlayer(tile))
            {
                statusMessage = "That hex is occupied by the other player.";
                return false;
            }

            if (IsTileSlotOccupied(tile, slot))
            {
                statusMessage = "That slot is already occupied.";
                return false;
            }

            int previousTile = assignments[activePlayer, worker];
            bool firstPlacementThisTurn = !workerPlacedThisTurn[activePlayer, worker];
            assignments[activePlayer, worker] = tile;
            assignmentSlots[activePlayer, worker] = slot;
            workerPlacedThisTurn[activePlayer, worker] = true;
            Terrain terrain = map.GetTerrain(tile);
            string action = tile == turnStartTile ? "stays on" : previousTile == tile ? "remains on" : "moved to";
            string destination = map.HasResource(tile)
                ? $"{terrain} ({ResourceCatalog.GetLabel(map.GetSelectedResource(tile))})"
                : $"{terrain} (no resource)";
            string placementMessage = $"Worker {action} {destination}.";

            if (firstPlacementThisTurn)
            {
                int nextWorker = FindNextUnplacedWorker(worker);
                selectedWorker = nextWorker;
                statusMessage = nextWorker >= 0
                    ? $"{placementMessage} The next worker is selected."
                    : $"{placementMessage} All workers are placed.";
            }
            else
            {
                selectedWorker = worker;
                statusMessage = placementMessage;
            }

            UpdateOccupiedTileOutlines();
            UpdateWorkerPlacementHighlights();
            return true;
        }

        private int FindNextUnplacedWorker(int currentWorker)
        {
            for (int offset = 1; offset <= WorkersPerPlayer; offset++)
            {
                int candidate = (currentWorker + offset) % WorkersPerPlayer;
                if (!workerPlacedThisTurn[activePlayer, candidate])
                {
                    return candidate;
                }
            }

            return -1;
        }

        public bool SelectActiveWorker(int worker)
        {
            if (resolutionPhase || IsPlacingHearth || worker < 0 || worker >= WorkersPerPlayer)
            {
                return false;
            }

            selectedWorker = worker;
            statusMessage = "Worker selected. Choose an available slot.";
            UpdateWorkerPlacementHighlights();
            return true;
        }

        private bool ResetActiveWorkerMove(int worker)
        {
            if (resolutionPhase || worker < 0 || worker >= WorkersPerPlayer)
            {
                return false;
            }

            assignments[activePlayer, worker] = turnStartTiles[activePlayer, worker];
            assignmentSlots[activePlayer, worker] = turnStartSlots[activePlayer, worker];
            workerPlacedThisTurn[activePlayer, worker] = false;
            selectedWorker = worker;
            statusMessage = "The worker's move was reset. Choose an available slot.";
            UpdateOccupiedTileOutlines();
            UpdateWorkerPlacementHighlights();
            return true;
        }

        public int GetAssignment(int player, int worker)
        {
            if (player < 0 || player >= PlayerCount || worker < 0 || worker >= WorkersPerPlayer)
            {
                return -1;
            }

            return assignments[player, worker];
        }

        public bool TryPlaceActiveHearth(int tile)
        {
            if (resolutionPhase || map == null || !IsPlacingHearth ||
                tile < 0 || tile >= map.GeneratedTileCount)
            {
                return false;
            }

            if (map.GetTerrain(tile) == Terrain.Water)
            {
                statusMessage = "A hearth cannot be placed on water. Choose a land hex.";
                return false;
            }

            if (IsTileOccupiedByOtherPlayer(tile))
            {
                statusMessage = "That hex is occupied by the other player. Choose another land hex.";
                return false;
            }

            hearthTiles[activePlayer] = tile;
            map.RemoveResource(tile);
            for (int worker = 0; worker < WorkersPerPlayer; worker++)
            {
                assignments[activePlayer, worker] = tile;
                assignmentSlots[activePlayer, worker] = worker;
                turnStartTiles[activePlayer, worker] = tile;
                turnStartSlots[activePlayer, worker] = worker;
                workerPlacedThisTurn[activePlayer, worker] = false;
            }

            selectedWorker = 0;
            statusMessage = "Hearth placed. All workers begin there; one worker is selected.";
            UpdateOccupiedTileOutlines();
            UpdateWorkerPlacementHighlights();
            return true;
        }

        public void EndAssignments()
        {
            if (IsPlacingHearth)
            {
                statusMessage = "Place your hearth before beginning your first assignments.";
                return;
            }

            completedFirstTurn[activePlayer] = true;
            ClearWorkerInteraction();

            if (activePlayer < PlayerCount - 1)
            {
                activePlayer++;
                BeginActivePlayerAssignments();
                return;
            }

            resolutionPhase = true;
            statusMessage = $"{SeasonNames[seasonIndex]} commitments are locked.";
            Debug.Log($"Year {year} {SeasonNames[seasonIndex]}: both players committed their workers.", this);
        }

        public void AdvanceSeason()
        {
            activePlayer = 0;
            resolutionPhase = false;
            seasonIndex++;
            if (seasonIndex >= SeasonNames.Length)
            {
                seasonIndex = 0;
                year++;
            }

            PrepareSeasonWorkerMoves();
            BeginActivePlayerAssignments();
        }

        private void RecallActivePlayerPieces()
        {
            if (CanRecallActiveHearth)
            {
                int recalledHearthTile = hearthTiles[activePlayer];
                hearthTiles[activePlayer] = -1;
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    assignments[activePlayer, worker] = -1;
                    assignmentSlots[activePlayer, worker] = -1;
                    turnStartTiles[activePlayer, worker] = -1;
                    turnStartSlots[activePlayer, worker] = -1;
                    workerPlacedThisTurn[activePlayer, worker] = false;
                }

                if (!IsOtherPlayersHearthOn(recalledHearthTile))
                {
                    map.RestoreResource(recalledHearthTile);
                }

                ClearWorkerInteraction();
                statusMessage = $"Player {activePlayer + 1}: place your hearth on any non-water hex.";
                UpdateOccupiedTileOutlines();
                return;
            }

            for (int worker = 0; worker < WorkersPerPlayer; worker++)
            {
                assignments[activePlayer, worker] = turnStartTiles[activePlayer, worker];
                assignmentSlots[activePlayer, worker] = turnStartSlots[activePlayer, worker];
                workerPlacedThisTurn[activePlayer, worker] = false;
            }

            selectedWorker = 0;
            ClearPointerState();
            statusMessage = "All worker moves were reset. A worker is selected.";
            UpdateOccupiedTileOutlines();
            UpdateWorkerPlacementHighlights();
        }

        private bool IsOtherPlayersHearthOn(int tile)
        {
            for (int player = 0; player < PlayerCount; player++)
            {
                if (player != activePlayer && hearthTiles[player] == tile)
                {
                    return true;
                }
            }

            return false;
        }

        private void PrepareSeasonWorkerMoves()
        {
            for (int player = 0; player < PlayerCount; player++)
            {
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    turnStartTiles[player, worker] = assignments[player, worker];
                    turnStartSlots[player, worker] = assignmentSlots[player, worker];
                    workerPlacedThisTurn[player, worker] = false;
                }
            }
        }

        private void BeginActivePlayerAssignments()
        {
            ClearWorkerInteraction();
            if (hearthTiles[activePlayer] < 0)
            {
                statusMessage = $"Player {activePlayer + 1}: place your hearth on any non-water hex.";
                return;
            }

            selectedWorker = 0;
            statusMessage = $"Player {activePlayer + 1}: a worker is selected. Choose an available slot.";
            UpdateWorkerPlacementHighlights();
        }

        private void ClearPointerState()
        {
            pressedWorker = -1;
            draggingWorker = -1;
        }

        private void ClearWorkerInteraction()
        {
            selectedWorker = -1;
            ClearPointerState();
            map?.ClearTileHighlights();
        }

        private void UpdateWorkerPlacementHighlights()
        {
            highlightedTiles.Clear();
            if (map == null || resolutionPhase || IsPlacingHearth ||
                selectedWorker < 0 || selectedWorker >= WorkersPerPlayer)
            {
                map?.ClearTileHighlights();
                return;
            }

            int startTile = turnStartTiles[activePlayer, selectedWorker];
            for (int tile = 0; tile < map.GeneratedTileCount; tile++)
            {
                if (map.GetHexDistance(startTile, tile) <= WorkerMoveRange &&
                    !IsTileOccupiedByOtherPlayer(tile) &&
                    HasAvailableTileSlot(tile))
                {
                    highlightedTiles.Add(tile);
                }
            }

            map.SetTileHighlights(highlightedTiles, PlayerColors[activePlayer]);
        }

        private bool IsTileOccupiedByOtherPlayer(int tile)
        {
            for (int player = 0; player < PlayerCount; player++)
            {
                if (player == activePlayer)
                {
                    continue;
                }

                if (hearthTiles[player] == tile)
                {
                    return true;
                }

                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    if (assignments[player, worker] == tile)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsTileOccupied(int tile)
        {
            for (int player = 0; player < PlayerCount; player++)
            {
                if (hearthTiles[player] == tile)
                {
                    return true;
                }
            }

            return GetTilePeripheralContentCount(tile) > 0;
        }

        private bool HasAvailableTileSlot(int tile)
        {
            return FindFirstAvailableTileSlot(tile) >= 0;
        }

        private int FindFirstAvailableTileSlot(int tile)
        {
            for (int slot = 0; slot < TileContentSlotCount; slot++)
            {
                if (!IsTileSlotOccupied(tile, slot))
                {
                    return slot;
                }
            }

            return -1;
        }

        private bool IsTileSlotOccupied(int tile, int slot)
        {
            for (int player = 0; player < PlayerCount; player++)
            {
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    if (assignments[player, worker] == tile && assignmentSlots[player, worker] == slot)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private int GetTilePeripheralContentCount(int tile)
        {
            int count = 0;
            for (int player = 0; player < PlayerCount; player++)
            {
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    if (assignments[player, worker] == tile)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void UpdateOccupiedTileOutlines()
        {
            if (map == null)
            {
                return;
            }

            map.ClearTileOccupancyOutlines();
            for (int player = 0; player < PlayerCount; player++)
            {
                List<int> occupiedTiles = occupiedTilesByPlayer[player];
                occupiedTiles.Clear();
                AddOccupiedTile(occupiedTiles, hearthTiles[player]);
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    AddOccupiedTile(occupiedTiles, assignments[player, worker]);
                }

                for (int index = 0; index < occupiedTiles.Count; index++)
                {
                    map.SetTileOccupancyOutline(occupiedTiles[index], PlayerColors[player]);
                }
            }
        }

        private static void AddOccupiedTile(List<int> occupiedTiles, int tile)
        {
            if (tile >= 0 && !occupiedTiles.Contains(tile))
            {
                occupiedTiles.Add(tile);
            }
        }

        private void ClearAssignments()
        {
            for (int player = 0; player < PlayerCount; player++)
            {
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    assignments[player, worker] = -1;
                    assignmentSlots[player, worker] = -1;
                    turnStartTiles[player, worker] = -1;
                    turnStartSlots[player, worker] = -1;
                    workerPlacedThisTurn[player, worker] = false;
                }

                completedFirstTurn[player] = false;
            }
        }

        private void ClearHearths()
        {
            for (int player = 0; player < PlayerCount; player++)
            {
                hearthTiles[player] = -1;
            }
        }

        private void EnsureStyles()
        {
            if (iconOutlineTexture == null)
            {
                iconOutlineTexture = CreateIconOutlineTexture();
            }

            if (panelStyle != null)
            {
                return;
            }

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(12, 12, 12, 12)
            };
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color32(238, 231, 210, 255) }
            };
            headingStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color32(224, 219, 203, 255) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = new Color32(210, 207, 196, 255) }
            };
            smallBodyStyle = new GUIStyle(bodyStyle)
            {
                fontSize = 11
            };
            zoomButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color32(238, 231, 210, 255) }
            };
            resourceFallbackStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color32(248, 245, 226, 255) }
            };
            tooltipStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color32(238, 231, 210, 255) }
            };
        }

        private static Texture2D CreateIconOutlineTexture()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Runtime Icon Outline",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            Color32[] pixels = new Color32[size * size];
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    bool inRing = distance >= size * 0.39f && distance <= size * 0.49f;
                    pixels[y * size + x] = inRing
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(0, 0, 0, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
