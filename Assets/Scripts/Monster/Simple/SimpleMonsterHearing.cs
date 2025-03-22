using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

# nullable enable

public class SimpleMonsterHearing{
    public struct SoundInfo{
        public bool isAudible;
        public Vector3 soundLocation;
        public Vector3[] pathToSound;
        public float distanceToSound;
    }

    private SimpleMonster _monster;
    private CharacterNoiseLevel[] _characters = { };

    private List<SoundInfo> _reusableChecksList = new List<SoundInfo>();
    private NavMeshPath _reusableNavmeshPath = new NavMeshPath();
    private NavigationManager _navManager;

    public SimpleMonsterHearing(SimpleMonster monster, Character[] characters, NavigationManager navManager){
        this._monster = monster;

        this._characters = characters.Select(
            c=>c.gameObject.GetComponent<CharacterNoiseLevel>()
        ).ToArray();
        if(this._characters.Length == 0)
            Debug.LogWarning("SimpleMonsterHearing has no characters.");
        if(this._characters.Any(c=>c==null))
            throw new System.Exception("At least one of our characters is missing a noise component");

        this._navManager = navManager;
    }

    public SoundInfo[] CheckForSounds(){
        // We're reusing the checks list
        // so we don't make a new one
        // every single time.
        _reusableChecksList.Clear();

        Vector3 myPos = this._monster.transform.position;
        foreach(var character in this._characters){
            Vector3 charPos = character.transform.position;

            // We first filter out anything whose linear
            // distance is greater than the max distance,
            // because if that's the case then our more
            // complex distance check will definitely fail.
            float sqrDist = (charPos - myPos).sqrMagnitude;
            float noiseDist = character.CurrentNoiseRadius;
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
                Debug.LogWarning($"Sound logic unable to find path from {this._monster.gameObject.name} to {character.gameObject.name}");
                continue;
            }
            
            // From here, we get our waypoints
            // and we figure out the distance.
            Vector3[] waypoints = _reusableNavmeshPath.corners;
            float distance = 0;
            for (int i = 1; i < waypoints.Length; i++)
                distance += Vector3.Distance(waypoints[i - 1], waypoints[i]);

            // And then we return our final result.
            _reusableChecksList.Add(new SoundInfo(){
                isAudible = distance < noiseDist,
                soundLocation = charPos,
                pathToSound = waypoints,
                distanceToSound = distance
            });
        }

        // We convert the list to an array to return it.
        return _reusableChecksList.ToArray();
    }
}