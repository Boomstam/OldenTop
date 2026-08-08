using System;
using System.Collections.Generic;
using UnityEngine;

namespace OldenTop.Prototype
{
    public enum PrototypeTerrain
    {
        Grassland,
        Woodland,
        Mountain,
        Water
    }

    internal static class PrototypeMapBootstrap
    {
        private const string RootName = "Prototype Hex Map";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void SpawnMap()
        {
            GameObject previousMap = GameObject.Find(RootName);
            if (previousMap != null)
            {
                UnityEngine.Object.Destroy(previousMap);
            }

            GameObject mapObject = new GameObject(RootName);
            mapObject.hideFlags = HideFlags.DontSave;
            mapObject.AddComponent<PrototypeHexMap>().Generate();
        }
    }

    internal sealed class PrototypeHexMap : MonoBehaviour
    {
        private const int Width = 20;
        private const int Height = 20;
        private const int TileCount = Width * Height;
        private const float HexRadius = 0.62f;
        private const float HorizontalSpacing = 1.7320508f * HexRadius;
        private const float VerticalSpacing = 1.5f * HexRadius;
        private const int RiverSourceCount = 4;

        private static readonly Color32 GrasslandColor = new Color32(174, 194, 104, 255);
        private static readonly Color32 WoodlandColor = new Color32(65, 113, 72, 255);
        private static readonly Color32 MountainColor = new Color32(125, 123, 132, 255);
        private static readonly Color32 WaterColor = new Color32(74, 139, 178, 255);
        private static readonly Color32 RiverBankColor = new Color32(43, 91, 119, 255);
        private static readonly Color32 RiverColor = new Color32(86, 184, 221, 255);

        private readonly PrototypeTerrain[] terrain = new PrototypeTerrain[TileCount];
        private readonly float[] elevation = new float[TileCount];
        private readonly float[] moisture = new float[TileCount];
        private readonly Vector2[] centers = new Vector2[TileCount];
        private readonly int[,] tileCornerVertices = new int[TileCount, 6];
        private readonly List<Vector2> riverVertices = new List<Vector2>();
        private readonly List<List<int>> riverVertexNeighbors = new List<List<int>>();
        private readonly List<List<int>> riverVertexTiles = new List<List<int>>();
        private readonly HashSet<RiverEdge> riverGraphEdges = new HashSet<RiverEdge>();
        private readonly HashSet<RiverEdge> riverEdges = new HashSet<RiverEdge>();
        private readonly HashSet<int> riverNodes = new HashSet<int>();

        private System.Random random;
        private Sprite hexSprite;
        private Sprite squareSprite;
        private Sprite circleSprite;
        private Transform tileRoot;
        private Transform riverRoot;

        public void Generate()
        {
            int seed = unchecked(Environment.TickCount ^ (int)DateTime.UtcNow.Ticks);
            random = new System.Random(seed);

            CreateSharedSprites();
            CalculateCenters();
            GenerateTerrain();
            BuildRiverGraph();
            GenerateRivers();
            ValidateRiverEdges();
            BuildTerrainObjects();
            BuildRiverObjects();
            FrameMapWithCamera();

            int grassland = CountTerrain(PrototypeTerrain.Grassland);
            int woodland = CountTerrain(PrototypeTerrain.Woodland);
            int mountain = CountTerrain(PrototypeTerrain.Mountain);
            int water = CountTerrain(PrototypeTerrain.Water);
            Debug.Log($"Prototype map spawned (seed {seed}): {Width}x{Height}, " +
                      $"grassland {grassland}, woodland {woodland}, mountain {mountain}, " +
                      $"water {water}, edge-following river segments {riverEdges.Count}. " +
                      "River edge invariant verified.", this);
        }

        private void CreateSharedSprites()
        {
            hexSprite = CreateHexSprite();
            squareSprite = CreateSquareSprite();
            circleSprite = CreateCircleSprite();
        }

        private static Sprite CreateHexSprite()
        {
            const int textureWidth = 128;
            const int textureHeight = 148;
            Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
            {
                name = "Runtime Pointy Hex",
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

                    if (insideDistance < 0f)
                    {
                        pixels[y * textureWidth + x] = new Color32(0, 0, 0, 0);
                    }
                    else
                    {
                        byte shade = insideDistance < 0.035f ? (byte)155 : (byte)255;
                        pixels[y * textureWidth + x] = new Color32(shade, shade, shade, 255);
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
                    terrain[index] = PrototypeTerrain.Grassland;
                }
            }

            List<int> byElevation = CreateIndexList();
            byElevation.Sort((a, b) => elevation[a].CompareTo(elevation[b]));

            int waterCount = Mathf.RoundToInt(TileCount * 0.07f);
            int mountainCount = Mathf.RoundToInt(TileCount * 0.11f);
            int woodlandCount = Mathf.RoundToInt(TileCount * 0.30f);

            for (int i = 0; i < waterCount; i++)
            {
                terrain[byElevation[i]] = PrototypeTerrain.Water;
            }

            for (int i = 0; i < mountainCount; i++)
            {
                terrain[byElevation[byElevation.Count - 1 - i]] = PrototypeTerrain.Mountain;
            }

            List<int> woodlandCandidates = new List<int>();
            for (int i = 0; i < TileCount; i++)
            {
                if (terrain[i] == PrototypeTerrain.Grassland)
                {
                    woodlandCandidates.Add(i);
                }
            }

            woodlandCandidates.Sort((a, b) => WoodlandScore(b).CompareTo(WoodlandScore(a)));
            for (int i = 0; i < woodlandCount; i++)
            {
                terrain[woodlandCandidates[i]] = PrototypeTerrain.Woodland;
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
                if (terrain[tile] == PrototypeTerrain.Mountain)
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
                if (terrain[index] == PrototypeTerrain.Water)
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
                if (VertexTouchesTerrain(vertex, PrototypeTerrain.Water) &&
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

        private bool VertexTouchesTerrain(int vertex, PrototypeTerrain terrainType)
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
                if (terrain[tile] != PrototypeTerrain.Water)
                {
                    return true;
                }
            }

            return false;
        }

        private bool VertexTouchesOnlyWater(int vertex)
        {
            return VertexTouchesTerrain(vertex, PrototypeTerrain.Water) && !VertexTouchesNonWater(vertex);
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
                    GameObject tileObject = new GameObject($"Hex {column:00},{row:00} - {terrain[index]}");
                    tileObject.transform.SetParent(tileRoot, false);
                    tileObject.transform.localPosition = centers[index];
                    tileObject.transform.localScale = new Vector3(0.982f, 0.982f, 1f);

                    SpriteRenderer renderer = tileObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = hexSprite;
                    renderer.color = GetTerrainColor(terrain[index]);
                    renderer.sortingOrder = 0;
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
            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 16f / 9f;

            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(mapHeight * 0.5f + 0.55f, (mapWidth * 0.5f + 0.55f) / aspect);
            camera.transform.position = new Vector3(center.x, center.y, -10f);
            camera.transform.rotation = Quaternion.identity;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(31, 39, 43, 255);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
        }

        private int CountTerrain(PrototypeTerrain value)
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

        private static Color32 GetTerrainColor(PrototypeTerrain value)
        {
            switch (value)
            {
                case PrototypeTerrain.Woodland:
                    return WoodlandColor;
                case PrototypeTerrain.Mountain:
                    return MountainColor;
                case PrototypeTerrain.Water:
                    return WaterColor;
                default:
                    return GrasslandColor;
            }
        }

        private static List<int> CreateIndexList()
        {
            List<int> result = new List<int>(TileCount);
            for (int i = 0; i < TileCount; i++)
            {
                result.Add(i);
            }

            return result;
        }

        private float NextRange(float minimum, float maximum)
        {
            return Mathf.Lerp(minimum, maximum, (float)random.NextDouble());
        }

        private static int ToIndex(int column, int row)
        {
            return row * Width + column;
        }

        private static void FromIndex(int index, out int column, out int row)
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
