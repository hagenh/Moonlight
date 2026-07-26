using System.Collections.Generic;
using NUnit.Framework;

public class RequestBookTests
{
    private static readonly ItemDef Shine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);

    private static StandRequest Note(string id) =>
        new StandRequest(id, RequestKind.Exact, new List<ItemDef> { Shine }, 2, "A carter", "Two jars.");

    [Test]
    public void NewBook_StartsEmpty()
    {
        var book = new RequestBook(3);

        Assert.AreEqual(3, book.SlotCount);
        Assert.AreEqual(3, book.FreeSlots);
        Assert.AreEqual(0, book.Active.Count);
    }

    [Test]
    public void TryPost_IntoFreeSlot_Succeeds()
    {
        var book = new RequestBook(3);

        Assert.IsTrue(book.TryPost(Note("a")));
        Assert.AreEqual(2, book.FreeSlots);
    }

    [Test]
    public void TryPost_WhenFull_ReturnsFalse()
    {
        var book = new RequestBook(2);
        book.TryPost(Note("a"));
        book.TryPost(Note("b"));

        Assert.IsFalse(book.TryPost(Note("c")));
        Assert.AreEqual(2, book.Active.Count);
    }

    [Test]
    public void TryPost_DuplicateId_ReturnsFalse()
    {
        var book = new RequestBook(3);
        book.TryPost(Note("a"));

        Assert.IsFalse(book.TryPost(Note("a")));
        Assert.AreEqual(1, book.Active.Count);
    }

    [Test]
    public void TryPost_Null_ReturnsFalse()
    {
        var book = new RequestBook(3);

        Assert.IsFalse(book.TryPost(null));
    }

    [Test]
    public void Take_RemovesAndReturnsRequest()
    {
        var book = new RequestBook(3);
        var note = Note("a");
        book.TryPost(note);

        Assert.AreSame(note, book.Take("a"));
        Assert.AreEqual(0, book.Active.Count);
        Assert.AreEqual(3, book.FreeSlots);
    }

    [Test]
    public void Take_UnknownId_ReturnsNull()
    {
        var book = new RequestBook(3);

        Assert.IsNull(book.Take("nope"));
    }

    [Test]
    public void Take_FreesTheSlotForANewNote()
    {
        var book = new RequestBook(1);
        book.TryPost(Note("a"));
        Assert.IsFalse(book.TryPost(Note("b")));

        book.Take("a");

        Assert.IsTrue(book.TryPost(Note("b")));
    }

    [Test]
    public void SetSlotCount_Grows_AddsFreeSlots()
    {
        var book = new RequestBook(3);
        book.TryPost(Note("a"));

        book.SetSlotCount(5);

        Assert.AreEqual(5, book.SlotCount);
        Assert.AreEqual(4, book.FreeSlots);
    }

    [Test]
    public void SetSlotCount_Shrinking_NeverDiscardsPostedRequests()
    {
        var book = new RequestBook(3);
        book.TryPost(Note("a"));
        book.TryPost(Note("b"));
        book.TryPost(Note("c"));

        book.SetSlotCount(1);

        Assert.AreEqual(3, book.Active.Count);
        Assert.AreEqual(0, book.FreeSlots);
    }
}
