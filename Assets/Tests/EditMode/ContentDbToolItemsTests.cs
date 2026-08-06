using NUnit.Framework;

public class ContentDbToolItemsTests
{
    [Test]
    public void Pickaxe_IsNotIngredient()
    {
        Assert.IsFalse(ContentDb.Pickaxe.isIngredient);
    }

    [Test]
    public void HandAxe_IsNotIngredient()
    {
        Assert.IsFalse(ContentDb.HandAxe.isIngredient);
    }
}
