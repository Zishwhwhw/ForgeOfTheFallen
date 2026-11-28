using System;
using UnityEngine;

namespace FotF.Data
{
    /// <summary>Отклик боя (GDD §10.2). Числа — из Balance/gamefeel.json.</summary>
    [CreateAssetMenu(menuName = "FotF/Data/Game Feel", fileName = "gamefeel_")]
    public sealed class GameFeelDefinition : ScriptableObject
    {
        /// <summary>Смерть (решение 27.11): мобы — мерцание → сплющивание → труп; босс — лежит → растворяется.</summary>
        [Serializable]
        public sealed class Death
        {
            [Tooltip("Белое мерцание, с")]
            public float flashSeconds = 0.15f;
            [Tooltip("Сплющивание, с")]
            public float squashSeconds = 0.15f;
            [Tooltip("Высота трупа от исходной")]
            public float squashScaleY = 0.2f;
            [Tooltip("Ширина трупа от исходной")]
            public float squashScaleX = 1.3f;
            [Range(0f, 1f), Tooltip("Насколько труп окрашен в цвет затемнения")]
            public float corpseDarken = 0.5f;
            public float bossLieSeconds = 1.5f;
            public float bossDissolveSeconds = 1f;
        }

        public string id = "gamefeel.default";
        [Tooltip("Белая вспышка врага при попадании, с")]
        public float enemyHitFlashSeconds = 0.05f;
        [Tooltip("Микропауза на убийстве, с")]
        public float killMicroPauseSeconds = 0.025f;
        [Tooltip("Hit-stop при сильном ударе, с (калибровать)")]
        public float hitStopSeconds;
        public Death death = new Death();

        [Header("Вид (не баланс)")]
        [Tooltip("Цвет затемнения трупа (Endesga 32 #262b44)")]
        public Color corpseTint = new Color32(0x26, 0x2b, 0x44, 0xff);
    }
}
