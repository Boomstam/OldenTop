using System;
using System.Collections.Generic;
using UnityEngine;

namespace OldenTop
{
    public enum Terrain
    {
        Plains,
        Woodland,
        Mountain,
        Water
    }

    public sealed class HexMap : MonoBehaviour
    {
        [Header("Map Dimensions")]
        [Tooltip("Number of hex columns generated for this map.")]
        [Min(1)] [SerializeField] private int width = 10;
        [Tooltip("Number of hex rows generated for this map.")]
        [Min(1)] [SerializeField] private int height = 10;

        private int Width => width;
        private int Height => height;
        private int TileCount => Width * Height;
        private const float HexRadius = 0.62f;
        private const float HorizontalSpacing = 1.7320508f * HexRadius;
        private const float VerticalSpacing = 1.5f * HexRadius;
        private const int RiverSourceCount = 4;
        private const float WaterFraction = 0.175f;
        private const float MountainFraction = 0.175f;
        private const float WoodlandFraction = 0.325f;
        private const int MaximumMapGenerationAttempts = 512;
        private const int LargeMapTileCount = 100;
        private const int SmallMapEdgeClearance = 1;
        private const int LargeMapEdgeClearance = 3;
        private const int StartResourceSearchRange = 2;
        private const int MinimumNearbyResourceVariety = 3;
        private const int MinimumStartCandidateSeparation = 3;
        private const int MaximumStartResourceVarietyDifference = 1;
        // Rivers are intentionally absent from the first playable prototype.
        private const bool RiversEnabled = false;

        private static readonly Color32 PlainsColor = new Color32(174, 194, 104, 255);
        private static readonly Color32 WoodlandColor = new Color32(65, 113, 72, 255);
        private static readonly Color32 MountainColor = new Color32(125, 123, 132, 255);
        private static readonly Color32 WaterColor = new Color32(74, 139, 178, 255);
        private static readonly Color32 RiverBankColor = new Color32(43, 91, 119, 255);
        private static readonly Color32 RiverColor = new Color32(86, 184, 221, 255);

        private Terrain[] terrain;
        private float[] elevation;
        private float[] moisture;
        private Vector2[] centers;
        private int[,] tileCornerVertices;
        private readonly List<Vector2> riverVertices = new List<Vector2>();
        private readonly List<List<int>> riverVertexNeighbors = new List<List<int>>();
        private readonly List<List<int>> riverVertexTiles = new List<List<int>>();
        private readonly HashSet<RiverEdge> riverGraphEdges = new HashSet<RiverEdge>();
        private readonly HashSet<RiverEdge> riverEdges = new HashSet<RiverEdge>();
        private readonly HashSet<int> riverNodes = new HashSet<int>();

        private System.Random random;
        private Sprite hexSprite;
        private Sprite hexOutlineSprite;
        private Sprite squareSprite;
        private Sprite circleSprite;
        private Transform tileRoot;
        private Transform riverRoot;
        private GameObject[] tileObjects;
        private SpriteRenderer[] tileHighlightRenderers;
        private SpriteRenderer[] tileOccupancyOutlineRenderers;
        private Resource[] selectedResources;
        private bool[] naturalResourcePresent;
        private bool[] resourcePresent;

        public int GeneratedTileCount => TileCount;
        public string MapSeed { get; private set; }
        public string BaseMapSeed { get; private set; }

        public void Generate(string seed)
        {
            InitializeMapBuffers();
            BaseMapSeed = seed ?? string.Empty;

            CreateSharedSprites();
            CalculateCenters();
            int acceptedAttempt = FindBalancedMapAttempt();
            BuildTerrainObjects();
            if (RiversEnabled)
            {
                BuildRiverGraph();
                GenerateRivers();
                ValidateRiverEdges();
                BuildRiverObjects();
            }
            FrameMapWithCamera();

            int plains = CountTerrain(Terrain.Plains);
            int woodland = CountTerrain(Terrain.Woodland);
            int mountain = CountTerrain(Terrain.Mountain);
            int water = CountTerrain(Terrain.Water);
            Debug.Log($"Map spawned (base seed \"{BaseMapSeed}\", accepted seed \"{MapSeed}\", " +
                      $"attempt {acceptedAttempt + 1}): {Width}x{Height}, " +
                      $"plains {plains}, woodland {woodland}, mountain {mountain}, " +
                      $"water {water}. Rivers {(RiversEnabled ? $"enabled ({riverEdges.Count} segments)" : "disabled")}.", this);
        }

        private int FindBalancedMapAttempt()
        {
            for (int attempt = 0; attempt < MaximumMapGenerationAttempts; attempt++)
            {
                MapSeed = GetAttemptSeed(attempt);
                random = new System.Random(MapSeedUtility.ToInt32(MapSeed));
                GenerateTerrain();

                Resource[] candidateResources = ResourceSave.CreateBalancedLayout(MapSeed, terrain, Width, Height,
                    out bool[] candidateResourcePresence);
                if (!HasBalancedStartingPair(candidateResources, candidateResourcePresence))
                {
                    continue;
                }

                selectedResources = ResourceSave.LoadOrCreate(MapSeed, terrain, Width, Height,
                    out bool[] generatedResourcePresence);
                ResetResourcePresence(generatedResourcePresence);
                return attempt;
            }

            throw new InvalidOperationException($"Could not generate a balanced {Width}x{Height} map from base seed " +
                                                $"\"{BaseMapSeed}\" after {MaximumMapGenerationAttempts} attempts.");
        }

        private string GetAttemptSeed(int attempt)
        {
            return attempt == 0 ? BaseMapSeed : $"{BaseMapSeed}#{attempt}";
        }

        private bool HasBalancedStartingPair(IReadOnlyList<Resource> candidateResources,
            IReadOnlyList<bool> candidateResourcePresence)
        {
            int requiredEdgeClearance = TileCount >= LargeMapTileCount
                ? LargeMapEdgeClearance
                : SmallMapEdgeClearance;
            List<int> candidates = new List<int>();

            for (int tile = 0; tile < TileCount; tile++)
            {
                if (terrain[tile] == Terrain.Water || GetDistanceToMapEdge(tile) < requiredEdgeClearance ||
                    HasEmptyWaterOrMountainNearby(tile, candidateResourcePresence) ||
                    !HasAdjacentResource(tile, Resource.Wood, candidateResources, candidateResourcePresence) ||
                    !HasAdjacentResource(tile, Resource.Roots, candidateResources, candidateResourcePresence) ||
                    GetNearbyResourceVariety(tile, candidateResources, candidateResourcePresence) <
                    MinimumNearbyResourceVariety)
                {
                    continue;
                }

                candidates.Add(tile);
            }

            // TODO: When supporting more than two players, replace this pair check with selection of a
            // mutually separated, similarly scored candidate set for every player.
            for (int first = 0; first < candidates.Count; first++)
            {
                int firstVariety = GetNearbyResourceVariety(candidates[first], candidateResources,
                    candidateResourcePresence);
                for (int second = first + 1; second < candidates.Count; second++)
                {
                    if (GetHexDistance(candidates[first], candidates[second]) < MinimumStartCandidateSeparation)
                    {
                        continue;
                    }

                    int secondVariety = GetNearbyResourceVariety(candidates[second], candidateResources,
                        candidateResourcePresence);
                    if (Mathf.Abs(firstVariety - secondVariety) <= MaximumStartResourceVarietyDifference)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HasAdjacentResource(int tile, Resource resource, IReadOnlyList<Resource> candidateResources,
            IReadOnlyList<bool> candidateResourcePresence)
        {
            for (int candidate = 0; candidate < TileCount; candidate++)
            {
                if (GetHexDistance(tile, candidate) == 1 && candidateResourcePresence[candidate] &&
                    candidateResources[candidate] == resource)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasEmptyWaterOrMountainNearby(int tile, IReadOnlyList<bool> candidateResourcePresence)
        {
            for (int candidate = 0; candidate < TileCount; candidate++)
            {
                if (GetHexDistance(tile, candidate) <= StartResourceSearchRange && !candidateResourcePresence[candidate] &&
                    (terrain[candidate] == Terrain.Water || terrain[candidate] == Terrain.Mountain))
                {
                    return true;
                }
            }

            return false;
        }

        private int GetNearbyResourceVariety(int tile, IReadOnlyList<Resource> candidateResources,
            IReadOnlyList<bool> candidateResourcePresence)
        {
            HashSet<Resource> resources = new HashSet<Resource>();
            for (int candidate = 0; candidate < TileCount; candidate++)
            {
                if (GetHexDistance(tile, candidate) <= StartResourceSearchRange && candidateResourcePresence[candidate])
                {
                    resources.Add(candidateResources[candidate]);
                }
            }

            return resources.Count;
        }

        private int GetDistanceToMapEdge(int tile)
        {
            int nearestEdgeDistance = int.MaxValue;
            for (int candidate = 0; candidate < TileCount; candidate++)
            {
                FromIndex(candidate, out int column, out int row);
                if (column != 0 && column != Width - 1 && row != 0 && row != Height - 1)
                {
                    continue;
                }

                nearestEdgeDistance = Mathf.Min(nearestEdgeDistance, GetHexDistance(tile, candidate));
            }

            return nearestEdgeDistance;
        }

        private void OnValidate()
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
        }

        private void InitializeMapBuffers()
        {
            terrain = new Terrain[TileCount];
            elevation = new float[TileCount];
            moisture = new float[TileCount];
            centers = new Vector2[TileCount];
            tileCornerVertices = new int[TileCount, 6];
            tileObjects = new GameObject[TileCount];
            tileHighlightRenderers = new SpriteRenderer[TileCount];
            tileOccupancyOutlineRenderers = new SpriteRenderer[TileCount];
            selectedResources = new Resource[TileCount];
            naturalResourcePresent = CreateInitialResourcePresence();
            resourcePresent = CreateInitialResourcePresence();
        }

        private void CreateSharedSprites()
        {
            hexSprite = CreateHexSprite(false);
            hexOutlineSprite = CreateHexSprite(true);
            squareSprite = CreateSquareSprite();
            circleSprite = CreateCircleSprite();
        }

        private static Sprite CreateHexSprite(bool outlineOnly)
        {
            const int textureWidth = 128;
            const int textureHeight = 148;
            const float outlineWidth = 0.086f;
            Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
            {
                name = outlineOnly ? "Runtime Pointy Hex Outline" : "Runtime Pointy Hex",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color32[] pixels = new Color32[textureWidth * textureHeight];
            float halfWidth = textureWidth * 0.5f;
            float halfHeight = textureHeight * 0.5f;
            float maxX = 0.8660254f;

            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    float localX = ((x + 0.5f) - halfWidth) / halfWidth * maxX;
                    float localY = ((y + 0.5f) - halfHeight) / halfHeight;
                    float absX = Mathf.Abs(localX);
                    float absY = Mathf.Abs(localY);
                    float sideDistance = maxX - absX;
                    float diagonalDistance = 1f - absX / 1.7320508f - absY;
                    float insideDistance = Mathf.Min(sideDistance, diagonalDistance);

                    if (insideDistance < 0f || (outlineOnly && insideDistance >= outlineWidth))
                    {
                        pixels[y * textureWidth + x] = new Color32(0, 0, 0, 0);
                    }
                    else if (outlineOnly)
                    {
                        float inwardProgress = Mathf.Clamp01(insideDistance / outlineWidth);
                        float alpha = 1f - Mathf.SmoothStep(0f, 1f, inwardProgress);
                        pixels[y * textureWidth + x] = new Color32(255, 255, 255,
                            (byte)Mathf.RoundToInt(alpha * 255f));
                    }
                    else
                    {
                        pixels[y * textureWidth + x] = new Color32(255, 255, 255, 255);
                    }
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            float pixelsPerUnit = textureHeight / (HexRadius * 2f);
            return Sprite.Create(texture, new Rect(0f, 0f, textureWidth, textureHeight),
                new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        private static Sprite CreateSquareSprite()
        {
            Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false)
            {
                name = "Runtime White Square",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            Color32[] pixels = new Color32[16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 255, 255, 255);
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
        }

        private static Sprite CreateCircleSprite()
        {
            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Runtime White Circle",
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
                    pixels[y * size + x] = distance <= size * 0.48f
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(0, 0, 0, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private void CalculateCenters()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    int index = ToIndex(column, row);
                    float offset = (row & 1) == 1 ? 0.5f : 0f;
                    centers[index] = new Vector2((column + offset) * HorizontalSpacing, row * VerticalSpacing);
                }
            }
        }

        private void GenerateTerrain()
        {
            float elevationOffsetX = NextRange(20f, 900f);
            float elevationOffsetY = NextRange(20f, 900f);
            float moistureOffsetX = NextRange(1000f, 1900f);
            float moistureOffsetY = NextRange(1000f, 1900f);

            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    int index = ToIndex(column, row);
                    elevation[index] = FractalNoise(column, row, elevationOffsetX, elevationOffsetY, 0.085f);
                    moisture[index] = FractalNoise(column, row, moistureOffsetX, moistureOffsetY, 0.105f);
                    terrain[index] = Terrain.Plains;
                }
            }

            List<int> byElevation = CreateIndexList();
            byElevation.Sort((a, b) => elevation[a].CompareTo(elevation[b]));

            int waterCount = Mathf.RoundToInt(TileCount * WaterFraction);
            int mountainCount = Mathf.RoundToInt(TileCount * MountainFraction);
            int woodlandCount = Mathf.RoundToInt(TileCount * WoodlandFraction);

            for (int i = 0; i < waterCount; i++)
            {
                terrain[byElevation[i]] = Terrain.Water;
            }

            for (int i = 0; i < mountainCount; i++)
            {
                terrain[byElevation[byElevation.Count - 1 - i]] = Terrain.Mountain;
            }

            List<int> woodlandCandidates = new List<int>();
            for (int i = 0; i < TileCount; i++)
            {
                if (terrain[i] == Terrain.Plains)
                {
                    woodlandCandidates.Add(i);
                }
            }

            woodlandCandidates.Sort((a, b) => WoodlandScore(b).CompareTo(WoodlandScore(a)));
            for (int i = 0; i < woodlandCount; i++)
            {
                terrain[woodlandCandidates[i]] = Terrain.Woodland;
            }
        }

        private float FractalNoise(int column, int row, float offsetX, float offsetY, float scale)
        {
            float amplitude = 1f;
            float frequency = 1f;
            float value = 0f;
            float amplitudeTotal = 0f;

            for (int octave = 0; octave < 4; octave++)
            {
                float sampleX = offsetX + column * scale * frequency;
                float sampleY = offsetY + row * scale * frequency;
                value += Mathf.PerlinNoise(sampleX, sampleY) * amplitude;
                amplitudeTotal += amplitude;
                amplitude *= 0.5f;
                frequency *= 2f;
            }

            return value / amplitudeTotal;
        }

        private float WoodlandScore(int index)
        {
            float middleElevationPreference = 1f - Mathf.Abs(elevation[index] - 0.52f);
            return moisture[index] * 0.82f + middleElevationPreference * 0.18f;
        }

        private void BuildRiverGraph()
        {
            Dictionary<VertexKey, int> vertexLookup = new Dictionary<VertexKey, int>();

            for (int tile = 0; tile < TileCount; tile++)
            {
                for (int corner = 0; corner < 6; corner++)
                {
                    float angle = (90f - corner * 60f) * Mathf.Deg2Rad;
                    Vector2 position = centers[tile] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * HexRadius;
                    VertexKey key = new VertexKey(position);

                    if (!vertexLookup.TryGetValue(key, out int vertex))
                    {
                        vertex = riverVertices.Count;
                        vertexLookup.Add(key, vertex);
                        riverVertices.Add(position);
                        riverVertexNeighbors.Add(new List<int>(3));
                        riverVertexTiles.Add(new List<int>(3));
                    }

                    tileCornerVertices[tile, corner] = vertex;
                    if (!riverVertexTiles[vertex].Contains(tile))
                    {
                        riverVertexTiles[vertex].Add(tile);
                    }
                }

                for (int corner = 0; corner < 6; corner++)
                {
                    int first = tileCornerVertices[tile, corner];
                    int second = tileCornerVertices[tile, (corner + 1) % 6];
                    RiverEdge edge = new RiverEdge(first, second);
                    if (riverGraphEdges.Add(edge))
                    {
                        riverVertexNeighbors[first].Add(second);
                        riverVertexNeighbors[second].Add(first);
                    }
                }
            }
        }

        private void GenerateRivers()
        {
            List<int> mountainTiles = new List<int>();
            for (int tile = 0; tile < TileCount; tile++)
            {
                if (terrain[tile] == Terrain.Mountain)
                {
                    mountainTiles.Add(tile);
                }
            }

            mountainTiles.Sort((a, b) => RiverSourceScore(b).CompareTo(RiverSourceScore(a)));
            List<int> sourceTiles = ChooseSeparatedSources(mountainTiles, RiverSourceCount);

            foreach (int sourceTile in sourceTiles)
            {
                int sourceVertex = ChooseSourceVertex(sourceTile);
                int targetVertex = FindNearestOutlet(sourceVertex);

                if (targetVertex < 0 || targetVertex == sourceVertex)
                {
                    continue;
                }

                List<int> path = FindRiverPath(sourceVertex, targetVertex);
                if (path.Count < 2)
                {
                    continue;
                }

                for (int i = 0; i < path.Count - 1; i++)
                {
                    RiverEdge edge = new RiverEdge(path[i], path[i + 1]);
                    riverEdges.Add(edge);
                    riverNodes.Add(path[i]);
                    riverNodes.Add(path[i + 1]);
                }
            }
        }

        private List<int> ChooseSeparatedSources(List<int> candidates, int desiredCount)
        {
            List<int> result = new List<int>();
            const float preferredSeparation = HorizontalSpacing * 4f;

            for (int pass = 0; pass < 2 && result.Count < desiredCount; pass++)
            {
                float requiredDistance = pass == 0 ? preferredSeparation : HorizontalSpacing * 2f;
                foreach (int candidate in candidates)
                {
                    bool separated = true;
                    foreach (int chosen in result)
                    {
                        if (Vector2.Distance(centers[candidate], centers[chosen]) < requiredDistance)
                        {
                            separated = false;
                            break;
                        }
                    }

                    if (separated)
                    {
                        result.Add(candidate);
                        if (result.Count == desiredCount)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private float RiverSourceScore(int source)
        {
            float distanceToWater = float.PositiveInfinity;
            for (int index = 0; index < TileCount; index++)
            {
                if (terrain[index] == Terrain.Water)
                {
                    distanceToWater = Mathf.Min(distanceToWater,
                        Vector2.Distance(centers[source], centers[index]));
                }
            }

            float normalizedDistance = float.IsPositiveInfinity(distanceToWater)
                ? 0f
                : distanceToWater / (HorizontalSpacing * Width);
            return elevation[source] + normalizedDistance * 0.7f;
        }

        private int ChooseSourceVertex(int sourceTile)
        {
            int bestVertex = tileCornerVertices[sourceTile, 0];
            float bestScore = float.NegativeInfinity;

            for (int corner = 0; corner < 6; corner++)
            {
                int vertex = tileCornerVertices[sourceTile, corner];
                float interiorBonus = riverVertexTiles[vertex].Count > 1 ? 0.05f : -0.2f;
                float score = GetVertexElevation(vertex) + interiorBonus;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestVertex = vertex;
                }
            }

            return bestVertex;
        }

        private int FindNearestOutlet(int sourceVertex)
        {
            int nearestShore = -1;
            int nearestBoundary = -1;
            float shoreDistance = float.PositiveInfinity;
            float boundaryDistance = float.PositiveInfinity;

            for (int vertex = 0; vertex < riverVertices.Count; vertex++)
            {
                float distance = Vector2.Distance(riverVertices[sourceVertex], riverVertices[vertex]);
                if (VertexTouchesTerrain(vertex, Terrain.Water) &&
                    VertexTouchesNonWater(vertex) && distance < shoreDistance)
                {
                    shoreDistance = distance;
                    nearestShore = vertex;
                }

                if (riverVertexTiles[vertex].Count == 1 && distance < boundaryDistance)
                {
                    boundaryDistance = distance;
                    nearestBoundary = vertex;
                }
            }

            return nearestShore >= 0 ? nearestShore : nearestBoundary;
        }

        private List<int> FindRiverPath(int start, int goal)
        {
            int vertexCount = riverVertices.Count;
            float[] cost = new float[vertexCount];
            float[] estimate = new float[vertexCount];
            int[] cameFrom = new int[vertexCount];
            bool[] open = new bool[vertexCount];
            bool[] closed = new bool[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                cost[i] = float.PositiveInfinity;
                estimate[i] = float.PositiveInfinity;
                cameFrom[i] = -1;
            }

            cost[start] = 0f;
            estimate[start] = Heuristic(start, goal);
            open[start] = true;

            while (true)
            {
                int current = -1;
                float bestEstimate = float.PositiveInfinity;
                for (int i = 0; i < vertexCount; i++)
                {
                    if (open[i] && estimate[i] < bestEstimate)
                    {
                        current = i;
                        bestEstimate = estimate[i];
                    }
                }

                if (current < 0)
                {
                    return new List<int>();
                }

                if (current == goal)
                {
                    return ReconstructPath(cameFrom, current);
                }

                open[current] = false;
                closed[current] = true;

                foreach (int neighbor in riverVertexNeighbors[current])
                {
                    if (closed[neighbor])
                    {
                        continue;
                    }

                    float neighborElevation = GetVertexElevation(neighbor);
                    float uphill = Mathf.Max(0f, neighborElevation - GetVertexElevation(current));
                    float moveCost = 1f + uphill * 9f + neighborElevation * 0.25f;
                    if (VertexTouchesOnlyWater(neighbor) && neighbor != goal)
                    {
                        moveCost += 15f;
                    }

                    if (riverVertexTiles[neighbor].Count == 1 && neighbor != goal)
                    {
                        moveCost += 0.4f;
                    }

                    if (riverNodes.Contains(neighbor))
                    {
                        moveCost *= 0.3f;
                    }

                    float tentative = cost[current] + moveCost;
                    if (tentative >= cost[neighbor])
                    {
                        continue;
                    }

                    cameFrom[neighbor] = current;
                    cost[neighbor] = tentative;
                    estimate[neighbor] = tentative + Heuristic(neighbor, goal);
                    open[neighbor] = true;
                }
            }
        }

        private float Heuristic(int from, int to)
        {
            return Vector2.Distance(riverVertices[from], riverVertices[to]) / HexRadius;
        }

        private static List<int> ReconstructPath(int[] cameFrom, int current)
        {
            List<int> path = new List<int> { current };
            while (cameFrom[current] >= 0)
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        private float GetVertexElevation(int vertex)
        {
            float total = 0f;
            foreach (int tile in riverVertexTiles[vertex])
            {
                total += elevation[tile];
            }

            return total / riverVertexTiles[vertex].Count;
        }

        private bool VertexTouchesTerrain(int vertex, Terrain terrainType)
        {
            foreach (int tile in riverVertexTiles[vertex])
            {
                if (terrain[tile] == terrainType)
                {
                    return true;
                }
            }

            return false;
        }

        private bool VertexTouchesNonWater(int vertex)
        {
            foreach (int tile in riverVertexTiles[vertex])
            {
                if (terrain[tile] != Terrain.Water)
                {
                    return true;
                }
            }

            return false;
        }

        private bool VertexTouchesOnlyWater(int vertex)
        {
            return VertexTouchesTerrain(vertex, Terrain.Water) && !VertexTouchesNonWater(vertex);
        }

        private void ValidateRiverEdges()
        {
            foreach (RiverEdge edge in riverEdges)
            {
                if (!riverGraphEdges.Contains(edge))
                {
                    throw new InvalidOperationException(
                        $"River segment {edge.A}-{edge.B} is not an actual hex edge.");
                }

                float length = Vector2.Distance(riverVertices[edge.A], riverVertices[edge.B]);
                if (Mathf.Abs(length - HexRadius) > 0.002f)
                {
                    throw new InvalidOperationException(
                        $"River segment {edge.A}-{edge.B} has length {length:F4}, expected hex-side length {HexRadius:F4}.");
                }
            }
        }

        private void BuildTerrainObjects()
        {
            tileRoot = new GameObject("Terrain").transform;
            tileRoot.SetParent(transform, false);

            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    int index = ToIndex(column, row);
                    string resourceLabel = resourcePresent[index]
                        ? ResourceCatalog.GetLabel(selectedResources[index])
                        : "No Resource";
                    GameObject tileObject = new GameObject(
                        $"Hex {column:00},{row:00} - {terrain[index]} - {resourceLabel}");
                    tileObjects[index] = tileObject;
                    tileObject.transform.SetParent(tileRoot, false);
                    tileObject.transform.localPosition = centers[index];
                    tileObject.transform.localScale = Vector3.one;

                    SpriteRenderer renderer = tileObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = hexSprite;
                    renderer.color = GetTerrainColor(terrain[index]);
                    renderer.sortingOrder = 0;

                    GameObject edgeObject = new GameObject("White Edge");
                    edgeObject.transform.SetParent(tileObject.transform, false);
                    SpriteRenderer edge = edgeObject.AddComponent<SpriteRenderer>();
                    edge.sprite = hexOutlineSprite;
                    edge.color = Color.white;
                    edge.sortingOrder = 1;

                    GameObject highlightObject = new GameObject("Placement Highlight");
                    highlightObject.transform.SetParent(tileObject.transform, false);
                    SpriteRenderer highlight = highlightObject.AddComponent<SpriteRenderer>();
                    highlight.sprite = hexSprite;
                    highlight.sortingOrder = 2;
                    highlight.enabled = false;
                    tileHighlightRenderers[index] = highlight;

                    GameObject occupancyOutlineObject = new GameObject("Occupancy Outline");
                    occupancyOutlineObject.transform.SetParent(tileObject.transform, false);
                    SpriteRenderer occupancyOutline = occupancyOutlineObject.AddComponent<SpriteRenderer>();
                    occupancyOutline.sprite = hexOutlineSprite;
                    occupancyOutline.sortingOrder = 3;
                    occupancyOutline.enabled = false;
                    tileOccupancyOutlineRenderers[index] = occupancyOutline;
                }
            }
        }

        private void BuildRiverObjects()
        {
            riverRoot = new GameObject("Rivers").transform;
            riverRoot.SetParent(transform, false);

            foreach (RiverEdge edge in riverEdges)
            {
                CreateRiverSegment(edge.A, edge.B, 0.15f, RiverBankColor, 8);
                CreateRiverSegment(edge.A, edge.B, 0.09f, RiverColor, 9);
            }

            foreach (int vertex in riverNodes)
            {
                CreateRiverNode(vertex, 0.15f, RiverBankColor, 8);
                CreateRiverNode(vertex, 0.09f, RiverColor, 9);
            }
        }

        private void CreateRiverSegment(int from, int to, float width, Color color, int sortingOrder)
        {
            Vector2 start = riverVertices[from];
            Vector2 end = riverVertices[to];
            Vector2 delta = end - start;
            GameObject segment = new GameObject($"River edge {from}-{to}");
            segment.transform.SetParent(riverRoot, false);
            segment.transform.localPosition = (start + end) * 0.5f;
            segment.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            segment.transform.localScale = new Vector3(delta.magnitude, width, 1f);

            SpriteRenderer renderer = segment.AddComponent<SpriteRenderer>();
            renderer.sprite = squareSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
        }

        private void CreateRiverNode(int vertex, float size, Color color, int sortingOrder)
        {
            GameObject node = new GameObject($"River corner {vertex}");
            node.transform.SetParent(riverRoot, false);
            node.transform.localPosition = riverVertices[vertex];
            node.transform.localScale = new Vector3(size, size, 1f);

            SpriteRenderer renderer = node.AddComponent<SpriteRenderer>();
            renderer.sprite = circleSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
        }

        private void FrameMapWithCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = FindFirstObjectByType<Camera>();
            }

            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            Vector2 minimum = centers[0];
            Vector2 maximum = centers[0];
            for (int i = 1; i < centers.Length; i++)
            {
                minimum = Vector2.Min(minimum, centers[i]);
                maximum = Vector2.Max(maximum, centers[i]);
            }

            Vector2 center = (minimum + maximum) * 0.5f;
            float mapWidth = maximum.x - minimum.x + HorizontalSpacing;
            float mapHeight = maximum.y - minimum.y + HexRadius * 2f;
            camera.rect = new Rect(0.25f, 0f, 0.75f, 1f);
            float aspect = camera.aspect > 0f ? camera.aspect : 16f / 9f;

            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(mapHeight * 0.5f + 0.55f, (mapWidth * 0.5f + 0.55f) / aspect);
            camera.transform.position = new Vector3(center.x, center.y, -10f);
            camera.transform.rotation = Quaternion.identity;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(31, 39, 43, 255);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
        }

        public bool TryGetTileAtWorldPosition(Vector2 worldPosition, out int tileIndex)
        {
            float bestDistance = float.PositiveInfinity;
            int nearest = -1;

            for (int i = 0; i < centers.Length; i++)
            {
                float distance = (worldPosition - centers[i]).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = i;
                }
            }

            if (nearest >= 0)
            {
                Vector2 local = worldPosition - centers[nearest];
                float absX = Mathf.Abs(local.x);
                float absY = Mathf.Abs(local.y);
                if (absY <= HexRadius && absX <= HexRadius * 0.8660254f &&
                    absX / 1.7320508f + absY <= HexRadius)
                {
                    tileIndex = nearest;
                    return true;
                }
            }

            tileIndex = -1;
            return false;
        }

        public Vector2 GetTileWorldPosition(int tileIndex)
        {
            return centers[Mathf.Clamp(tileIndex, 0, centers.Length - 1)];
        }

        public Vector2 GetTileVertexWorldPosition(int tileIndex, int vertexIndex)
        {
            int tile = Mathf.Clamp(tileIndex, 0, centers.Length - 1);
            int vertex = ((vertexIndex % 6) + 6) % 6;
            float angle = (90f - vertex * 60f) * Mathf.Deg2Rad;
            return centers[tile] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * HexRadius;
        }

        public Terrain GetTerrain(int tileIndex)
        {
            return terrain[Mathf.Clamp(tileIndex, 0, terrain.Length - 1)];
        }

        public Resource GetSelectedResource(int tileIndex)
        {
            return selectedResources[Mathf.Clamp(tileIndex, 0, selectedResources.Length - 1)];
        }

        public bool HasResource(int tileIndex)
        {
            return tileIndex >= 0 && tileIndex < TileCount && resourcePresent[tileIndex];
        }

        public bool IsShoreWaterTile(int tileIndex)
        {
            return ResourceSave.IsShoreWaterTile(terrain, Width, Height, tileIndex);
        }

        public void RemoveResource(int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= TileCount)
            {
                return;
            }

            resourcePresent[tileIndex] = false;
            if (tileObjects[tileIndex] != null)
            {
                tileObjects[tileIndex].name = $"Hex {tileIndex % Width:00},{tileIndex / Width:00} - " +
                                              $"{terrain[tileIndex]} - No Resource";
            }
        }

        public void RestoreResource(int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= TileCount)
            {
                return;
            }

            resourcePresent[tileIndex] = naturalResourcePresent[tileIndex];
            if (tileObjects[tileIndex] != null)
            {
                string resourceLabel = resourcePresent[tileIndex]
                    ? ResourceCatalog.GetLabel(selectedResources[tileIndex])
                    : "No Resource";
                tileObjects[tileIndex].name = $"Hex {tileIndex % Width:00},{tileIndex / Width:00} - " +
                                              $"{terrain[tileIndex]} - " +
                                              resourceLabel;
            }
        }

        public void SetTileHighlights(IReadOnlyList<int> tileIndices, Color color)
        {
            ClearTileHighlights();
            Color highlightColor = new Color(color.r, color.g, color.b, 0.38f);
            for (int i = 0; i < tileIndices.Count; i++)
            {
                int tile = tileIndices[i];
                if (tile < 0 || tile >= tileHighlightRenderers.Length || tileHighlightRenderers[tile] == null)
                {
                    continue;
                }

                tileHighlightRenderers[tile].color = highlightColor;
                tileHighlightRenderers[tile].enabled = true;
            }
        }

        public void ClearTileHighlights()
        {
            for (int tile = 0; tile < tileHighlightRenderers.Length; tile++)
            {
                if (tileHighlightRenderers[tile] != null)
                {
                    tileHighlightRenderers[tile].enabled = false;
                }
            }
        }

        public void SetTileOccupancyOutline(int tileIndex, Color color)
        {
            if (tileIndex < 0 || tileIndex >= tileOccupancyOutlineRenderers.Length ||
                tileOccupancyOutlineRenderers[tileIndex] == null)
            {
                return;
            }

            tileOccupancyOutlineRenderers[tileIndex].color = color;
            tileOccupancyOutlineRenderers[tileIndex].enabled = true;
        }

        public void ClearTileOccupancyOutlines()
        {
            for (int tile = 0; tile < tileOccupancyOutlineRenderers.Length; tile++)
            {
                if (tileOccupancyOutlineRenderers[tile] != null)
                {
                    tileOccupancyOutlineRenderers[tile].enabled = false;
                }
            }
        }

        public int GetHexDistance(int firstTileIndex, int secondTileIndex)
        {
            if (firstTileIndex < 0 || firstTileIndex >= TileCount ||
                secondTileIndex < 0 || secondTileIndex >= TileCount)
            {
                return int.MaxValue;
            }

            FromIndex(firstTileIndex, out int firstColumn, out int firstRow);
            FromIndex(secondTileIndex, out int secondColumn, out int secondRow);

            int firstX = firstColumn - (firstRow - (firstRow & 1)) / 2;
            int firstZ = firstRow;
            int firstY = -firstX - firstZ;
            int secondX = secondColumn - (secondRow - (secondRow & 1)) / 2;
            int secondZ = secondRow;
            int secondY = -secondX - secondZ;

            return Mathf.Max(Mathf.Abs(firstX - secondX),
                Mathf.Abs(firstY - secondY), Mathf.Abs(firstZ - secondZ));
        }

        private int CountTerrain(Terrain value)
        {
            int count = 0;
            for (int i = 0; i < terrain.Length; i++)
            {
                if (terrain[i] == value)
                {
                    count++;
                }
            }

            return count;
        }

        private static Color32 GetTerrainColor(Terrain value)
        {
            switch (value)
            {
                case Terrain.Woodland:
                    return WoodlandColor;
                case Terrain.Mountain:
                    return MountainColor;
                case Terrain.Water:
                    return WaterColor;
                default:
                    return PlainsColor;
            }
        }

        private List<int> CreateIndexList()
        {
            List<int> result = new List<int>(TileCount);
            for (int i = 0; i < TileCount; i++)
            {
                result.Add(i);
            }

            return result;
        }

        private bool[] CreateInitialResourcePresence()
        {
            bool[] result = new bool[TileCount];
            for (int tile = 0; tile < result.Length; tile++)
            {
                result[tile] = true;
            }

            return result;
        }

        private void ResetResourcePresence(IReadOnlyList<bool> generatedResourcePresence)
        {
            for (int tile = 0; tile < resourcePresent.Length; tile++)
            {
                bool present = generatedResourcePresence != null && tile < generatedResourcePresence.Count &&
                               generatedResourcePresence[tile];
                naturalResourcePresent[tile] = present;
                resourcePresent[tile] = present;
            }
        }

        private float NextRange(float minimum, float maximum)
        {
            return Mathf.Lerp(minimum, maximum, (float)random.NextDouble());
        }

        private int ToIndex(int column, int row)
        {
            return row * Width + column;
        }

        private void FromIndex(int index, out int column, out int row)
        {
            column = index % Width;
            row = index / Width;
        }

        private readonly struct VertexKey : IEquatable<VertexKey>
        {
            private readonly int x;
            private readonly int y;

            public VertexKey(Vector2 position)
            {
                x = Mathf.RoundToInt(position.x * 10000f);
                y = Mathf.RoundToInt(position.y * 10000f);
            }

            public bool Equals(VertexKey other)
            {
                return x == other.x && y == other.y;
            }

            public override bool Equals(object obj)
            {
                return obj is VertexKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (x * 397) ^ y;
                }
            }
        }

        private readonly struct RiverEdge : IEquatable<RiverEdge>
        {
            public readonly int A;
            public readonly int B;

            public RiverEdge(int first, int second)
            {
                A = Mathf.Min(first, second);
                B = Mathf.Max(first, second);
            }

            public bool Equals(RiverEdge other)
            {
                return A == other.A && B == other.B;
            }

            public override bool Equals(object obj)
            {
                return obj is RiverEdge other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (A * 397) ^ B;
                }
            }
        }
    }
}
