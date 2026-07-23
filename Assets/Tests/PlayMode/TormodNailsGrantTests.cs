using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine.TestTools;

public class TormodNailsGrantTests
{
    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator FirstTormodDelivery_GrantsThreeNails()
    {
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        TestBootstrap.CreateSingleton<GameManager>();
        TestBootstrap.CreateSingleton<SellManager>();

        yield return null;

        GameEvents.OnDeliveryMade(DeliveryType.Tormod, ContentDb.BerryShine, 1, 15);

        Assert.AreEqual(3, inventory.GetCount(ContentDb.Nails));
    }

    [UnityTest]
    public IEnumerator SecondTormodDelivery_DoesNotGrantNailsAgain()
    {
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        TestBootstrap.CreateSingleton<GameManager>();
        TestBootstrap.CreateSingleton<SellManager>();

        yield return null;

        GameEvents.OnDeliveryMade(DeliveryType.Tormod, ContentDb.BerryShine, 1, 15);
        GameEvents.OnDeliveryMade(DeliveryType.Tormod, ContentDb.BerryShine, 1, 15);

        Assert.AreEqual(3, inventory.GetCount(ContentDb.Nails));
    }

    [UnityTest]
    public IEnumerator CartDelivery_DoesNotGrantNails()
    {
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        TestBootstrap.CreateSingleton<GameManager>();
        TestBootstrap.CreateSingleton<SellManager>();

        yield return null;

        GameEvents.OnDeliveryMade(DeliveryType.Cart, ContentDb.BerryShine, 1, 15);

        Assert.AreEqual(0, inventory.GetCount(ContentDb.Nails));
    }
}
