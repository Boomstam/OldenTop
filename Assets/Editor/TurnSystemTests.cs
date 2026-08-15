using NUnit.Framework;
using UnityEngine;

namespace OldenTop.Tests
{
    public sealed class TurnSystemTests
    {
        private GameObject root;
        private TurnSystem turnSystem;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Turn system test");
            HexMap map = root.AddComponent<HexMap>();
            turnSystem = root.AddComponent<TurnSystem>();
            turnSystem.Initialize(map);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
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
    }
}
