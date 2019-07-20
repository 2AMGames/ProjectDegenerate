using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region const variables

    #endregion

    #region main variables

    [SerializeField]
    private Camera MainCamera;

    [SerializeField]
    public CustomBoxCollider2D LeftCameraBound;

    [SerializeField]
    public CustomBoxCollider2D RightCameraBound;

    #endregion

    #region monobehaviour methods

    private void Awake()
    {
        if (MainCamera == null)
        {
            MainCamera = GetComponent<Camera>();
        }
    }

    private void Update()
    {
        UpdateCameraPosition();
    }

    #endregion

    #region private methods

    private void UpdateCameraPosition()
    {
        if (MainCamera != null)
        {
            float z = MainCamera.transform.position.z;
            Vector2 characterPosition = Overseer.Instance.GetCharacterByIndex(0).CharacterStats.transform.position;
            Vector2 character2 = Overseer.Instance.GetCharacterByIndex(1).CharacterStats.transform.position;
            Vector3 displacement = (characterPosition + character2) / 2;
            Vector3 cameraPosition = displacement;
           cameraPosition.z = z;
            MainCamera.transform.position = cameraPosition;
            
        }
    }

    #endregion
}
