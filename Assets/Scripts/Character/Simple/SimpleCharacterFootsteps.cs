using UnityEditor.PackageManager;
using UnityEngine;

#nullable enable

[RequireComponent(typeof(AudioSource))]
public class SimpleCharacterFootsteps : MonoBehaviour {
    [SerializeField]
    private AudioClip[] _footstepClips = {};

    [SerializeField]
    [Range(0.5f, 5)]
    private float _stepRate = 0.7f;

    [SerializeField]
    [Range(1,10)]
    private float _maxVolumeSpeed = 5;

    private Character? _character;
    private AudioSource? _audioSource;
    private float _stepTimer = 0;
    private int _currentClipIndex = 0;

    void Awake()
    {
        _character = GetComponentInParent<Character>();
        if(_character == null)
            throw new MonsterPartyException("Missing character");

        _audioSource = GetComponent<AudioSource>();
        if(_audioSource == null)
            throw new MonsterPartyException("Missing footsteps audio source");
    }

    void Update()
    {
        if(_character == null)
            throw new MonsterPartyException("Missing character");
        if(_audioSource == null)
            throw new MonsterPartyException("Missing footsteps audio source");

        _stepTimer += Time.deltaTime;

        float speed = this._character.CurrentVelocity.magnitude;

        _audioSource.volume = Mathf.InverseLerp(0, _maxVolumeSpeed, speed);

        float maxTimer = 1 / (speed * _stepRate);
        if(_stepTimer >= maxTimer){
            _audioSource.Stop();

            _audioSource.clip = _footstepClips[_currentClipIndex];
            if(_audioSource.clip == null)
                throw new MonsterPartyException("Null audio clip");
            _audioSource.Play();

            _stepTimer = 0;
            _currentClipIndex = (_currentClipIndex + 1) % _footstepClips.Length;
        }
    }
}