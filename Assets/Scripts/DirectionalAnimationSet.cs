using System.Collections.Generic;

[System.Serializable]
public class NamedClip
{
    public string name;
    public DirectionalClip clip;
}

[System.Serializable]
public class DirectionalAnimationSet
{
    public List<NamedClip> clips = new();
    public string defaultClip = "idle";

    public DirectionalClip GetClip(string name)
    {
        foreach (var entry in clips)
            if (entry.name == name) return entry.clip;
        return null;
    }

    public void AddClip(string name, DirectionalClip clip)
    {
        clips.Add(new NamedClip { name = name, clip = clip });
    }
}
