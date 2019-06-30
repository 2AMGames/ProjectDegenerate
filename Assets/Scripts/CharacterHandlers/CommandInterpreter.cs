using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandInterpreter : MonoBehaviour
{

    #region main variables

    Animator Animator;

    #endregion

    #region monobehaviour methods

    void Start()
    {

    }

    void Update()
    {

    }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
    }

    private void OnValidate()
    {

    }

    #endregion
}
