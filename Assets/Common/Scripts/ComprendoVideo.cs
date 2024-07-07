using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ComprendoVideo : MonoBehaviour
{

	private VideoPlayer videoPlayer;

    public string videoName;

    public Button pauseButton;
    public Button skipButton;

    private Action onEndVideo;

#if UNITY_WEBGL && !UNITY_EDITOR
    private readonly string basePath = "/video"; // URL relativo al server
#else
    private readonly string basePath = "https://mondoelli.anastasis.it/video";
#endif

    void Awake()
    {
        videoPlayer = Camera.main.gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.isLooping = false;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        videoPlayer.started += VideoStarted;
        videoPlayer.loopPointReached += EndReached;
    }

    public void Play(Action onEndVideo) {
        Debug.Log(videoPlayer);
        this.onEndVideo = onEndVideo;
#if UNITY_EDITOR || UNITY_LINUX
        string ext = ".ogv";
#else
        string ext = ".mp4";
#endif
        videoPlayer.url = basePath + "/" + videoName + ext;
        videoPlayer.Play();
    }

    public void Pause() {
        if (videoPlayer.isPlaying) {
            videoPlayer.Pause();
        } else if (videoPlayer.isPaused) {
            videoPlayer.Play();
        }
    }

    public void Skip() {
        EndReached(videoPlayer);
    }

    /* Funzione che attiva i pulsanti pausa e salta quando il video è riprodotto */
    private void VideoStarted(VideoPlayer vp) {
        pauseButton.gameObject.SetActive(true);
        skipButton.gameObject.SetActive(true);
    }

    /* Funzione che nasconde i pulsanti pausa e salta quando il video termina */
    private void EndReached(VideoPlayer vp) {
        vp.Stop();
        pauseButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        onEndVideo.Invoke();
    }

}
