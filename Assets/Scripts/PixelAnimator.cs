using UnityEngine;

namespace FrankenArms
{
    public sealed class PixelAnimator : MonoBehaviour
    {
        [SerializeField] private float frameDuration = 0.18f;
        private SpriteRenderer renderer;
        private Sprite[] frames;
        private float timer;
        private int frame;

        internal void Initialize(Sprite[] animationFrames)
        {
            frames = animationFrames;
            renderer = GetComponent<SpriteRenderer>();
            if (renderer != null && frames != null && frames.Length > 0) renderer.sprite = frames[0];
        }

        private void Update()
        {
            if (frames == null || frames.Length < 2 || renderer == null) return;
            timer += Time.deltaTime;
            if (timer < frameDuration) return;
            timer -= frameDuration;
            frame = (frame + 1) % frames.Length;
            renderer.sprite = frames[frame];
        }
    }
}
