using UnityEngine;

namespace FotF.Data
{
    /// <summary>Все статусы стихий среза (GDD §7.3) — для врагов, которые их получают.</summary>
    [CreateAssetMenu(menuName = "FotF/Data/Status Catalog", fileName = "status_catalog")]
    public sealed class StatusCatalog : ScriptableObject
    {
        public StatusDefinition[] statuses = System.Array.Empty<StatusDefinition>();
    }
}
