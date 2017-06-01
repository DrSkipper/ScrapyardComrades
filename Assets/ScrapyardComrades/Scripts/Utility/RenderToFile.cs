using UnityEngine;

public class RenderToFile : MonoBehaviour
{
    public Camera Camera;
    public WorldLoadingManager LoadingManager;
    public Shader ScreenshotShader;
    public Color ScreenshotSkybox;
    public LayerMask ScreenshotCullingMask;
    public float ResizeMultiplier;
    public bool RunOnStart = false;
    public int Delay;

    public const string OUTPUT_PATH = "/Snapshots/";

    /**
     * NOTE: Need to deactive CameraController script and comment out its Awake method for proper snapshots for main menu. Also if it's a room with parallax, set parallax scale to 2.
     **/
    void Start()
    {
#if UNITY_EDITOR
        //this.gameObject.SetActive(false);
    }
#else
        if (this.RunOnStart)
        {
            _delayTimer = new Timer(this.Delay, false, true, this.CaptureScreenshot);
        }
    }

    void FixedUpdate()
    {
        _delayTimer.update();
    }
#endif

    public void CaptureScreenshot()
    {
        float ratio = 16.0f / 9.0f;
        int height = Mathf.RoundToInt(this.Camera.orthographicSize * this.ResizeMultiplier);
        int width = Mathf.RoundToInt(ratio * height);
        RenderTexture tempRT = new RenderTexture(width, height, 24);

        float oldAspect = this.Camera.aspect;
        RenderTexture oldRT = this.Camera.targetTexture;
        Color oldSkybox = this.Camera.backgroundColor;
        int oldCullingMask = this.Camera.cullingMask;
        this.Camera.aspect = ratio;
        this.Camera.targetTexture = tempRT;
        this.Camera.backgroundColor = this.ScreenshotSkybox;
        this.Camera.cullingMask = this.ScreenshotCullingMask;
        //this.Camera.RenderWithShader(this.ScreenshotShader, null);
        this.Camera.Render();

        RenderTexture.active = tempRT;
        Texture2D virtualPhoto = new Texture2D(width, height, TextureFormat.RGB24, false);
        virtualPhoto.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        Destroy(tempRT);
        RenderTexture.active = null;
        this.Camera.targetTexture = oldRT;
        this.Camera.aspect = oldAspect;
        this.Camera.backgroundColor = oldSkybox;
        this.Camera.cullingMask = oldCullingMask;
        
        System.IO.File.WriteAllBytes(Application.streamingAssetsPath + OUTPUT_PATH + LoadingManager.CurrentQuadName + StringExtensions.PNG_SUFFIX, virtualPhoto.EncodeToPNG());
        this.gameObject.SetActive(false);
    }

    /**
     * Private
     */
    private Timer _delayTimer;
}
