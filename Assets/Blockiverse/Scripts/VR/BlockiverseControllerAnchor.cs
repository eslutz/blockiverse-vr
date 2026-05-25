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

            if (!TryFindAction(BlockiverseInputActionNames.IsTracked, out InputAction isTracked) ||
                !isTracked.IsPressed())
                return;

            if (!TryFindAction(BlockiverseInputActionNames.Position, out InputAction position) ||
                !TryFindAction(BlockiverseInputActionNames.Rotation, out InputAction rotation))
                return;

            Quaternion controllerRotation = rotation.ReadValue<Quaternion>();
            if (IsZeroQuaternion(controllerRotation))
                return;

            transform.localPosition = position.ReadValue<Vector3>();
            transform.localRotation = controllerRotation;
        }

        bool TryFindAction(string actionName, out InputAction action)
        {
            InputActionMap map = inputRig.InputActions.FindActionMap(MapName, throwIfNotFound: false);
            action = map?.FindAction(actionName, throwIfNotFound: false);
            return action != null;
        }

        static bool IsZeroQuaternion(Quaternion rotation)
        {
            return rotation.x == 0.0f &&
                rotation.y == 0.0f &&
                rotation.z == 0.0f &&
                rotation.w == 0.0f;
        }
    }
}
