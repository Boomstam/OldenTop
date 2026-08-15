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
    }

    public static class MapSave
    {
        private const string SeedKey = "OldenTop.MapSeed";

        public static int GetOrCreateSeed()
        {
            if (PlayerPrefs.HasKey(SeedKey))
            {
                return PlayerPrefs.GetInt(SeedKey);
            }

            int seed = unchecked(Environment.TickCount ^ (int)DateTime.UtcNow.Ticks);
            PlayerPrefs.SetInt(SeedKey, seed);
            PlayerPrefs.Save();
            return seed;
        }
    }

    public static class ResourceSave
    {
        private const int CurrentVersion = 2;
        private const string LayoutKey = "OldenTop.TileResourceLayout";

        [Serializable]
        private sealed class TileResourceLayout
        {
            public int version;
            public int mapSeed;
            public int[] resources;
        }

        public static Resource[] LoadOrCreate(int mapSeed, Terrain[] terrain)
        {
            if (TryLoad(mapSeed, terrain, out Resource[] choices))
            {
                return choices;
            }

            choices = CreateBalancedLayout(mapSeed, terrain);
            Save(mapSeed, choices);
            return choices;
        }

        public static bool TryLoad(int mapSeed, Terrain[] terrain, out Resource[] choices)
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

            if (data == null || data.version != CurrentVersion || data.mapSeed != mapSeed ||
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

        private static Resource[] CreateBalancedLayout(int mapSeed, Terrain[] terrain)
        {
            Resource[] choices = new Resource[terrain.Length];
            System.Random resourceRandom = new System.Random(unchecked(mapSeed ^ 0x5A17C9E3));

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

        private static void Save(int mapSeed, Resource[] choices)
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

        private static readonly string[] SeasonNames = { "Spring", "Summer", "Autumn", "Winter" };
        private static readonly Color[] PlayerColors =
        {
            new Color32(231, 167, 67, 255),
            new Color32(98, 177, 220, 255)
        };

        private readonly int[,] assignments = new int[PlayerCount, WorkersPerPlayer];

        private HexMap map;
        private Camera mapCamera;
        private int activePlayer;
        private int seasonIndex;
        private int year = 1;
        private int selectedWorker = -1;
        private int pressedWorker = -1;
        private int draggingWorker = -1;
        private Vector2 workerPressPosition;
        private bool resolutionPhase;
        private string statusMessage = "Click a worker, then a resource hex — or drag the worker onto the map.";

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle headingStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallBodyStyle;
        private GUIStyle workerStyle;
        private GUIStyle mapWorkerStyle;
        private GUIStyle tileResourceStyle;
        private GUIStyle tileResourceShadowStyle;

        public int ActivePlayer => activePlayer;
        public int Year => year;
        public string Season => SeasonNames[seasonIndex];
        public bool IsResolutionPhase => resolutionPhase;
        public int SelectedWorker => selectedWorker;

        public void Initialize(HexMap hexMap)
        {
            map = hexMap;
            mapCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            ClearAssignments();
            ClearWorkerInteraction();
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
            DrawTileResourceLabels();
            DrawAssignmentsOnMap();
            DrawMapHeader();
            DrawInventoryPanel();
            HandlePointerInteraction(Event.current);

            if (draggingWorker >= 0)
            {
                DrawDragGhost(Event.current.mousePosition);
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

            GUI.Label(new Rect(x, y, contentWidth, 34f), "OLDEN TOP", titleStyle);
            y += 38f;
            GUI.Label(new Rect(x, y, contentWidth, 24f), $"Year {year}  •  {SeasonNames[seasonIndex]}", headingStyle);
            y += 30f;

            string phaseText = resolutionPhase ? "COMMITMENTS READY" : $"PLAYER {activePlayer + 1} ASSIGNS";
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
                string[] labels = Array.ConvertAll(options, ResourceCatalog.GetLabel);
                string line = $"{terrain}: {string.Join(", ", labels)}";
                GUI.Label(new Rect(x + 8f, y, contentWidth - 8f, 31f), line, smallBodyStyle);
                y += 31f;
            }

            y += 8f;
            GUI.Label(new Rect(x, y, contentWidth, 24f), "WORKER INVENTORY", headingStyle);
            y += 29f;

            if (!resolutionPhase)
            {
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    DrawWorkerCard(new Rect(x, y, contentWidth, 42f), worker);
                    y += 48f;
                }

                if (GUI.Button(new Rect(x, Screen.height - 94f, contentWidth, 30f), "Recall this player's workers"))
                {
                    RecallActivePlayersWorkers();
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
            bool assigned = assignments[activePlayer, worker] >= 0;
            bool selected = selectedWorker == worker;
            string label = assigned
                ? $"Worker {worker + 1}  •  assigned"
                : $"Worker {worker + 1}  •  available";
            if (selected)
            {
                label += "  •  SELECTED";
            }

            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = selected
                ? Color.Lerp(PlayerColors[activePlayer], Color.white, 0.3f)
                : assigned
                    ? new Color(0.62f, 0.62f, 0.62f, 1f)
                    : PlayerColors[activePlayer];
            GUI.Box(rect, label, workerStyle);
            GUI.backgroundColor = previousBackground;

            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0 && rect.Contains(current.mousePosition))
            {
                BeginWorkerPress(worker, current);
            }
        }

        private void BeginWorkerPress(int worker, Event current)
        {
            SelectActiveWorker(worker);
            pressedWorker = worker;
            workerPressPosition = current.mousePosition;
            current.Use();
        }

        private void HandlePointerInteraction(Event current)
        {
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
                statusMessage = $"Drop Worker {draggingWorker + 1} on a resource hex, or off the map to return them to inventory.";
                current.Use();
                return;
            }

            if (current.type == EventType.MouseUp && current.button == 0 && draggingWorker >= 0)
            {
                int worker = draggingWorker;
                if (TryGetTileAtGuiPosition(current.mousePosition, out int tile))
                {
                    TryAssignActiveWorker(worker, tile);
                }
                else
                {
                    ReturnActiveWorkerToInventory(worker);
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
                TryGetTileAtGuiPosition(current.mousePosition, out int clickedTile))
            {
                TryAssignActiveWorker(selectedWorker, clickedTile);
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

        private void DrawTileResourceLabels()
        {
            if (mapCamera == null)
            {
                return;
            }

            Rect mapRect = mapCamera.pixelRect;
            for (int tile = 0; tile < map.GeneratedTileCount; tile++)
            {
                Vector3 screen = mapCamera.WorldToScreenPoint(map.GetTileWorldPosition(tile));
                if (screen.z < 0f || !mapRect.Contains(new Vector2(screen.x, screen.y)))
                {
                    continue;
                }

                string label = ResourceCatalog.GetLabel(map.GetSelectedResource(tile));
                Rect labelRect = new Rect(screen.x - 31f, Screen.height - screen.y - 9f, 62f, 18f);
                Rect shadowRect = new Rect(labelRect.x + 1f, labelRect.y + 1f, labelRect.width, labelRect.height);
                GUI.Label(shadowRect, label, tileResourceShadowStyle);
                GUI.Label(labelRect, label, tileResourceStyle);
            }
        }

        private void DrawAssignmentsOnMap()
        {
            if (mapCamera == null)
            {
                return;
            }

            for (int player = 0; player < PlayerCount; player++)
            {
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    int tile = assignments[player, worker];
                    if (tile < 0 || (player == activePlayer && worker == draggingWorker))
                    {
                        continue;
                    }

                    Vector2 offset = GetMarkerOffset(player, worker);
                    Vector3 screen = mapCamera.WorldToScreenPoint(map.GetTileWorldPosition(tile) + offset);
                    if (screen.z < 0f)
                    {
                        continue;
                    }

                    bool selected = player == activePlayer && worker == selectedWorker;
                    float markerSize = selected ? 34f : 28f;
                    Rect marker = new Rect(screen.x - markerSize * 0.5f,
                        Screen.height - screen.y - markerSize * 0.5f, markerSize, markerSize);
                    Color previousBackground = GUI.backgroundColor;
                    GUI.backgroundColor = selected
                        ? Color.Lerp(PlayerColors[player], Color.white, 0.35f)
                        : PlayerColors[player];
                    GUI.Box(marker, selected ? $"*{player + 1}.{worker + 1}" : $"{player + 1}.{worker + 1}", mapWorkerStyle);
                    GUI.backgroundColor = previousBackground;

                    Event current = Event.current;
                    if (!resolutionPhase && player == activePlayer && current.type == EventType.MouseDown &&
                        current.button == 0 && marker.Contains(current.mousePosition))
                    {
                        BeginWorkerPress(worker, current);
                    }
                }
            }
        }

        private void DrawMapHeader()
        {
            float panelWidth = Screen.width * PanelFraction;
            Rect header = new Rect(panelWidth + 18f, 16f, Screen.width - panelWidth - 36f, 38f);
            GUI.Box(header, resolutionPhase
                ? "All commitments are visible — advance when ready"
                : $"Player {activePlayer + 1}: click a worker then a tile, or drag a worker", headingStyle);
        }

        private void DrawDragGhost(Vector2 mousePosition)
        {
            Rect ghost = new Rect(mousePosition.x - 48f, mousePosition.y - 18f, 96f, 36f);
            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = PlayerColors[activePlayer];
            GUI.Box(ghost, $"Worker {draggingWorker + 1}", workerStyle);
            GUI.backgroundColor = previousBackground;
        }

        public bool TryAssignActiveWorker(int worker, int tile)
        {
            if (resolutionPhase || map == null || worker < 0 || worker >= WorkersPerPlayer ||
                tile < 0 || tile >= map.GeneratedTileCount)
            {
                return false;
            }

            int previousTile = assignments[activePlayer, worker];
            assignments[activePlayer, worker] = tile;
            selectedWorker = worker;
            Terrain terrain = map.GetTerrain(tile);
            Resource resource = map.GetSelectedResource(tile);
            string action = previousTile < 0 ? "assigned to" : previousTile == tile ? "remains on" : "moved to";
            statusMessage = $"Worker {worker + 1} {action} {terrain} ({ResourceCatalog.GetLabel(resource)}).";
            return true;
        }

        public bool SelectActiveWorker(int worker)
        {
            if (resolutionPhase || worker < 0 || worker >= WorkersPerPlayer)
            {
                return false;
            }

            selectedWorker = worker;
            statusMessage = $"Worker {worker + 1} selected. Click a resource hex to place them, or drag to move them.";
            return true;
        }

        public bool ReturnActiveWorkerToInventory(int worker)
        {
            if (resolutionPhase || worker < 0 || worker >= WorkersPerPlayer)
            {
                return false;
            }

            bool wasAssigned = assignments[activePlayer, worker] >= 0;
            assignments[activePlayer, worker] = -1;
            selectedWorker = worker;
            statusMessage = wasAssigned
                ? $"Worker {worker + 1} returned to inventory."
                : $"Worker {worker + 1} remains in inventory.";
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

        public void EndAssignments()
        {
            ClearWorkerInteraction();

            if (activePlayer < PlayerCount - 1)
            {
                activePlayer++;
                statusMessage = $"Player {activePlayer + 1}, assign your workers.";
                return;
            }

            resolutionPhase = true;
            statusMessage = $"{SeasonNames[seasonIndex]} commitments are locked.";
            Debug.Log($"Year {year} {SeasonNames[seasonIndex]}: both players committed their workers.", this);
        }

        public void AdvanceSeason()
        {
            ClearAssignments();
            activePlayer = 0;
            resolutionPhase = false;
            seasonIndex++;
            if (seasonIndex >= SeasonNames.Length)
            {
                seasonIndex = 0;
                year++;
            }

            ClearWorkerInteraction();
            statusMessage = $"Player 1 begins {SeasonNames[seasonIndex]}. Click a worker then a tile, or drag a worker.";
        }

        private void RecallActivePlayersWorkers()
        {
            for (int worker = 0; worker < WorkersPerPlayer; worker++)
            {
                assignments[activePlayer, worker] = -1;
            }

            ClearWorkerInteraction();
            statusMessage = "All of this player's workers returned to inventory.";
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
        }

        private void ClearAssignments()
        {
            for (int player = 0; player < PlayerCount; player++)
            {
                for (int worker = 0; worker < WorkersPerPlayer; worker++)
                {
                    assignments[player, worker] = -1;
                }
            }
        }

        private static Vector2 GetMarkerOffset(int player, int worker)
        {
            float x = ((worker & 1) == 0 ? -0.14f : 0.14f) + (player == 0 ? -0.035f : 0.035f);
            float y = (worker < 2 ? 0.14f : -0.14f) + (player == 0 ? 0.035f : -0.035f);
            return new Vector2(x, y);
        }

        private void EnsureStyles()
        {
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
            workerStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            mapWorkerStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            tileResourceStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 8,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Overflow,
                normal = { textColor = new Color32(248, 245, 226, 255) }
            };
            tileResourceShadowStyle = new GUIStyle(tileResourceStyle)
            {
                normal = { textColor = new Color32(18, 20, 18, 255) }
            };
        }
    }
}
