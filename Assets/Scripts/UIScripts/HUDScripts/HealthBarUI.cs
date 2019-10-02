using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Our basic ui display for a single player. This will display the player's current health
/// </summary>
public class HealthBarUI : MonoBehaviour
{


    #region main variables
    public Slider healthSlider;
    public Slider comboHealthBar;
    public Slider regenHealthBar;

    #endregion main variables

    #region monobehavouir methods

    private void Awake()
    {
        
    }
    #endregion monobehaviour methods


    #region visually update methods
    private void OnPlayerHealthChanged()
    {

    }

    /// <summary>
    /// When our combo ends, we will call this method to begin a coroutine that will drop the combo damage to match the current health
    /// </summary>
    private void OnComboEnded()
    {

    }
    #endregion visually update methods


}
