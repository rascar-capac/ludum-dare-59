using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using Rascar.Toolbox.Collections;
using UnityEngine;

public class Tuner : Singleton<Tuner>
{
    [SerializeField] private SerializableDictionary<TuningType, TuningInfo> _tuningInfoList;
    [SerializeField] private float _delayAfterCompletion = 1f;
    [SerializeField] private EventReference _tuningCompleteSfx;

    private List<PaintingObject> _paintingObjectList = new();
    private bool _isCompleting;
    private EventInstance _tuningCompleteSfxInstance;
    private EventInstance _tuningAudioInstance;

    protected override void Awake()
    {
        base.Awake();

        if (!_tuningCompleteSfx.IsNull)
        {
            _tuningCompleteSfxInstance = RuntimeManager.CreateInstance(_tuningCompleteSfx);
        }

        PaintingManager.OnPaintingChanged += PaintingManager_OnPaintingChanged;
    }

    private void OnDestroy()
    {
        PaintingManager.OnPaintingChanged += PaintingManager_OnPaintingChanged;
    }

    private void PaintingManager_OnPaintingChanged()
    {
        if (PaintingManager.PaintingIsLoaded && !PaintingManager.CurrentPainting.TuningAudio.IsNull)
        {
            _tuningAudioInstance = RuntimeManager.CreateInstance(PaintingManager.CurrentPainting.TuningAudio);
            _tuningAudioInstance.start();
        }
        else if (_tuningAudioInstance.isValid())
        {
            _tuningAudioInstance.release();
        }
    }

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

        RefreshTuningAudio(type, intensity01);
    }

    private static void RefreshTuningAudio(TuningType type, float intensity01)
    {
        string parameterName = PaintingManager.CurrentPainting.Channels[type].FmodParameterName;
        RuntimeManager.StudioSystem.setParameterByName(parameterName, intensity01);
    }

    private void ApplyTransformation(float intensity01)
    {
        foreach (PaintingObject paintingObject in _paintingObjectList)
        {
            paintingObject.ApplyTransformation(intensity01);
        }
    }

    private bool AnyKnobIsUsed()
    {
        return _tuningInfoList.Any(knob => knob.Value.Knob.IsUsed);
    }

    private async void Update()
    {
        //TODO: could be optimized since we only need to check once until a knob is used
        await CheckCompletionAsync();
    }

    private async Task CheckCompletionAsync()
    {
        if (_isCompleting)
        {
            return;
        }

        if (!PaintingManager.PaintingIsLoaded || PaintingManager.CurrentPainting.Channels.Count == 0)
        {
            return;
        }

        if (AnyKnobIsUsed())
        {
            return;
        }

        foreach ((TuningType type, PaintingManager.ChannelInfo channel) in PaintingManager.CurrentPainting.Channels)
        {
            if (!_tuningInfoList.TryGetValue(type, out TuningInfo tuningInfo) || tuningInfo.Knob == null)
            {
                Debug.LogWarning($"No tuning info or knob found for the the tuning type {type}. Impossible combination.");

                return;
            }

            if (!tuningInfo.ApproximatelyEquals(channel.TargetValue))
            {
                return;
            }
        }

        await CompleteTuningAsync();
    }

    private async Task CompleteTuningAsync()
    {
        _isCompleting = true;

        if (_tuningAudioInstance.isValid())
        {
            _tuningAudioInstance.release();
        }

        if (_tuningCompleteSfxInstance.isValid())
        {
            _tuningCompleteSfxInstance.start();
        }

        //TODO should probably go through GameManager instead
        await PaintingManager.ShowNextPaintingAsync();

        await Task.Delay((int)(_delayAfterCompletion * 1000));

        _isCompleting = false;
    }

    [Serializable]
    public struct TuningInfo
    {
        public Knob Knob;
        public float Tolerance;

        public bool ApproximatelyEquals(float value)
        {
            return Mathf.Abs(Knob.Value - value) < Tolerance;
        }
    }
}

public enum TuningType
{
    None = 0,
    Transformation = 1,
}
