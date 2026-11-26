using FotF.View.Heroes;
using UnityEngine;

namespace FotF.View.World
{
    /// <summary>То, с чем герой взаимодействует клавишей E (сундук, кузница, оружие на полу).</summary>
    public interface IInteractable
    {
        bool CanInteract { get; }
        string Prompt { get; }
        Vector3 Position { get; }
        void Interact(HeroController hero);
    }
}
