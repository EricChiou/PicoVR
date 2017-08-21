using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FPS_S : MonoBehaviour
{
    public Text fpsText;
    private float updateInterval = 0.5f;
    private float accum = 0.0f;
    private float frames = 0;
    private float timeleft;
    private float fps = 60;

    // Use this for initialization
    void Start()
    {
        timeleft = updateInterval;

    }

    // Update is called once per frame
    void Update()
    {
        if (fpsText != null)
        {
            fpsText.text = "FPS:" + ShowFps1();
        }
    }
    /// <summary>
    /// http://wiki.unity3d.com/index.php?title=FramesPerSecond
    /// It calculates frames/second over each updateInterval,
    /// so the display does not keep changing wildly.
    ///
    /// It is also fairly accurate at very low FPS counts (<10).
    /// We do this not by simply counting frames per interval, but
    /// by accumulating FPS for each frame. This way we end up with
    /// correct overall FPS even if the interval renders something like
    /// 5.5 frames.
    /// </summary>
    /// <returns></returns>
    public string ShowFps()
    {
        string temp;
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        if (timeleft <= 0.0)
        {
            temp = (accum / frames).ToString("f2");
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;
            return temp;
        }
        else
        {
            return "0";
        }

    }

    /// <summary>
    /// 
    ///  cardboard method
    /// </summary>
    /// <returns></returns>
    public string ShowFps1()
    {
	float interp = Time.deltaTime / (0.5f + Time.deltaTime);
        if (float.IsNaN(interp) || float.IsInfinity(interp))
        {
            interp = 0;
        }
        float currentFPS = 1.0f / Time.deltaTime;
        if (float.IsNaN(currentFPS) || float.IsInfinity(currentFPS))
        {
            currentFPS = 0;
        }
        if (float.IsNaN(fps) || float.IsInfinity(fps))
        {
            fps = 0;
        }
        fps = Mathf.Lerp(fps, currentFPS, interp);
     

        return (Mathf.RoundToInt(fps) + "fps");
    }
}
