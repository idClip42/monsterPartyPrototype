using UnityEngine;

#nullable enable

public class SimpleMonsterKillerMachine : MonoBehaviour {
    [SerializeField]
    private SimpleMonsterKillerMachineReceptacle[] _receptacles = {};

    void Awake()
    {
        if(_receptacles.Length == 0)
            Debug.LogWarning($"No receptacles on {gameObject.name}.");
    }
}