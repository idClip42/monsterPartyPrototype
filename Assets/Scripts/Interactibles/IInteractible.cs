using UnityEngine;

#nullable enable

namespace MonsterParty
{
    public interface IInteractible
    {
        GameObject gameObject { get; }
        Vector3 InteractionWorldPosition { get; }
        bool IsInteractible(Character interactor);
        string GetInteractionName(Character interactor);
        void DoInteraction(Character interactor);
    }
}