using UnityEngine;

public class LipSync : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer _mouthRenderer;
    public SkinnedMeshRenderer MouthRenderer
    {
        get => _mouthRenderer;
        set
        {
            _mouthRenderer = value;
            _mouthInstance = value.materials[mouthMaterialIndex];
            _cachedOffset = _mouthInstance.mainTextureOffset;
        }
    }

    [SerializeField]
    private int mouthMaterialIndex;

    [SerializeField]
    private Vector2 closedOffset;

    [SerializeField]
    private Vector2 openOffset;

    [SerializeField]
    private float loudnessThreshold;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private float sampleStep = 0.1f;

    private float CurrentAudioLoudness
    {
        get
        {
            if (!audioSource.isPlaying) return 0;

            return Mathf.Cos(Time.time * 20);

            // audioSource.clip.GetData goes out of memory sometimes on WebGL
            // audioSorce.GetOutputData is not supported on WebGL
            //audioSource.clip.GetData(_sampleData, audioSource.timeSamples);

            //float loudness = 0;

            //foreach (float sample in _sampleData)
            //    loudness += sample;

            //loudness /= _sampleData.Length;

            //return loudness;
        }
    }

    private Material _mouthInstance;
    private Vector2 _cachedOffset;
    private float _elapsedTime = 0;
    //private float[] _sampleData = new float[256];

    private void OnEnable()
    {
        if (_mouthRenderer == null) return;

        _mouthInstance = MouthRenderer.materials[mouthMaterialIndex];
        _cachedOffset = _mouthInstance.mainTextureOffset;
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime < sampleStep) return;

        _elapsedTime = 0;

        if (_mouthInstance == null) return;

        _mouthInstance.mainTextureOffset = CurrentAudioLoudness < loudnessThreshold ? closedOffset : openOffset;
    }

    private void OnDisable()
    {
        if (_mouthInstance == null) return;

        _mouthInstance.mainTextureOffset = _cachedOffset;
    }
}
