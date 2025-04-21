using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#nullable enable

namespace MonsterParty
{
    [DisallowMultipleComponent]
    public abstract class CharacterComponentNoiseLevel : CharacterComponent, INoiseSource
    {
        public abstract float CurrentNoiseRadius { get; }

        public sealed override string DebugHeader => "Noise";

        public override void FillInDebugInfo(Dictionary<string, string> infoTarget)
        {
            infoTarget["Noise Radius"] = $"{CurrentNoiseRadius:F2}m";
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            NoiseUtilities.NoiseSourceGizmos(this);
        }
#endif
    }
}