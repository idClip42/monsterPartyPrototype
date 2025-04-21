using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace MonsterParty
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(AudioSource))]
    public class RigidbodyNoiseSource : MonoBehaviour, INoiseSource
    {
        private struct CollisionRecord
        {
            public Vector3 position;
            public float collisionForce;
        }

        [SerializeField]
        [Range(0, 10)]
        private float _minForceForNoise = 1;

        [SerializeField]
        [Range(0.1f, 10)]
        private float _noiseRadiusFromForceMultiplier = 2;

        [SerializeField]
        [Range(0.1f, 60)]
        private float _maxNoiseDistance = 20;

        [SerializeField]
        [Range(1, 20)]
        private int _maxCollisionRecords = 10;

        private AudioSource? _audioSource;

        private float _currentNoiseRadius = 0;

        public float CurrentNoiseRadius => _currentNoiseRadius;

        private readonly List<CollisionRecord> _collisionRecords = new List<CollisionRecord>();

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                Debug.LogWarning($"Missing audio source on {gameObject.name}");
        }

        private void FixedUpdate()
        {
            // This occurs before OnCollisionEnter(),
            // which in turn occurs before Update()
            _currentNoiseRadius = 0;
        }

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

                _collisionRecords.Add(new CollisionRecord
                {
                    position = collision.GetContact(0).point,
                    collisionForce = collisionForce
                });
                while (_collisionRecords.Count > this._maxCollisionRecords)
                    _collisionRecords.RemoveAt(0);


                if (_audioSource != null)
                {
                    _audioSource.Stop();
                    _audioSource.volume = noiseRadius / _maxNoiseDistance;
                    _audioSource.Play();
                }

                Debug.Log($"RigidbodyNoiseSource '{gameObject.name}' made a noise that could be heard from up to {noiseRadius}m away.");
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            NoiseUtilities.NoiseSourceGizmos(this);

            foreach (var col in _collisionRecords)
            {
                Handles.Label(
                    col.position,
                    $"{col.collisionForce:F2} m/s"
                );
            }
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
}