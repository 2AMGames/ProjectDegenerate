using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DebugUI : MonoBehaviour
{
    private static DebugUI instance;
    public static DebugUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<DebugUI>();
            }
            return instance;
        }
    }

    public Text debugTextElement;

    private List<string> listOfStrings = new List<string>();

    private void Awake()
    {
        instance = this;
        //this.gameObject.SetActive(!Overseer.Instance.DebugEnabled);

        
        debugTextElement.text = "";
    }


    private void Update()
    {
        if (listOfStrings.Count > 0)
        {
            UpdateTextElement();
        }
        else
        {
            debugTextElement.text = "";
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = .3f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 1;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateTextElement()
    {
        string textToWrite = "";
        for (int i = 0; i < listOfStrings.Count; i++)
        {
            textToWrite += listOfStrings[i] + "\n";
        }
        debugTextElement.text = textToWrite;
    }


    public void DisplayDebugTextForFrames(string textToDisplay, int framesToDisplay = 1)
    {
        if (!this.gameObject.activeSelf)
            return;
        StartCoroutine(DisplayTextForFramesCoroutine(textToDisplay, framesToDisplay));
    }

    public void DisplayDebugTextForSeconds(string textToDisplay, float timeToDisplay = 1)
    {
        if (!this.gameObject.activeSelf)
            return;
        StartCoroutine(DisplayTextForSecondsCoroutine(textToDisplay, timeToDisplay));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="textToDisplay"></param>
    /// <param name="numberOfFramesToDisplay"></param>
    /// <returns></returns>
    private IEnumerator DisplayTextForFramesCoroutine(string textToDisplay, int numberOfFramesToDisplay)
    {
        listOfStrings.Add(textToDisplay);
        for (int i = 0; i < numberOfFramesToDisplay; i++)
        {
            yield return null;
        }
        listOfStrings.Remove(textToDisplay);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="textToDisplay"></param>
    /// <param name="timeToDisplay"></param>
    /// <returns></returns>
    private IEnumerator DisplayTextForSecondsCoroutine(string textToDisplay, float timeToDisplay)
    {
        listOfStrings.Add(textToDisplay);
        yield return new WaitForSecondsRealtime(timeToDisplay);
        listOfStrings.Remove(textToDisplay);
    }
}
