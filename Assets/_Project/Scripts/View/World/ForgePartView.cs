using FotF.View.Heroes;
using UnityEngine;

namespace FotF.View.World
{
    /// <summary>Часть кузницы, с которой герой взаимодействует клавишей E: плита A, плита B или рычаг.</summary>
    public sealed class ForgePartView : MonoBehaviour, IInteractable
    {
        public enum Part { PlateA, PlateB, Lever }
        [SerializeField] ForgeView forge;
        [SerializeField] Part part;

        public bool CanInteract => forge != null && (part == Part.Lever || !forge.Busy);
        public Vector3 Position => transform.position;
        public string Prompt => part switch
        {
            Part.Lever => forge.Busy ? "E — пропустить ритуал" : "E — дёрнуть рычаг (сплав)",
            Part.PlateA => forge.OnA == null ? "E — положить оружие на плиту" : "E — забрать с плиты",
            _ => forge.OnB == null ? "E — положить оружие на плиту" : "E — забрать с плиты",
        };

        public void Interact(HeroController hero)
        {
            if (part == Part.Lever) forge.PullLever(hero);
            else forge.UsePlate(part == Part.PlateA, hero);
        }
    }
}
