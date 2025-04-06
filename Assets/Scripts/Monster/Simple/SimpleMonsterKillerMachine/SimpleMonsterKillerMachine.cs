using System.Linq;
using UnityEngine;

#nullable enable

public class SimpleMonsterKillerMachine : MonoBehaviour {
    [SerializeField]
    private SimpleMonsterKillerMachineReceptacle[] _receptacles = {};

    [SerializeField]
    private GameObject[] _thingsToTurnOn = {};

    private void Awake()
    {
        if(_receptacles.Length == 0)
            Debug.LogWarning($"No receptacles on {gameObject.name}.");

        foreach(var thing in _thingsToTurnOn)
            thing.SetActive(false);
    }

    private void Update()
    {
        bool isOn = IsReady();
        foreach(var thing in _thingsToTurnOn)
            thing.SetActive(isOn);
    }

#if UNITY_EDITOR
    public void ForceFillAllReceptacles(){
        foreach(var thing in _receptacles)
            thing.ForceLockInTarget();
    }
#endif

    public bool IsReady(){
        return _receptacles.All(r=>r.HasComponent);
    }
}