using FotF.EditorTools.Art;
using FotF.View.Combat;
using NUnit.Framework;
using UnityEngine;

namespace FotF.Tests.View
{
    public sealed class WeaponHoldTests
    {
        [Test]
        public void BodyBob_IsHowMuchSilhouetteTopRisesAboveIdle()
        {
            // Верх силуэта: idle на 5-м ряду, кадры бега на 7 и 5 → тело опускается на 2 px и возвращается.
            CollectionAssert.AreEqual(new[] { -2, 0, 0, -2 }, BodyBobBaker.BobFromTops(new[] { 7, 5, 5, 7 }, 5));
            CollectionAssert.AreEqual(new[] { 0 }, BodyBobBaker.BobFromTops(new[] { -1 }, 5));
        }

        [Test]
        public void SpritePixel_MapsToLocalUnitsAroundPivot()
        {
            var tex = new Texture2D(32, 24);
            var sprite = Sprite.Create(tex, new Rect(0, 0, 32, 24), new Vector2(0.5f, 0.5f), 32f);
            // Центр пикселя (27,17) относительно центра спрайта (16,12): (11.5, 5.5) px → юниты при 32 PPU.
            var local = WeaponView.SpritePixelToLocal(sprite, new Vector2(27, 17));
            Assert.AreEqual(11.5f / 32f, local.x, 1e-5);
            Assert.AreEqual(5.5f / 32f, local.y, 1e-5);
            Object.DestroyImmediate(sprite); Object.DestroyImmediate(tex);
        }
    
        [Test]
        public void BakedIndex_PicksNearestOf32Angles_AndWraps()
        {
            Assert.AreEqual(0, WeaponView.BakedIndex(0f, 32));
            Assert.AreEqual(8, WeaponView.BakedIndex(90f, 32));
            Assert.AreEqual(1, WeaponView.BakedIndex(10f, 32));       // 10° ближе к 11.25°, чем к 0°
            Assert.AreEqual(31, WeaponView.BakedIndex(-11f, 32));
            Assert.AreEqual(0, WeaponView.BakedIndex(359f, 32));
        }
    }
}
