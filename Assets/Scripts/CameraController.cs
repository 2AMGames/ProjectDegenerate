using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region const variables

    private const float CameraBoundHorizontalBuffer = .2f;

    private const float CameraYOffset = .40f;

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
        SetCameraBounds();
    }

    private void LateUpdate()
    {
        UpdateCameraPosition();
        SetCameraBounds();
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
            cameraPosition.y += CameraYOffset;
            MainCamera.transform.position = cameraPosition;

        }
    }

    private void SetCameraBounds()
    {
        float cameraZValue = MainCamera.transform.position.z;
        Vector3 leftEdge = MainCamera.ViewportToWorldPoint(new Vector3(0, .5f, -cameraZValue));
        Vector3 rightEdge = MainCamera.ViewportToWorldPoint(new Vector3(1, .5f, -cameraZValue));

        Debug.LogWarning("Left Edge: " + leftEdge);
        Debug.LogWarning("Right Edge: " + rightEdge);

         if (LeftCameraBound != null)
        {
            leftEdge.x += CameraBoundHorizontalBuffer;
            leftEdge.y = 0f;
            LeftCameraBound.transform.position = leftEdge;
        }

         if (RightCameraBound != null)
        {
            rightEdge.x -= CameraBoundHorizontalBuffer;
            rightEdge.y = 0f;
            RightCameraBound.transform.position = rightEdge;
        }

    }

    #endregion
}
