using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class IntroVideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        string videoPath = Path.Combine(
            Application.streamingAssetsPath,
            "Intro.mp4"
        );

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;

        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("VIDEO PREPARADO CORRECTAMENTE");
        vp.Play();
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.prepareCompleted -= OnVideoPrepared;
    }
}