using UnityEngine;
using UnityEditor;

#nullable enable

namespace MonsterParty
{
    [DisallowMultipleComponent]
    public class CharacterLookRaycastTarget : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            using (new Handles.DrawingScope(Color.magenta))
            {
                Handles.DrawWireCube(
                    transform.position,
                    Vector3.one * 0.1f
                );
            }
        }
#endif
    }
}