using UnityEngine;
using UnityEditor;

#nullable enable

[DisallowMultipleComponent]
public class SimpleCharacterComponentNoiseLevel : CharacterComponentNoiseLevel
{
    [SerializeField]
    [Range(0, 10)]
    private float _minNoiseSpeed = 1.6f;

    [SerializeField]
    [Range(0, 10)]
    private float _maxNoiseSpeed = 5;

    [SerializeField]
    [Range(0, 100)]
    private float _minNoiseDistance = 1;

    [SerializeField]
    [Range(0, 100)]
    private float _maxNoiseDistance = 20;

    private float _currentNoiseRadius;

    public sealed override float CurrentNoiseRadius { get{
        if (this.Character == null)
            return 0;
        if(this.Character.Alive == false) 
            return 0;
        return _currentNoiseRadius;
    }}

    protected sealed override void Awake()
    {
        base.Awake();

        if(_minNoiseSpeed > _maxNoiseSpeed)
            throw new MonsterPartyException($"Invalid values for min and max noise speed");
        if(_minNoiseDistance > _maxNoiseDistance)
            throw new MonsterPartyException($"Invalid values for min and max noise distance");
    }

    void Update()
    {
        if (this.Character == null)
            throw new MonsterPartyException($"Null character on {this.gameObject.name}");

        float speed = this.Character.CurrentVelocity.magnitude;

        if (speed < _minNoiseSpeed)
        {
            this._currentNoiseRadius = 0; // No noise if below min speed
        }
        else
        {
            float noisePerc = Mathf.InverseLerp(_minNoiseSpeed, _maxNoiseSpeed, speed);
            this._currentNoiseRadius = Mathf.Lerp(_minNoiseDistance, _maxNoiseDistance, noisePerc);
        }
    }

    void OnDrawGizmosSelected()
    {
        using(new Handles.DrawingScope(Color.cyan)){
            Handles.DrawWireDisc(
                transform.position,
                Vector3.up,
                _minNoiseDistance
            );
            Handles.DrawWireDisc(
                transform.position,
                Vector3.up,
                _maxNoiseDistance
            );
        }
    }
}