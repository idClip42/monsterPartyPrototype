using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

# nullable enable

public struct SoundInfo
{
    public bool isAudible;
    public Vector3 soundLocation;
    public Vector3[] pathToSound;
    public float distanceToSound;
    public SoundNavBlocker[] blockers;

    public static SoundInfo GetNearest(SoundInfo[] sounds)
    {
        if (sounds.Length == 0)
            throw new System.Exception("Passed in empty sound array.");
        SoundInfo? targetSound = null;
        foreach (SoundInfo sound in sounds)
        {
            if (targetSound == null)
                targetSound = sound;
            if (sound.distanceToSound < targetSound.Value.distanceToSound)
                targetSound = sound;
        }
        if (targetSound == null)
            throw new System.Exception("Target sound should not be null.");
        return targetSound.Value;
    }
}