using UnityEngine;

namespace FotF.Data
{
    /// <summary>Проклятие святилища (GDD §7.8): бонус + штраф.</summary>
    [CreateAssetMenu(menuName = "FotF/Data/Curse", fileName = "curse_")]
    public sealed class CurseDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
        public string displayName;
        public EffectSpec bonus = new EffectSpec();
        public EffectSpec penalty = new EffectSpec();
        public Sprite icon;
    }
}
