using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlacementGridTests
{
    private PlacementGrid _placementGrid;

    [SetUp]
    public void SetUp()
    {
        _placementGrid = TestBootstrap.CreateSingleton<PlacementGrid>();
        var gridGo = TestBootstrap.CreateGameObject("Grid");
        var grid = gridGo.AddComponent<Grid>();
        _placementGrid.SetGrid(grid);
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
    }

    [UnityTest]
    public IEnumerator IsAreaFree_EmptyCell_ReturnsTrue()
    {
        yield return null;

        Assert.IsTrue(_placementGrid.IsAreaFree(new Vector3Int(3, 3, 0), 1, 1));
    }

    [UnityTest]
    public IEnumerator IsAreaFree_SolidCollider_ReturnsFalse()
    {
        var obstacle = TestBootstrap.CreateGameObject("Obstacle");
        obstacle.transform.position = _placementGrid.CellCenterWorld(new Vector3Int(2, 2, 0));
        obstacle.AddComponent<BoxCollider2D>();
        yield return null;

        Assert.IsFalse(_placementGrid.IsAreaFree(new Vector3Int(2, 2, 0), 1, 1));
    }

    [UnityTest]
    public IEnumerator IsAreaFree_TriggerCollider_ReturnsTrue()
    {
        var interactable = TestBootstrap.CreateGameObject("Interactable");
        interactable.transform.position = _placementGrid.CellCenterWorld(new Vector3Int(4, 4, 0));
        var col = interactable.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        yield return null;

        Assert.IsTrue(_placementGrid.IsAreaFree(new Vector3Int(4, 4, 0), 1, 1));
    }

    [UnityTest]
    public IEnumerator IsAreaFree_CellReservedByPriorPlacement_ReturnsFalse()
    {
        var marker = TestBootstrap.AddComponent<PlacedInfrastructure>();
        _placementGrid.Reserve(new Vector3Int(5, 5, 0), 1, 1, marker);
        yield return null;

        Assert.IsFalse(_placementGrid.IsAreaFree(new Vector3Int(5, 5, 0), 1, 1));
    }

    [UnityTest]
    public IEnumerator IsAreaFree_MultiCellFootprint_BlockedByOneOccupiedCell()
    {
        var marker = TestBootstrap.AddComponent<PlacedInfrastructure>();
        _placementGrid.Reserve(new Vector3Int(7, 5, 0), 1, 1, marker);
        yield return null;

        Assert.IsFalse(_placementGrid.IsAreaFree(new Vector3Int(6, 5, 0), 2, 1));
    }

    [UnityTest]
    public IEnumerator Reserve_SetsFootprintFieldsOnInstance()
    {
        var marker = TestBootstrap.AddComponent<PlacedInfrastructure>();

        _placementGrid.Reserve(new Vector3Int(1, 1, 0), 2, 3, marker);
        yield return null;

        Assert.AreEqual(new Vector3Int(1, 1, 0), marker.OriginCell);
        Assert.AreEqual(2, marker.FootprintWidth);
        Assert.AreEqual(3, marker.FootprintHeight);
    }

    [UnityTest]
    public IEnumerator IsAreaFree_ColliderInAdjacentCell_DoesNotBlockThisCell()
    {
        var obstacle = TestBootstrap.CreateGameObject("Obstacle");
        obstacle.transform.position = _placementGrid.CellCenterWorld(new Vector3Int(10, 10, 0));
        obstacle.AddComponent<BoxCollider2D>();
        yield return null;

        Assert.IsTrue(_placementGrid.IsAreaFree(new Vector3Int(11, 10, 0), 1, 1));
    }

    [UnityTest]
    public IEnumerator WorldToCell_RoundTripsWithCellCenterWorld()
    {
        yield return null;

        var cell = new Vector3Int(8, -2, 0);
        var world = _placementGrid.CellCenterWorld(cell);

        Assert.AreEqual(cell, _placementGrid.WorldToCell(world));
    }
}
