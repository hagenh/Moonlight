using System.Collections.Generic;

public class DirectionalAnimationSet
{
    public Dictionary<string, DirectionalClip> clips = new();
    public string defaultClip = "idle";

    public DirectionalClip GetClip(string name)
    {
        return clips.GetValueOrDefault(name);
    }

    public void AddClip(string name, DirectionalClip clip)
    {
        clips[name] = clip;
    }
}
