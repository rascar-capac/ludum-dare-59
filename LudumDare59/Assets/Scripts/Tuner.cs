using System.Collections.Generic;

public class Tuner : Singleton<Tuner>
{
    private List<PaintingObject> _paintingObjectList = new();

    public static void RegisterPaintingObject(PaintingObject paintingObject)
    {
        if (HasInstance && !Instance._paintingObjectList.Contains(paintingObject))
        {
            Instance._paintingObjectList.Add(paintingObject);
        }
    }

    public static void UnregisterPaintingObject(PaintingObject paintingObject)
    {
        if (HasInstance)
        {
            Instance._paintingObjectList.Remove(paintingObject);
        }
    }

    public static void ApplyTuning(TuningType type, float intensity01)
    {
        switch (type)
        {
            case TuningType.Transformation:

                Instance.ApplyTransformation(intensity01);
                break;

            case TuningType.None:

                break;
        }
    }

    private void ApplyTransformation(float intensity01)
    {
        foreach (PaintingObject paintingObject in _paintingObjectList)
        {
            paintingObject.ApplyTransformation(intensity01);
        }
    }
}

public enum TuningType
{
    None = 0,
    Transformation = 1,
}
