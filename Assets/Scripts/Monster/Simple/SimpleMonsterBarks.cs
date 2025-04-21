using UnityEngine;

# nullable enable

namespace MonsterParty
{
    public class SimpleMonsterBarks
    {
        [System.Serializable]
        public class Config
        {
            public AudioSource? audioSource;
            public AudioClip? onSeeTarget;
            public AudioClip? onHearTarget;
            public AudioClip? onLoseTarget;
            public AudioClip? onReturnToWander;
            public AudioClip? onDeath;
        }

        private Config _config;

        public SimpleMonsterBarks(Config config)
        {
            _config = config;
        }

        public void PlayOnSeeTarget()
        {
            if (_config.onSeeTarget == null)
            {
                Debug.LogWarning("Missing audio clip");
                return;
            }
            PlayClip(_config.onSeeTarget);
        }

        public void PlayOnHearTarget()
        {
            if (_config.onHearTarget == null)
            {
                Debug.LogWarning("Missing audio clip");
                return;
            }
            PlayClip(_config.onHearTarget);
        }

        public void PlayOnLoseTarget()
        {
            if (_config.onLoseTarget == null)
            {
                Debug.LogWarning("Missing audio clip");
                return;
            }
            PlayClip(_config.onLoseTarget);
        }

        public void PlayOnReturnToWander()
        {
            if (_config.onReturnToWander == null)
            {
                Debug.LogWarning("Missing audio clip");
                return;
            }
            PlayClip(_config.onReturnToWander);
        }

        public void PlayOnDeath()
        {
            if (_config.onDeath == null)
            {
                Debug.LogWarning("Missing audio clip");
                return;
            }
            PlayClip(_config.onDeath);
        }

        private void PlayClip(AudioClip clip)
        {
            if (_config.audioSource == null)
            {
                Debug.LogWarning("Missing audio source");
                return;
            }
            _config.audioSource.Stop();
            _config.audioSource.clip = clip;
            _config.audioSource.Play();
        }
    }
}