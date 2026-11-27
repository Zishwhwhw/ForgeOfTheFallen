using System;
using UnityEngine;

namespace FotF.View.Animation
{
    /// <summary>
    /// Набор покадровых анимаций персонажа в 3 направлениях (вниз / вбок / вверх; влево = зеркало бока, Art Bible §5).
    /// Ассет вида, не игровые данные: меняется вместе с артом без правки Logic (A9).
    /// </summary>
    [CreateAssetMenu(menuName = "FotF/View/Sprite Animation Set", fileName = "anim_")]
    public sealed class SpriteAnimationSet : ScriptableObject
    {
        [Serializable]
        public sealed class Clip
        {
            public string name = "idle";
            [Min(1f)] public float fps = 10f;
            public bool loop = true;
            public Sprite[] down = Array.Empty<Sprite>();
            public Sprite[] side = Array.Empty<Sprite>();
            public Sprite[] up = Array.Empty<Sprite>();
            [Tooltip("Покачивание тела по кадрам, пиксели (+ вверх). Заполняется меню FotF/Art/Bake Body Bob по самим спрайтам")]
            public int[] bobDown = Array.Empty<int>();
            public int[] bobSide = Array.Empty<int>();
            public int[] bobUp = Array.Empty<int>();
        }

        public Clip[] clips = Array.Empty<Clip>();
        [Tooltip("Зеркалить кадры «спиной»: у PixelLab-рыцаря X2 щит в них не на той стороне (спина симметрична)")]
        public bool mirrorUp;

        public Clip Find(string clipName)
        {
            foreach (var c in clips) if (c.name == clipName) return c;
            return clips.Length > 0 ? clips[0] : null;
        }
    }
}
