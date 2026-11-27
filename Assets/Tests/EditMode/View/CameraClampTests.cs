using FotF.View.Cameras;
using NUnit.Framework;

namespace FotF.Tests.View
{
    public sealed class CameraClampTests
    {
        [Test]
        public void RoomWiderThanView_ClampsSoEdgeStaysInside()
        {
            // Комната -10..10, половина кадра 4 → центр камеры в пределах -6..6.
            Assert.AreEqual(6f, AimCameraFollow.ClampAxis(9f, -10f, 10f, 4f), 1e-4);
            Assert.AreEqual(-6f, AimCameraFollow.ClampAxis(-9f, -10f, 10f, 4f), 1e-4);
            Assert.AreEqual(2f, AimCameraFollow.ClampAxis(2f, -10f, 10f, 4f), 1e-4);
        }

        [Test]
        public void RoomNarrowerThanView_CentersCamera()
        {
            Assert.AreEqual(1f, AimCameraFollow.ClampAxis(5f, -9f, 11f, 12f), 1e-4);
        }
    }
}
