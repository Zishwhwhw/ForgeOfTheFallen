using UnityEngine;

namespace FotF.Data
{
    /// <summary>
    /// Отклик боя (GDD §10.2). Числа — из Balance/gamefeel.json (gamefeel.default) и constants.json.
    /// </summary>
    [CreateAssetMenu(menuName = "FotF/Data/Game Feel", fileName = "gamefeel_")]
    public sealed class GameFeelDefinition : ScriptableObject
    {
        public string id = "gamefeel.default";

        [Header("Попадание и убийство")]
        [Tooltip("Белая вспышка врага при попадании, с")]
        public float enemyHitFlashSeconds = 0.05f;
        [Tooltip("Микропауза на убийстве, с")]
        public float killMicroPauseSeconds = 0.025f;

        [Header("Смерть врага (решение 27.11): мерцание → сплющивание → труп")]
        [Tooltip("Белое мерцание, с")]
        public float deathFlashSeconds = 0.15f;
        [Tooltip("Сплющивание, с")]
        public float deathSquashSeconds = 0.15f;
        [Tooltip("Высота трупа от исходной")]
        public float deathSquashScaleY = 0.2f;
        [Tooltip("Ширина трупа от исходной")]
        public float deathSquashScaleX = 1.3f;
        [Tooltip("Насколько труп окрашен в цвет затемнения, 0..1")]
        [Range(0f, 1f)] public float corpseDarken = 0.5f;
        [Tooltip("Цвет затемнения трупа (Endesga 32 #262b44)")]
        public Color corpseTint = new Color32(0x26, 0x2b, 0x44, 0xff);
        [Tooltip("Общий лимит трупов на этаж (constants.corpseLimitPerFloor)")]
        public int corpseLimitPerFloor = 300;

        [Header("Смерть босса")]
        public float bossLieSeconds = 1.5f;
        public float bossDissolveSeconds = 1.0f;
    }
}
