using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;
using System;

public class WebGLVideoPlayer : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip videoClip;
    [SerializeField] public string videoFileName = "0001-0639.mp4";
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RenderTexture targetTexture;
    [SerializeField] private RawImage outputImage;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cinematicAudioClip;

    private void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer == null)
        {
            Debug.LogError("No se encontró un VideoPlayer en el mismo GameObject.");
            return;
        }

        if (audioSource == null)
        {
            GameObject audioObject = GameObject.Find("Audio1");
            if (audioObject != null)
                audioSource = audioObject.GetComponent<AudioSource>();
        }

        if (audioSource == null && cinematicAudioClip != null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (audioSource != null)
        {
            if (cinematicAudioClip != null)
                audioSource.clip = cinematicAudioClip;

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;
            audioSource.Stop();
        }
        else
        {
            Debug.LogWarning("No se encontró un AudioSource en el objeto Audio1.");
        }

        string videoPath = Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Application.platform == RuntimePlatform.WebGLPlayer
            ? videoPath.Replace("\\", "/")
            : new Uri(videoPath).AbsoluteUri;
        Debug.Log("VIDEO URL: " + videoPlayer.url);

        if (string.IsNullOrEmpty(videoPlayer.url))
        {
            Debug.LogError("No se pudo construir la URL del vídeo.");
            return;
        }

        // El audio embebido del MP4 provoca AudioSampleProvider buffer overflow
        // en WebGL cuando el navegador no consume las muestras a tiempo.
        // La cinematica se reproduce sin audio para evitar pérdida de frames.
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        if (targetCamera == null)
            targetCamera = Camera.main;

        // CameraFarPlane funciona directamente en WebGL y no depende de una
        // RenderTexture que pueda no estar inicializada al comenzar la escena.
        if (targetTexture != null)
        {
            if (!targetTexture.IsCreated())
                targetTexture.Create();

            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = targetTexture;

            if (outputImage != null)
                outputImage.texture = targetTexture;
        }
        else if (targetCamera != null)
        {
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            videoPlayer.targetCamera = targetCamera;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = false;
        videoPlayer.skipOnDrop = false;
        videoPlayer.isLooping = false;
        videoPlayer.playbackSpeed = 1.0f;
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.GameTime;

        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.prepareCompleted += OnVideoPrepared;

        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("VIDEO PREPARADO CORRECTAMENTE");

        if (targetTexture == null && vp.texture == null)
        {
            Debug.LogError("El VideoPlayer se preparó, pero no creó una textura de salida.");
            return;
        }

        if (outputImage != null)
            outputImage.texture = targetTexture != null ? targetTexture : vp.texture;

        vp.Play();
        if (audioSource != null && audioSource.clip != null)
            audioSource.Play();

        Debug.Log($"VIDEO REPRODUCIENDO: {vp.width}x{vp.height}, frame {vp.frame}");
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("VIDEO ERROR: " + message);
    }

    private void OnDestroy()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.prepareCompleted -= OnVideoPrepared;

        if (audioSource != null)
            audioSource.Stop();
    }
}