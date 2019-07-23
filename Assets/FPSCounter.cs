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

    private void Update()
    {
        framesThatHavePassed += 1;
        totalTimeThatHasPassed += Time.deltaTime;
        if (framesThatHavePassed >= framesBetweenDisplayText)
        {
            
            fpsText.text = (1f / (totalTimeThatHasPassed / framesBetweenDisplayText)).ToString("0.0") + "fps";
            framesThatHavePassed = 0;
            totalTimeThatHasPassed = 0;
        }
    }

}
