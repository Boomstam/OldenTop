using NUnit.Framework;
using UnityEngine;

namespace OldenTop.Tests
{
    public sealed class TurnSystemTests
    {
        private GameObject root;
        private TurnSystem turnSystem;
        private Camera mapCamera;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Turn system test");
            mapCamera = root.AddComponent<Camera>();
            mapCamera.orthographic = true;
            mapCamera.orthographicSize = 10f;
            HexMap map = root.AddComponent<HexMap>();
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
        public void SelectedWorker_CanBePlacedAndMovedToAnotherTile()
        {
            Assert.That(turnSystem.SelectActiveWorker(1), Is.True);
            Assert.That(turnSystem.SelectedWorker, Is.EqualTo(1));

            Assert.That(turnSystem.TryAssignActiveWorker(turnSystem.SelectedWorker, 12), Is.True);
            Assert.That(turnSystem.GetAssignment(0, 1), Is.EqualTo(12));

            Assert.That(turnSystem.TryAssignActiveWorker(turnSystem.SelectedWorker, 37), Is.True);
            Assert.That(turnSystem.GetAssignment(0, 1), Is.EqualTo(37));
        }

        [Test]
        public void PlacedWorker_CanReturnToInventory()
        {
            Assert.That(turnSystem.TryAssignActiveWorker(2, 25), Is.True);

            Assert.That(turnSystem.ReturnActiveWorkerToInventory(2), Is.True);

            Assert.That(turnSystem.GetAssignment(0, 2), Is.EqualTo(-1));
            Assert.That(turnSystem.SelectedWorker, Is.EqualTo(2));
        }

        [Test]
        public void EndingAssignments_ClearsSelectionForNextPlayer()
        {
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
        public void WorkerIconCatalog_AllPlayerIconsCanBeLoaded()
        {
            for (int player = 0; player < 2; player++)
            {
                Assert.That(WorkerIconCatalog.GetIcon(player), Is.Not.Null,
                    $"Missing icon at Resources/{WorkerIconCatalog.GetIconResourcePath(player)}");
            }
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
