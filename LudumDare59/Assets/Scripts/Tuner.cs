using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using PrimeTween;
using Rascar.Toolbox.Collections;
using UnityEngine;

public class Tuner : Singleton<Tuner>
{
    [SerializeField] private SerializableDictionary<TuningType, TuningInfo> _tuningInfoList;
    [SerializeField] private float _delayAfterCompletion = 1f;
    [SerializeField] private EventReference _tuningCompleteSfx;
    [SerializeField] private Renderer _screenRenderer;
    [SerializeField] private TweenSettings<float> _screenFlashInTweenSettings;
    [SerializeField] private TweenSettings<float> _screenFlashOutTweenSettings;

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
            _tuningAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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

    public static void ApplyTuning(TuningType type, float value)
    {
        if (!PaintingManager.PaintingIsLoaded)
        {
            return;
        }

        if (!PaintingManager.CurrentPainting.Channels.TryGetValue(type, out PaintingManager.ChannelInfo channel))
        {
            return;
        }

        float offsetFromTunedValue = value - channel.TunedValue;

        switch (type)
        {
            case TuningType.Transformation:

                Instance.ApplyTransformation(offsetFromTunedValue);
                break;

            case TuningType.None:

                break;
        }

        Instance.RefreshTuningAudio(type, offsetFromTunedValue, channel);
    }

    private void RefreshTuningAudio(TuningType type, float offset, PaintingManager.ChannelInfo channel)
    {
        if (_tuningInfoList.TryGetValue(type, out TuningInfo tuningInfo) && tuningInfo.ApproximatelyEquals(channel.TunedValue))
        {
            offset = 0f;
        }

        float fmodValue = offset / 2;

        string parameterName = PaintingManager.CurrentPainting.Channels[type].FmodParameterName;
        RuntimeManager.StudioSystem.setParameterByName(parameterName, fmodValue);
    }

    private void ApplyTransformation(float offset)
    {
        foreach (PaintingObject paintingObject in _paintingObjectList)
        {
            paintingObject.ApplyTransformation(offset);
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

            if (!tuningInfo.ApproximatelyEquals(channel.TunedValue))
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
            _tuningAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _tuningAudioInstance.release();
        }

        if (_tuningCompleteSfxInstance.isValid())
        {
            _tuningCompleteSfxInstance.start();
        }

        await Tween.MaterialProperty(_screenRenderer.material, Shader.PropertyToID("_FlashIntensity"), _screenFlashInTweenSettings);

        //TODO should probably go through GameManager instead
        await PaintingManager.ShowNextPaintingAsync();

        await Tween.MaterialProperty(_screenRenderer.material, Shader.PropertyToID("_FlashIntensity"), _screenFlashOutTweenSettings);

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
