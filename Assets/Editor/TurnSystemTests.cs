using NUnit.Framework;
using UnityEngine;

namespace OldenTop.Tests
{
    public sealed class TurnSystemTests
    {
        private GameObject root;
        private TurnSystem turnSystem;
        private Camera mapCamera;
        private HexMap map;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Turn system test");
            mapCamera = root.AddComponent<Camera>();
            mapCamera.orthographic = true;
            mapCamera.orthographicSize = 10f;
            map = root.AddComponent<HexMap>();
            turnSystem = root.AddComponent<TurnSystem>();
            turnSystem.Initialize(map, mapCamera);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void MapSeedUtility_NumericTextPreservesLegacySeed()
        {
            Assert.That(MapSeedUtility.ToInt32("1375840081"), Is.EqualTo(1375840081));
        }

        [Test]
        public void MapSeedUtility_TextSeedIsStable()
        {
            Assert.That(MapSeedUtility.ToInt32("Olden Top"), Is.EqualTo(679649534));
        }

        [Test]
        public void SelectedWorker_CanStayOrMoveOneTileFromTurnStart()
        {
            Assert.That(turnSystem.TryPlaceActiveHearth(0), Is.True);
            Assert.That(turnSystem.SelectActiveWorker(1), Is.True);
            Assert.That(turnSystem.SelectedWorker, Is.EqualTo(1));

            Assert.That(turnSystem.TryAssignActiveWorker(turnSystem.SelectedWorker, 1), Is.True);
            Assert.That(turnSystem.GetAssignment(0, 1), Is.EqualTo(1));

            Assert.That(turnSystem.TryAssignActiveWorker(1, 20), Is.True);
            Assert.That(turnSystem.GetAssignment(0, 1), Is.EqualTo(20));
        }

        [Test]
        public void EndingAssignments_ClearsSelectionForNextPlayer()
        {
            Assert.That(turnSystem.TryPlaceActiveHearth(0), Is.True);
            Assert.That(turnSystem.SelectActiveWorker(0), Is.True);

            turnSystem.EndAssignments();

            Assert.That(turnSystem.ActivePlayer, Is.EqualTo(1));
            Assert.That(turnSystem.SelectedWorker, Is.EqualTo(-1));
        }

        [Test]
        public void ResourceCatalog_HasExpectedOptions()
        {
            Assert.That(ResourceCatalog.GetOptions(Terrain.Mountain),
                Is.EqualTo(new[] { Resource.Flintstone, Resource.Firestone }));
            Assert.That(ResourceCatalog.GetOptions(Terrain.Plains),
                Is.EqualTo(new[] { Resource.Grains, Resource.Aurochs }));
            Assert.That(ResourceCatalog.GetOptions(Terrain.Water),
                Is.EqualTo(new[] { Resource.Fish, Resource.Reeds }));
            Assert.That(ResourceCatalog.GetOptions(Terrain.Woodland),
                Is.EqualTo(new[] { Resource.Wood, Resource.Mushrooms }));
        }

        [Test]
        public void ResourceCatalog_AllResourceIconsCanBeLoaded()
        {
            foreach (Resource resource in System.Enum.GetValues(typeof(Resource)))
            {
                Assert.That(ResourceCatalog.GetIcon(resource), Is.Not.Null,
                    $"Missing icon at Resources/{ResourceCatalog.GetIconResourcePath(resource)}");
            }
        }

        [Test]
        public void AdvancingSeason_AddsOneResourcePerAssignedWorkerToEachPlayersStockpile()
        {
            Assert.That(turnSystem.TryPlaceActiveHearth(0), Is.True);
            Assert.That(turnSystem.TryAssignActiveWorker(0, 1), Is.True);
            Assert.That(turnSystem.TryAssignActiveWorker(1, 1), Is.True);
            turnSystem.EndAssignments();

            Assert.That(turnSystem.TryPlaceActiveHearth(21), Is.True);
            Assert.That(turnSystem.TryAssignActiveWorker(0, 22), Is.True);
            turnSystem.EndAssignments();

            Resource playerOneResource = map.GetSelectedResource(1);
            Resource playerTwoResource = map.GetSelectedResource(22);
            turnSystem.AdvanceSeason();

            Assert.That(turnSystem.GetStockpileAmount(0, playerOneResource), Is.EqualTo(2));
            Assert.That(turnSystem.GetStockpileAmount(1, playerTwoResource), Is.EqualTo(1));
            Assert.That(turnSystem.GetLatestSeasonGain(0, playerOneResource), Is.EqualTo(2));
            Assert.That(turnSystem.GetLatestSeasonGain(1, playerTwoResource), Is.EqualTo(1));
            Assert.That(turnSystem.IsSeasonGainsDialogVisible, Is.True);

            turnSystem.DismissSeasonGainsDialog();
            Assert.That(turnSystem.IsSeasonGainsDialogVisible, Is.False);
        }

        [Test]
        public void FoodAssignment_ConsumesFoodAndKeepsFedWorkersAlive()
        {
            Assert.That(turnSystem.TryPlaceActiveHearth(0), Is.True);
            Resource food = map.GetSelectedResource(1);
            Assert.That(ResourceCatalog.IsFood(food), Is.True,
                "The adjacent prototype resource must be edible for this feeding-flow test.");
            for (int worker = 0; worker < 4; worker++)
            {
                Assert.That(turnSystem.TryAssignActiveWorker(worker, 1), Is.True);
            }
            turnSystem.EndAssignments();

            Assert.That(turnSystem.TryPlaceActiveHearth(21), Is.True);
            turnSystem.EndAssignments();

            Assert.That(turnSystem.IsFoodAssignmentPhase, Is.True);
            Assert.That(turnSystem.GetStockpileAmount(0, food), Is.EqualTo(4));
            for (int worker = 0; worker < 4; worker++)
            {
                Assert.That(turnSystem.TryAssignFoodToActiveWorker(worker, food), Is.True);
            }
            Assert.That(turnSystem.GetStockpileAmount(0, food), Is.Zero);

            turnSystem.TryEndFoodAssignments();

            Assert.That(turnSystem.ActivePlayer, Is.EqualTo(1));
            Assert.That(turnSystem.GetAncestorCount(0), Is.Zero);
            for (int worker = 0; worker < 4; worker++)
            {
                Assert.That(turnSystem.IsWorkerAlive(0, worker), Is.True);
            }
        }

        [Test]
        public void FoodAssignment_WarnsThenRemovesUnfedWorkersAsAncestors()
        {
            Assert.That(turnSystem.TryPlaceActiveHearth(0), Is.True);
            turnSystem.EndAssignments();
            Assert.That(turnSystem.TryPlaceActiveHearth(21), Is.True);
            turnSystem.EndAssignments();

            turnSystem.TryEndFoodAssignments();

            Assert.That(turnSystem.IsFoodShortageDialogVisible, Is.True);
            turnSystem.ConfirmFoodShortage();

            Assert.That(turnSystem.GetAncestorCount(0), Is.EqualTo(4));
            for (int worker = 0; worker < 4; worker++)
            {
                Assert.That(turnSystem.IsWorkerAlive(0, worker), Is.False);
            }
        }

        [Test]
        public void HearthFuelAssignment_ConsumesWoodAndKeepsTheHearthLit()
        {
            map.Generate("hearth-fuel-test");
            int woodTile = FindTileWithResource(Resource.Wood);
            int playerOneHearth = FindAdjacentLandTile(woodTile);
            int playerTwoHearth = FindLandTileOtherThan(playerOneHearth);

            Assert.That(playerOneHearth, Is.GreaterThanOrEqualTo(0));
            Assert.That(playerTwoHearth, Is.GreaterThanOrEqualTo(0));
            Assert.That(turnSystem.TryPlaceActiveHearth(playerOneHearth), Is.True);
            for (int worker = 0; worker < 4; worker++)
            {
                Assert.That(turnSystem.TryAssignActiveWorker(worker, woodTile), Is.True);
            }
            turnSystem.EndAssignments();

            Assert.That(turnSystem.TryPlaceActiveHearth(playerTwoHearth), Is.True);
            turnSystem.EndAssignments();

            Assert.That(turnSystem.GetStockpileAmount(0, Resource.Wood), Is.EqualTo(4));
            Assert.That(turnSystem.TryAssignWoodToActiveHearth(), Is.True);
            Assert.That(turnSystem.GetStockpileAmount(0, Resource.Wood), Is.EqualTo(3));
            Assert.That(turnSystem.IsHearthFueled(0), Is.True);
            Assert.That(turnSystem.GetHearthTile(0), Is.EqualTo(playerOneHearth));
        }

        [Test]
        public void UnfueledHearth_GoesOutAndRestoresItsResourceSite()
        {
            const int playerOneHearth = 0;
            Assert.That(map.HasResource(playerOneHearth), Is.True);
            Assert.That(turnSystem.TryPlaceActiveHearth(playerOneHearth), Is.True);
            Assert.That(map.HasResource(playerOneHearth), Is.False);
            turnSystem.EndAssignments();

            Assert.That(turnSystem.TryPlaceActiveHearth(21), Is.True);
            turnSystem.EndAssignments();

            turnSystem.TryEndFoodAssignments();
            Assert.That(turnSystem.IsFoodShortageDialogVisible, Is.True);
            turnSystem.ConfirmFoodShortage();

            Assert.That(turnSystem.GetHearthTile(0), Is.EqualTo(-1));
            Assert.That(map.HasResource(playerOneHearth), Is.True);
        }

        private int FindTileWithResource(Resource resource)
        {
            for (int tile = 0; tile < map.GeneratedTileCount; tile++)
            {
                if (map.HasResource(tile) && map.GetSelectedResource(tile) == resource)
                {
                    return tile;
                }
            }

            return -1;
        }

        private int FindAdjacentLandTile(int tile)
        {
            for (int candidate = 0; candidate < map.GeneratedTileCount; candidate++)
            {
                if (map.GetTerrain(candidate) != Terrain.Water &&
                    map.GetHexDistance(tile, candidate) == 1)
                {
                    return candidate;
                }
            }

            return -1;
        }

        private int FindLandTileOtherThan(int excludedTile)
        {
            for (int tile = 0; tile < map.GeneratedTileCount; tile++)
            {
                if (tile != excludedTile && map.GetTerrain(tile) != Terrain.Water)
                {
                    return tile;
                }
            }

            return -1;
        }

        [Test]
        public void WorkerIconCatalog_AllPlayerIconsCanBeLoaded()
        {
            for (int player = 0; player < 2; player++)
            {
                Assert.That(WorkerIconCatalog.GetIcon(player), Is.Not.Null,
                    $"Missing icon at Resources/{WorkerIconCatalog.GetIconResourcePath(player)}");
            }
        }

        [Test]
        public void AncestorIconCatalog_CanLoadTombstoneIcon()
        {
            Assert.That(AncestorIconCatalog.GetIcon(), Is.Not.Null,
                "Missing ancestor icon at Resources/AncestorIcons/ancestor-tombstone");
        }

        [Test]
        public void Zoom_ChangesCameraSizeAndClampsToConfiguredRange()
        {
            Assert.That(turnSystem.MapResourceIconSize, Is.EqualTo(42f).Within(0.001f));

            turnSystem.AdjustZoom(1f);
            Assert.That(turnSystem.MapCameraSize, Is.LessThan(10f));
            Assert.That(turnSystem.MapResourceIconSize, Is.GreaterThan(42f).And.LessThan(84f));

            turnSystem.AdjustZoom(100f);
            Assert.That(turnSystem.MapCameraSize, Is.EqualTo(turnSystem.MinimumMapCameraSize).Within(0.001f));
            Assert.That(turnSystem.MapResourceIconSize, Is.EqualTo(84f).Within(0.001f));

            turnSystem.AdjustZoom(-100f);
            Assert.That(turnSystem.MapCameraSize, Is.EqualTo(turnSystem.MaximumMapCameraSize).Within(0.001f));
            Assert.That(turnSystem.MapResourceIconSize, Is.EqualTo(42f).Within(0.001f));
        }
    }
}
