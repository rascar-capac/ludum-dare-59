using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PaintingObject : MonoBehaviour
{
    [SerializeField] private Vector2 _minMaxTranslationMagnitude;
    [SerializeField] private float _maxScaleDifference;
    [Space]
    [SerializeField] private Vector3 _translationSeed;
    [SerializeField] private Quaternion _rotationSeed;
    [SerializeField] private Vector3 _scaleSeed;

    private Vector3 _tunedPosition;
    private Quaternion _tunedRotation;
    private Vector3 _tunedScale;

    private void Awake()
    {
        _tunedPosition = transform.position;
        _tunedRotation = transform.rotation;
        _tunedScale = transform.localScale;
        Tuner.RegisterPaintingObject(this);
    }

    private void OnDestroy()
    {
        Tuner.UnregisterPaintingObject(this);
    }

    [ContextMenu("Generate random translation seed")]
    private void GenerateTranslationSeed()
    {
        _translationSeed = Random.Range(_minMaxTranslationMagnitude.x, _minMaxTranslationMagnitude.y) * Random.insideUnitSphere;
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
        _scaleSeed = _maxScaleDifference * Random.insideUnitSphere;
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

    public void ApplyTransformation(float offset)
    {
        //TODO: apply damping
        Vector3 newPosition = _tunedPosition + offset * _translationSeed; //if offset is 1, object is translated by _translationSeed
        _rotationSeed.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        Quaternion newRotation = _tunedRotation * Quaternion.AngleAxis(angle * offset, axis); //if offset 1, object has _rotationSeed as a rotation. If offset -1, object has -_rotationSeed as a rotation
        transform.SetPositionAndRotation(newPosition, newRotation);

        Vector3 newScale = _tunedScale + offset * _scaleSeed; //if offset is 1, object is scaled by _scaleSeed
        transform.localScale = math.abs(newScale);
    }
}
