using NUnit.Framework;
using UnityEngine;

public class FacingMathTests
{
    [Test]
    public void FromVector_Right()
    {
        Assert.AreEqual(FacingDirection.Right, FacingMath.FromVector(new Vector2(1, 0)));
    }

    [Test]
    public void FromVector_Up()
    {
        Assert.AreEqual(FacingDirection.Up, FacingMath.FromVector(new Vector2(0, 1)));
    }

    [Test]
    public void FromVector_Left()
    {
        Assert.AreEqual(FacingDirection.Left, FacingMath.FromVector(new Vector2(-1, 0)));
    }

    [Test]
    public void FromVector_Down()
    {
        Assert.AreEqual(FacingDirection.Down, FacingMath.FromVector(new Vector2(0, -1)));
    }

    [Test]
    public void FromVector_Diagonal_UpRight()
    {
        Assert.AreEqual(FacingDirection.Up, FacingMath.FromVector(new Vector2(1, 1)));
    }

    [Test]
    public void FromVector_Diagonal_DownLeft()
    {
        Assert.AreEqual(FacingDirection.Down, FacingMath.FromVector(new Vector2(-1, -1)));
    }

    [Test]
    public void GetFacingOffset_Down()
    {
        Assert.AreEqual(new Vector2(0, -0.5f), FacingMath.GetFacingOffset(FacingDirection.Down));
    }

    [Test]
    public void GetFacingOffset_Up()
    {
        Assert.AreEqual(new Vector2(0, 0.5f), FacingMath.GetFacingOffset(FacingDirection.Up));
    }

    [Test]
    public void GetFacingOffset_Left()
    {
        Assert.AreEqual(new Vector2(-0.5f, 0), FacingMath.GetFacingOffset(FacingDirection.Left));
    }

    [Test]
    public void GetFacingOffset_Right()
    {
        Assert.AreEqual(new Vector2(0.5f, 0), FacingMath.GetFacingOffset(FacingDirection.Right));
    }
}
