using UnityEngine;

public class OriginalPainting : MonoBehaviour
{
    [SerializeField] private Renderer _paintingRenderer;

    private void Awake()
    {
        PaintingManager.OnPaintingChanged += PaintingManager_OnPaintingChanged;
    }

    private void OnDestroy()
    {
        PaintingManager.OnPaintingChanged -= PaintingManager_OnPaintingChanged;
    }

    private void PaintingManager_OnPaintingChanged()
    {
        Refresh();
    }

    private void Refresh()
    {
        Texture2D paintingTexture = PaintingManager.CurrentPainting.Original;
        _paintingRenderer.material.SetTexture("_BaseMap", paintingTexture);
    }
}
