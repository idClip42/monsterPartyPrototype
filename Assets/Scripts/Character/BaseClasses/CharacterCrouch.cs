using System.Collections.Generic;
using UnityEngine;

#nullable enable

[DisallowMultipleComponent]
public abstract class CharacterCrouch : MonoBehaviour, IDebugInfoProvider
{
    private Character? _characterBase = null;

    private bool _isCrouching = false;
    public bool IsCrouching => _isCrouching;
    private bool _canUncrouch = false;

    public string DebugHeader => "Crouch";

    public virtual void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        infoTarget["State"] = _isCrouching ? "Crouching" : "Standing";
        if(_isCrouching)
            infoTarget["Can Stand"] = _canUncrouch.ToString();
    }

    public delegate void CrouchToggleHandler(bool isCrouching);
    public CrouchToggleHandler? OnCrouchToggle;

    protected virtual void Awake()
    {
        _characterBase = GetComponent<Character>();
        if(_characterBase == null)
            throw new System.Exception($"Null character base on {this.gameObject.name}");
    }

    private void Update()
    {
        if(_characterBase == null) throw new System.Exception("Null _characterBase");
        
        _canUncrouch = CanUncrouch();
        
        if(_characterBase.State == Character.StateType.Player){
            if(Input.GetButtonDown("Crouch")){
                ToggleCrouch();
            }
        }
    }

    private void ToggleCrouch(){
        if(this._isCrouching && !_canUncrouch)
            return;

        this._isCrouching = !this._isCrouching; 
        if(this._isCrouching) 
            EnableCrouch();
        else 
            DisableCrouch();
        OnCrouchToggle?.Invoke(this._isCrouching);
    }

    protected abstract void EnableCrouch();
    protected abstract void DisableCrouch();

    protected abstract bool CanUncrouch();
}
