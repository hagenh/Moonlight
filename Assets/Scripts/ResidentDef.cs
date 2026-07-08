using UnityEngine;

public class ResidentDef
{
    public string id;
    public string displayName;
    public string buildingId;
    public Color spriteColor;
    public Color portraitColor;
    public ScheduleEntry[] schedule;
    public string[][] dialogueLines;
    public string moveInLine;

    public ResidentDef(
        string id,
        string displayName,
        string buildingId,
        Color spriteColor,
        Color portraitColor,
        ScheduleEntry[] schedule,
        string[][] dialogueLines,
        string moveInLine)
    {
        this.id = id;
        this.displayName = displayName;
        this.buildingId = buildingId;
        this.spriteColor = spriteColor;
        this.portraitColor = portraitColor;
        this.schedule = schedule;
        this.dialogueLines = dialogueLines;
        this.moveInLine = moveInLine;
    }

    public string GetDialogueLine(int reputation)
    {
        int tier = reputation < 34 ? 0 : reputation < 67 ? 1 : 2;
        var pool = dialogueLines[tier];
        if (pool == null || pool.Length == 0) return "...";
        return pool[Random.Range(0, pool.Length)];
    }
}

[System.Serializable]
public struct ScheduleEntry
{
    public int hour;
    public string markerName;

    public ScheduleEntry(int hour, string markerName)
    {
        this.hour = hour;
        this.markerName = markerName;
    }
}
