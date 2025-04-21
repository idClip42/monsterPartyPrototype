using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

# nullable enable

namespace MonsterParty
{
    public class SimpleMonsterHearing
    {
        [System.Serializable]
        public class Config
        {
            [SerializeField]
            public LayerMask soundBlockerRaycastMask = Physics.AllLayers;
        }

        private SimpleMonster _monster;
        private INoiseSource[] _noiseSources = { };

        private List<SoundInfo> _reusableChecksList = new List<SoundInfo>();
        private NavMeshPath _reusableNavmeshPath = new NavMeshPath();
        private NavigationManager _navManager;
        private Config _config;

        public SimpleMonsterHearing(SimpleMonster monster, INoiseSource[] noiseSources, NavigationManager navManager, Config config)
        {
            this._monster = monster;

            this._noiseSources = noiseSources;
            if (this._noiseSources.Length == 0)
                Debug.LogWarning("SimpleMonsterHearing has no noise sources.");

            this._navManager = navManager;
            this._config = config;
        }

        public SoundInfo[] CheckForSounds()
        {
            // We're reusing the checks list
            // so we don't make a new one
            // every single time.
            _reusableChecksList.Clear();

            const float SAMPLE_DIST = 5.0f;
            NavMeshHit hit;

            Vector3 baseMonsterPos = this._monster.transform.position;
            bool foundMonsterNavPos = NavMesh.SamplePosition(
                baseMonsterPos,
                out hit,
                SAMPLE_DIST,
                NavMesh.AllAreas
            );
            if(foundMonsterNavPos == false){
                Debug.LogWarning($"Unable to find Nav Mesh position for {this._monster.gameObject.name} at {baseMonsterPos}");
                return _reusableChecksList.ToArray();
            }
            Vector3 myPos = hit.position;

            foreach (var noiseSource in this._noiseSources)
            {
                if (noiseSource == null)
                    throw new MonsterPartyNullReferenceException("noiseSource");

                if (noiseSource.CurrentNoiseRadius <= 0)
                {
                    // No point in doing anything for 0-level noise-makers
                    continue;
                }

                Vector3 baseNoisePos = noiseSource.gameObject.transform.position;
                bool foundNoiseNavPos = NavMesh.SamplePosition(
                    baseNoisePos,
                    out hit,
                    SAMPLE_DIST,
                    NavMesh.AllAreas
                );
                if(foundNoiseNavPos == false){
                    Debug.LogWarning($"Unable to find Nav Mesh position for {noiseSource.gameObject.name} at {baseNoisePos}");
                    continue;
                }
                Vector3 noisePos = hit.position;

                // We first filter out anything whose linear
                // distance is greater than the max distance,
                // because if that's the case then our more
                // complex distance check will definitely fail.
                float sqrDist = (noisePos - myPos).sqrMagnitude;
                float noiseDist = noiseSource.CurrentNoiseRadius;
                if (sqrDist > (noiseDist * noiseDist)) continue;

                // We calculate a walking path to the target.
                // This is the path the sound will have to
                // travel along, and therefore it gives us a
                // better sense of whether it's audible or not.
                bool hasPath = NavMesh.CalculatePath(
                    myPos,
                    noisePos,
                    new NavMeshQueryFilter()
                    {
                        areaMask = NavMesh.AllAreas,
                        agentTypeID = _navManager.GetCrouchingAgentTypeId()
                    },
                    _reusableNavmeshPath
                );
                // (If there's no path, the whole thing is moot,
                // but this should never happen. Probably.)
                if (!hasPath)
                {
                    Debug.LogWarning($"Sound logic unable to find path from {this._monster.gameObject.name} to {noiseSource.gameObject.name}");
                    continue;
                }

                // And then we calculate our final result
                _reusableChecksList.Add(new SoundInfo(
                    noiseSource,
                    _reusableNavmeshPath,
                    _config.soundBlockerRaycastMask
                ));
            }

            // We convert the list to an array to return it.
            return _reusableChecksList.ToArray();
        }

        public sealed override bool Equals(object other) => base.Equals(other);
        public sealed override int GetHashCode() => base.GetHashCode();
        public sealed override string ToString() => base.ToString();
    }
}