using UnityEngine;

public class OriginalPainting : MonoBehaviour
{
    [SerializeField] private Renderer _paintingRenderer;
    [SerializeField] private Hand _hand;

    private void Awake()
    {
        _hand.OnHidden += Hand_OnHidden;
    }

    private void OnDestroy()
    {
        _hand.OnHidden -= Hand_OnHidden;
    }

    private void Hand_OnHidden()
    {
        Refresh();
    }

    private void Refresh()
    {
        Texture2D paintingTexture;

        if (PaintingManager.PaintingIsLoaded)
        {
            paintingTexture = PaintingManager.CurrentPainting.Original;
        }
        else
        {
            paintingTexture = null;
        }

        _paintingRenderer.material.SetTexture("_BaseMap", paintingTexture);
    }
}
