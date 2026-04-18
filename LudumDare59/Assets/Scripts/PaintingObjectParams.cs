using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu]
public class PaintingObjectParams : ScriptableObject
{
    [SerializeField] private Vector2 _minMaxTranslationFactor;
    [SerializeField] private Vector2 _minMaxScaleFactor;

    public Vector2 MinMaxTranslationFactor => _minMaxTranslationFactor;
    public Vector2 MinMaxScaleFactor => _minMaxScaleFactor;

    public Vector3 GetRandomTranslation()
    {
        return math.remap(0, 1, _minMaxTranslationFactor.x, _minMaxTranslationFactor.y, Random.insideUnitSphere);
    }

    public Quaternion GetRandomRotation()
    {
        return Random.rotation;
    }

    public Vector3 GetRandomScale()
    {
        return math.remap(0, 1, _minMaxScaleFactor.x, _minMaxScaleFactor.y, Random.insideUnitSphere);
    }
}