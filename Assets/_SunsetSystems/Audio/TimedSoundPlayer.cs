using UnityEngine;
using Sirenix.OdinInspector;

public class TimedSoundPlayer : MonoBehaviour
{
    [Title("Audio Settings")]
    [SerializeField] private AudioClip _soundClip;
    [SerializeField] private AudioSource _audioSource;

    [Title("Timing Settings")]
    [SerializeField] private float _interval = 5f;
    [SerializeField] private bool _autoStart = true;

    private float _timer;
    private bool _isPlaying = false;

    private void Start()
    {
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        _audioSource.clip = _soundClip;

        if (_autoStart)
            StartPlaying();
    }

    private void Update()
    {
        if (!_isPlaying || _soundClip == null)
            return;

        _timer += Time.deltaTime;

        if (_timer >= _interval)
        {
            _audioSource.Play();
            _timer = 0f;
        }
    }

    public void StartPlaying()
    {
        _isPlaying = true;
        _timer = 0f;
    }

    public void StopPlaying()
    {
        _isPlaying = false;
    }

    public void SetInterval(float newInterval)
    {
        _interval = Mathf.Max(0.1f, newInterval);
    }
}
