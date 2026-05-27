using Blockiverse.Voxel;
using UnityEngine;

namespace Blockiverse.Gameplay
{
    public sealed class PlacementPreview : MonoBehaviour
    {
        static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        static readonly int ColorProperty = Shader.PropertyToID("_Color");
        static readonly Color PlaceableColor = new(0.34f, 0.84f, 0.52f, 0.42f);
        static readonly Color BlockedColor = new(0.95f, 0.25f, 0.20f, 0.42f);

        [SerializeField] Renderer targetRenderer;

        MaterialPropertyBlock propertyBlock;

        public bool IsVisible => gameObject.activeSelf;
        public BlockPosition CurrentPosition { get; private set; }

        public void Configure(Renderer renderer)
        {
            targetRenderer = renderer;
            Hide();
        }

        public void ShowAt(BlockPosition position, bool canPlace)
        {
            CurrentPosition = position;
            transform.position = new Vector3(position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f);
            transform.localScale = Vector3.one * 0.98f;
            gameObject.SetActive(true);

            if (targetRenderer != null)
            {
                targetRenderer.enabled = true;
                propertyBlock ??= new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(propertyBlock);
                Color color = canPlace ? PlaceableColor : BlockedColor;
                propertyBlock.SetColor(BaseColorProperty, color);
                propertyBlock.SetColor(ColorProperty, color);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        public void Hide()
        {
            if (targetRenderer != null)
                targetRenderer.enabled = false;

            gameObject.SetActive(false);
        }

        void Awake()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();

            propertyBlock = new MaterialPropertyBlock();
        }
    }
}
