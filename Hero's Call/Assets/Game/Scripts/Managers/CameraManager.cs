using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Shared { get; private set; }
    private VideoPlayer _videoPlayer;

    void Start()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Awake()
    {
        if (Shared == null)
        {
            Shared = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowVideo()
    {
        _videoPlayer.enabled = true;
        StartCoroutine(IsVideoOver());
    }

    /**
     * Checks if the video stopped playing
     */
    private IEnumerator IsVideoOver()
    {
        while (true)
        {
            long playerCurrentFrame = _videoPlayer.frame;
            long playerFrameCount = (long)_videoPlayer.frameCount;
            if(playerCurrentFrame < playerFrameCount - 1)
            {
                yield return null;
            }
            else
            {
                StartMenu.Shared.AfterVideoPlayed();
                yield break;
            }            
        }
    }
}