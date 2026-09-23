using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float xAngle = 0f;
    [SerializeField] private float yAngle = 0f;
    [SerializeField] private float zAngle = 0f;


    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 cameraPosition = targetCamera.transform.position;
        cameraPosition.y = transform.position.y;

        transform.LookAt(cameraPosition);
        transform.Rotate(xAngle, yAngle, zAngle);
    }
}