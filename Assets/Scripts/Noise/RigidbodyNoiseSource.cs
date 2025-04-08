using UnityEditor;
using UnityEngine;

#nullable enable

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyNoiseSource : MonoBehaviour, INoiseSource
{
    [SerializeField]
    [Range(0, 10)]
    private float _minForceForNoise = 1;

    [SerializeField]
    [Range(0.1f, 10)]
    private float _noiseRadiusFromForceMultiplier = 1;

    [SerializeField]
    [Range(0.1f, 60)]
    private float _maxNoiseDistance = 10;

    private float _currentNoiseRadius = 0;

    public float CurrentNoiseRadius => _currentNoiseRadius;

    private void OnCollisionEnter(Collision collision)
    {
        // Calculate collision force based purely on the relative velocity between the objects
        float collisionForce = collision.relativeVelocity.magnitude;

        // Check if the collision force exceeds the threshold for noise
        if (collisionForce >= _minForceForNoise)
        {
            // Calculate the noise radius based on the collision force
            float noiseRadius = Mathf.Clamp(collisionForce * _noiseRadiusFromForceMultiplier, 0, _maxNoiseDistance);

            // Trigger the noise event (this is where you would notify your noise system)
            _currentNoiseRadius = noiseRadius;
        }
        else {
            _currentNoiseRadius = 0;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        NoiseUtilities.NoiseSourceGizmos(this);
    }

    void OnDrawGizmosSelected()
    {
        using (new Handles.DrawingScope(Color.cyan))
        {
            Handles.DrawWireDisc(
                transform.position,
                Vector3.up,
                _minForceForNoise * _noiseRadiusFromForceMultiplier
            );
            Handles.DrawWireDisc(
                transform.position,
                Vector3.up,
                _maxNoiseDistance
            );
        }
    }
#endif
}