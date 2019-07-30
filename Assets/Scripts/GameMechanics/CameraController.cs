using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region const variables

    private const float CameraBoundHorizontalBuffer = .2f;
    private const float CameraVerticalOffset = .75f;
    private const float HorizontalDistanceThreshold = .15f;

    #endregion

    #region main variables

    [SerializeField]
    private Camera MainCamera;

    [SerializeField]
    public CustomBoxCollider2D LeftCameraBound;

    [SerializeField]
    public CustomBoxCollider2D RightCameraBound;

    // How far from the center can the camera travel?
    public float CameraWorldBounds = 10f;

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
        if (Overseer.Instance.IsGameReady)
        {
            UpdateCameraPosition();
            SetCameraBounds();
        }
    }

    private void OnValidate()
    {
        if (CameraWorldBounds < 10f)
        {
            CameraWorldBounds = 10f;
        }
    }

    #endregion

    #region private methods

    private void UpdateCameraPosition()
    {
        if (MainCamera != null)
        {
            Vector3 cameraPosition = MainCamera.transform.position;
            Vector2 character1 = Overseer.Instance.GetCharacterByIndex(0).CharacterStats.transform.position;
            Vector2 character2 = Overseer.Instance.GetCharacterByIndex(1).CharacterStats.transform.position;

            float highestY = Mathf.Max(character1.y, character2.y);
            cameraPosition.y = highestY + CameraVerticalOffset;

            cameraPosition.x = (character1.x + character2.x) / 2f;

            float distanceFromLeftBound = -CameraWorldBounds - LeftCameraBound.transform.position.x;
            float distanceFromRightBound = CameraWorldBounds - RightCameraBound.transform.position.x;

            if (LeftCameraBound.transform.position.x < -CameraWorldBounds)
            {
                cameraPosition.x += -CameraWorldBounds - LeftCameraBound.transform.position.x; ;
            }
            else if (Mathf.Abs(distanceFromLeftBound) < HorizontalDistanceThreshold && cameraPosition.x < MainCamera.transform.position.x)
            {
                cameraPosition.x = MainCamera.transform.position.x;
            }

            if (RightCameraBound.transform.position.x > CameraWorldBounds)
            {
                cameraPosition.x -= (RightCameraBound.transform.position.x - CameraWorldBounds);
            }
            else if (Mathf.Abs(distanceFromRightBound) < HorizontalDistanceThreshold && cameraPosition.x > MainCamera.transform.position.x)
            {
                cameraPosition.x = MainCamera.transform.position.x;
            }

            MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, cameraPosition, .5f);
        }
    }

    private void SetCameraBounds()
    {
        float cameraZValue = MainCamera.transform.position.z;
        Vector3 leftEdge = MainCamera.ViewportToWorldPoint(new Vector3(0, .5f, -cameraZValue));
        Vector3 rightEdge = MainCamera.ViewportToWorldPoint(new Vector3(1, .5f, -cameraZValue));

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
