using NUnit.Framework;

public class BuildBookTests
{
    private static readonly ItemDef Lamppost = new ItemDef("lamppost", "Lamppost", isPlaceable: true);
    private static readonly ItemDef Bench = new ItemDef("bench", "Bench", isPlaceable: true, footprintWidth: 2);

    [Test]
    public void NewBook_HasNoEntries()
    {
        var book = new BuildBook();

        Assert.AreEqual(0, book.Entries.Count);
    }

    [Test]
    public void Add_NewItem_CreatesEntry()
    {
        var book = new BuildBook();

        book.Add(Lamppost, 5);

        Assert.AreEqual(1, book.Entries.Count);
        Assert.AreEqual(Lamppost, book.Entries[0].Item);
        Assert.AreEqual(5, book.Entries[0].Available);
    }

    [Test]
    public void Add_ExistingItem_IncreasesAvailable()
    {
        var book = new BuildBook();
        book.Add(Lamppost, 5);

        book.Add(Lamppost, 3);

        Assert.AreEqual(1, book.Entries.Count);
        Assert.AreEqual(8, book.Available(Lamppost));
    }

    [Test]
    public void Add_ZeroOrNegative_NoOp()
    {
        var book = new BuildBook();

        book.Add(Lamppost, 0);
        book.Add(Lamppost, -1);

        Assert.AreEqual(0, book.Entries.Count);
    }

    [Test]
    public void Available_UnknownItem_ReturnsZero()
    {
        var book = new BuildBook();

        Assert.AreEqual(0, book.Available(Bench));
    }

    [Test]
    public void TryConsume_WithStock_DecrementsAndReturnsTrue()
    {
        var book = new BuildBook();
        book.Add(Lamppost, 2);

        Assert.IsTrue(book.TryConsume(Lamppost));
        Assert.AreEqual(1, book.Available(Lamppost));
    }

    [Test]
    public void TryConsume_AtZero_ReturnsFalse()
    {
        var book = new BuildBook();
        book.Add(Lamppost, 1);
        book.TryConsume(Lamppost);

        Assert.IsFalse(book.TryConsume(Lamppost));
        Assert.AreEqual(0, book.Available(Lamppost));
    }

    [Test]
    public void TryConsume_UnknownItem_ReturnsFalse()
    {
        var book = new BuildBook();

        Assert.IsFalse(book.TryConsume(Bench));
    }
}
