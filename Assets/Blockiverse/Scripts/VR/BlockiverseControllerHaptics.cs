using UnityEngine;
using UnityEngine.XR;

namespace Blockiverse.VR
{
    public sealed class BlockiverseControllerHaptics : MonoBehaviour
    {
        [SerializeField] BlockiverseControllerRole role;

        public BlockiverseControllerRole Role => role;

        public void Configure(BlockiverseControllerRole controllerRole)
        {
            role = controllerRole;
        }

        public bool SendImpulse(float amplitude, float durationSeconds)
        {
            InputDeviceCharacteristics hand = role == BlockiverseControllerRole.Left
                ? InputDeviceCharacteristics.Left
                : InputDeviceCharacteristics.Right;

            InputDeviceCharacteristics characteristics =
                InputDeviceCharacteristics.Controller | hand;

            var devices = new System.Collections.Generic.List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

            foreach (InputDevice device in devices)
            {
                if (device.isValid && device.TryGetHapticCapabilities(out HapticCapabilities capabilities) &&
                    capabilities.supportsImpulse)
                {
                    return device.SendHapticImpulse(0u, amplitude, durationSeconds);
                }
            }

            return false;
        }
    }
}
