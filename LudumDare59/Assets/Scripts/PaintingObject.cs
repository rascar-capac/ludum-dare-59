using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Generate random rotation seed")]
    private void GenerateRotationSeed()
    {
        _rotationSeed = _params.GetRandomRotation();
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Generate random scale seed")]
    private void GenerateScaleSeed()
    {
        _scaleSeed = _params.GetRandomScale();
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Generate all")]
    private void GenerateAll()
    {
        GenerateTranslationSeed();
        GenerateRotationSeed();
        GenerateScaleSeed();
    }

    [ContextMenu("Set rotation to zero")]
    private void SetRotationToZero()
    {
        _rotationSeed = Quaternion.identity;
    }

    public void ApplyTransformation(float intensity)
    {
        //TODO: tween
        Vector3 newPosition = _initialPosition + intensity * _translationSeed;
        Quaternion newRotation = Quaternion.Slerp(_initialRotation, _rotationSeed, intensity);
        transform.SetPositionAndRotation(newPosition, newRotation);

        Vector3 newScale = _initialScale + intensity * _scaleSeed;
        transform.localScale = math.abs(newScale);
    }
}
