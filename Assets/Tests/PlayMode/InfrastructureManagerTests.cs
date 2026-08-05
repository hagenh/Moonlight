using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine.TestTools;

public class InfrastructureManagerTests
{
    private InfrastructureManager _manager;

    [SetUp]
    public void SetUp()
    {
        _manager = TestBootstrap.CreateSingleton<InfrastructureManager>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
    }

    [UnityTest]
    public IEnumerator Awake_SeedsAllFivePlaceableItemsAtFive()
    {
        yield return null;

        Assert.AreEqual(5, _manager.Book.Available(ContentDb.Lamppost));
        Assert.AreEqual(5, _manager.Book.Available(ContentDb.PlankSidewalk));
        Assert.AreEqual(5, _manager.Book.Available(ContentDb.Bench));
        Assert.AreEqual(5, _manager.Book.Available(ContentDb.FlowerBox));
        Assert.AreEqual(5, _manager.Book.Available(ContentDb.Sign));
    }

    [UnityTest]
    public IEnumerator TryConsume_WithStock_DecrementsBook()
    {
        yield return null;

        Assert.IsTrue(_manager.TryConsume(ContentDb.Lamppost));
        Assert.AreEqual(4, _manager.Book.Available(ContentDb.Lamppost));
    }

    [UnityTest]
    public IEnumerator TryConsume_UnknownItem_ReturnsFalse()
    {
        var notInBook = new ItemDef("mystery", "Mystery", isPlaceable: true);
        yield return null;

        Assert.IsFalse(_manager.TryConsume(notInBook));
    }
}
