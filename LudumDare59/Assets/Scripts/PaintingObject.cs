using UnityEngine;

public class PaintingObject : MonoBehaviour
{
    [SerializeField] private PaintingObjectParams _params;
    [SerializeField] private Vector3 _translationSeed;
    [SerializeField] private Quaternion _rotationSeed;
    [SerializeField] private Vector3 _scaleSeed;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Vector3 _initialScale;

    private void Awake()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _initialScale = transform.localScale;
        Tuner.RegisterPaintingObject(this);
    }

    private void OnDestroy()
    {
        Tuner.UnregisterPaintingObject(this);
    }

    [ContextMenu("Generate random translation seed")]
    private void GenerateTranslationSeed()
    {
        _translationSeed = _params.GetRandomTranslation();
    }

    [ContextMenu("Generate random rotation seed")]
    private void GenerateRotationSeed()
    {
        _rotationSeed = _params.GetRandomRotation();
    }

    [ContextMenu("Generate random scale seed")]
    private void GenerateScaleSeed()
    {
        _scaleSeed = _params.GetRandomScale();
    }

    [ContextMenu("Generate all")]
    private void GenerateAll()
    {
        GenerateTranslationSeed();
        GenerateRotationSeed();
        GenerateScaleSeed();
    }

    public void ApplyTransformation(float intensity01)
    {
        Vector3 newPosition = _initialPosition + intensity01 * _translationSeed;
        //TODO: handle negative values
        Quaternion newRotation = Quaternion.Slerp(_initialRotation, _rotationSeed, intensity01);
        transform.SetPositionAndRotation(newPosition, newRotation);

        Vector3 newScale = _initialScale + intensity01 * _scaleSeed;
        transform.localScale = newScale;
    }
}
