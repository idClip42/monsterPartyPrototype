using UnityEditor;
using UnityEngine;

#nullable enable

namespace MonsterParty
{
    public static class NoiseUtilities
    {
#if UNITY_EDITOR
        public static void NoiseSourceGizmos(INoiseSource source)
        {
            const float ARC_DEGREES = 80;
            const int INTERVAL = 5;
            using (new Handles.DrawingScope(Color.cyan))
            {
                if (source.CurrentNoiseRadius > 0)
                {
                    Vector3 origin = source.gameObject.transform.position;
                    Vector3[] directions = new Vector3[]{
                    Vector3.forward,
                    Vector3.right,
                    Vector3.back,
                    Vector3.left
                };
                    foreach (var dir in directions)
                    {
                        for (int i = INTERVAL; i < source.CurrentNoiseRadius; i += INTERVAL)
                        {
                            Handles.DrawWireArc(
                                origin,
                                Vector3.up,
                                Quaternion.Euler(0, -ARC_DEGREES / 2, 0) * dir,
                                ARC_DEGREES,
                                i
                            );
                        }
                        Handles.DrawWireArc(
                            origin,
                            Vector3.up,
                            Quaternion.Euler(0, -ARC_DEGREES / 2, 0) * dir,
                            ARC_DEGREES,
                            source.CurrentNoiseRadius
                        );
                    }
                }
            }
        }
#endif
    }
}