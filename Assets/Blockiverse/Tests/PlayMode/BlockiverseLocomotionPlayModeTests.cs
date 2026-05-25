using Blockiverse.VR;
using NUnit.Framework;
using UnityEngine.InputSystem.XR;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace Blockiverse.Tests.PlayMode
{
    public sealed class BlockiverseLocomotionPlayModeTests
    {
        [Test]
        public void TeleportMovesXrOriginToRequestedWorldPosition()
        {
            GameObject rigObject = CreateXrOrigin(out XROrigin origin);

            try
            {
                var settings = rigObject.AddComponent<BlockiverseComfortSettings>();
                var teleport = rigObject.AddComponent<BlockiverseTeleportLocomotion>();
                teleport.Configure(origin, settings);

                Assert.That(teleport.TryTeleportTo(new Vector3(2.0f, 0.0f, 3.0f)), Is.True);
                Assert.That(Vector3.Distance(origin.transform.position, new Vector3(2.0f, 0.0f, 3.0f)), Is.LessThan(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(rigObject);
            }
        }

        [Test]
        public void SnapTurnRotatesXrOriginByConfiguredDegrees()
        {
            GameObject rigObject = CreateXrOrigin(out XROrigin origin);

            try
            {
                var settings = rigObject.AddComponent<BlockiverseComfortSettings>();
                var snapTurn = rigObject.AddComponent<BlockiverseSnapTurnLocomotion>();
                snapTurn.Configure(origin, settings);

                snapTurn.ApplySnapTurn(1);
                Assert.That(Mathf.DeltaAngle(origin.transform.eulerAngles.y, 45.0f), Is.EqualTo(0.0f).Within(0.1f));
            }
            finally
            {
                Object.DestroyImmediate(rigObject);
            }
        }

        [Test]
        public void HeightResetRestoresStandingEyeHeight()
        {
            GameObject rigObject = CreateXrOrigin(out XROrigin origin);

            try
            {
                origin.CameraYOffset = 1.2f;

                var settings = rigObject.AddComponent<BlockiverseComfortSettings>();
                var heightReset = rigObject.AddComponent<BlockiverseHeightReset>();
                heightReset.Configure(origin, settings);

                heightReset.ResetHeight();
                Assert.That(origin.CameraYOffset, Is.EqualTo(1.6f).Within(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(rigObject);
            }
        }

        static GameObject CreateXrOrigin(out XROrigin origin)
        {
            GameObject rigObject = new("Test XR Origin");
            rigObject.SetActive(false);

            GameObject cameraOffset = new("Camera Offset");
            cameraOffset.transform.SetParent(rigObject.transform, false);

            GameObject cameraObject = new("Main Camera");
            cameraObject.transform.SetParent(cameraOffset.transform, false);
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<TrackedPoseDriver>();

            origin = rigObject.AddComponent<XROrigin>();
            origin.CameraFloorOffsetObject = cameraOffset;
            origin.Camera = camera;
            rigObject.SetActive(true);

            return rigObject;
        }
    }
}
