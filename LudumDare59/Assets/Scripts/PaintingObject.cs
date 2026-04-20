using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PaintingObject : MonoBehaviour
{
    [SerializeField] private Vector3 _translationSeed;
    [SerializeField] private Quaternion _rotationSeed;
    [SerializeField] private Vector3 _scaleSeed;
    [SerializeField] private float _seedMultiplier;

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
        _translationSeed = Random.insideUnitSphere;
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Generate random rotation seed")]
    private void GenerateRotationSeed()
    {
        _rotationSeed = Random.rotation;
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Generate random scale seed")]
    private void GenerateScaleSeed()
    {
        _scaleSeed = Random.insideUnitSphere;
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

    public void ApplyTransformation(float value)
    {
        //TODO: tween
        value = (value / 2f + 0.5f) * _seedMultiplier;
        Vector3 newPosition = _initialPosition + value * _translationSeed;
        Quaternion newRotation = Quaternion.Slerp(_initialRotation, _rotationSeed, value);
        transform.SetPositionAndRotation(newPosition, newRotation);

        Vector3 newScale = _initialScale + value * _scaleSeed;
        transform.localScale = math.abs(newScale);
    }
}
