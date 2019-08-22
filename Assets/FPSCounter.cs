using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public Text fpsText;
    public int framesBetweenDisplayText;

    private float totalTimeThatHasPassed;
    private int framesThatHavePassed;

    private void Awake()
    {
        enabled = false;
        Overseer.Instance.OnGameReady += OnGameReady;
    }

    private void OnGameReady(bool isReady)
    {
        enabled = isReady;
    }

    private void Update()
    {
        framesThatHavePassed += 1;
        totalTimeThatHasPassed += Time.unscaledDeltaTime;
        if (framesThatHavePassed >= framesBetweenDisplayText)
        {

            fpsText.text = (1f / (totalTimeThatHasPassed / framesBetweenDisplayText)).ToString("0.0") + "fps" + "\n" + "Frame # " + GameStateManager.Instance.FrameCount;
            framesThatHavePassed = 0;
            totalTimeThatHasPassed = 0;
        }
    }

}
