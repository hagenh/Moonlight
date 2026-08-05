using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BuildModeControllerTests
{
    private BuildModeController _controller;
    private PlacementGrid _placementGrid;
    private InfrastructureManager _infraManager;
    private static readonly ItemDef TestItem = new ItemDef("test_placeable", "Test Placeable", isPlaceable: true);

    [SetUp]
    public void SetUp()
    {
        _placementGrid = TestBootstrap.CreateSingleton<PlacementGrid>();
        var gridGo = TestBootstrap.CreateGameObject("Grid");
        _placementGrid.SetGrid(gridGo.AddComponent<Grid>());

        _infraManager = TestBootstrap.CreateSingleton<InfrastructureManager>();
        _infraManager.Book.Add(TestItem, 2);

        _controller = TestBootstrap.CreateSingleton<BuildModeController>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
    }

    [UnityTest]
    public IEnumerator Enter_PlaceableItem_ActivatesWithItem()
    {
        _controller.Enter(TestItem);
        yield return null;

        Assert.IsTrue(_controller.IsActive);
        Assert.AreEqual(TestItem, _controller.CurrentItem);
    }

    [UnityTest]
    public IEnumerator Enter_NonPlaceableItem_StaysInactive()
    {
        var notPlaceable = new ItemDef("grain", "Grain");

        _controller.Enter(notPlaceable);
        yield return null;

        Assert.IsFalse(_controller.IsActive);
    }

    [UnityTest]
    public IEnumerator Cancel_DeactivatesWithoutConsuming()
    {
        _controller.Enter(TestItem);
        _controller.Cancel();
        yield return null;

        Assert.IsFalse(_controller.IsActive);
        Assert.AreEqual(2, _infraManager.Book.Available(TestItem));
    }

    [UnityTest]
    public IEnumerator TryConfirmAt_FreeCell_ConsumesAndDeactivates()
    {
        _controller.Enter(TestItem);
        yield return null;

        bool result = _controller.TryConfirmAt(new Vector3Int(3, 3, 0));

        Assert.IsTrue(result);
        Assert.IsFalse(_controller.IsActive);
        Assert.AreEqual(1, _infraManager.Book.Available(TestItem));
        Assert.IsFalse(_placementGrid.IsAreaFree(new Vector3Int(3, 3, 0), 1, 1));
    }

    [UnityTest]
    public IEnumerator TryConfirmAt_OccupiedCell_ReturnsFalseAndStaysActive()
    {
        _controller.Enter(TestItem);
        yield return null;
        _controller.TryConfirmAt(new Vector3Int(3, 3, 0));

        _controller.Enter(TestItem);
        bool result = _controller.TryConfirmAt(new Vector3Int(3, 3, 0));

        Assert.IsFalse(result);
        Assert.IsTrue(_controller.IsActive);
        Assert.AreEqual(1, _infraManager.Book.Available(TestItem));
    }

    [UnityTest]
    public IEnumerator TryConfirmAt_NoStockLeft_ReturnsFalse()
    {
        _controller.Enter(TestItem);
        _controller.TryConfirmAt(new Vector3Int(1, 1, 0));
        _controller.Enter(TestItem);
        _controller.TryConfirmAt(new Vector3Int(2, 2, 0));

        _controller.Enter(TestItem);
        yield return null;
        bool result = _controller.TryConfirmAt(new Vector3Int(9, 9, 0));

        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator TryConfirmAt_WhenNotActive_ReturnsFalse()
    {
        yield return null;

        Assert.IsFalse(_controller.TryConfirmAt(new Vector3Int(0, 0, 0)));
    }
}
