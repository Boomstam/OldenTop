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

        public static bool IsFood(Resource resource)
        {
            return resource == Resource.Grains || resource == Resource.Aurochs ||
                   resource == Resource.Mushrooms || resource == Resource.Fish;
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

    public static class AncestorIconCatalog
    {
        private const string IconResourcePath = "AncestorIcons/ancestor-tombstone";
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
        private const float StockpileResourceIconSize = 56f;
        private const float StockpilePlayerLabelGap = 8f;
        private const int ResourceTypeCount = 8;
        private const float InventoryWorkerIconSize = 48f;
        private const float MapWorkerIconSize = 32f;
        private const float WorkerEdgeInsetPixels = 2f;
        private const float SelectionPulseSpeed = 4.25f;
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
        private readonly bool[,] workerAlive = new bool[PlayerCount, WorkersPerPlayer];
        private readonly int[,] assignedFood = new int[PlayerCount, WorkersPerPlayer];
        private readonly bool[] completedFirstTurn = new bool[PlayerCount];
        private readonly int[,] resourceStockpiles = new int[PlayerCount, ResourceTypeCount];
        private readonly int[,] latestSeasonGains = new int[PlayerCount, ResourceTypeCount];
        private readonly int[] ancestorCounts = new int[PlayerCount];
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
        private int selectedFoodResource = -1;
        private int pressedWorker = -1;
        private int draggingWorker = -1;
        private int pressedFoodResource = -1;
        private int draggingFoodResource = -1;
        private Vector2 workerPressPosition;
        private Vector2 foodPressPosition;
        private float fittedCameraSize = 1f;
        private bool resolutionPhase;
        private bool foodAssignmentPhase;
        private bool showSeasonGainsDialog;
        private bool showFoodShortageDialog;
        private string resolvedSeasonName;
        private int resolvedYear;
        private string statusMessage = "Player 1: place your hearth on any non-water hex.";

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle headingStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallBodyStyle;
        private GUIStyle buttonStyle;
        private GUIStyle zoomButtonStyle;
        private GUIStyle resourceFallbackStyle;
        private GUIStyle stockpileCountStyle;
        private GUIStyle stockpileCountShadowStyle;
        private GUIStyle dialogTitleStyle;
        private GUIStyle dialogStyle;
        private GUIStyle tooltipStyle;
        private Texture2D iconOutlineTexture;
        private Texture2D ancestorBackdropTexture;
        private string hoveredTooltip;

        public int ActivePlayer => activePlayer;
        public int Year => year;
        public string Season => SeasonNames[seasonIndex];
        public bool IsResolutionPhase => resolutionPhase;
        public bool IsFoodAssignmentPhase => foodAssignmentPhase;
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
            ClearStockpiles();
            ClearLatestSeasonGains();
            Array.Clear(ancestorCounts, 0, ancestorCounts.Length);
            showSeasonGainsDialog = false;
            showFoodShortageDialog = false;
            foodAssignmentPhase = false;
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
            bool previousGuiEnabled = GUI.enabled;
            GUI.enabled = !showSeasonGainsDialog && !showFoodShortageDialog;
            DrawTileResourceIcons();
            DrawHearthsOnMap();
            DrawPlacementSlotsOnMap();
            DrawAssignmentsOnMap();
            DrawMapHeader();
            DrawInventoryPanel();
            GUI.enabled = previousGuiEnabled;

            if (showSeasonGainsDialog)
            {
                hoveredTooltip = null;
                DrawSeasonGainsDialog();
            }
            else if (showFoodShortageDialog)
            {
                hoveredTooltip = null;
                DrawFoodShortageDialog();
            }
            else
            {
                HandleZoomInput(Event.current);
                HandlePointerInteraction(Event.current);

                if (draggingWorker >= 0)
                {
                    DrawDragGhost(Event.current.mousePosition);
                }
                else if (draggingFoodResource >= 0)
                {
                    DrawFoodDragGhost(Event.current.mousePosition);
                }

                DrawTooltip(Event.current.mousePosition);
            }
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

            GUI.Label(new Rect(x, y, contentWidth, 58f), "OLDEN TOP", titleStyle);
            y += 66f;
            GUI.Label(new Rect(x, y, contentWidth, 42f), $"Year {year}  •  {SeasonNames[seasonIndex]}", headingStyle);
            y += 48f;

            string phaseText = resolutionPhase
                ? "COMMITMENTS READY"
                : foodAssignmentPhase
                    ? $"PLAYER {activePlayer + 1} FEEDS WORKERS"
                : IsPlacingHearth
                    ? $"PLAYER {activePlayer + 1} PLACES HEARTH"
                    : $"PLAYER {activePlayer + 1} ASSIGNS";
            Color previousColor = GUI.color;
            GUI.color = resolutionPhase ? Color.white : PlayerColors[activePlayer];
            GUI.Label(new Rect(x, y, contentWidth, 48f), phaseText, headingStyle);
            GUI.color = previousColor;
            y += 58f;

            GUI.Label(new Rect(x, y, contentWidth, 86f), statusMessage, bodyStyle);
            y += 94f;

            GUI.Label(new Rect(x, y, contentWidth, 42f), "STOCKPILES", headingStyle);
            y += 46f;
            for (int player = 0; player < PlayerCount; player++)
            {
                y += DrawPlayerStockpile(x + 8f, y, contentWidth - 8f, player);
            }

            y += 14f;
            GUI.Label(new Rect(x, y, contentWidth, 32f), "ANCESTORS", headingStyle);
            y += 36f;
            for (int player = 0; player < PlayerCount; player++)
            {
                y += DrawAncestorCounter(x + 8f, y, contentWidth - 8f, player);
            }

            y += 8f;
            GUI.Label(new Rect(x, y, contentWidth, 42f),
                IsPlacingHearth ? "STARTING HEARTH" : "WORKERS", headingStyle);
            y += 48f;

            if (IsPlacingHearth)
            {
                GUI.Label(new Rect(x, y, contentWidth, 140f),
                    "Click any non-water hex to place your permanent hearth. All workers begin there.",
                    bodyStyle);
            }
            else if (!resolutionPhase)
            {
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    if (!workerAlive[activePlayer, worker])
                    {
                        continue;
                    }

                    int column = worker % 2;
                    int row = worker / 2;
                    DrawWorkerCard(new Rect(x + column * (InventoryWorkerIconSize + 10f),
                        y + row * (InventoryWorkerIconSize + 8f),
                        InventoryWorkerIconSize, InventoryWorkerIconSize), worker);
                }

                y += InventoryWorkerIconSize * 2f + 16f;

                if (foodAssignmentPhase)
                {
                    if (GUI.Button(new Rect(x, Screen.height - 128f, contentWidth, 52f),
                            "Clear food assignments", buttonStyle))
                    {
                        ClearActivePlayerFoodAssignments();
                    }

                    GUI.backgroundColor = PlayerColors[activePlayer];
                    if (GUI.Button(new Rect(x, Screen.height - 68f, contentWidth, 52f),
                            "End food assignment", buttonStyle))
                    {
                        TryEndFoodAssignments();
                    }
                }
                else
                {
                    string recallLabel = CanRecallActiveHearth
                        ? "Recall workers and hearth"
                        : "Reset this player's worker moves";
                    if (GUI.Button(new Rect(x, Screen.height - 128f, contentWidth, 52f), recallLabel, buttonStyle))
                    {
                        RecallActivePlayerPieces();
                    }

                    GUI.backgroundColor = PlayerColors[activePlayer];
                    if (GUI.Button(new Rect(x, Screen.height - 68f, contentWidth, 52f), "End assignments", buttonStyle))
                    {
                        EndAssignments();
                    }
                }

                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.Label(new Rect(x, y, contentWidth, 110f),
                    "Both players have committed. Workers remain on the map until the next season begins.", bodyStyle);
                string nextSeason = SeasonNames[(seasonIndex + 1) % SeasonNames.Length];
                GUI.backgroundColor = new Color32(139, 187, 114, 255);
                if (GUI.Button(new Rect(x, Screen.height - 72f, contentWidth, 56f), $"Begin {nextSeason}", buttonStyle))
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

            DrawWorkerIcon(rect, activePlayer, placedThisTurn,
                placedThisTurn ? "placed this turn" : "ready to move", selected);
            DrawAssignedFoodOverlay(rect, activePlayer, worker);

            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0 && rect.Contains(current.mousePosition))
            {
                if (foodAssignmentPhase)
                {
                    TryAssignSelectedFoodToActiveWorker(worker, current);
                }
                else
                {
                    BeginWorkerPress(worker, current);
                }
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
            if (foodAssignmentPhase)
            {
                HandleFoodPointerInteraction(current);
                return;
            }

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

        private void HandleFoodPointerInteraction(Event current)
        {
            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape &&
                (pressedFoodResource >= 0 || draggingFoodResource >= 0))
            {
                ClearFoodPointerState();
                statusMessage = "Food assignment cancelled.";
                current.Use();
                return;
            }

            if (current.type == EventType.MouseDrag && pressedFoodResource >= 0 &&
                Vector2.Distance(foodPressPosition, current.mousePosition) >= DragThreshold)
            {
                draggingFoodResource = pressedFoodResource;
                statusMessage = "Drop the food onto one of your workers.";
                current.Use();
                return;
            }

            if (current.type == EventType.MouseUp && current.button == 0 && draggingFoodResource >= 0)
            {
                int resource = draggingFoodResource;
                if (TryGetActiveWorkerAtGuiPosition(current.mousePosition, out int worker))
                {
                    TryAssignFoodToActiveWorker(worker, (Resource)resource);
                }
                else
                {
                    statusMessage = "Food assignment cancelled. Drop food onto one of your workers.";
                }

                ClearFoodPointerState();
                current.Use();
                return;
            }

            if (current.type == EventType.MouseUp && current.button == 0 && pressedFoodResource >= 0)
            {
                pressedFoodResource = -1;
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

        private float DrawPlayerStockpile(float x, float y, float width, int player)
        {
            const float headingHeight = 30f;
            const float horizontalGap = 10f;
            const float verticalGap = 6f;
            Color previousColor = GUI.color;
            GUI.color = PlayerColors[player];
            GUI.Label(new Rect(x, y, width, headingHeight), $"PLAYER {player + 1}", smallBodyStyle);
            GUI.color = previousColor;

            for (int resourceIndex = 0; resourceIndex < ResourceTypeCount; resourceIndex++)
            {
                int column = resourceIndex % 4;
                int row = resourceIndex / 4;
                Rect iconRect = new Rect(
                    x + column * (StockpileResourceIconSize + horizontalGap),
                    y + headingHeight + StockpilePlayerLabelGap +
                    row * (StockpileResourceIconSize + verticalGap),
                    StockpileResourceIconSize,
                    StockpileResourceIconSize);
                DrawStockpileResourceIcon(iconRect, (Resource)resourceIndex,
                    resourceStockpiles[player, resourceIndex], player);
            }

            return headingHeight + StockpilePlayerLabelGap +
                   (StockpileResourceIconSize + verticalGap) * 2f + 8f;
        }

        private void DrawStockpileResourceIcon(Rect rect, Resource resource, int amount, int player)
        {
            Color previousColor = GUI.color;
            if (amount == 0)
            {
                GUI.color = new Color(0.42f, 0.42f, 0.42f, 0.48f);
            }

            DrawResourceIcon(rect, resource);
            Rect countRect = new Rect(
                rect.x + rect.width * 0.34f,
                rect.y + rect.height * 0.34f,
                rect.width * 0.58f,
                rect.height * 0.58f);
            GUI.Label(new Rect(countRect.x + 1.5f, countRect.y + 1.5f,
                countRect.width, countRect.height), amount.ToString(), stockpileCountShadowStyle);
            GUI.Label(countRect, amount.ToString(), stockpileCountStyle);

            bool foodSelection = foodAssignmentPhase && player == activePlayer && ResourceCatalog.IsFood(resource);
            if (foodSelection && selectedFoodResource == (int)resource)
            {
                DrawSelectedWorkerHighlight(rect, activePlayer);
            }

            Event current = Event.current;
            if (foodSelection && current.type == EventType.MouseDown && current.button == 0 &&
                rect.Contains(current.mousePosition))
            {
                BeginFoodPress(resource, current);
            }

            GUI.color = previousColor;
        }

        private float DrawAncestorCounter(float x, float y, float width, int player)
        {
            const float iconSize = 46f;
            Rect iconRect = new Rect(x, y, iconSize, iconSize);
            DrawAncestorIcon(iconRect);
            Color previousColor = GUI.color;
            GUI.color = PlayerColors[player];
            GUI.Label(new Rect(x + iconSize + 8f, y, width - iconSize - 8f, iconSize),
                $"Player {player + 1}: {ancestorCounts[player]}", smallBodyStyle);
            GUI.color = previousColor;
            return iconSize + 4f;
        }

        private void DrawSeasonGainsDialog()
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.52f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height),
                Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previousColor;

            float width = Mathf.Min(680f, Screen.width - 40f);
            float height = Mathf.Min(430f, Screen.height - 40f);
            Rect dialog = new Rect((Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f, width, height);
            GUI.color = new Color32(20, 22, 20, 248);
            GUI.DrawTexture(dialog, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previousColor;
            GUI.Box(dialog, GUIContent.none, dialogStyle);

            float x = dialog.x + 28f;
            float y = dialog.y + 18f;
            float contentWidth = dialog.width - 56f;
            GUI.Label(new Rect(x, y, contentWidth, 48f),
                $"{resolvedSeasonName.ToUpperInvariant()} GAINS", dialogTitleStyle);
            y += 48f;
            GUI.Label(new Rect(x, y, contentWidth, 32f),
                $"Year {resolvedYear} season results", smallBodyStyle);
            y += 42f;

            for (int player = 0; player < PlayerCount; player++)
            {
                y += DrawSeasonGainsPlayer(x, y, contentWidth, player);
            }

            if (GUI.Button(new Rect(x, dialog.yMax - 70f, contentWidth, 48f), "Continue", buttonStyle))
            {
                DismissSeasonGainsDialog();
            }

            Event current = Event.current;
            if (current.type == EventType.KeyDown &&
                (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.Escape))
            {
                DismissSeasonGainsDialog();
                current.Use();
            }
        }

        private float DrawSeasonGainsPlayer(float x, float y, float width, int player)
        {
            const float headingHeight = 30f;
            const float iconSize = 56f;
            const float iconGap = 10f;
            Color previousColor = GUI.color;
            GUI.color = PlayerColors[player];
            GUI.Label(new Rect(x, y, width, headingHeight), $"PLAYER {player + 1}", smallBodyStyle);
            GUI.color = previousColor;

            int drawnResources = 0;
            for (int resourceIndex = 0; resourceIndex < ResourceTypeCount; resourceIndex++)
            {
                int amount = latestSeasonGains[player, resourceIndex];
                if (amount <= 0)
                {
                    continue;
                }

                Rect iconRect = new Rect(x + drawnResources * (iconSize + iconGap),
                    y + headingHeight + 6f, iconSize, iconSize);
                DrawResourceIcon(iconRect, (Resource)resourceIndex);
                Rect countRect = new Rect(iconRect.x + iconRect.width * 0.28f,
                    iconRect.y + iconRect.height * 0.34f,
                    iconRect.width * 0.64f, iconRect.height * 0.58f);
                string amountText = $"+{amount}";
                GUI.Label(new Rect(countRect.x + 1.5f, countRect.y + 1.5f,
                    countRect.width, countRect.height), amountText, stockpileCountShadowStyle);
                GUI.Label(countRect, amountText, stockpileCountStyle);
                drawnResources++;
            }

            if (drawnResources == 0)
            {
                GUI.Label(new Rect(x, y + headingHeight + 4f, width, iconSize),
                    "Nothing gathered", bodyStyle);
            }

            return headingHeight + iconSize + 16f;
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
            if (mapCamera == null || selectedWorker < 0 || resolutionPhase || foodAssignmentPhase || IsPlacingHearth)
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
            if (selectedWorker < 0 || resolutionPhase || foodAssignmentPhase || IsPlacingHearth)
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
                        if (!workerAlive[player, worker])
                        {
                            continue;
                        }

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

                        float markerSize = MapWorkerIconSize * OccupiedTileIconScale;
                        Rect marker = new Rect(screen.x - markerSize * 0.5f,
                            Screen.height - screen.y - markerSize * 0.5f, markerSize, markerSize);
                        DrawWorkerIcon(marker, player, false, "placed", selected);
                        DrawAssignedFoodOverlay(marker, player, worker);

                        Event current = Event.current;
                        if (!resolutionPhase && player == activePlayer && current.type == EventType.MouseDown &&
                            current.button == 0 && marker.Contains(current.mousePosition))
                        {
                            if (foodAssignmentPhase)
                            {
                                TryAssignSelectedFoodToActiveWorker(worker, current);
                            }
                            else
                            {
                                BeginWorkerPress(worker, current);
                            }
                        }
                    }
                }
            }
        }

        private bool TryGetActiveWorkerAtGuiPosition(Vector2 guiPoint, out int worker)
        {
            worker = -1;
            if (mapCamera == null)
            {
                return false;
            }

            for (int candidate = 0; candidate < WorkersPerPlayer; candidate++)
            {
                if (!workerAlive[activePlayer, candidate])
                {
                    continue;
                }

                int tile = assignments[activePlayer, candidate];
                int slot = assignmentSlots[activePlayer, candidate];
                if (tile < 0 || slot < 0 || slot >= TileContentSlotCount)
                {
                    continue;
                }

                Vector3 screen = GetWorkerSlotScreenPosition(tile, slot);
                float size = MapWorkerIconSize * OccupiedTileIconScale;
                Rect marker = new Rect(screen.x - size * 0.5f, Screen.height - screen.y - size * 0.5f, size, size);
                if (marker.Contains(guiPoint))
                {
                    worker = candidate;
                    return true;
                }
            }

            return false;
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
            centerToWorker -= WorkerEdgeInsetPixels;
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
            const float buttonSize = 54f;
            const float buttonGap = 6f;
            float controlsWidth = buttonSize * 2f + buttonGap;
            float controlsX = Screen.width - controlsWidth - 18f;
            Rect header = new Rect(panelWidth + 18f, 16f,
                controlsX - panelWidth - 26f, buttonSize);
            GUI.Box(header, resolutionPhase
                ? "All commitments are visible — advance when ready"
                : foodAssignmentPhase
                    ? $"Player {activePlayer + 1}: choose food, then assign it to every worker"
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

        private void DrawFoodDragGhost(Vector2 mousePosition)
        {
            const float size = 52f;
            Rect ghost = new Rect(mousePosition.x - size * 0.5f, mousePosition.y - size * 0.5f, size, size);
            DrawResourceIcon(ghost, (Resource)draggingFoodResource);
        }

        private void DrawWorkerIcon(Rect rect, int player, bool dimmed, string state, bool selected = false)
        {
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

            if (selected)
            {
                DrawSelectedWorkerHighlight(rect, player);
            }

            if (rect.Contains(Event.current.mousePosition))
            {
                hoveredTooltip = $"Player {player + 1} • Worker • {state}";
            }
        }

        private void DrawAssignedFoodOverlay(Rect workerRect, int player, int worker)
        {
            if (!foodAssignmentPhase || player < 0 || player >= PlayerCount ||
                worker < 0 || worker >= WorkersPerPlayer)
            {
                return;
            }

            int food = assignedFood[player, worker];
            if (food < 0 || food >= ResourceTypeCount)
            {
                return;
            }

            float size = workerRect.width * 0.48f;
            Rect foodRect = new Rect(workerRect.xMax - size, workerRect.yMax - size, size, size);
            Color previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.96f);
            DrawResourceIcon(foodRect, (Resource)food);
            GUI.color = previousColor;
        }

        private void DrawAncestorIcon(Rect rect)
        {
            Color previousColor = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(rect, ancestorBackdropTexture, ScaleMode.ScaleToFit, true);
            Texture2D icon = AncestorIconCatalog.GetIcon();
            if (icon != null)
            {
                float inset = rect.width * 0.14f;
                Rect glyphRect = new Rect(rect.x + inset, rect.y + inset, rect.width - inset * 2f, rect.height - inset * 2f);
                GUI.color = new Color32(190, 190, 183, 255);
                GUI.DrawTexture(glyphRect, icon, ScaleMode.ScaleToFit, true);
            }
            GUI.color = previousColor;
        }

        private void DrawSelectedWorkerHighlight(Rect rect, int player)
        {
            if (iconOutlineTexture == null)
            {
                return;
            }

            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * SelectionPulseSpeed);
            Color highlight = Color.Lerp(PlayerColors[player], Color.white, 0.3f + pulse * 0.45f);
            highlight.a = 0.58f + pulse * 0.38f;

            Color previousColor = GUI.color;
            GUI.color = highlight;
            GUI.DrawTexture(rect, iconOutlineTexture, ScaleMode.ScaleToFit, true);

            Rect innerRing = new Rect(rect.x + rect.width * 0.07f, rect.y + rect.height * 0.07f,
                rect.width * 0.86f, rect.height * 0.86f);
            GUI.color = new Color(1f, 1f, 1f, 0.12f + pulse * 0.2f);
            GUI.DrawTexture(innerRing, iconOutlineTexture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;
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
                slot < 0 || slot >= TileContentSlotCount || IsPlacingHearth || foodAssignmentPhase ||
                !workerAlive[activePlayer, worker])
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
                if (workerAlive[activePlayer, candidate] && !workerPlacedThisTurn[activePlayer, candidate])
                {
                    return candidate;
                }
            }

            return -1;
        }

        public bool SelectActiveWorker(int worker)
        {
            if (resolutionPhase || foodAssignmentPhase || IsPlacingHearth || worker < 0 || worker >= WorkersPerPlayer ||
                !workerAlive[activePlayer, worker])
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
            if (resolutionPhase || foodAssignmentPhase || worker < 0 || worker >= WorkersPerPlayer ||
                !workerAlive[activePlayer, worker])
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
                workerAlive[activePlayer, worker] = true;
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
            if (foodAssignmentPhase)
            {
                TryEndFoodAssignments();
                return;
            }

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

            ResolveSeasonAndBeginFoodAssignments();
        }

        public void AdvanceSeason()
        {
            if (!resolutionPhase)
            {
                return;
            }

            BeginFoodAssignments();
        }

        private void ResolveSeasonAndBeginFoodAssignments()
        {
            resolvedSeasonName = SeasonNames[seasonIndex];
            resolvedYear = year;
            CollectAssignedResources();
            activePlayer = 0;
            resolutionPhase = false;
            BeginFoodAssignments();
            showSeasonGainsDialog = true;
        }

        private void CollectAssignedResources()
        {
            ClearLatestSeasonGains();
            if (map == null)
            {
                return;
            }

            for (int player = 0; player < PlayerCount; player++)
            {
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    if (!workerAlive[player, worker])
                    {
                        continue;
                    }

                    int tile = assignments[player, worker];
                    if (tile < 0 || tile >= map.GeneratedTileCount || !map.HasResource(tile))
                    {
                        continue;
                    }

                    int resourceIndex = (int)map.GetSelectedResource(tile);
                    if (resourceIndex >= 0 && resourceIndex < ResourceTypeCount)
                    {
                        resourceStockpiles[player, resourceIndex]++;
                        latestSeasonGains[player, resourceIndex]++;
                    }
                }
            }
        }

        public int GetStockpileAmount(int player, Resource resource)
        {
            int resourceIndex = (int)resource;
            return player >= 0 && player < PlayerCount &&
                   resourceIndex >= 0 && resourceIndex < ResourceTypeCount
                ? resourceStockpiles[player, resourceIndex]
                : 0;
        }

        public int GetLatestSeasonGain(int player, Resource resource)
        {
            int resourceIndex = (int)resource;
            return player >= 0 && player < PlayerCount &&
                   resourceIndex >= 0 && resourceIndex < ResourceTypeCount
                ? latestSeasonGains[player, resourceIndex]
                : 0;
        }

        public bool IsSeasonGainsDialogVisible => showSeasonGainsDialog;

        public void DismissSeasonGainsDialog()
        {
            showSeasonGainsDialog = false;
        }

        public int GetAssignedFood(int player, int worker)
        {
            return player >= 0 && player < PlayerCount && worker >= 0 && worker < WorkersPerPlayer
                ? assignedFood[player, worker]
                : -1;
        }

        public bool IsWorkerAlive(int player, int worker)
        {
            return player >= 0 && player < PlayerCount && worker >= 0 && worker < WorkersPerPlayer &&
                   workerAlive[player, worker];
        }

        public int GetAncestorCount(int player)
        {
            return player >= 0 && player < PlayerCount ? ancestorCounts[player] : 0;
        }

        public bool IsFoodShortageDialogVisible => showFoodShortageDialog;

        public bool TryAssignFoodToActiveWorker(int worker, Resource food)
        {
            int foodIndex = (int)food;
            if (!foodAssignmentPhase || worker < 0 || worker >= WorkersPerPlayer ||
                !workerAlive[activePlayer, worker] || !ResourceCatalog.IsFood(food) ||
                foodIndex < 0 || foodIndex >= ResourceTypeCount)
            {
                return false;
            }

            int previouslyAssigned = assignedFood[activePlayer, worker];
            if (previouslyAssigned == foodIndex)
            {
                statusMessage = "That worker already has this food assigned.";
                return false;
            }

            if (resourceStockpiles[activePlayer, foodIndex] <= 0)
            {
                statusMessage = $"No {ResourceCatalog.GetLabel(food)} remains in this stockpile.";
                return false;
            }

            if (previouslyAssigned >= 0)
            {
                resourceStockpiles[activePlayer, previouslyAssigned]++;
            }

            resourceStockpiles[activePlayer, foodIndex]--;
            assignedFood[activePlayer, worker] = foodIndex;
            selectedFoodResource = foodIndex;
            statusMessage = $"Assigned {ResourceCatalog.GetLabel(food)} to a worker.";
            return true;
        }

        private void TryAssignSelectedFoodToActiveWorker(int worker, Event current)
        {
            if (selectedFoodResource < 0)
            {
                statusMessage = "Select a food stockpile first.";
            }
            else
            {
                TryAssignFoodToActiveWorker(worker, (Resource)selectedFoodResource);
            }

            current.Use();
        }

        private void BeginFoodPress(Resource food, Event current)
        {
            int foodIndex = (int)food;
            if (resourceStockpiles[activePlayer, foodIndex] <= 0)
            {
                statusMessage = $"No {ResourceCatalog.GetLabel(food)} remains in this stockpile.";
                current.Use();
                return;
            }

            selectedFoodResource = foodIndex;
            pressedFoodResource = foodIndex;
            foodPressPosition = current.mousePosition;
            statusMessage = $"{ResourceCatalog.GetLabel(food)} selected. Click or drag it onto a worker.";
            current.Use();
        }

        public void TryEndFoodAssignments()
        {
            if (!foodAssignmentPhase)
            {
                return;
            }

            if (CountUnfedWorkers(activePlayer) > 0)
            {
                showFoodShortageDialog = true;
                return;
            }

            CompleteActivePlayerFoodAssignments();
        }

        public void ConfirmFoodShortage()
        {
            if (!showFoodShortageDialog || !foodAssignmentPhase)
            {
                return;
            }

            showFoodShortageDialog = false;
            CompleteActivePlayerFoodAssignments();
        }

        private int CountUnfedWorkers(int player)
        {
            int count = 0;
            for (int worker = 0; worker < WorkersPerPlayer; worker++)
            {
                if (workerAlive[player, worker] && assignedFood[player, worker] < 0)
                {
                    count++;
                }
            }

            return count;
        }

        private void CompleteActivePlayerFoodAssignments()
        {
            int deaths = 0;
            for (int worker = 0; worker < WorkersPerPlayer; worker++)
            {
                if (!workerAlive[activePlayer, worker])
                {
                    continue;
                }

                if (assignedFood[activePlayer, worker] < 0)
                {
                    workerAlive[activePlayer, worker] = false;
                    assignments[activePlayer, worker] = -1;
                    assignmentSlots[activePlayer, worker] = -1;
                    deaths++;
                }

                assignedFood[activePlayer, worker] = -1;
            }

            ancestorCounts[activePlayer] += deaths;
            ClearFoodInteraction();
            UpdateOccupiedTileOutlines();

            if (activePlayer < PlayerCount - 1)
            {
                activePlayer++;
                statusMessage = $"Player {activePlayer + 1}: assign one food to every worker.";
                return;
            }

            foodAssignmentPhase = false;
            seasonIndex++;
            if (seasonIndex >= SeasonNames.Length)
            {
                seasonIndex = 0;
                year++;
            }

            PrepareSeasonWorkerMoves();
            BeginActivePlayerAssignments();
        }

        private void ClearActivePlayerFoodAssignments()
        {
            if (!foodAssignmentPhase)
            {
                return;
            }

            for (int worker = 0; worker < WorkersPerPlayer; worker++)
            {
                int food = assignedFood[activePlayer, worker];
                if (food >= 0)
                {
                    resourceStockpiles[activePlayer, food]++;
                    assignedFood[activePlayer, worker] = -1;
                }
            }

            selectedFoodResource = -1;
            statusMessage = "Food assignments cleared. Choose a stockpile to assign food again.";
        }

        private void BeginFoodAssignments()
        {
            foodAssignmentPhase = true;
            activePlayer = 0;
            ClearFoodInteraction();
            for (int player = 0; player < PlayerCount; player++)
            {
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    assignedFood[player, worker] = -1;
                }
            }

            statusMessage = "Player 1: assign one food item to every worker.";
        }

        private void DrawFoodShortageDialog()
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previousColor;

            float width = Mathf.Min(620f, Screen.width - 40f);
            float height = Mathf.Min(330f, Screen.height - 40f);
            Rect dialog = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUI.color = new Color32(35, 22, 20, 248);
            GUI.DrawTexture(dialog, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previousColor;
            GUI.Box(dialog, GUIContent.none, dialogStyle);

            int unfed = CountUnfedWorkers(activePlayer);
            float x = dialog.x + 28f;
            float contentWidth = dialog.width - 56f;
            GUI.Label(new Rect(x, dialog.y + 20f, contentWidth, 44f), "WORKERS WILL STARVE", dialogTitleStyle);
            GUI.Label(new Rect(x, dialog.y + 78f, contentWidth, 94f),
                $"{unfed} worker{(unfed == 1 ? string.Empty : "s")} has no food assigned and will die, joining your ancestors.", bodyStyle);
            if (GUI.Button(new Rect(x, dialog.yMax - 72f, contentWidth * 0.48f, 48f), "Assign food", buttonStyle))
            {
                showFoodShortageDialog = false;
            }
            if (GUI.Button(new Rect(dialog.xMax - 28f - contentWidth * 0.48f, dialog.yMax - 72f,
                    contentWidth * 0.48f, 48f), "Let them starve", buttonStyle))
            {
                ConfirmFoodShortage();
            }
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
            foodAssignmentPhase = false;
            if (hearthTiles[activePlayer] < 0)
            {
                statusMessage = $"Player {activePlayer + 1}: place your hearth on any non-water hex.";
                return;
            }

            selectedWorker = FindFirstAliveWorker(activePlayer);
            if (selectedWorker < 0)
            {
                statusMessage = $"Player {activePlayer + 1} has no living workers. End assignments to continue.";
                return;
            }

            statusMessage = $"Player {activePlayer + 1}: a worker is selected. Choose an available slot.";
            UpdateWorkerPlacementHighlights();
        }

        private int FindFirstAliveWorker(int player)
        {
            for (int worker = 0; worker < WorkersPerPlayer; worker++)
            {
                if (workerAlive[player, worker])
                {
                    return worker;
                }
            }

            return -1;
        }

        private void ClearPointerState()
        {
            pressedWorker = -1;
            draggingWorker = -1;
        }

        private void ClearFoodPointerState()
        {
            pressedFoodResource = -1;
            draggingFoodResource = -1;
        }

        private void ClearFoodInteraction()
        {
            selectedFoodResource = -1;
            ClearFoodPointerState();
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
            if (map == null || resolutionPhase || foodAssignmentPhase || IsPlacingHearth ||
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
                    if (workerAlive[player, worker] && assignments[player, worker] == tile)
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
                    if (workerAlive[player, worker] && assignments[player, worker] == tile && assignmentSlots[player, worker] == slot)
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
                    if (workerAlive[player, worker] && assignments[player, worker] == tile)
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
                    if (workerAlive[player, worker])
                    {
                        AddOccupiedTile(occupiedTiles, assignments[player, worker]);
                    }
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
                    workerAlive[player, worker] = true;
                    assignedFood[player, worker] = -1;
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

        private void ClearStockpiles()
        {
            Array.Clear(resourceStockpiles, 0, resourceStockpiles.Length);
        }

        private void ClearLatestSeasonGains()
        {
            Array.Clear(latestSeasonGains, 0, latestSeasonGains.Length);
        }

        private void EnsureStyles()
        {
            if (iconOutlineTexture == null)
            {
                iconOutlineTexture = CreateIconOutlineTexture();
            }

            if (ancestorBackdropTexture == null)
            {
                ancestorBackdropTexture = CreateAncestorBackdropTexture();
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
                fontSize = 44,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color32(238, 231, 210, 255) }
            };
            headingStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color32(224, 219, 203, 255) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                wordWrap = true,
                normal = { textColor = new Color32(210, 207, 196, 255) }
            };
            smallBodyStyle = new GUIStyle(bodyStyle)
            {
                fontSize = 22
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            zoomButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 44,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color32(238, 231, 210, 255) }
            };
            resourceFallbackStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color32(248, 245, 226, 255) }
            };
            stockpileCountStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerRight,
                normal = { textColor = Color.white }
            };
            stockpileCountShadowStyle = new GUIStyle(stockpileCountStyle)
            {
                normal = { textColor = new Color(0f, 0f, 0f, 0.85f) }
            };
            dialogTitleStyle = new GUIStyle(titleStyle)
            {
                fontSize = 34,
                alignment = TextAnchor.MiddleCenter
            };
            dialogStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(20, 20, 20, 20)
            };
            tooltipStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 24,
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

        private static Texture2D CreateAncestorBackdropTexture()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Ancestor Icon Backdrop",
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
                    pixels[y * size + x] = distance <= size * 0.45f
                        ? new Color32(25, 28, 25, 255)
                        : distance <= size * 0.49f
                            ? new Color32(239, 226, 193, 255)
                            : new Color32(0, 0, 0, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
