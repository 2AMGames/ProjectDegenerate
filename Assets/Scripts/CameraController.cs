using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region const variables

    private const float CameraBoundHorizontalBuffer = .2f;
    private const float CameraVerticalOffset = .75f;

    public float HorizontalDistanceThreshold = 2.5f;

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

            Vector3 cameraPosition = MainCamera.transform.position;
            Vector2 character1 = Overseer.Instance.GetCharacterByIndex(0).CharacterStats.transform.position;
            Vector2 character2 = Overseer.Instance.GetCharacterByIndex(1).CharacterStats.transform.position;

            float highestY = Mathf.Max(character1.y, character2.y);
            cameraPosition.y = highestY + CameraVerticalOffset;

            Vector2 cameraCenterPoint = MainCamera.ViewportToWorldPoint(new Vector3(.5f, .5f, -MainCamera.transform.position.z));

            float character1Distance = (character1 - cameraCenterPoint).x;
            float character2Distance = (character2 - cameraCenterPoint).x;

            float maxDistanceFromCameraCenter = Mathf.Max(Mathf.Abs(character1Distance), Mathf.Abs(character2Distance));
  
            if (maxDistanceFromCameraCenter > HorizontalDistanceThreshold)
            {
                cameraPosition.x = ((character1 + character2) / 2f).x;
            }

            MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, cameraPosition, .75f);
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
