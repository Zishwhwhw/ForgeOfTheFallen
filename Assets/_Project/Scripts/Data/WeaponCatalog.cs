using UnityEngine;

namespace FotF.Data
{
    /// <summary>Список всех WeaponDefinition (по id): нужен, чтобы по WeaponInstance найти спрайты базы и детали гибрида.</summary>
    [CreateAssetMenu(menuName = "FotF/Data/Weapon Catalog", fileName = "weapon_catalog")]
    public sealed class WeaponCatalog : ScriptableObject
    {
        public WeaponDefinition[] weapons = System.Array.Empty<WeaponDefinition>();

        public WeaponDefinition Find(string id)
        {
            foreach (var w in weapons) if (w != null && w.id == id) return w;
            return null;
        }
    }
}
