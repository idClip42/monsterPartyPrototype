using UnityEngine;

namespace MonsterParty
{
    public class SoundNavBlocker : MonoBehaviour
    {
        [SerializeField]
        [Range(0.1f, 30)]
        private float _distancePenalty = 5;

        public float DistancePenalty => _distancePenalty;
    }
}