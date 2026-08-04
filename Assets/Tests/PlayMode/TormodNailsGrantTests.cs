using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
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
    public IEnumerator FirstConversation_GrantsThreeNails()
    {
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        var tormod = new GameObject("Tormod").AddComponent<TormodInteractable>();

        yield return null;

        tormod.Interact();

        Assert.AreEqual(3, inventory.GetCount(ContentDb.Nails));
    }

    [UnityTest]
    public IEnumerator SecondConversation_DoesNotGrantNailsAgain()
    {
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        var tormod = new GameObject("Tormod").AddComponent<TormodInteractable>();

        yield return null;

        tormod.Interact();
        tormod.Interact();

        Assert.AreEqual(3, inventory.GetCount(ContentDb.Nails));
    }
}
