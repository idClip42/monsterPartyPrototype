using UnityEngine;

#nullable enable

namespace MonsterParty
{
    public interface INoiseSource
    {
        GameObject gameObject { get; }
        float CurrentNoiseRadius { get; }
    }
}