using UnityEngine;

#nullable enable

public interface INoiseSource
{
    GameObject gameObject { get; }
    float CurrentNoiseRadius { get; }
}