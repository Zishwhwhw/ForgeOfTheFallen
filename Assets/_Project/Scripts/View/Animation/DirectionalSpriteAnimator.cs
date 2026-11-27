using UnityEngine;

namespace FotF.View.Animation
{
    /// <summary>Проигрывает клипы SpriteAnimationSet по направлению взгляда. Только вид, решений не принимает (A4).</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DirectionalSpriteAnimator : MonoBehaviour
    {
        public enum Facing { Down, Side, Up }

        [SerializeField] SpriteAnimationSet animationSet;
        [SerializeField] string startClip = "idle";

        SpriteRenderer _renderer;
        SpriteAnimationSet.Clip _clip;
        float _time;
        Facing _facing = Facing.Down;
        bool _flip;

        public bool IsFinished => _clip != null && !_clip.loop && _time * _clip.fps >= Frames(_clip, _facing).Length;
        public Facing CurrentFacing => _facing;
        public bool IsFlipped => _flip;
        /// <summary>Покачивание тела на текущем кадре, пиксели (+ вверх): по нему двигается оружие в руке.</summary>
        public int CurrentBobPixels { get; private set; }

        void Awake() { _renderer = GetComponent<SpriteRenderer>(); Play(startClip, true); }

        /// <summary>Сменить клип. restart=false — продолжить, если это тот же клип.</summary>
        public void Play(string clipName, bool restart = false)
        {
            if (animationSet == null) return;
            var next = animationSet.Find(clipName);
            if (next == _clip && !restart) return;
            _clip = next; _time = 0f;
        }

        /// <summary>Направление по вектору взгляда (мировые оси, y вверх).</summary>
        public void SetFacing(Vector2 look)
        {
            if (look.sqrMagnitude < 0.0001f) return;
            if (Mathf.Abs(look.x) > Mathf.Abs(look.y) * 1.1f) { _facing = Facing.Side; _flip = look.x < 0f; }
            else { _facing = look.y > 0f ? Facing.Up : Facing.Down; _flip = false; }
        }

        void Update()
        {
            if (_clip == null) return;
            _time += Time.deltaTime;
            var frames = Frames(_clip, _facing);
            if (frames.Length == 0) return;
            int i = (int)(_time * _clip.fps);
            i = _clip.loop ? i % frames.Length : Mathf.Min(i, frames.Length - 1);
            _renderer.sprite = frames[i];
            _renderer.flipX = _facing == Facing.Up ? animationSet.mirrorUp : _flip;
            var bob = Bob(_clip, _facing);
            CurrentBobPixels = i < bob.Length ? bob[i] : 0;
        }

        static int[] Bob(SpriteAnimationSet.Clip c, Facing f) =>
            (f == Facing.Up ? c.bobUp : f == Facing.Side ? c.bobSide : c.bobDown) ?? System.Array.Empty<int>();

        static Sprite[] Frames(SpriteAnimationSet.Clip c, Facing f) =>
            f == Facing.Up ? c.up : f == Facing.Side ? c.side : c.down;
    }
}
