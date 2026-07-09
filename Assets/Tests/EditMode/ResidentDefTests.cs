using Lamplight.TestSupport.Fakes;
using NUnit.Framework;
using UnityEngine;

public class ResidentDefTests
{
    private ResidentDef CreateBerta()
    {
        return new ResidentDef(
            "berta", "Berta", "Bakery",
            new Color(0.85f, 0.65f, 0.45f),
            new Color(0.85f, 0.65f, 0.45f),
            new ScheduleEntry[] { new ScheduleEntry(8, "Home") },
            new string[][]
            {
                new string[] { "Low1", "Low2", "Low3" },
                new string[] { "Mid1", "Mid2" },
                new string[] { "High1", "High2" },
            },
            "MoveIn line"
        );
    }

    [Test]
    public void GetDialogueLine_Tier0_Below34()
    {
        var berta = CreateBerta();
        var rng = new SeededRng(1);
        string line = berta.GetDialogueLine(0);
        Assert.IsTrue(line == "Low1" || line == "Low2" || line == "Low3");
    }

    [Test]
    public void GetDialogueLine_Tier1_34To66()
    {
        var berta = CreateBerta();
        string line = berta.GetDialogueLine(50);
        Assert.IsTrue(line == "Mid1" || line == "Mid2");
    }

    [Test]
    public void GetDialogueLine_Tier2_67AndAbove()
    {
        var berta = CreateBerta();
        string line = berta.GetDialogueLine(100);
        Assert.IsTrue(line == "High1" || line == "High2");
    }

    [Test]
    public void GetDialogueLine_Boundary_33_IsTier0()
    {
        var berta = CreateBerta();
        string line = berta.GetDialogueLine(33);
        Assert.IsTrue(line == "Low1" || line == "Low2" || line == "Low3");
    }

    [Test]
    public void GetDialogueLine_Boundary_34_IsTier1()
    {
        var berta = CreateBerta();
        string line = berta.GetDialogueLine(34);
        Assert.IsTrue(line == "Mid1" || line == "Mid2");
    }

    [Test]
    public void GetDialogueLine_Boundary_66_IsTier1()
    {
        var berta = CreateBerta();
        string line = berta.GetDialogueLine(66);
        Assert.IsTrue(line == "Mid1" || line == "Mid2");
    }

    [Test]
    public void GetDialogueLine_Boundary_67_IsTier2()
    {
        var berta = CreateBerta();
        string line = berta.GetDialogueLine(67);
        Assert.IsTrue(line == "High1" || line == "High2");
    }

    [Test]
    public void GetDialogueLine_WithSeededRng_IsDeterministic()
    {
        var berta = CreateBerta();
        UnityEngine.Random.InitState(42);
        string line1 = berta.GetDialogueLine(10);
        UnityEngine.Random.InitState(42);
        string line2 = berta.GetDialogueLine(10);
        Assert.AreEqual(line1, line2);
    }
}
