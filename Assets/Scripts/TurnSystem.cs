using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OldenTop
{
    public enum Resource
    {
        Aurochs,
        Roots,
        Wood,
        Mushrooms,
        Stone,
        Fish,
        Shells
    }

    public enum WorkerAction
    {
        Gather,
        Preserve,
        Craft,
        Ritual,
        BuildMonument
    }

    public enum Tool
    {
        Axe,
        Basket,
        Pickaxe,
        Nets
    }

    // Effects are generated before players commit, but remain hidden until a future reveal step.
    internal enum GodEffect
    {
        None,
        GainPreservedRoots,
        GainWood,
        GainPreservedAurochs,
        GainStone,
        GainPreservedMushrooms,
        GainSacrality
    }

    internal static class GodIconCatalog
    {
        private const string IconResourcePath = "GodIcons/hand-of-god";
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

    public static class ToolCatalog
    {
        private static readonly Tool[] AllTools = { Tool.Axe, Tool.Basket, Tool.Pickaxe, Tool.Nets };
        private static readonly string[] IconResourcePaths =
        {
            "ToolIcons/axe",
            "ToolIcons/basket",
            "ToolIcons/pickaxe",
            "ToolIcons/nets"
        };
        private static readonly Texture2D[] Icons = new Texture2D[IconResourcePaths.Length];

        public static IReadOnlyList<Tool> All => AllTools;

        public static Texture2D GetIcon(Tool tool)
        {
            int index = (int)tool;
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

        public static bool IsAppropriateFor(Tool tool, Resource resource)
        {
            return (tool == Tool.Axe && (resource == Resource.Wood || resource == Resource.Aurochs)) ||
                   (tool == Tool.Basket && (resource == Resource.Mushrooms || resource == Resource.Shells)) ||
                   (tool == Tool.Pickaxe && (resource == Resource.Stone || resource == Resource.Roots)) ||
                   (tool == Tool.Nets && resource == Resource.Fish);
        }

        public static bool CanCraft(Tool tool, int[,] stockpiles, int player)
        {
            foreach (Resource cost in GetCosts(tool))
            {
                if (stockpiles[player, (int)cost] < GetCostAmount(tool, cost))
                {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<Resource> GetCosts(Tool tool)
        {
            switch (tool)
            {
                case Tool.Axe:
                case Tool.Pickaxe:
                    yield return Resource.Stone;
                    yield return Resource.Wood;
                    break;
                case Tool.Basket:
                case Tool.Nets:
                    yield return Resource.Roots;
                    break;
            }
        }

        public static int GetCostAmount(Tool tool, Resource resource)
        {
            return (tool == Tool.Basket || tool == Tool.Nets) && resource == Resource.Roots ? 2 : 1;
        }

        public static bool UsesCost(Tool tool, Resource resource)
        {
            foreach (Resource cost in GetCosts(tool))
            {
                if (cost == resource)
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetCostLabel(Tool tool)
        {
            switch (tool)
            {
                case Tool.Axe:
                case Tool.Pickaxe:
                    return "1 Stone • 1 Wood";
                case Tool.Basket:
                case Tool.Nets:
                    return "2 Roots";
                default:
                    return string.Empty;
            }
        }
    }

    public static class ResourceCatalog
    {
        private static readonly Resource[][] Options =
        {
            new[] { Resource.Aurochs, Resource.Roots },
            new[] { Resource.Wood, Resource.Mushrooms },
            new[] { Resource.Stone },
            new[] { Resource.Fish, Resource.Shells }
        };

        private static readonly string[] IconResourcePaths =
        {
            "ResourceIcons/aurochs",
            "ResourceIcons/roots",
            "ResourceIcons/wood",
            "ResourceIcons/mushrooms",
            "ResourceIcons/flintstone",
            "ResourceIcons/fish",
            "ResourceIcons/shells"
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
            return resource == Resource.Roots || resource == Resource.Aurochs ||
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

    public static class MonumentIconCatalog
    {
        private const string IconResourcePath = "MonumentIcons/dolmen";
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
        private const int CurrentVersion = 5;
        private const string LayoutKey = "OldenTop.TileResourceLayout";

        [Serializable]
        private sealed class TileResourceLayout
        {
            public int version;
            public string mapSeed;
            public int[] resources;
            public bool[] resourcePresent;
        }

        public static Resource[] LoadOrCreate(string mapSeed, Terrain[] terrain, int width, int height,
            out bool[] resourcePresent)
        {
            if (TryLoad(mapSeed, terrain, width, height, out Resource[] choices, out resourcePresent))
            {
                return choices;
            }

            choices = CreateBalancedLayout(mapSeed, terrain, width, height, out resourcePresent);
            Save(mapSeed, choices, resourcePresent);
            return choices;
        }

        public static bool TryLoad(string mapSeed, Terrain[] terrain, int width, int height,
            out Resource[] choices, out bool[] resourcePresent)
        {
            choices = null;
            resourcePresent = null;
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
                width <= 0 || height <= 0 || width * height != terrain.Length ||
                data.resources == null || data.resources.Length != terrain.Length ||
                data.resourcePresent == null || data.resourcePresent.Length != terrain.Length)
            {
                return false;
            }

            choices = new Resource[terrain.Length];
            resourcePresent = new bool[terrain.Length];
            int mountainTiles = 0;
            int mountainSites = 0;
            for (int tile = 0; tile < terrain.Length; tile++)
            {
                Resource resource = (Resource)data.resources[tile];
                if (!Enum.IsDefined(typeof(Resource), resource) ||
                    !ResourceCatalog.IsAllowed(terrain[tile], resource))
                {
                    choices = null;
                    resourcePresent = null;
                    return false;
                }

                bool hasResource = data.resourcePresent[tile];
                switch (terrain[tile])
                {
                    case Terrain.Mountain:
                        mountainTiles++;
                        if (hasResource)
                        {
                            mountainSites++;
                        }
                        break;
                    case Terrain.Water:
                        if (hasResource != IsShoreWaterTile(terrain, width, height, tile))
                        {
                            choices = null;
                            resourcePresent = null;
                            return false;
                        }
                        break;
                    default:
                        if (!hasResource)
                        {
                            choices = null;
                            resourcePresent = null;
                            return false;
                        }
                        break;
                }

                choices[tile] = resource;
                resourcePresent[tile] = hasResource;
            }

            if (mountainSites != mountainTiles / 2)
            {
                choices = null;
                resourcePresent = null;
                return false;
            }

            return true;
        }

        public static Resource[] CreateBalancedLayout(string mapSeed, Terrain[] terrain,
            int width, int height, out bool[] resourcePresent)
        {
            Resource[] choices = new Resource[terrain.Length];
            resourcePresent = new bool[terrain.Length];
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
                if (terrainType == Terrain.Mountain)
                {
                    int stoneSites = tiles.Count / 2;
                    for (int i = 0; i < tiles.Count; i++)
                    {
                        choices[tiles[i]] = Resource.Stone;
                        resourcePresent[tiles[i]] = i < stoneSites;
                    }
                }
                else if (terrainType == Terrain.Water)
                {
                    int shoreSite = 0;
                    for (int i = 0; i < tiles.Count; i++)
                    {
                        int tile = tiles[i];
                        bool isShore = IsShoreWaterTile(terrain, width, height, tile);
                        choices[tile] = isShore ? options[shoreSite % options.Length] : Resource.Fish;
                        resourcePresent[tile] = isShore;
                        if (isShore)
                        {
                            shoreSite++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < tiles.Count; i++)
                    {
                        choices[tiles[i]] = options[i % options.Length];
                        resourcePresent[tiles[i]] = true;
                    }
                }
            }

            return choices;
        }

        public static bool IsShoreWaterTile(IReadOnlyList<Terrain> terrain, int width, int height, int tile)
        {
            if (terrain == null || width <= 0 || height <= 0 || width * height != terrain.Count ||
                tile < 0 || tile >= terrain.Count || terrain[tile] != Terrain.Water)
            {
                return false;
            }

            for (int candidate = 0; candidate < terrain.Count; candidate++)
            {
                if (terrain[candidate] != Terrain.Water && GetHexDistance(tile, candidate, width) == 1)
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetHexDistance(int firstTile, int secondTile, int width)
        {
            int firstColumn = firstTile % width;
            int firstRow = firstTile / width;
            int secondColumn = secondTile % width;
            int secondRow = secondTile / width;
            int firstX = firstColumn - (firstRow - (firstRow & 1)) / 2;
            int firstZ = firstRow;
            int firstY = -firstX - firstZ;
            int secondX = secondColumn - (secondRow - (secondRow & 1)) / 2;
            int secondZ = secondRow;
            int secondY = -secondX - secondZ;
            return Math.Max(Math.Abs(firstX - secondX),
                Math.Max(Math.Abs(firstY - secondY), Math.Abs(firstZ - secondZ)));
        }

        private static void Save(string mapSeed, Resource[] choices, bool[] resourcePresent)
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
                resources = resourceIds,
                resourcePresent = resourcePresent
            };
            PlayerPrefs.SetString(LayoutKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }

    public sealed class TurnSystem : MonoBehaviour
    {
        private const int PlayerCount = 2;
        private const int WorkersPerPlayer = 12;
        private const int StartingWorkersPerPlayer = 4;
        private const int MonumentStoneCost = 5;
        private const float PanelFraction = 0.25f;
        private const float DragThreshold = 6f;
        // These ratios reproduce the current 20x20 fitted-map marker sizes while making
        // markers track the projected hex radius at every map size and zoom level.
        private const float ResourceIconDiameterPerHexRadius = 0.85f;
        private const float OccupiedResourceIconDiameterPerHexRadius = 0.6f;
        private const float StockpileResourceIconSize = 56f;
        private const float StockpilePlayerLabelGap = 8f;
        private static readonly int ResourceTypeCount = Enum.GetValues(typeof(Resource)).Length;
        private static readonly int ToolTypeCount = Enum.GetValues(typeof(Tool)).Length;
        private const float InventoryWorkerIconSize = 48f;
        private const float WorkerIconDiameterPerHexRadius = 0.455f;
        private const float WorkerEdgeInsetPixels = 2f;
        private const float SelectionPulseSpeed = 4.25f;
        private static readonly Color UnfulfilledFlashColor = new Color32(235, 75, 75, 255);
        private const float HearthIconDiameterPerHexRadius = 0.54f;
        private const float MonumentIconDiameterPerHexRadius = 0.62f;
        private const int WorkerMoveRange = 1;
        private const int TileContentSlotCount = 6;
        private const int TopWorkerSlot = 0;
        private const int BottomRightWorkerSlot = 2;
        private const int BottomLeftWorkerSlot = 4;
        private const float HexEdgeNormalProjection = 0.8660254f;
        private const float ZoomStepMultiplier = 0.85f;
        private const float MinimumZoomFraction = 0.35f;
        private const float MaximumZoomFraction = 1.35f;
        private const float ScrollZoomSensitivity = 0.25f;
        private const float KeyboardPanSpeed = 8f;
        private const float MapPanStartThreshold = 2f;
        private const float WorkerBondThickness = 4f;
        private const float WorkerBondOutlineThickness = 7f;
        private const string DeveloperConsoleControlName = "DeveloperConsoleInput";

        private static readonly string[] SeasonNames = { "Spring", "Summer", "Autumn", "Winter" };
        private static readonly Resource[] FoodResources =
        {
            Resource.Aurochs, Resource.Roots, Resource.Mushrooms, Resource.Fish
        };
        private static readonly Resource[] StockpileResources =
        {
            Resource.Wood, Resource.Stone, Resource.Shells
        };
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
        private readonly WorkerAction[,] workerActions = new WorkerAction[PlayerCount, WorkersPerPlayer];
        private readonly int[,] craftTools = new int[PlayerCount, WorkersPerPlayer];
        private readonly int[,] preserveTargetWorkers = new int[PlayerCount, WorkersPerPlayer];
        private readonly int[,] assignedFood = new int[PlayerCount, WorkersPerPlayer];
        private readonly int[,] workerTools = new int[PlayerCount, WorkersPerPlayer];
        private readonly bool[,] assignedFoodWasPreserved = new bool[PlayerCount, WorkersPerPlayer];
        private readonly bool[] assignedHearthFuel = new bool[PlayerCount];
        private readonly bool[] completedFirstTurn = new bool[PlayerCount];
        private readonly int[,] resourceStockpiles = new int[PlayerCount, ResourceTypeCount];
        private readonly int[,] freshFoodStockpiles = new int[PlayerCount, ResourceTypeCount];
        private readonly int[,] preservedFoodStockpiles = new int[PlayerCount, ResourceTypeCount];
        private readonly int[,] latestSeasonGains = new int[PlayerCount, ResourceTypeCount];
        private readonly int[,] latestPreservedSeasonGains = new int[PlayerCount, ResourceTypeCount];
        private readonly int[,] toolStockpiles = new int[PlayerCount, ToolTypeCount];
        private readonly int[,] latestSeasonToolGains = new int[PlayerCount, ToolTypeCount];
        private readonly int[] sacralityStockpiles = new int[PlayerCount];
        private readonly int[] ancestorCounts = new int[PlayerCount];
        private readonly int[,] ancestorToolCounts = new int[PlayerCount, ToolTypeCount];
        private readonly int[] hearthTiles = { -1, -1 };
        private readonly int[] monumentTiles = new int[PlayerCount * WorkersPerPlayer];
        private readonly int[] monumentBuildSeasons = new int[PlayerCount * WorkersPerPlayer];
        private readonly bool[] feastThrownThisTurn = new bool[PlayerCount];
        private readonly bool[] feastScheduledNextSeason = new bool[PlayerCount];
        private readonly bool[] feastMovementOnlyThisSeason = new bool[PlayerCount];
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
        private int selectedTool = -1;
        private int pressedWorker = -1;
        private int draggingWorker = -1;
        private int pressedFoodResource = -1;
        private int draggingFoodResource = -1;
        private int pressedTool = -1;
        private int draggingTool = -1;
        private Vector2 workerPressPosition;
        private Vector2 foodPressPosition;
        private Vector2 toolPressPosition;
        private Vector2 mapPanLastScreenPosition;
        private bool mapPanCandidate;
        private bool isPanningMap;
        private bool hearthPlacementPointerPressed;
        private bool hearthPlacementWasDragged;
        private float fittedCameraSize = 1f;
        private bool resolutionPhase;
        private bool godPhase;
        private bool foodAssignmentPhase;
        private bool showSeasonGainsDialog;
        private bool showFoodShortageDialog;
        private bool showWorkerActionMenu;
        private bool selectingPreserveTarget;
        private bool showDeveloperConsole;
        private bool focusDeveloperConsole;
        private Vector2 inventoryScrollPosition;
        private int actionMenuWorker = -1;
        private string resolvedSeasonName;
        private int resolvedYear;
        private GodEffect hiddenGodEffect;
        private GodEffect resolvedGodEffect;
        private bool godEffectRevealed;
        private string statusMessage = "Player 1: place your hearth on any non-water hex.";
        private bool statusIsWarning;
        private string developerConsoleInput = string.Empty;
        private string developerConsoleMessage = "Enter: <resource> <amount>";

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle headingStyle;
        private GUIStyle bodyStyle;
        private GUIStyle warningBodyStyle;
        private GUIStyle smallBodyStyle;
        private GUIStyle buttonStyle;
        private GUIStyle zoomButtonStyle;
        private GUIStyle resourceFallbackStyle;
        private GUIStyle stockpileCountStyle;
        private GUIStyle stockpileCountShadowStyle;
        private GUIStyle preservedStockpileCountStyle;
        private GUIStyle preservedStockpileCountShadowStyle;
        private GUIStyle dialogTitleStyle;
        private GUIStyle dialogStyle;
        private GUIStyle tooltipStyle;
        private GUIStyle developerConsoleInputStyle;
        private GUIStyle developerConsoleMessageStyle;
        private Texture2D iconOutlineTexture;
        private Texture2D ancestorBackdropTexture;
        private string hoveredTooltip;

        public int ActivePlayer => activePlayer;
        public int Year => year;
        public string Season => SeasonNames[seasonIndex];
        public bool IsResolutionPhase => resolutionPhase;
        public bool IsGodPhase => godPhase;
        public bool IsFoodAssignmentPhase => foodAssignmentPhase;
        public bool IsActiveHearthFueled => foodAssignmentPhase && assignedHearthFuel[activePlayer];
        public bool IsPlacingHearth => !resolutionPhase && hearthTiles[activePlayer] < 0;
        private bool CanRecallActiveHearth => !completedFirstTurn[activePlayer] && hearthTiles[activePlayer] >= 0;
        public int SelectedWorker => selectedWorker;
        public string StatusMessage => statusMessage;
        public bool StatusIsWarning => statusIsWarning;
        public float MapCameraSize => mapCamera != null ? mapCamera.orthographicSize : 0f;
        public float MinimumMapCameraSize => fittedCameraSize * MinimumZoomFraction;
        public float MaximumMapCameraSize => fittedCameraSize * MaximumZoomFraction;
        private void SetStatusMessage(string message, bool isWarning = false)
        {
            statusMessage = message;
            statusIsWarning = isWarning;
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
            ClearMonuments();
            ClearAssignments();
            ClearStockpiles();
            ClearLatestSeasonGains();
            Array.Clear(ancestorCounts, 0, ancestorCounts.Length);
            Array.Clear(ancestorToolCounts, 0, ancestorToolCounts.Length);
            Array.Clear(sacralityStockpiles, 0, sacralityStockpiles.Length);
            Array.Clear(feastThrownThisTurn, 0, feastThrownThisTurn.Length);
            Array.Clear(feastScheduledNextSeason, 0, feastScheduledNextSeason.Length);
            Array.Clear(feastMovementOnlyThisSeason, 0, feastMovementOnlyThisSeason.Length);
            showSeasonGainsDialog = false;
            showFoodShortageDialog = false;
            godPhase = false;
            foodAssignmentPhase = false;
            ClearWorkerInteraction();
            map?.ClearTileHighlights();
            map?.ClearTileOccupancyOutlines();
            BeginGodPhase();
        }

        private void Awake()
        {
            if (map == null)
            {
                map = GetComponent<HexMap>();
            }
        }

        private void Update()
        {
            if (mapCamera == null || godPhase || showSeasonGainsDialog || showFoodShortageDialog || showWorkerActionMenu ||
                showDeveloperConsole)
            {
                return;
            }

            HandleKeyboardPan();
            HandleMapDragPan();
        }

        private void OnGUI()
        {
            if (map == null)
            {
                return;
            }

            EnsureStyles();
            hoveredTooltip = null;
            HandleDeveloperConsoleToggle(Event.current);
            bool previousGuiEnabled = GUI.enabled;
            bool consoleIsOpen = showDeveloperConsole;
            GUI.enabled = !consoleIsOpen && !godPhase && !showSeasonGainsDialog && !showFoodShortageDialog;
            DrawTileResourceIcons();
            DrawHearthsOnMap();
            DrawMonumentsOnMap();
            DrawWorkerBondsOnMap();
            DrawPlacementSlotsOnMap();
            DrawAssignmentsOnMap();
            DrawMapHeader();
            DrawInventoryPanel();
            if (showWorkerActionMenu)
            {
                DrawWorkerActionMenu();
            }
            GUI.enabled = previousGuiEnabled;

            if (godPhase)
            {
                hoveredTooltip = null;
                DrawGodPhaseDialog();
            }
            else if (showSeasonGainsDialog)
            {
                hoveredTooltip = null;
                DrawSeasonGainsDialog();
            }
            else if (showFoodShortageDialog)
            {
                hoveredTooltip = null;
                DrawFoodShortageDialog();
            }
            else if (!consoleIsOpen)
            {
                if (!showWorkerActionMenu)
                {
                    HandleZoomInput(Event.current);
                    if (!isPanningMap || IsPlacingHearth)
                    {
                        HandlePointerInteraction(Event.current);
                    }
                }

                if (draggingWorker >= 0)
                {
                    DrawDragGhost(Event.current.mousePosition);
                }
                else if (draggingFoodResource >= 0)
                {
                    DrawFoodDragGhost(Event.current.mousePosition);
                }
                else if (draggingTool >= 0)
                {
                    DrawToolDragGhost(Event.current.mousePosition);
                }

                DrawTooltip(Event.current.mousePosition);
            }

            if (consoleIsOpen)
            {
                GUI.enabled = true;
                DrawDeveloperConsole();
                GUI.enabled = previousGuiEnabled;
            }
        }

        private void HandleDeveloperConsoleToggle(Event current)
        {
            if (current.type != EventType.KeyDown || current.character != '#')
            {
                return;
            }

            if (showDeveloperConsole)
            {
                showDeveloperConsole = false;
                current.Use();
                return;
            }

            showDeveloperConsole = true;
            focusDeveloperConsole = true;
            developerConsoleInput = string.Empty;
            developerConsoleMessage = "Enter: <resource> <amount>";
            current.Use();
        }

        private void DrawDeveloperConsole()
        {
            const float width = 430f;
            const float height = 88f;
            Rect panel = new Rect(Screen.width - width - 20f, Screen.height - height - 20f, width, height);
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.88f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previousColor;
            GUI.Box(panel, GUIContent.none, dialogStyle);

            Event current = Event.current;
            if (current.type == EventType.KeyDown)
            {
                if (current.keyCode == KeyCode.Escape)
                {
                    showDeveloperConsole = false;
                    current.Use();
                    return;
                }

                if (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
                {
                    if (ExecuteDeveloperConsoleCommand(developerConsoleInput))
                    {
                        showDeveloperConsole = false;
                    }

                    current.Use();
                    return;
                }
            }

            Rect inputRect = new Rect(panel.x + 12f, panel.y + 10f, panel.width - 24f, 34f);
            GUI.SetNextControlName(DeveloperConsoleControlName);
            developerConsoleInput = GUI.TextField(inputRect, developerConsoleInput, developerConsoleInputStyle);

            if (focusDeveloperConsole && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl(DeveloperConsoleControlName);
                focusDeveloperConsole = false;
            }

            GUI.Label(new Rect(panel.x + 12f, panel.y + 48f, panel.width - 24f, 28f),
                developerConsoleMessage, developerConsoleMessageStyle);
        }

        private bool ExecuteDeveloperConsoleCommand(string command)
        {
            string[] parts = command.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2 || !int.TryParse(parts[parts.Length - 1], out int amount) || amount <= 0)
            {
                developerConsoleMessage = "Use: <resource> <positive amount>";
                return false;
            }

            string resourceName = string.Join(" ", parts, 0, parts.Length - 1);
            bool preserved = resourceName.StartsWith("preserved ", StringComparison.OrdinalIgnoreCase);
            if (preserved)
            {
                resourceName = resourceName.Substring("preserved ".Length).Trim();
            }

            if (string.Equals(resourceName, "sacrality", StringComparison.OrdinalIgnoreCase))
            {
                if (preserved)
                {
                    developerConsoleMessage = "Sacrality cannot be preserved.";
                    return false;
                }

                AddSacrality(activePlayer, amount);
                developerConsoleMessage = $"Granted {amount} Sacrality to Player {activePlayer + 1}.";
            }
            else if (Enum.TryParse(resourceName, true, out Resource resource))
            {
                int resourceIndex = (int)resource;
                if (preserved && !ResourceCatalog.IsFood(resource))
                {
                    developerConsoleMessage = $"{ResourceCatalog.GetLabel(resource)} cannot be preserved.";
                    return false;
                }

                resourceStockpiles[activePlayer, resourceIndex] += amount;
                if (ResourceCatalog.IsFood(resource))
                {
                    if (preserved)
                    {
                        preservedFoodStockpiles[activePlayer, resourceIndex] += amount;
                    }
                    else
                    {
                        freshFoodStockpiles[activePlayer, resourceIndex] += amount;
                    }
                }

                string preservationLabel = preserved ? "preserved " : string.Empty;
                developerConsoleMessage = $"Granted {amount} {preservationLabel}{ResourceCatalog.GetLabel(resource)} to Player {activePlayer + 1}.";
            }
            else
            {
                developerConsoleMessage = $"Unknown resource: {resourceName}.";
                return false;
            }

            developerConsoleInput = string.Empty;
            focusDeveloperConsole = true;
            return true;
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

            float footerHeight = GetInventoryFooterHeight();
            Rect contentViewport = new Rect(0f, 0f, width, Screen.height - footerHeight);
            // Leave room for the vertical scrollbar so it does not create horizontal overflow.
            Rect contentRect = new Rect(0f, 0f, width - 22f,
                Mathf.Max(contentViewport.height, GetInventoryContentHeight()));
            inventoryScrollPosition = GUI.BeginScrollView(contentViewport, inventoryScrollPosition, contentRect,
                false, true);

            float x = 18f;
            float contentWidth = width - 36f;
            float y = 18f;

            GUI.Label(new Rect(x, y, contentWidth, 58f), "OLDEN TOP", titleStyle);
            y += 66f;
            GUI.Label(new Rect(x, y, contentWidth, 42f), $"Year {year}  •  {SeasonNames[seasonIndex]}", headingStyle);
            y += 48f;

            y += DrawGodEffectsCard(x, y, contentWidth);

            string phaseText = godPhase
                ? "GODS PHASE"
                : resolutionPhase
                ? "COMMITMENTS READY"
                : foodAssignmentPhase
                    ? $"PLAYER {activePlayer + 1} FEEDS & FUELS"
                : IsPlacingHearth
                    ? $"PLAYER {activePlayer + 1} PLACES HEARTH"
                    : feastMovementOnlyThisSeason[activePlayer]
                        ? $"PLAYER {activePlayer + 1} MOVES ONLY"
                    : $"PLAYER {activePlayer + 1} ASSIGNS";
            Color previousColor = GUI.color;
            GUI.color = resolutionPhase || godPhase ? Color.white : PlayerColors[activePlayer];
            GUI.Label(new Rect(x, y, contentWidth, 48f), phaseText, headingStyle);
            GUI.color = previousColor;
            y += 58f;

            GUI.Label(new Rect(x, y, contentWidth, 86f), statusMessage,
                statusIsWarning ? warningBodyStyle : bodyStyle);
            y += 94f;

            GUI.Label(new Rect(x, y, contentWidth, 42f), "STOCKPILES", headingStyle);
            y += 46f;
            for (int player = 0; player < PlayerCount; player++)
            {
                y += DrawPlayerStockpile(x + 8f, y, contentWidth - 8f, player);
                y += DrawSacralityStockpile(x + 8f, y, contentWidth - 8f, player);
                y += DrawPlayerToolStockpile(x + 8f, y, contentWidth - 8f, player);
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
            }
            else
            {
                GUI.Label(new Rect(x, y, contentWidth, 110f),
                    "Both players have committed. Workers remain on the map until the next season begins.", bodyStyle);
            }

            GUI.EndScrollView();
            DrawInventoryFooter(x, contentWidth, footerHeight);
        }

        private float GetInventoryContentHeight()
        {
            const float playerStockpileHeight = 290f;
            const float sacralityHeight = 42f;
            const float toolStockpileHeight = 94f;

            // Title, season, god effects, phase, status, and the stockpile heading.
            float height = 422f;
            height += PlayerCount * (playerStockpileHeight + sacralityHeight + toolStockpileHeight);
            height += 14f + 32f + 36f;

            for (int player = 0; player < PlayerCount; player++)
            {
                bool hasAncestorTools = false;
                for (int tool = 0; tool < ToolTypeCount; tool++)
                {
                    if (ancestorToolCounts[player, tool] > 0)
                    {
                        hasAncestorTools = true;
                        break;
                    }
                }

                height += 46f + (hasAncestorTools ? 16f : 4f);
            }

            height += 8f + 42f + 48f;
            height += IsPlacingHearth ? 140f : resolutionPhase ? 110f : InventoryWorkerIconSize * 2f + 16f;
            return height + 18f;
        }

        private float GetInventoryFooterHeight()
        {
            if (IsPlacingHearth)
            {
                return 0f;
            }

            if (resolutionPhase)
            {
                return 72f;
            }

            return foodAssignmentPhase ? 128f : 188f;
        }

        private void DrawInventoryFooter(float x, float contentWidth, float footerHeight)
        {
            if (footerHeight <= 0f)
            {
                return;
            }

            Rect footer = new Rect(0f, Screen.height - footerHeight, Screen.width * PanelFraction, footerHeight);
            Color previousColor = GUI.color;
            GUI.color = new Color32(12, 13, 12, 255);
            GUI.DrawTexture(footer, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previousColor;

            if (resolutionPhase)
            {
                string nextSeason = SeasonNames[(seasonIndex + 1) % SeasonNames.Length];
                GUI.backgroundColor = new Color32(139, 187, 114, 255);
                if (GUI.Button(new Rect(x, Screen.height - 72f, contentWidth, 56f), $"Begin {nextSeason}", buttonStyle))
                {
                    AdvanceSeason();
                }

                GUI.backgroundColor = Color.white;
                return;
            }

            if (foodAssignmentPhase)
            {
                if (GUI.Button(new Rect(x, Screen.height - 128f, contentWidth, 52f),
                        "Clear food and fuel", buttonStyle))
                {
                    ClearActivePlayerResourceAssignments();
                }

                GUI.backgroundColor = PlayerColors[activePlayer];
                if (GUI.Button(new Rect(x, Screen.height - 68f, contentWidth, 52f),
                        "Finish food and fuel", buttonStyle))
                {
                    TryEndFoodAssignments();
                }
            }
            else
            {
                bool canThrowFeast = CanThrowFeast(activePlayer);
                GUI.enabled = canThrowFeast;
                if (GUI.Button(new Rect(x, Screen.height - 188f, contentWidth, 52f),
                        feastThrownThisTurn[activePlayer]
                            ? "Feast already held this season"
                            : "Throw feast  •  1 Stone • 1 Wood • 1 Aurochs • 1 Mushroom", buttonStyle))
                {
                    ThrowFeast(activePlayer);
                }
                GUI.enabled = true;

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

        private void DrawWorkerCard(Rect rect, int worker)
        {
            bool placedThisTurn = workerPlacedThisTurn[activePlayer, worker];
            bool selected = selectedWorker == worker;

            DrawWorkerIcon(rect, activePlayer, placedThisTurn,
                placedThisTurn ? "placed this turn" : "ready to move", selected);
            DrawAssignedFoodOverlay(rect, activePlayer, worker);
            DrawWorkerToolOverlay(rect, activePlayer, worker);
            if (foodAssignmentPhase && assignedFood[activePlayer, worker] < 0)
            {
                DrawUnfulfilledFlash(rect);
            }

            Event current = Event.current;
            if (!showWorkerActionMenu && !selectingPreserveTarget && current.type == EventType.MouseDown &&
                current.button == 0 && rect.Contains(current.mousePosition))
            {
                if (foodAssignmentPhase)
                {
                    TryAssignSelectedProvisionToActiveWorker(worker, current);
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

            if (selectingPreserveTarget)
            {
                if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
                {
                    selectingPreserveTarget = false;
                    SetWorkerActionToGather(actionMenuWorker);
                    actionMenuWorker = -1;
                    SetStatusMessage("Preserve cancelled; the worker will gather instead.");
                    current.Use();
                }

                return;
            }

            if (IsPlacingHearth)
            {
                if (current.type == EventType.MouseDown && current.button == 0)
                {
                    hearthPlacementPointerPressed = TryGetTileAtGuiPosition(current.mousePosition,
                        out _);
                    hearthPlacementWasDragged = false;
                    current.Use();
                }
                else if (current.type == EventType.MouseUp && current.button == 0 &&
                    hearthPlacementPointerPressed)
                {
                    int hearthTile = -1;
                    bool shouldPlaceHearth = !hearthPlacementWasDragged &&
                        TryGetTileAtGuiPosition(current.mousePosition, out hearthTile);
                    hearthPlacementPointerPressed = false;
                    hearthPlacementWasDragged = false;
                    if (shouldPlaceHearth)
                    {
                        TryPlaceActiveHearth(hearthTile);
                    }

                    current.Use();
                }

                return;
            }

            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape &&
                (pressedWorker >= 0 || draggingWorker >= 0))
            {
                ClearPointerState();
                SetStatusMessage("Worker placement cancelled.");
                current.Use();
                return;
            }

            if (current.type == EventType.MouseDrag && pressedWorker >= 0 &&
                Vector2.Distance(workerPressPosition, current.mousePosition) >= DragThreshold)
            {
                draggingWorker = pressedWorker;
                SetStatusMessage("Drop the worker onto an available slot, or elsewhere to cancel the move.");
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
                int worker = pressedWorker;
                ClearPointerState();
                if (workerPlacedThisTurn[activePlayer, worker])
                {
                    OpenWorkerActionMenu(worker);
                }
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
                (pressedFoodResource >= 0 || draggingFoodResource >= 0 || pressedTool >= 0 || draggingTool >= 0))
            {
                ClearFoodPointerState();
                ClearToolPointerState();
                SetStatusMessage("Provision assignment cancelled.");
                current.Use();
                return;
            }

            if (current.type == EventType.MouseDrag && pressedTool >= 0 &&
                Vector2.Distance(toolPressPosition, current.mousePosition) >= DragThreshold)
            {
                draggingTool = pressedTool;
                SetStatusMessage("Drop the tool onto a worker who does not already have one.");
                current.Use();
                return;
            }

            if (current.type == EventType.MouseUp && current.button == 0 && draggingTool >= 0)
            {
                if (TryGetActiveWorkerAtGuiPosition(current.mousePosition, out int worker))
                {
                    TryAssignToolToActiveWorker(worker, (Tool)draggingTool);
                }
                else
                {
                    SetStatusMessage("Tool assignment cancelled. Drop it onto a worker.");
                }

                ClearToolPointerState();
                current.Use();
                return;
            }

            if (current.type == EventType.MouseUp && current.button == 0 && pressedTool >= 0)
            {
                pressedTool = -1;
                current.Use();
                return;
            }

            if (current.type == EventType.MouseDrag && pressedFoodResource >= 0 &&
                Vector2.Distance(foodPressPosition, current.mousePosition) >= DragThreshold)
            {
                draggingFoodResource = pressedFoodResource;
                SetStatusMessage(draggingFoodResource == (int)Resource.Wood
                    ? "Drop the wood onto your hearth."
                    : "Drop the food onto one of your workers.");
                current.Use();
                return;
            }

            if (current.type == EventType.MouseUp && current.button == 0 && draggingFoodResource >= 0)
            {
                int resource = draggingFoodResource;
                if (resource == (int)Resource.Wood &&
                    TryGetActiveHearthAtGuiPosition(current.mousePosition))
                {
                    TryAssignWoodToActiveHearth();
                }
                else if (ResourceCatalog.IsFood((Resource)resource) &&
                         TryGetActiveWorkerAtGuiPosition(current.mousePosition, out int worker))
                {
                    TryAssignFoodToActiveWorker(worker, (Resource)resource);
                }
                else
                {
                    SetStatusMessage(resource == (int)Resource.Wood
                        ? "Wood assignment cancelled. Drop wood onto your hearth."
                        : "Food assignment cancelled. Drop food onto one of your workers.");
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
                float iconSize = GetResourceIconSize(tile);
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

        private void DrawToolIcon(Rect rect, Tool tool)
        {
            Texture2D icon = ToolCatalog.GetIcon(tool);
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
                hoveredTooltip = tool.ToString();
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

            float resourcesY = y + headingHeight + StockpilePlayerLabelGap;
            for (int resourceIndex = 0; resourceIndex < StockpileResources.Length; resourceIndex++)
            {
                Resource resource = StockpileResources[resourceIndex];
                Rect iconRect = new Rect(
                    x + resourceIndex * (StockpileResourceIconSize + horizontalGap),
                    resourcesY,
                    StockpileResourceIconSize,
                    StockpileResourceIconSize);
                DrawStockpileResourceIcon(iconRect, resource,
                    resourceStockpiles[player, (int)resource], player, false);
            }

            float freshY = resourcesY + StockpileResourceIconSize + verticalGap;
            GUI.Label(new Rect(x, freshY, width, headingHeight), "FRESH", smallBodyStyle);
            for (int foodColumn = 0; foodColumn < 4; foodColumn++)
            {
                Resource resource = FoodResources[foodColumn];
                Rect iconRect = new Rect(x + foodColumn * (StockpileResourceIconSize + horizontalGap),
                    freshY + headingHeight, StockpileResourceIconSize, StockpileResourceIconSize);
                DrawStockpileResourceIcon(iconRect, resource,
                    freshFoodStockpiles[player, (int)resource], player, false);
            }

            float preservedY = freshY + headingHeight + StockpileResourceIconSize + verticalGap;
            GUI.Label(new Rect(x, preservedY, width, headingHeight), "PRESERVED", smallBodyStyle);
            for (int foodColumn = 0; foodColumn < 4; foodColumn++)
            {
                Resource resource = FoodResources[foodColumn];
                Rect iconRect = new Rect(x + foodColumn * (StockpileResourceIconSize + horizontalGap),
                    preservedY + headingHeight, StockpileResourceIconSize, StockpileResourceIconSize);
                DrawStockpileResourceIcon(iconRect, resource,
                    preservedFoodStockpiles[player, (int)resource], player, false);
            }

            return preservedY + headingHeight + StockpileResourceIconSize + 12f - y;
        }

        private void DrawStockpileResourceIcon(Rect rect, Resource resource, int amount, int player, bool preserved)
        {
            Color previousColor = GUI.color;
            if (amount == 0)
            {
                GUI.color = new Color(0.42f, 0.42f, 0.42f, 0.48f);
            }

            DrawResourceIcon(rect, resource);
            Rect countRect = new Rect(
                rect.x + rect.width * (preserved ? 0.08f : 0.34f),
                rect.y + rect.height * 0.34f,
                rect.width * 0.58f,
                rect.height * 0.58f);
            GUI.Label(new Rect(countRect.x + 1.5f, countRect.y + 1.5f,
                countRect.width, countRect.height), amount.ToString(),
                preserved ? preservedStockpileCountShadowStyle : stockpileCountShadowStyle);
            GUI.Label(countRect, amount.ToString(), preserved ? preservedStockpileCountStyle : stockpileCountStyle);

            bool canAssignResource = !preserved && foodAssignmentPhase && player == activePlayer &&
                                     (ResourceCatalog.IsFood(resource) || resource == Resource.Wood);
            if (canAssignResource && selectedFoodResource == (int)resource)
            {
                DrawSelectedWorkerHighlight(rect, activePlayer);
            }

            Event current = Event.current;
            if (canAssignResource && current.type == EventType.MouseDown && current.button == 0 &&
                rect.Contains(current.mousePosition))
            {
                BeginFoodPress(resource, current);
            }

            GUI.color = previousColor;
        }

        private float DrawSacralityStockpile(float x, float y, float width, int player)
        {
            const float iconSize = 38f;
            Texture2D icon = GodIconCatalog.GetIcon();
            Rect iconRect = new Rect(x, y, iconSize, iconSize);
            if (icon != null)
            {
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
            }

            Color previousColor = GUI.color;
            GUI.color = PlayerColors[player];
            GUI.Label(new Rect(x + iconSize + 8f, y, width - iconSize - 8f, iconSize),
                $"SACRALITY: {sacralityStockpiles[player]}  •  Monuments: {GetMonumentCount(player)}", smallBodyStyle);
            GUI.color = previousColor;
            return iconSize + 4f;
        }

        private float DrawPlayerToolStockpile(float x, float y, float width, int player)
        {
            const float headingHeight = 24f;
            const float horizontalGap = 10f;
            GUI.Label(new Rect(x, y, width, headingHeight), "TOOLS", smallBodyStyle);
            for (int toolIndex = 0; toolIndex < ToolTypeCount; toolIndex++)
            {
                Rect iconRect = new Rect(x + toolIndex * (StockpileResourceIconSize + horizontalGap),
                    y + headingHeight, StockpileResourceIconSize, StockpileResourceIconSize);
                DrawToolStockpileIcon(iconRect, (Tool)toolIndex, toolStockpiles[player, toolIndex], player);
            }

            return headingHeight + StockpileResourceIconSize + 14f;
        }

        private void DrawToolStockpileIcon(Rect rect, Tool tool, int amount, int player)
        {
            Color previousColor = GUI.color;
            if (amount == 0)
            {
                GUI.color = new Color(0.42f, 0.42f, 0.42f, 0.48f);
            }

            DrawToolIcon(rect, tool);
            Rect countRect = new Rect(rect.x + rect.width * 0.34f, rect.y + rect.height * 0.34f,
                rect.width * 0.58f, rect.height * 0.58f);
            GUI.Label(new Rect(countRect.x + 1.5f, countRect.y + 1.5f, countRect.width, countRect.height),
                amount.ToString(), stockpileCountShadowStyle);
            GUI.Label(countRect, amount.ToString(), stockpileCountStyle);

            bool canAssignTool = foodAssignmentPhase && player == activePlayer;
            if (canAssignTool && selectedTool == (int)tool)
            {
                DrawSelectedWorkerHighlight(rect, activePlayer);
            }

            Event current = Event.current;
            if (canAssignTool && current.type == EventType.MouseDown && current.button == 0 &&
                rect.Contains(current.mousePosition))
            {
                BeginToolPress(tool, current);
            }

            GUI.color = previousColor;
        }

        private void DrawWorkerActionMenu()
        {
            if (actionMenuWorker < 0 || actionMenuWorker >= WorkersPerPlayer ||
                !workerAlive[activePlayer, actionMenuWorker])
            {
                CloseWorkerActionMenu();
                return;
            }

            int tile = assignments[activePlayer, actionMenuWorker];
            if (tile < 0)
            {
                CloseWorkerActionMenu();
                return;
            }

            List<Tool> craftableTools = new List<Tool>();
            foreach (Tool tool in ToolCatalog.All)
            {
                if (CanCraftWithCommittedCosts(tool, actionMenuWorker))
                {
                    craftableTools.Add(tool);
                }
            }

            bool canPreserve = tile == hearthTiles[activePlayer] && HasEligiblePreserveTarget(actionMenuWorker);
            bool canRitual = CanCommitRitual(activePlayer, actionMenuWorker);
            bool canBuildMonument = CanCommitMonument(activePlayer, actionMenuWorker);
            float width = Mathf.Min(420f, Screen.width - 40f);
            float height = 156f + craftableTools.Count * 52f + (canPreserve ? 56f : 0f) +
                           (canRitual ? 56f : 0f) + (canBuildMonument ? 56f : 0f);
            Rect dialog = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUI.color = new Color32(20, 22, 20, 248);
            GUI.DrawTexture(dialog, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = Color.white;
            GUI.Box(dialog, GUIContent.none, dialogStyle);

            float x = dialog.x + 18f;
            float contentWidth = dialog.width - 36f;
            Resource resource = map.HasResource(tile) ? map.GetSelectedResource(tile) : Resource.Wood;
            string gatherLabel = map.HasResource(tile)
                ? $"Gather {ResourceCatalog.GetLabel(resource)}"
                : "Gather (nothing here)";
            GUI.Label(new Rect(x, dialog.y + 12f, contentWidth, 30f), "CHOOSE WORKER ACTION", smallBodyStyle);
            if (GUI.Button(new Rect(x, dialog.y + 48f, contentWidth, 48f), gatherLabel, buttonStyle))
            {
                SetWorkerActionToGather(actionMenuWorker);
                SetStatusMessage($"{gatherLabel} selected.");
                CloseWorkerActionMenu();
            }

            float actionY = dialog.y + 104f;
            GUI.Label(new Rect(x, actionY, contentWidth, 24f), "CRAFT A TOOL", smallBodyStyle);
            actionY += 26f;
            if (craftableTools.Count == 0)
            {
                GUI.Label(new Rect(x, actionY, contentWidth, 30f), "No tools can be crafted with this stockpile.", bodyStyle);
                actionY += 38f;
            }
            else
            {
                for (int i = 0; i < craftableTools.Count; i++)
                {
                    Tool tool = craftableTools[i];
                    Rect buttonRect = new Rect(x, actionY, contentWidth, 46f);
                    if (GUI.Button(buttonRect, $"      {tool}     {ToolCatalog.GetCostLabel(tool)}", buttonStyle))
                    {
                        SetWorkerActionToCraft(actionMenuWorker, tool);
                        SetStatusMessage($"Craft {tool} selected. It will enter the tool stockpile this season.");
                        CloseWorkerActionMenu();
                    }

                    DrawToolIcon(new Rect(buttonRect.x + 8f, buttonRect.y + 5f, 36f, 36f), tool);
                    actionY += 52f;
                }
            }

            if (canPreserve && GUI.Button(new Rect(x, actionY, contentWidth, 48f), "Preserve", buttonStyle))
            {
                BeginPreserveTargetSelection(actionMenuWorker);
                return;
            }
            if (canPreserve)
            {
                actionY += 56f;
            }

            if (canRitual && GUI.Button(new Rect(x, actionY, contentWidth, 48f),
                    $"Ritual ({SeasonNames[seasonIndex]})  •  1 Mushroom • 1 Aurochs", buttonStyle))
            {
                SetWorkerActionToRitual(actionMenuWorker);
                SetStatusMessage("Ritual selected. It will consume a Mushroom and Aurochs for +1 Sacrality during execution.");
                CloseWorkerActionMenu();
                return;
            }
            if (canRitual)
            {
                actionY += 56f;
            }

            if (canBuildMonument && GUI.Button(new Rect(x, actionY, contentWidth, 48f),
                    "Build monument  •  5 Stone", buttonStyle))
            {
                SetWorkerActionToBuildMonument(actionMenuWorker);
                SetStatusMessage("Build monument selected. It will replace this tile's resource during execution.");
                CloseWorkerActionMenu();
            }

            Event current = Event.current;
            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
            {
                CloseWorkerActionMenu();
                current.Use();
            }
            else if (current.type == EventType.MouseDown && current.button == 0 && !dialog.Contains(current.mousePosition))
            {
                CloseWorkerActionMenu();
                current.Use();
            }
        }

        private void OpenWorkerActionMenu(int worker)
        {
            if (resolutionPhase || foodAssignmentPhase || worker < 0 || worker >= WorkersPerPlayer ||
                !workerAlive[activePlayer, worker] || assignments[activePlayer, worker] < 0)
            {
                return;
            }

            if (feastMovementOnlyThisSeason[activePlayer])
            {
                SetStatusMessage("This tribe is feasting this season. Workers may move, but cannot perform actions.");
                return;
            }

            selectingPreserveTarget = false;
            actionMenuWorker = worker;
            showWorkerActionMenu = true;
            ClearPointerState();
            SetStatusMessage("Choose this worker's action.");
        }

        private void CloseWorkerActionMenu()
        {
            showWorkerActionMenu = false;
            actionMenuWorker = -1;
        }

        private void BeginPreserveTargetSelection(int worker)
        {
            if (!HasEligiblePreserveTarget(worker))
            {
                SetStatusMessage("No adjacent food-gathering worker can be preserved.", isWarning: true);
                return;
            }

            showWorkerActionMenu = false;
            actionMenuWorker = worker;
            selectingPreserveTarget = true;
            SetStatusMessage("Choose an adjacent food-gathering worker to preserve.");
        }

        private void SetWorkerActionToGather(int worker)
        {
            workerActions[activePlayer, worker] = WorkerAction.Gather;
            craftTools[activePlayer, worker] = -1;
            preserveTargetWorkers[activePlayer, worker] = -1;
            for (int candidate = 0; candidate < WorkersPerPlayer; candidate++)
            {
                if (preserveTargetWorkers[activePlayer, candidate] == worker)
                {
                    workerActions[activePlayer, candidate] = WorkerAction.Gather;
                    preserveTargetWorkers[activePlayer, candidate] = -1;
                }
            }
        }

        private void SetWorkerActionToCraft(int worker, Tool tool)
        {
            workerActions[activePlayer, worker] = WorkerAction.Craft;
            craftTools[activePlayer, worker] = (int)tool;
            preserveTargetWorkers[activePlayer, worker] = -1;
        }

        private void SetWorkerActionToRitual(int worker)
        {
            SetWorkerActionToGather(worker);
            workerActions[activePlayer, worker] = WorkerAction.Ritual;
        }

        private void SetWorkerActionToBuildMonument(int worker)
        {
            SetWorkerActionToGather(worker);
            workerActions[activePlayer, worker] = WorkerAction.BuildMonument;
        }

        private bool CanCommitRitual(int player, int worker)
        {
            if (player < 0 || player >= PlayerCount || worker < 0 || worker >= WorkersPerPlayer)
            {
                return false;
            }

            int tile = assignments[player, worker];
            if (GetMonumentBuildSeason(tile) != seasonIndex)
            {
                return false;
            }

            int availableMushrooms = resourceStockpiles[player, (int)Resource.Mushrooms];
            int availableAurochs = resourceStockpiles[player, (int)Resource.Aurochs];
            for (int candidate = 0; candidate < WorkersPerPlayer; candidate++)
            {
                if (candidate != worker && workerActions[player, candidate] == WorkerAction.Ritual)
                {
                    availableMushrooms--;
                    availableAurochs--;
                }
            }

            return availableMushrooms > 0 && availableAurochs > 0;
        }

        private bool CanCommitMonument(int player, int worker)
        {
            if (map == null || player < 0 || player >= PlayerCount || worker < 0 || worker >= WorkersPerPlayer)
            {
                return false;
            }

            int tile = assignments[player, worker];
            if (tile < 0 || tile == hearthTiles[player] || map.GetTerrain(tile) == Terrain.Water ||
                IsMonumentTile(tile) || IsMonumentBuildCommitted(tile, worker))
            {
                return false;
            }

            int available = resourceStockpiles[player, (int)Resource.Stone];
            for (int candidate = 0; candidate < WorkersPerPlayer; candidate++)
            {
                if (candidate != worker && workerActions[player, candidate] == WorkerAction.BuildMonument)
                {
                    available -= MonumentStoneCost;
                }
                else if (candidate != worker && workerActions[player, candidate] == WorkerAction.Craft)
                {
                    int toolIndex = craftTools[player, candidate];
                    if (toolIndex >= 0 && toolIndex < ToolTypeCount &&
                        ToolCatalog.UsesCost((Tool)toolIndex, Resource.Stone))
                    {
                        available -= ToolCatalog.GetCostAmount((Tool)toolIndex, Resource.Stone);
                    }
                }
            }

            return available >= MonumentStoneCost;
        }

        private bool IsMonumentBuildCommitted(int tile, int ignoredWorker)
        {
            for (int player = 0; player < PlayerCount; player++)
            {
                if (feastMovementOnlyThisSeason[player])
                {
                    continue;
                }

                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    if (worker != ignoredWorker && workerActions[player, worker] == WorkerAction.BuildMonument &&
                        assignments[player, worker] == tile)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CanCraftWithCommittedCosts(Tool tool, int craftingWorker)
        {
            foreach (Resource cost in ToolCatalog.GetCosts(tool))
            {
                int available = resourceStockpiles[activePlayer, (int)cost];
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    if (worker == craftingWorker || workerActions[activePlayer, worker] != WorkerAction.Craft)
                    {
                        if (worker != craftingWorker && cost == Resource.Stone &&
                            workerActions[activePlayer, worker] == WorkerAction.BuildMonument)
                        {
                            available -= MonumentStoneCost;
                        }
                        continue;
                    }

                    int committedTool = craftTools[activePlayer, worker];
                    if (committedTool >= 0 && committedTool < ToolTypeCount &&
                        ToolCatalog.UsesCost((Tool)committedTool, cost))
                    {
                        available -= ToolCatalog.GetCostAmount((Tool)committedTool, cost);
                    }
                }

                if (available < ToolCatalog.GetCostAmount(tool, cost))
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasEligiblePreserveTarget(int preserver)
        {
            for (int worker = 0; worker < WorkersPerPlayer; worker++)
            {
                if (IsEligiblePreserveTarget(preserver, worker))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsEligiblePreserveTarget(int preserver, int target)
        {
            if (map == null || preserver < 0 || preserver >= WorkersPerPlayer ||
                target < 0 || target >= WorkersPerPlayer || preserver == target ||
                !workerAlive[activePlayer, preserver] || !workerAlive[activePlayer, target] ||
                assignments[activePlayer, preserver] != hearthTiles[activePlayer] ||
                !workerPlacedThisTurn[activePlayer, target] ||
                workerActions[activePlayer, target] != WorkerAction.Gather)
            {
                return false;
            }

            int targetTile = assignments[activePlayer, target];
            return targetTile >= 0 && map.GetHexDistance(hearthTiles[activePlayer], targetTile) == 1 &&
                   map.HasResource(targetTile) && ResourceCatalog.IsFood(map.GetSelectedResource(targetTile)) &&
                   !IsWorkerAlreadyPreserved(target);
        }

        private bool IsWorkerAlreadyPreserved(int target)
        {
            for (int worker = 0; worker < WorkersPerPlayer; worker++)
            {
                if (workerActions[activePlayer, worker] == WorkerAction.Preserve &&
                    preserveTargetWorkers[activePlayer, worker] == target)
                {
                    return true;
                }
            }

            return false;
        }

        private void TrySelectPreserveTarget(int target, Event current)
        {
            if (!IsEligiblePreserveTarget(actionMenuWorker, target))
            {
                SetStatusMessage("Choose an adjacent worker gathering food that is not already preserved.", isWarning: true);
                current.Use();
                return;
            }

            workerActions[activePlayer, actionMenuWorker] = WorkerAction.Preserve;
            craftTools[activePlayer, actionMenuWorker] = -1;
            preserveTargetWorkers[activePlayer, actionMenuWorker] = target;
            selectingPreserveTarget = false;
            actionMenuWorker = -1;
            SetStatusMessage("Preserve selected. That worker's food will be permanent.");
            current.Use();
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

            int displayedTools = 0;
            for (int toolIndex = 0; toolIndex < ToolTypeCount; toolIndex++)
            {
                int amount = ancestorToolCounts[player, toolIndex];
                if (amount <= 0)
                {
                    continue;
                }

                Rect toolRect = new Rect(x + iconSize + 8f + displayedTools * 42f, y + 28f, 30f, 30f);
                DrawToolIcon(toolRect, (Tool)toolIndex);
                GUI.Label(new Rect(toolRect.xMax - 2f, toolRect.yMax - 12f, 18f, 16f), amount.ToString(),
                    stockpileCountStyle);
                displayedTools++;
            }

            return iconSize + (displayedTools > 0 ? 16f : 4f);
        }

        private void DrawSeasonGainsDialog()
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.52f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height),
                Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previousColor;

            float width = Mathf.Min(680f, Screen.width - 40f);
            float height = Mathf.Min(800f, Screen.height - 40f);
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

        private void DrawGodPhaseDialog()
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.58f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height),
                Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previousColor;

            float width = Mathf.Min(560f, Screen.width - 40f);
            float height = Mathf.Min(440f, Screen.height - 40f);
            Rect dialog = new Rect((Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f, width, height);
            GUI.color = new Color32(20, 22, 20, 248);
            GUI.DrawTexture(dialog, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previousColor;
            GUI.Box(dialog, GUIContent.none, dialogStyle);

            float x = dialog.x + 28f;
            float contentWidth = dialog.width - 56f;
            GUI.Label(new Rect(x, dialog.y + 18f, contentWidth, 44f), "GODS PHASE", dialogTitleStyle);

            Rect iconRect = new Rect((Screen.width - 112f) * 0.5f, dialog.y + 76f, 112f, 112f);
            Texture2D icon = GodIconCatalog.GetIcon();
            if (icon != null)
            {
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.Label(iconRect, "✦", dialogTitleStyle);
            }

            GUI.Label(new Rect(x, dialog.y + 210f, contentWidth, 64f),
                "The gods have set this season's fate.", bodyStyle);
            GUI.Label(new Rect(x, dialog.y + 274f, contentWidth, 48f),
                "Its effect remains hidden until it is revealed later.", smallBodyStyle);
            if (GUI.Button(new Rect(x, dialog.yMax - 70f, contentWidth, 48f), "Continue", buttonStyle))
            {
                DismissGodPhase();
            }

            Event current = Event.current;
            if (current.type == EventType.KeyDown &&
                (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.Escape))
            {
                DismissGodPhase();
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

            for (int toolIndex = 0; toolIndex < ToolTypeCount; toolIndex++)
            {
                int amount = latestSeasonToolGains[player, toolIndex];
                if (amount <= 0)
                {
                    continue;
                }

                Rect iconRect = new Rect(x + drawnResources * (iconSize + iconGap),
                    y + headingHeight + 6f, iconSize, iconSize);
                DrawToolIcon(iconRect, (Tool)toolIndex);
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

            float resultHeight = headingHeight + iconSize + 16f;

            if (HasPreservedSeasonGains(player))
            {
                float preservedY = y + resultHeight - 8f;
                GUI.Label(new Rect(x, preservedY, width, headingHeight), "PRESERVED", smallBodyStyle);
                int drawnPreservedFoods = 0;
                for (int foodIndex = 0; foodIndex < FoodResources.Length; foodIndex++)
                {
                    Resource resource = FoodResources[foodIndex];
                    int amount = latestPreservedSeasonGains[player, (int)resource];
                    if (amount <= 0)
                    {
                        continue;
                    }

                    Rect iconRect = new Rect(x + drawnPreservedFoods * (iconSize + iconGap),
                        preservedY + headingHeight, iconSize, iconSize);
                    DrawSeasonGainResourceIcon(iconRect, resource, amount, true);
                    drawnPreservedFoods++;
                }

                resultHeight += headingHeight + iconSize + 12f;
            }

            return resultHeight + DrawGodGift(x, y + resultHeight, width, player);
        }

        private float DrawGodEffectsCard(float x, float y, float width)
        {
            const float iconSize = 46f;
            const float cardHeight = 82f;
            Rect cardRect = new Rect(x, y, width, cardHeight);
            GUI.Box(cardRect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(x + 10f, y + 3f, width - 20f, 26f), "GOD EFFECTS", smallBodyStyle);

            Rect iconRect = new Rect(x + 10f, y + 30f, iconSize, iconSize);
            DrawGodEffectIcon(iconRect, godEffectRevealed ? resolvedGodEffect : hiddenGodEffect,
                godEffectRevealed);
            string effectLabel = godEffectRevealed
                ? GetGodGiftLabel(resolvedGodEffect)
                : "Hidden until execution";
            GUI.Label(new Rect(iconRect.xMax + 10f, y + 30f, width - iconSize - 30f, iconSize),
                effectLabel, bodyStyle);
            return cardHeight + 10f;
        }

        private float DrawGodGift(float x, float y, float width, int player)
        {
            const float iconSize = 48f;
            Texture2D icon = GodIconCatalog.GetIcon();
            Rect iconRect = new Rect(x, y + 4f, iconSize, iconSize);
            if (icon != null)
            {
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
            }

            string giftLabel = GetGodGiftLabel(resolvedGodEffect);
            GUI.Label(new Rect(x + iconSize + 10f, y, width - iconSize - 10f, 28f),
                "GODS' GIFT", smallBodyStyle);
            GUI.Label(new Rect(x + iconSize + 10f, y + 28f, width - iconSize - 10f, 30f),
                giftLabel, bodyStyle);
            return iconSize + 14f;
        }

        private void DrawGodEffectIcon(Rect rect, GodEffect effect, bool revealed)
        {
            if (!revealed)
            {
                DrawHiddenGodEffectIcon(rect);
                return;
            }

            if (TryGetGodEffectResource(effect, out Resource resource))
            {
                DrawResourceIcon(rect, resource);
                return;
            }

            if (effect == GodEffect.GainSacrality)
            {
                Texture2D icon = GodIconCatalog.GetIcon();
                if (icon != null)
                {
                    GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
                    return;
                }
            }

            DrawHiddenGodEffectIcon(rect);
        }

        private void DrawHiddenGodEffectIcon(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, dialogStyle);
            GUI.Label(rect, "?", dialogTitleStyle);
        }

        private static bool TryGetGodEffectResource(GodEffect effect, out Resource resource)
        {
            switch (effect)
            {
                case GodEffect.GainPreservedRoots:
                    resource = Resource.Roots;
                    return true;
                case GodEffect.GainWood:
                    resource = Resource.Wood;
                    return true;
                case GodEffect.GainPreservedAurochs:
                    resource = Resource.Aurochs;
                    return true;
                case GodEffect.GainStone:
                    resource = Resource.Stone;
                    return true;
                case GodEffect.GainPreservedMushrooms:
                    resource = Resource.Mushrooms;
                    return true;
                default:
                    resource = default;
                    return false;
            }
        }

        private static string GetGodGiftLabel(GodEffect effect)
        {
            switch (effect)
            {
                case GodEffect.GainPreservedRoots:
                    return "+2 preserved Roots";
                case GodEffect.GainWood:
                    return "+2 Wood";
                case GodEffect.GainPreservedAurochs:
                    return "+2 preserved Aurochs";
                case GodEffect.GainStone:
                    return "+2 Stone";
                case GodEffect.GainPreservedMushrooms:
                    return "+2 preserved Mushrooms";
                case GodEffect.GainSacrality:
                    return "+2 Sacrality";
                default:
                    return "No divine gift";
            }
        }

        private bool HasPreservedSeasonGains(int player)
        {
            for (int i = 0; i < FoodResources.Length; i++)
            {
                if (latestPreservedSeasonGains[player, (int)FoodResources[i]] > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void DrawSeasonGainResourceIcon(Rect rect, Resource resource, int amount, bool preserved)
        {
            Color previousColor = GUI.color;
            if (amount == 0)
            {
                GUI.color = new Color(0.42f, 0.42f, 0.42f, 0.48f);
            }

            DrawResourceIcon(rect, resource);
            GUI.color = previousColor;

            Rect countRect = new Rect(rect.x + rect.width * 0.28f,
                rect.y + rect.height * 0.34f,
                rect.width * 0.64f, rect.height * 0.58f);
            string amountText = $"+{amount}";
            GUI.Label(new Rect(countRect.x + 1.5f, countRect.y + 1.5f,
                countRect.width, countRect.height), amountText,
                preserved ? preservedStockpileCountShadowStyle : stockpileCountShadowStyle);
            GUI.Label(countRect, amountText,
                preserved ? preservedStockpileCountStyle : stockpileCountStyle);
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
                    if (!IsWorkerSlotAllowedOnTile(tile, slot) || IsTileSlotOccupied(tile, slot))
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
                    if (!IsWorkerSlotAllowedOnTile(candidateTile, candidateSlot) ||
                        IsTileSlotOccupied(candidateTile, candidateSlot) ||
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
            float markerSize = GetWorkerIconSize(tile);
            return new Rect(screen.x - markerSize * 0.5f,
                Screen.height - screen.y - markerSize * 0.5f, markerSize, markerSize);
        }

        private void DrawWorkerBondsOnMap()
        {
            if (mapCamera == null)
            {
                return;
            }

            for (int player = 0; player < PlayerCount; player++)
            {
                if (feastMovementOnlyThisSeason[player])
                {
                    continue;
                }

                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    int target = preserveTargetWorkers[player, worker];
                    if (workerActions[player, worker] != WorkerAction.Preserve ||
                        !IsValidPreserveAssignment(player, worker, target))
                    {
                        continue;
                    }

                    Vector3 sourceScreen = GetWorkerSlotScreenPosition(assignments[player, worker],
                        assignmentSlots[player, worker]);
                    Vector3 targetScreen = GetWorkerSlotScreenPosition(assignments[player, target],
                        assignmentSlots[player, target]);
                    if (sourceScreen.z < 0f || targetScreen.z < 0f)
                    {
                        continue;
                    }

                    DrawWorkerBond(new Vector2(sourceScreen.x, Screen.height - sourceScreen.y),
                        new Vector2(targetScreen.x, Screen.height - targetScreen.y), PlayerColors[player]);
                }
            }
        }

        private static void DrawWorkerBond(Vector2 source, Vector2 target, Color color)
        {
            Vector2 direction = target - source;
            float length = direction.magnitude;
            if (length <= 0.001f)
            {
                return;
            }

            Color previousColor = GUI.color;
            Matrix4x4 previousMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, source);
            GUI.color = new Color(color.r, color.g, color.b, 0.95f);
            GUI.DrawTexture(new Rect(source.x, source.y - WorkerBondOutlineThickness * 0.5f,
                length, WorkerBondOutlineThickness), Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = new Color(color.r, color.g, color.b, 0.95f);
            GUI.DrawTexture(new Rect(source.x, source.y - WorkerBondThickness * 0.5f,
                length, WorkerBondThickness), Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.matrix = previousMatrix;
            GUI.color = previousColor;
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

                        float markerSize = GetWorkerIconSize(tile);
                        Rect marker = new Rect(screen.x - markerSize * 0.5f,
                            Screen.height - screen.y - markerSize * 0.5f, markerSize, markerSize);
                        DrawWorkerIcon(marker, player, false, "placed", selected);
                        if (selectingPreserveTarget && player == activePlayer &&
                            IsEligiblePreserveTarget(actionMenuWorker, worker))
                        {
                            DrawPreserveTargetHighlight(marker);
                        }
                        DrawAssignedFoodOverlay(marker, player, worker);
                        DrawWorkerToolOverlay(marker, player, worker);
                        if (foodAssignmentPhase && player == activePlayer && assignedFood[player, worker] < 0)
                        {
                            DrawUnfulfilledFlash(marker);
                        }

                        Event current = Event.current;
                        if (!resolutionPhase && !showWorkerActionMenu && player == activePlayer &&
                            current.type == EventType.MouseDown && current.button == 0 && marker.Contains(current.mousePosition))
                        {
                            if (selectingPreserveTarget)
                            {
                                TrySelectPreserveTarget(worker, current);
                            }
                            else if (foodAssignmentPhase)
                            {
                                TryAssignSelectedProvisionToActiveWorker(worker, current);
                            }
                            else
                            {
                                BeginWorkerPress(worker, current);
                            }
                        }
                        else if (!resolutionPhase && !foodAssignmentPhase && !selectingPreserveTarget &&
                                 !showWorkerActionMenu && player == activePlayer &&
                                 current.type == EventType.MouseDown && current.button == 1 &&
                                 marker.Contains(current.mousePosition))
                        {
                            OpenWorkerActionMenu(worker);
                            current.Use();
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
                float size = GetWorkerIconSize(tile);
                Rect marker = new Rect(screen.x - size * 0.5f, Screen.height - screen.y - size * 0.5f, size, size);
                if (marker.Contains(guiPoint))
                {
                    worker = candidate;
                    return true;
                }
            }

            return false;
        }

        private bool TryGetActiveHearthAtGuiPosition(Vector2 guiPoint)
        {
            if (mapCamera == null || hearthTiles[activePlayer] < 0)
            {
                return false;
            }

            return GetHearthGuiRect(activePlayer).Contains(guiPoint);
        }

        private float GetResourceIconSize(int tile)
        {
            float diameterPerRadius = IsTileOccupied(tile)
                ? OccupiedResourceIconDiameterPerHexRadius
                : ResourceIconDiameterPerHexRadius;
            return GetHexScreenRadius(tile) * diameterPerRadius;
        }

        private float GetWorkerIconSize(int tile)
        {
            return GetHexScreenRadius(tile) * WorkerIconDiameterPerHexRadius;
        }

        private float GetHearthIconSize(int tile)
        {
            return GetHexScreenRadius(tile) * HearthIconDiameterPerHexRadius;
        }

        private float GetMonumentIconSize(int tile)
        {
            return GetHexScreenRadius(tile) * MonumentIconDiameterPerHexRadius;
        }

        private float GetHexScreenRadius(int tile)
        {
            if (mapCamera == null || map == null)
            {
                return 0f;
            }

            Vector3 center = mapCamera.WorldToScreenPoint(map.GetTileWorldPosition(tile));
            Vector3 vertex = mapCamera.WorldToScreenPoint(map.GetTileVertexWorldPosition(tile, TopWorkerSlot));
            return Vector2.Distance(center, vertex);
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
                    return GetHearthIconSize(tile);
                }
            }

            if (IsMonumentTile(tile))
            {
                return GetMonumentIconSize(tile);
            }

            return map.HasResource(tile) ? GetResourceIconSize(tile) : 0f;
        }

        private void DrawHearthsOnMap()
        {
            if (mapCamera == null)
            {
                return;
            }

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

                Rect marker = GetHearthGuiRect(player);
                Texture2D hearthIcon = HearthIconCatalog.GetIcon();
                if (hearthIcon != null)
                {
                    GUI.DrawTexture(marker, hearthIcon, ScaleMode.ScaleToFit, true);
                }
                else
                {
                    GUI.Label(marker, "?", resourceFallbackStyle);
                }

                DrawAssignedHearthFuelOverlay(marker, player);
                if (foodAssignmentPhase && player == activePlayer && !assignedHearthFuel[player])
                {
                    DrawUnfulfilledFlash(marker);
                }

                Event current = Event.current;
                if (foodAssignmentPhase && player == activePlayer && current.type == EventType.MouseDown &&
                    current.button == 0 && marker.Contains(current.mousePosition))
                {
                    TryAssignSelectedWoodToActiveHearth(current);
                }

                if (marker.Contains(Event.current.mousePosition))
                {
                    hoveredTooltip = foodAssignmentPhase && player == activePlayer
                        ? $"Player {player + 1} hearth • assign wood here"
                        : $"Player {player + 1} hearth";
                }
            }
        }

        private void DrawMonumentsOnMap()
        {
            if (mapCamera == null)
            {
                return;
            }

            for (int index = 0; index < monumentTiles.Length; index++)
            {
                int tile = monumentTiles[index];
                if (tile < 0)
                {
                    continue;
                }

                Vector3 screen = mapCamera.WorldToScreenPoint(map.GetTileWorldPosition(tile));
                if (screen.z < 0f)
                {
                    continue;
                }

                float size = GetMonumentIconSize(tile);
                Rect marker = new Rect(screen.x - size * 0.5f,
                    Screen.height - screen.y - size * 0.5f, size, size);
                Texture2D monumentIcon = MonumentIconCatalog.GetIcon();
                if (monumentIcon != null)
                {
                    GUI.DrawTexture(marker, monumentIcon, ScaleMode.ScaleToFit, true);
                }
                else
                {
                    GUI.Label(marker, "M", resourceFallbackStyle);
                }

                if (marker.Contains(Event.current.mousePosition))
                {
                    hoveredTooltip = $"Player {GetMonumentOwner(index) + 1} monument • vision quests here only in {SeasonNames[monumentBuildSeasons[index]]}";
                }
            }
        }

        private Rect GetHearthGuiRect(int player)
        {
            int tile = hearthTiles[player];
            Vector3 screen = mapCamera.WorldToScreenPoint(map.GetTileWorldPosition(tile));
            float markerSize = GetHearthIconSize(tile);
            return new Rect(screen.x - markerSize * 0.5f,
                Screen.height - screen.y - markerSize * 0.5f, markerSize, markerSize);
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
                    ? $"Player {activePlayer + 1}: assign food to workers and wood to the hearth"
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

        private void HandleKeyboardPan()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            Vector2 direction = Vector2.zero;
            if (keyboard.leftArrowKey.isPressed || keyboard.qKey.isPressed)
            {
                direction.x -= 1f;
            }

            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed)
            {
                direction.x += 1f;
            }

            if (keyboard.upArrowKey.isPressed || keyboard.zKey.isPressed)
            {
                direction.y += 1f;
            }

            if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed)
            {
                direction.y -= 1f;
            }

            if (direction.sqrMagnitude > 0f)
            {
                mapCamera.transform.position += (Vector3)(direction.normalized * KeyboardPanSpeed * Time.deltaTime);
            }
        }

        private void HandleMapDragPan()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            Vector2 pointerPosition = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame)
            {
                mapPanCandidate = mapCamera.pixelRect.Contains(pointerPosition);
                isPanningMap = false;
                mapPanLastScreenPosition = pointerPosition;
                return;
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                mapPanCandidate = false;
                isPanningMap = false;
                return;
            }

            if (!mouse.leftButton.isPressed || !mapPanCandidate)
            {
                return;
            }

            if (pressedWorker >= 0 || draggingWorker >= 0 || pressedFoodResource >= 0 ||
                draggingFoodResource >= 0 || pressedTool >= 0 || draggingTool >= 0)
            {
                mapPanCandidate = false;
                return;
            }

            Vector2 screenDelta = pointerPosition - mapPanLastScreenPosition;
            if (!isPanningMap && screenDelta.sqrMagnitude < MapPanStartThreshold * MapPanStartThreshold)
            {
                return;
            }

            isPanningMap = true;
            if (IsPlacingHearth && hearthPlacementPointerPressed)
            {
                hearthPlacementWasDragged = true;
            }

            PanMapByScreenDelta(mapPanLastScreenPosition, pointerPosition);
            mapPanLastScreenPosition = pointerPosition;
        }

        private void PanMapByScreenDelta(Vector2 previousScreenPosition, Vector2 currentScreenPosition)
        {
            Vector3 previousWorldPosition = mapCamera.ScreenToWorldPoint(
                new Vector3(previousScreenPosition.x, previousScreenPosition.y, 0f));
            Vector3 currentWorldPosition = mapCamera.ScreenToWorldPoint(
                new Vector3(currentScreenPosition.x, currentScreenPosition.y, 0f));
            mapCamera.transform.position += previousWorldPosition - currentWorldPosition;
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

        private void DrawToolDragGhost(Vector2 mousePosition)
        {
            const float size = 52f;
            Rect ghost = new Rect(mousePosition.x - size * 0.5f, mousePosition.y - size * 0.5f, size, size);
            DrawToolIcon(ghost, (Tool)draggingTool);
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

        private void DrawAssignedHearthFuelOverlay(Rect hearthRect, int player)
        {
            if (!foodAssignmentPhase || !assignedHearthFuel[player])
            {
                return;
            }

            float size = hearthRect.width * 0.48f;
            Rect fuelRect = new Rect(hearthRect.xMax - size, hearthRect.yMax - size, size, size);
            Color previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.96f);
            DrawResourceIcon(fuelRect, Resource.Wood);
            GUI.color = previousColor;
        }

        private void DrawWorkerToolOverlay(Rect workerRect, int player, int worker)
        {
            if (player < 0 || player >= PlayerCount || worker < 0 || worker >= WorkersPerPlayer)
            {
                return;
            }

            int tool = workerTools[player, worker];
            if (tool < 0 || tool >= ToolTypeCount)
            {
                return;
            }

            float size = workerRect.width * 0.45f;
            Rect toolRect = new Rect(workerRect.x, workerRect.yMax - size, size, size);
            Color previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.96f);
            DrawToolIcon(toolRect, (Tool)tool);
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

        private void DrawPreserveTargetHighlight(Rect rect)
        {
            if (iconOutlineTexture == null)
            {
                return;
            }

            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * SelectionPulseSpeed);
            Color previousColor = GUI.color;
            GUI.color = new Color(0.2f, 0.92f, 0.66f, 0.58f + pulse * 0.4f);
            GUI.DrawTexture(rect, iconOutlineTexture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;
        }

        private void DrawUnfulfilledFlash(Rect rect)
        {
            if (iconOutlineTexture == null)
            {
                return;
            }

            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * SelectionPulseSpeed);
            Color flash = Color.Lerp(UnfulfilledFlashColor, Color.white, 0.15f + pulse * 0.3f);
            flash.a = 0.55f + pulse * 0.4f;

            Color previousColor = GUI.color;
            GUI.color = flash;
            GUI.DrawTexture(rect, iconOutlineTexture, ScaleMode.ScaleToFit, true);

            Rect innerRing = new Rect(rect.x + rect.width * 0.07f, rect.y + rect.height * 0.07f,
                rect.width * 0.86f, rect.height * 0.86f);
            GUI.color = new Color(1f, 0.2f, 0.2f, 0.08f + pulse * 0.18f);
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
                !IsWorkerSlotAllowedOnTile(tile, slot) || IsPlacingHearth || foodAssignmentPhase ||
                !workerAlive[activePlayer, worker])
            {
                return false;
            }

            int turnStartTile = turnStartTiles[activePlayer, worker];
            int distance = map.GetHexDistance(turnStartTile, tile);
            if (turnStartTile < 0 || distance > WorkerMoveRange)
            {
                SetStatusMessage("That hex is not highlighted. A worker may stay or move one tile.", isWarning: true);
                return false;
            }

            if (IsTileOccupiedByOtherPlayer(tile))
            {
                SetStatusMessage("That hex is occupied by the other player.", isWarning: true);
                return false;
            }

            if (IsTileSlotOccupied(tile, slot))
            {
                SetStatusMessage("That slot is already occupied.", isWarning: true);
                return false;
            }

            int previousTile = assignments[activePlayer, worker];
            bool firstPlacementThisTurn = !workerPlacedThisTurn[activePlayer, worker];
            assignments[activePlayer, worker] = tile;
            assignmentSlots[activePlayer, worker] = slot;
            workerPlacedThisTurn[activePlayer, worker] = true;
            SetWorkerActionToGather(worker);
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
                SetStatusMessage(nextWorker >= 0
                    ? $"{placementMessage} The next worker is selected."
                    : $"{placementMessage} All workers are placed.");
            }
            else
            {
                selectedWorker = worker;
                SetStatusMessage(placementMessage);
            }

            UpdateOccupiedTileOutlines();
            UpdateWorkerPlacementHighlights();
            OpenWorkerActionMenu(worker);
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
            SetStatusMessage("Worker selected. Choose an available slot.");
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
            SetWorkerActionToGather(worker);
            selectedWorker = worker;
            SetStatusMessage("The worker's move was reset. Choose an available slot.");
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
                SetStatusMessage("A hearth cannot be placed on water. Choose a land hex.", isWarning: true);
                return false;
            }

            if (IsTileOccupiedByOtherPlayer(tile))
            {
                SetStatusMessage("That hex is occupied by the other player. Choose another land hex.", isWarning: true);
                return false;
            }

            hearthTiles[activePlayer] = tile;
            map.RemoveResource(tile);
            for (int worker = 0; worker < WorkersPerPlayer; worker++)
            {
                bool startsAlive = worker < StartingWorkersPerPlayer;
                workerAlive[activePlayer, worker] = startsAlive;
                assignments[activePlayer, worker] = startsAlive ? tile : -1;
                assignmentSlots[activePlayer, worker] = startsAlive ? worker : -1;
                turnStartTiles[activePlayer, worker] = startsAlive ? tile : -1;
                turnStartSlots[activePlayer, worker] = startsAlive ? worker : -1;
                workerPlacedThisTurn[activePlayer, worker] = false;
            }

            selectedWorker = 0;
            SetStatusMessage("Hearth placed. All workers begin there; one worker is selected.");
            UpdateOccupiedTileOutlines();
            UpdateWorkerPlacementHighlights();
            return true;
        }

        public void EndAssignments()
        {
            if (showWorkerActionMenu || selectingPreserveTarget)
            {
                SetStatusMessage("Finish choosing the worker action before ending assignments.", isWarning: true);
                return;
            }

            if (foodAssignmentPhase)
            {
                TryEndFoodAssignments();
                return;
            }

            if (IsPlacingHearth)
            {
                SetStatusMessage("Place your hearth before beginning your first assignments.", isWarning: true);
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

        private void BeginGodPhase()
        {
            activePlayer = 0;
            ClearWorkerInteraction();
            hiddenGodEffect = GenerateHiddenGodEffect();
            godEffectRevealed = false;
            godPhase = true;
            SetStatusMessage("The gods have set this season's hidden fate.");
        }

        private GodEffect GenerateHiddenGodEffect()
        {
            // The prototype's opening year contains only beneficial effects. Later-year
            // generation belongs here once harmful and more varied effects are introduced.
            if (year != 1)
            {
                return GodEffect.None;
            }

            switch (seasonIndex)
            {
                case 0:
                    return UnityEngine.Random.value < 0.5f
                        ? GodEffect.GainPreservedRoots
                        : GodEffect.GainWood;
                case 1:
                    return UnityEngine.Random.value < 0.5f
                        ? GodEffect.GainPreservedAurochs
                        : GodEffect.GainStone;
                case 2:
                    return UnityEngine.Random.value < 0.5f
                        ? GodEffect.GainPreservedMushrooms
                        : GodEffect.GainWood;
                case 3:
                    return GodEffect.GainSacrality;
                default:
                    return GodEffect.None;
            }
        }

        private void DismissGodPhase()
        {
            if (!godPhase)
            {
                return;
            }

            godPhase = false;
            BeginActivePlayerAssignments();
        }

        private void ResolveSeasonAndBeginFoodAssignments()
        {
            resolvedSeasonName = SeasonNames[seasonIndex];
            resolvedYear = year;
            resolvedGodEffect = hiddenGodEffect;
            godEffectRevealed = true;
            ApplyGodGift();
            CollectAssignedResources();
            activePlayer = 0;
            resolutionPhase = false;
            BeginFoodAssignments();
            showSeasonGainsDialog = true;
        }

        private void ApplyGodGift()
        {
            for (int player = 0; player < PlayerCount; player++)
            {
                switch (resolvedGodEffect)
                {
                    case GodEffect.GainPreservedRoots:
                        AddPreservedGodFood(player, Resource.Roots);
                        break;
                    case GodEffect.GainWood:
                        resourceStockpiles[player, (int)Resource.Wood] += 2;
                        break;
                    case GodEffect.GainPreservedAurochs:
                        AddPreservedGodFood(player, Resource.Aurochs);
                        break;
                    case GodEffect.GainStone:
                        resourceStockpiles[player, (int)Resource.Stone] += 2;
                        break;
                    case GodEffect.GainPreservedMushrooms:
                        AddPreservedGodFood(player, Resource.Mushrooms);
                        break;
                    case GodEffect.GainSacrality:
                        AddSacrality(player, 2);
                        break;
                }
            }
        }

        private void AddPreservedGodFood(int player, Resource resource)
        {
            resourceStockpiles[player, (int)resource] += 2;
            preservedFoodStockpiles[player, (int)resource] += 2;
        }

        private void CollectAssignedResources()
        {
            ClearLatestSeasonGains();
            if (map == null)
            {
                return;
            }

            bool[,] preservedWorkers = new bool[PlayerCount, WorkersPerPlayer];
            for (int player = 0; player < PlayerCount; player++)
            {
                if (feastMovementOnlyThisSeason[player])
                {
                    continue;
                }

                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    int target = preserveTargetWorkers[player, worker];
                    if (workerActions[player, worker] == WorkerAction.Preserve &&
                        IsValidPreserveAssignment(player, worker, target))
                    {
                        preservedWorkers[player, target] = true;
                    }
                }
            }

            for (int player = 0; player < PlayerCount; player++)
            {
                if (feastMovementOnlyThisSeason[player])
                {
                    continue;
                }

                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    if (!workerAlive[player, worker])
                    {
                        continue;
                    }

                    if (workerActions[player, worker] == WorkerAction.BuildMonument)
                    {
                        TryBuildAssignedMonument(player, worker);
                    }
                    else if (workerActions[player, worker] == WorkerAction.Ritual)
                    {
                        TryResolveRitual(player, worker);
                    }
                }
            }

            for (int player = 0; player < PlayerCount; player++)
            {
                if (feastMovementOnlyThisSeason[player])
                {
                    continue;
                }

                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    if (workerAlive[player, worker] && workerActions[player, worker] == WorkerAction.Craft)
                    {
                        TryCraftAssignedTool(player, worker);
                    }
                }
            }

            for (int player = 0; player < PlayerCount; player++)
            {
                if (feastMovementOnlyThisSeason[player])
                {
                    continue;
                }

                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    if (!workerAlive[player, worker])
                    {
                        continue;
                    }

                    if (workerActions[player, worker] == WorkerAction.Craft ||
                        workerActions[player, worker] == WorkerAction.Ritual ||
                        workerActions[player, worker] == WorkerAction.BuildMonument)
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
                        Resource resource = (Resource)resourceIndex;
                        int amount = HasAppropriateTool(player, worker, resource) ? 2 : 1;
                        if (ResourceCatalog.IsFood(resource))
                        {
                            if (preservedWorkers[player, worker])
                            {
                                preservedFoodStockpiles[player, resourceIndex] += amount;
                                latestPreservedSeasonGains[player, resourceIndex] += amount;
                            }
                            else
                            {
                                freshFoodStockpiles[player, resourceIndex] += amount;
                            }
                        }

                        resourceStockpiles[player, resourceIndex] += amount;
                        latestSeasonGains[player, resourceIndex] += amount;
                    }
                }
            }
        }

        private void TryCraftAssignedTool(int player, int worker)
        {
            int toolIndex = craftTools[player, worker];
            if (toolIndex < 0 || toolIndex >= ToolTypeCount)
            {
                return;
            }

            Tool tool = (Tool)toolIndex;
            if (!ToolCatalog.CanCraft(tool, resourceStockpiles, player))
            {
                return;
            }

            foreach (Resource cost in ToolCatalog.GetCosts(tool))
            {
                resourceStockpiles[player, (int)cost] -= ToolCatalog.GetCostAmount(tool, cost);
            }

            toolStockpiles[player, toolIndex]++;
            latestSeasonToolGains[player, toolIndex]++;
        }

        private void TryResolveRitual(int player, int worker)
        {
            int tile = assignments[player, worker];
            if (resourceStockpiles[player, (int)Resource.Mushrooms] <= 0 ||
                resourceStockpiles[player, (int)Resource.Aurochs] <= 0 ||
                GetMonumentBuildSeason(tile) != seasonIndex)
            {
                return;
            }

            SpendFood(player, Resource.Mushrooms, 1);
            SpendFood(player, Resource.Aurochs, 1);
            AddSacrality(player, 1);
        }

        private void TryBuildAssignedMonument(int player, int worker)
        {
            if (!CanBuildMonumentAtExecution(player, worker))
            {
                return;
            }

            int tile = assignments[player, worker];
            resourceStockpiles[player, (int)Resource.Stone] -= MonumentStoneCost;
            AddMonument(player, tile, seasonIndex);
            map.RemoveResource(tile);
            UpdateOccupiedTileOutlines();
        }

        private bool CanBuildMonumentAtExecution(int player, int worker)
        {
            int tile = assignments[player, worker];
            return map != null && tile >= 0 && map.GetTerrain(tile) != Terrain.Water &&
                   tile != hearthTiles[player] && !IsMonumentTile(tile) &&
                   resourceStockpiles[player, (int)Resource.Stone] >= MonumentStoneCost;
        }

        private void AddSacrality(int player, int amount)
        {
            if (amount <= 0 || player < 0 || player >= PlayerCount)
            {
                return;
            }

            sacralityStockpiles[player] += amount;
        }

        private bool HasAppropriateTool(int player, int worker, Resource resource)
        {
            int tool = workerTools[player, worker];
            return tool >= 0 && tool < ToolTypeCount && ToolCatalog.IsAppropriateFor((Tool)tool, resource);
        }

        private bool IsValidPreserveAssignment(int player, int preserver, int target)
        {
            if (map == null || player < 0 || player >= PlayerCount || preserver < 0 || preserver >= WorkersPerPlayer ||
                target < 0 || target >= WorkersPerPlayer || !workerAlive[player, preserver] ||
                !workerAlive[player, target] || assignments[player, preserver] != hearthTiles[player] ||
                !workerPlacedThisTurn[player, target] ||
                workerActions[player, target] != WorkerAction.Gather)
            {
                return false;
            }

            int targetTile = assignments[player, target];
            return targetTile >= 0 && map.GetHexDistance(hearthTiles[player], targetTile) == 1 &&
                   map.HasResource(targetTile) && ResourceCatalog.IsFood(map.GetSelectedResource(targetTile));
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

        public int GetPreservedFoodAmount(int player, Resource resource)
        {
            int resourceIndex = (int)resource;
            return player >= 0 && player < PlayerCount && ResourceCatalog.IsFood(resource) &&
                   resourceIndex >= 0 && resourceIndex < ResourceTypeCount
                ? preservedFoodStockpiles[player, resourceIndex]
                : 0;
        }

        public int GetSacralityAmount(int player)
        {
            return player >= 0 && player < PlayerCount ? sacralityStockpiles[player] : 0;
        }

        public int GetMonumentCount(int player)
        {
            int count = 0;
            for (int index = 0; index < monumentTiles.Length; index++)
            {
                if (monumentTiles[index] >= 0 && GetMonumentOwner(index) == player)
                {
                    count++;
                }
            }

            return count;
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

        public int GetHearthTile(int player)
        {
            return player >= 0 && player < PlayerCount ? hearthTiles[player] : -1;
        }

        public bool IsHearthFueled(int player)
        {
            return foodAssignmentPhase && player >= 0 && player < PlayerCount && assignedHearthFuel[player];
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
                SetStatusMessage("That worker already has this food assigned.", isWarning: true);
                return false;
            }

            if (resourceStockpiles[activePlayer, foodIndex] <= 0)
            {
                SetStatusMessage($"No {ResourceCatalog.GetLabel(food)} remains in this stockpile.", isWarning: true);
                return false;
            }

            if (previouslyAssigned >= 0)
            {
                resourceStockpiles[activePlayer, previouslyAssigned]++;
                if (assignedFoodWasPreserved[activePlayer, worker])
                {
                    preservedFoodStockpiles[activePlayer, previouslyAssigned]++;
                }
                else
                {
                    freshFoodStockpiles[activePlayer, previouslyAssigned]++;
                }
            }

            resourceStockpiles[activePlayer, foodIndex]--;
            if (freshFoodStockpiles[activePlayer, foodIndex] > 0)
            {
                freshFoodStockpiles[activePlayer, foodIndex]--;
                assignedFoodWasPreserved[activePlayer, worker] = false;
            }
            else
            {
                preservedFoodStockpiles[activePlayer, foodIndex]--;
                assignedFoodWasPreserved[activePlayer, worker] = true;
            }
            assignedFood[activePlayer, worker] = foodIndex;
            selectedFoodResource = foodIndex;
            SetStatusMessage($"Assigned {ResourceCatalog.GetLabel(food)} to a worker.");
            return true;
        }

        public bool TryAssignWoodToActiveHearth()
        {
            if (!foodAssignmentPhase || hearthTiles[activePlayer] < 0)
            {
                return false;
            }

            if (assignedHearthFuel[activePlayer])
            {
                SetStatusMessage("This hearth already has wood assigned.", isWarning: true);
                return false;
            }

            int woodIndex = (int)Resource.Wood;
            if (resourceStockpiles[activePlayer, woodIndex] <= 0)
            {
                SetStatusMessage("No Wood remains in this stockpile.", isWarning: true);
                return false;
            }

            resourceStockpiles[activePlayer, woodIndex]--;
            assignedHearthFuel[activePlayer] = true;
            selectedFoodResource = woodIndex;
            SetStatusMessage("Assigned Wood to the hearth.");
            return true;
        }

        private void TryAssignSelectedProvisionToActiveWorker(int worker, Event current)
        {
            if (selectedTool >= 0)
            {
                TryAssignToolToActiveWorker(worker, (Tool)selectedTool);
            }
            else if (selectedFoodResource < 0)
            {
                SetStatusMessage("Select food or a tool stockpile first.", isWarning: true);
            }
            else if (!ResourceCatalog.IsFood((Resource)selectedFoodResource))
            {
                SetStatusMessage("Wood can only be assigned to your hearth.", isWarning: true);
            }
            else
            {
                TryAssignFoodToActiveWorker(worker, (Resource)selectedFoodResource);
            }

            current.Use();
        }

        public bool TryAssignToolToActiveWorker(int worker, Tool tool)
        {
            int toolIndex = (int)tool;
            if (!foodAssignmentPhase || worker < 0 || worker >= WorkersPerPlayer ||
                !workerAlive[activePlayer, worker] || toolIndex < 0 || toolIndex >= ToolTypeCount)
            {
                return false;
            }

            if (workerTools[activePlayer, worker] >= 0)
            {
                SetStatusMessage("That worker already has a permanent tool.", isWarning: true);
                return false;
            }

            if (toolStockpiles[activePlayer, toolIndex] <= 0)
            {
                SetStatusMessage($"No {tool} remains in this tool stockpile.", isWarning: true);
                return false;
            }

            toolStockpiles[activePlayer, toolIndex]--;
            workerTools[activePlayer, worker] = toolIndex;
            selectedTool = toolIndex;
            selectedFoodResource = -1;
            SetStatusMessage($"Assigned a permanent {tool} to a worker.");
            return true;
        }

        private void TryAssignSelectedWoodToActiveHearth(Event current)
        {
            if (selectedFoodResource != (int)Resource.Wood)
            {
                SetStatusMessage("Select Wood from your stockpile first.", isWarning: true);
            }
            else
            {
                TryAssignWoodToActiveHearth();
            }

            current.Use();
        }

        private void BeginFoodPress(Resource food, Event current)
        {
            int foodIndex = (int)food;
            if (!ResourceCatalog.IsFood(food) && food != Resource.Wood)
            {
                current.Use();
                return;
            }

            if (resourceStockpiles[activePlayer, foodIndex] <= 0)
            {
                SetStatusMessage($"No {ResourceCatalog.GetLabel(food)} remains in this stockpile.", isWarning: true);
                current.Use();
                return;
            }

            selectedFoodResource = foodIndex;
            selectedTool = -1;
            pressedFoodResource = foodIndex;
            foodPressPosition = current.mousePosition;
            SetStatusMessage(food == Resource.Wood
                ? "Wood selected. Click or drag it onto your hearth."
                : $"{ResourceCatalog.GetLabel(food)} selected. Click or drag it onto a worker.");
            current.Use();
        }

        private void BeginToolPress(Tool tool, Event current)
        {
            int toolIndex = (int)tool;
            if (toolStockpiles[activePlayer, toolIndex] <= 0)
            {
                SetStatusMessage($"No {tool} remains in this tool stockpile.", isWarning: true);
                current.Use();
                return;
            }

            selectedTool = toolIndex;
            selectedFoodResource = -1;
            pressedTool = toolIndex;
            toolPressPosition = current.mousePosition;
            SetStatusMessage($"{tool} selected. Click or drag it onto a worker with no tool.");
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
                    int tool = workerTools[activePlayer, worker];
                    if (tool >= 0 && tool < ToolTypeCount)
                    {
                        ancestorToolCounts[activePlayer, tool]++;
                    }
                    workerAlive[activePlayer, worker] = false;
                    assignments[activePlayer, worker] = -1;
                    assignmentSlots[activePlayer, worker] = -1;
                    deaths++;
                }

                assignedFood[activePlayer, worker] = -1;
                assignedFoodWasPreserved[activePlayer, worker] = false;
            }

            bool hearthWentOut = ExtinguishUnfueledActiveHearth();
            ExpireFreshFood(activePlayer);

            ancestorCounts[activePlayer] += deaths;
            ClearFoodInteraction();
            UpdateOccupiedTileOutlines();

            if (activePlayer < PlayerCount - 1)
            {
                activePlayer++;
                SetStatusMessage(hearthWentOut
                    ? $"The hearth went out. Player {activePlayer + 1}: assign food and wood."
                    : $"Player {activePlayer + 1}: assign food to workers and wood to the hearth.");
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
            BeginGodPhase();
        }

        private bool ExtinguishUnfueledActiveHearth()
        {
            int hearthTile = hearthTiles[activePlayer];
            bool wasFueled = assignedHearthFuel[activePlayer];
            assignedHearthFuel[activePlayer] = false;
            if (hearthTile < 0 || wasFueled)
            {
                return false;
            }

            hearthTiles[activePlayer] = -1;
            map?.RestoreResource(hearthTile);
            return true;
        }

        private void ExpireFreshFood(int player)
        {
            for (int i = 0; i < FoodResources.Length; i++)
            {
                int resourceIndex = (int)FoodResources[i];
                resourceStockpiles[player, resourceIndex] -= freshFoodStockpiles[player, resourceIndex];
                freshFoodStockpiles[player, resourceIndex] = 0;
            }
        }

        private void ClearActivePlayerResourceAssignments()
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
                    if (assignedFoodWasPreserved[activePlayer, worker])
                    {
                        preservedFoodStockpiles[activePlayer, food]++;
                    }
                    else
                    {
                        freshFoodStockpiles[activePlayer, food]++;
                    }
                    assignedFood[activePlayer, worker] = -1;
                    assignedFoodWasPreserved[activePlayer, worker] = false;
                }
            }

            if (assignedHearthFuel[activePlayer])
            {
                resourceStockpiles[activePlayer, (int)Resource.Wood]++;
                assignedHearthFuel[activePlayer] = false;
            }

            selectedFoodResource = -1;
            selectedTool = -1;
            SetStatusMessage("Food and fuel assignments cleared. Permanent tools remain with their workers.");
        }

        private void BeginFoodAssignments()
        {
            foodAssignmentPhase = true;
            activePlayer = 0;
            ClearFoodInteraction();
            for (int player = 0; player < PlayerCount; player++)
            {
                assignedHearthFuel[player] = false;
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    assignedFood[player, worker] = -1;
                    assignedFoodWasPreserved[player, worker] = false;
                }
            }

            SetStatusMessage("Player 1: assign food to every worker and Wood to the hearth.");
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

        private bool CanThrowFeast(int player)
        {
            if (foodAssignmentPhase || resolutionPhase || godPhase || feastThrownThisTurn[player])
            {
                return false;
            }

            return resourceStockpiles[player, (int)Resource.Stone] >= 1 &&
                   resourceStockpiles[player, (int)Resource.Wood] >= 1 &&
                   resourceStockpiles[player, (int)Resource.Aurochs] >= 1 &&
                   resourceStockpiles[player, (int)Resource.Mushrooms] >= 1 &&
                   FindFirstInactiveWorker(player) >= 0;
        }

        private void ThrowFeast(int player)
        {
            if (!CanThrowFeast(player))
            {
                return;
            }

            resourceStockpiles[player, (int)Resource.Stone]--;
            resourceStockpiles[player, (int)Resource.Wood]--;
            SpendFood(player, Resource.Aurochs, 1);
            SpendFood(player, Resource.Mushrooms, 1);

            int newWorker = FindFirstInactiveWorker(player);
            workerAlive[player, newWorker] = true;
            assignments[player, newWorker] = hearthTiles[player];
            assignmentSlots[player, newWorker] = FindFirstAvailableTileSlot(hearthTiles[player]);
            turnStartTiles[player, newWorker] = hearthTiles[player];
            turnStartSlots[player, newWorker] = assignmentSlots[player, newWorker];
            workerPlacedThisTurn[player, newWorker] = false;
            workerActions[player, newWorker] = WorkerAction.Gather;
            feastThrownThisTurn[player] = true;
            feastScheduledNextSeason[player] = true;
            AddSacrality(player, 1 + GetMonumentCount(player));
            SetStatusMessage($"Feast held: +{1 + GetMonumentCount(player)} Sacrality and one new worker. Next season, workers may move but cannot act.");
            UpdateOccupiedTileOutlines();
        }

        private void SpendFood(int player, Resource food, int amount)
        {
            int resourceIndex = (int)food;
            int freshSpent = Mathf.Min(amount, freshFoodStockpiles[player, resourceIndex]);
            freshFoodStockpiles[player, resourceIndex] -= freshSpent;
            int preservedSpent = amount - freshSpent;
            preservedFoodStockpiles[player, resourceIndex] -= preservedSpent;
            resourceStockpiles[player, resourceIndex] -= amount;
        }

        private int FindFirstInactiveWorker(int player)
        {
            for (int worker = 0; worker < WorkersPerPlayer; worker++)
            {
                if (!workerAlive[player, worker])
                {
                    return worker;
                }
            }

            return -1;
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
                SetStatusMessage($"Player {activePlayer + 1}: place your hearth on any non-water hex.");
                UpdateOccupiedTileOutlines();
                return;
            }

            for (int worker = 0; worker < WorkersPerPlayer; worker++)
            {
                assignments[activePlayer, worker] = turnStartTiles[activePlayer, worker];
                assignmentSlots[activePlayer, worker] = turnStartSlots[activePlayer, worker];
                workerPlacedThisTurn[activePlayer, worker] = false;
                workerActions[activePlayer, worker] = WorkerAction.Gather;
                craftTools[activePlayer, worker] = -1;
                preserveTargetWorkers[activePlayer, worker] = -1;
            }

            selectedWorker = 0;
            ClearPointerState();
            SetStatusMessage("All worker moves were reset. A worker is selected.");
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
            Array.Clear(feastThrownThisTurn, 0, feastThrownThisTurn.Length);
            for (int player = 0; player < PlayerCount; player++)
            {
                feastMovementOnlyThisSeason[player] = feastScheduledNextSeason[player];
                feastScheduledNextSeason[player] = false;
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    turnStartTiles[player, worker] = assignments[player, worker];
                    turnStartSlots[player, worker] = assignmentSlots[player, worker];
                    workerPlacedThisTurn[player, worker] = false;
                    workerActions[player, worker] = WorkerAction.Gather;
                    craftTools[player, worker] = -1;
                    preserveTargetWorkers[player, worker] = -1;
                }
            }
        }

        private void BeginActivePlayerAssignments()
        {
            ClearWorkerInteraction();
            foodAssignmentPhase = false;
            if (hearthTiles[activePlayer] < 0)
            {
                SetStatusMessage($"Player {activePlayer + 1}: place your hearth on any non-water hex.");
                return;
            }

            selectedWorker = FindFirstAliveWorker(activePlayer);
            if (selectedWorker < 0)
            {
                SetStatusMessage($"Player {activePlayer + 1} has no living workers. End assignments to continue.");
                return;
            }

            SetStatusMessage(feastMovementOnlyThisSeason[activePlayer]
                ? $"Player {activePlayer + 1} is feasting. Workers may move, but cannot perform actions this season."
                : $"Player {activePlayer + 1}: a worker is selected. Choose an available slot.");
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
            selectedTool = -1;
            ClearToolPointerState();
        }

        private void ClearToolPointerState()
        {
            pressedTool = -1;
            draggingTool = -1;
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

                if (IsMonumentOwnedBy(player, tile))
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

            if (IsMonumentTile(tile))
            {
                return true;
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
                if (IsWorkerSlotAllowedOnTile(tile, slot) && !IsTileSlotOccupied(tile, slot))
                {
                    return slot;
                }
            }

            return -1;
        }

        private bool IsWorkerSlotAllowedOnTile(int tile, int slot)
        {
            if (slot < 0 || slot >= TileContentSlotCount)
            {
                return false;
            }

            // Hearths replace their resource and retain all six peripheral slots.
            // Resource tiles expose only the top and two lower-corner worker slots.
            return !map.HasResource(tile) ||
                   slot == TopWorkerSlot ||
                   slot == BottomRightWorkerSlot ||
                   slot == BottomLeftWorkerSlot;
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
                for (int monument = 0; monument < monumentTiles.Length; monument++)
                {
                    if (GetMonumentOwner(monument) == player)
                    {
                        AddOccupiedTile(occupiedTiles, monumentTiles[monument]);
                    }
                }
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
                    workerAlive[player, worker] = worker < StartingWorkersPerPlayer;
                    workerActions[player, worker] = WorkerAction.Gather;
                    preserveTargetWorkers[player, worker] = -1;
                    assignedFood[player, worker] = -1;
                    workerTools[player, worker] = -1;
                    assignedFoodWasPreserved[player, worker] = false;
                }

                completedFirstTurn[player] = false;
                assignedHearthFuel[player] = false;
            }
        }

        private void ClearHearths()
        {
            for (int player = 0; player < PlayerCount; player++)
            {
                hearthTiles[player] = -1;
            }
        }

        private void ClearMonuments()
        {
            for (int index = 0; index < monumentTiles.Length; index++)
            {
                monumentTiles[index] = -1;
                monumentBuildSeasons[index] = -1;
            }
        }

        private void AddMonument(int player, int tile, int buildSeason)
        {
            for (int slot = player * WorkersPerPlayer; slot < (player + 1) * WorkersPerPlayer; slot++)
            {
                if (monumentTiles[slot] < 0)
                {
                    monumentTiles[slot] = tile;
                    monumentBuildSeasons[slot] = buildSeason;
                    return;
                }
            }
        }

        private bool IsMonumentTile(int tile)
        {
            for (int index = 0; index < monumentTiles.Length; index++)
            {
                if (monumentTiles[index] == tile)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsMonumentOwnedBy(int player, int tile)
        {
            for (int index = player * WorkersPerPlayer; index < (player + 1) * WorkersPerPlayer; index++)
            {
                if (monumentTiles[index] == tile)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetMonumentBuildSeason(int tile)
        {
            for (int index = 0; index < monumentTiles.Length; index++)
            {
                if (monumentTiles[index] == tile)
                {
                    return monumentBuildSeasons[index];
                }
            }

            return -1;
        }

        private int GetMonumentOwner(int index)
        {
            return index >= 0 && index < monumentTiles.Length ? index / WorkersPerPlayer : -1;
        }

        private void ClearStockpiles()
        {
            Array.Clear(resourceStockpiles, 0, resourceStockpiles.Length);
            Array.Clear(freshFoodStockpiles, 0, freshFoodStockpiles.Length);
            Array.Clear(preservedFoodStockpiles, 0, preservedFoodStockpiles.Length);
            Array.Clear(toolStockpiles, 0, toolStockpiles.Length);
        }

        private void ClearLatestSeasonGains()
        {
            Array.Clear(latestSeasonGains, 0, latestSeasonGains.Length);
            Array.Clear(latestPreservedSeasonGains, 0, latestPreservedSeasonGains.Length);
            Array.Clear(latestSeasonToolGains, 0, latestSeasonToolGains.Length);
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
            warningBodyStyle = new GUIStyle(bodyStyle)
            {
                normal = { textColor = new Color32(235, 75, 75, 255) }
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
            preservedStockpileCountStyle = new GUIStyle(stockpileCountStyle)
            {
                alignment = TextAnchor.LowerLeft,
                normal = { textColor = Color.white }
            };
            preservedStockpileCountShadowStyle = new GUIStyle(preservedStockpileCountStyle)
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
            developerConsoleInputStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color32(238, 231, 210, 255) }
            };
            developerConsoleMessageStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color32(210, 207, 196, 255) }
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
