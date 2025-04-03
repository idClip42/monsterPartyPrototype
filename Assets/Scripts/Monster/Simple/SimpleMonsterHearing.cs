using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

# nullable enable

public class SimpleMonsterHearing{
    [System.Serializable]
    public class Config{
        [SerializeField]
        public LayerMask soundBlockerRaycastMask = Physics.AllLayers;
    }

    private SimpleMonster _monster;
    private INoiseSource[] _noiseSources = { };

    private List<SoundInfo> _reusableChecksList = new List<SoundInfo>();
    private NavMeshPath _reusableNavmeshPath = new NavMeshPath();
    private NavigationManager _navManager;
    private Config _config;

    public SimpleMonsterHearing(SimpleMonster monster, INoiseSource[] noiseSources, NavigationManager navManager, Config config){
        this._monster = monster;

        this._noiseSources = noiseSources;
        if(this._noiseSources.Length == 0)
            Debug.LogWarning("SimpleMonsterHearing has no noise sources.");

        this._navManager = navManager;
        this._config = config;
    }

    public SoundInfo[] CheckForSounds(){
        // We're reusing the checks list
        // so we don't make a new one
        // every single time.
        _reusableChecksList.Clear();

        Vector3 myPos = this._monster.transform.position;
        foreach(var noiseSource in this._noiseSources){
            if(noiseSource == null)
                throw new System.Exception("Noise source is null.");

            if(noiseSource.CurrentNoiseRadius <= 0){
                // No point in doing anything for 0-level noise-makers
                continue;
            }

            Vector3 charPos = noiseSource.gameObject.transform.position;

            // We first filter out anything whose linear
            // distance is greater than the max distance,
            // because if that's the case then our more
            // complex distance check will definitely fail.
            float sqrDist = (charPos - myPos).sqrMagnitude;
            float noiseDist = noiseSource.CurrentNoiseRadius;
            if(sqrDist > (noiseDist * noiseDist)) continue;

            // We calculate a walking path to the target.
            // This is the path the sound will have to
            // travel along, and therefore it gives us a
            // better sense of whether it's audible or not.
            bool hasPath = NavMesh.CalculatePath(
                myPos, 
                charPos,
                new NavMeshQueryFilter(){
                    areaMask=NavMesh.AllAreas, 
                    agentTypeID=_navManager.CrouchingAgentTypeId
                },
                _reusableNavmeshPath
            );
            // (If there's no path, the whole thing is moot,
            // but this should never happen. Probably.)
            if(!hasPath) {
                Debug.LogWarning($"Sound logic unable to find path from {this._monster.gameObject.name} to {noiseSource.gameObject.name}");
                continue;
            }
            
            // From here, we get our waypoints
            // and we figure out the distance.
            // TODO: Maybe we move this logic into a SoundInfo() constructor
            // TODO: Maybe we move a lot of stuff into that constructor
            Vector3[] waypoints = _reusableNavmeshPath.corners;
            List<SoundNavBlocker> blockers = new List<SoundNavBlocker>();
            RaycastHit hitInfo;
            float distance = 0;
            for (int i = 1; i < waypoints.Length; i++){
                Vector3 start = waypoints[i - 1];
                Vector3 end = waypoints[i];
                Vector3 diff = end - start;
                float rawDistance = diff.magnitude;

                float penalty = 0;
                if(Physics.Raycast(start, diff, out hitInfo, rawDistance, _config.soundBlockerRaycastMask)){
                    GameObject go = hitInfo.collider.gameObject;
                    SoundNavBlocker? blocker = go.GetComponentInParent<SoundNavBlocker>(false);
                    if(blocker != null){
                        blockers.Add(blocker);
                        penalty = blocker.DistancePenalty;
                    }
                }

                distance += rawDistance + penalty;
            }

            // And then we return our final result.
            _reusableChecksList.Add(new SoundInfo(){
                isAudible = distance < noiseDist,
                soundLocation = charPos,
                pathToSound = waypoints,
                distanceToSound = distance,
                blockers = blockers.ToArray()
            });
        }

        // We convert the list to an array to return it.
        return _reusableChecksList.ToArray();
    }

    public sealed override bool Equals(object other) => base.Equals(other);
    public sealed override int GetHashCode() => base.GetHashCode();
    public sealed override string ToString() => base.ToString();
}