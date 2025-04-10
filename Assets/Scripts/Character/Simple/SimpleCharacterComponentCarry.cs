using UnityEngine;

#nullable enable

[RequireComponent(typeof(SimpleCharacter))]
public class SimpleCharacterComponentCarry : CharacterComponentCarry
{
    [SerializeField]
    private Transform? _carryParent;

    protected override Transform CarryParent { get {
        if(_carryParent == null)
            throw new System.Exception($"Null carry parent on {this.gameObject.name}");
        return _carryParent;
    }}

    protected override void Awake()
    {
        base.Awake();
        
        if(_carryParent == null)
            throw new System.Exception($"Null carry parent on {this.gameObject.name}");
    }
}
