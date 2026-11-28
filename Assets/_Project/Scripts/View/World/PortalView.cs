using System;
using FotF.View.Heroes;
using UnityEngine;

namespace FotF.View.World
{
    /// <summary>Портал на следующий этаж (GDD §3.1): появляется в последней зачищенной боевой комнате.</summary>
    public sealed class PortalView : MonoBehaviour, IInteractable
    {
        public event Action<HeroController> Used;
        bool _used;

        public bool CanInteract => !_used;
        public string Prompt => "E — спуститься глубже";
        public Vector3 Position => transform.position;

        public void Interact(HeroController hero)
        {
            if (_used) return;
            _used = true;
            Used?.Invoke(hero);
        }
    }
}
