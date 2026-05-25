using UnityEngine;
using UnityEngine.InputSystem;

namespace Blockiverse.VR
{
    public sealed class BlockiverseControllerAnchor : MonoBehaviour
    {
        [SerializeField] BlockiverseInputRig inputRig;
        [SerializeField] BlockiverseControllerRole role;

        public BlockiverseControllerRole Role => role;

        string MapName => role == BlockiverseControllerRole.Left
            ? BlockiverseInputActionNames.LeftHandMap
            : BlockiverseInputActionNames.RightHandMap;

        public void Configure(BlockiverseInputRig rig, BlockiverseControllerRole controllerRole)
        {
            inputRig = rig;
            role = controllerRole;
        }

        void Update()
        {
            if (inputRig == null || inputRig.InputActions == null)
                return;

            InputAction position = inputRig.FindAction(MapName, BlockiverseInputActionNames.Position);
            InputAction rotation = inputRig.FindAction(MapName, BlockiverseInputActionNames.Rotation);
            transform.localPosition = position.ReadValue<Vector3>();
            transform.localRotation = rotation.ReadValue<Quaternion>();
        }
    }
}
