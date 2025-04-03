using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

# nullable enable

public struct SoundInfo
{
    public readonly bool isAudible;
    public readonly Vector3 soundLocation;
    public readonly Vector3[] pathToSound;
    public readonly float distanceToSound;
    public readonly SoundNavBlocker[] blockers;

    public SoundInfo(INoiseSource noise, NavMeshPath pathToNoise, LayerMask blockerRaycastMask)
    {
        if (noise.CurrentNoiseRadius <= 0)
        {
            Debug.LogWarning("You probably shouldn't be allowing 0-level noise sources in here.");
        }

        // We get our waypoints
        // and we figure out the distance.
        Vector3[] waypoints = pathToNoise.corners;
        List<SoundNavBlocker> tempBlockers = new List<SoundNavBlocker>();
        RaycastHit hitInfo;
        float distance = 0;
        for (int i = 1; i < waypoints.Length; i++)
        {
            Vector3 start = waypoints[i - 1];
            Vector3 end = waypoints[i];
            Vector3 diff = end - start;
            float rawDistance = diff.magnitude;

            float penalty = 0;
            if (Physics.Raycast(start, diff, out hitInfo, rawDistance, blockerRaycastMask))
            {
                GameObject go = hitInfo.collider.gameObject;
                SoundNavBlocker? blocker = go.GetComponentInParent<SoundNavBlocker>(false);
                if (blocker != null)
                {
                    tempBlockers.Add(blocker);
                    penalty = blocker.DistancePenalty;
                }
            }

            distance += rawDistance + penalty;
        }

        isAudible = distance < noise.CurrentNoiseRadius;
        soundLocation = noise.gameObject.transform.position;
        pathToSound = waypoints;
        distanceToSound = distance;
        blockers = tempBlockers.ToArray();
    }

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