using UnityEngine;
using System.Collections;

public class FPScounter : MonoBehaviour
{
    // Attach this to a GUIText to make a frames/second indicator.
    //
    // It calculates frames/second over each updateInterval,
    // so the display does not keep changing wildly.
    //
    // It is also fairly accurate at very low FPS counts (<10).
    // We do this not by simply counting frames per interval, but
    // by accumulating FPS for each frame. This way we end up with
    // correct overall FPS even if the interval renders something like
    // 5.5f frames.

    public float updateInterval = 1.0f;
    private double accum = 0; // FPS accumulated over the interval
    private double frames = 0; // Frames drawn over the interval
    private double timeleft; // Left time for current interval
    private double fps = 15.0; // Current FPS
    private double lastSample;
    private float gotIntervals = 0;

    void Start()
    {
        timeleft = updateInterval;
        lastSample = Time.realtimeSinceStartup;
    }

    double GetFPS() { return fps; }
    bool HasFPS() { return gotIntervals > 2; }

    void Update()
    {
        ++frames;
        double newSample = Time.realtimeSinceStartup;
        double deltaTime = newSample - lastSample;
        lastSample = newSample;

        timeleft -= deltaTime;
        accum += 1.0f / deltaTime;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0f)
        {
            // display two fractional digits (f2 format)
            fps = accum / frames;
            //        guiText.text = fps.ToString("f2");
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;
            ++gotIntervals;
        }
    }

    void OnGUI()
    {
        GUI.Box(new Rect(Screen.width - 160, 10, 150, 40), fps.ToString("f2"));
    }
}