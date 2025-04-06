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
        bool isOn = _receptacles.All(r=>r.HasComponent);
        foreach(var thing in _thingsToTurnOn)
            thing.SetActive(isOn);
    }
}