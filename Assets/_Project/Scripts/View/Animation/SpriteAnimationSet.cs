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
        }

        public Clip[] clips = Array.Empty<Clip>();

        public Clip Find(string clipName)
        {
            foreach (var c in clips) if (c.name == clipName) return c;
            return clips.Length > 0 ? clips[0] : null;
        }
    }
}
