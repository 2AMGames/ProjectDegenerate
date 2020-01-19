using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays current round time. Changes color based on time left. Should display infinity if no time limit set.
/// </summary>
public class RoundTimerUI : MonoBehaviour
{
    #region const variables

    private const string Infinity = "\u221E";

    private const float TimeWarningPercentage = .25f;

    private readonly Color TimeWarningColor = Color.red;

    #endregion

    #region main variables

    public Text RoundTimer;

    #endregion

    #region monobehaviour methods

    public void Start()
    {

    }

    public void Awake()
    {
        if (GameStateManager.Instance.RoundLimit == null)
        {
            RoundTimer.text = Infinity;
            enabled = false;
        }
    }

    public void Update()
    {
        if (Overseer.Instance.HasGameStarted && Overseer.Instance.GameReady && GameStateManager.Instance.RoundLimit != null)
        {
            UpdateRoundTimerText();
        }
    }

    #endregion

    #region private interface

    private void UpdateRoundTimerText()
    {
        int timeRemaining = Mathf.RoundToInt(GameStateManager.Instance.RoundLimit.GetValueOrDefault() - GameStateManager.Instance.RoundTime);
        RoundTimer.text = timeRemaining.ToString();
        RoundTimer.color = timeRemaining > GameStateManager.Instance.RoundLimit.GetValueOrDefault() * TimeWarningPercentage ? Color.white : TimeWarningColor;
    }

    #endregion

}
