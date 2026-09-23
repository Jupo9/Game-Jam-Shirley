using UnityEngine;

public class GrassCameraProvider : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Material grassMaterial;

    private static readonly int CameraPositionID = Shader.PropertyToID("_GrassCameraPositionWS");

    private void Awake()
    {
        if (targetCamera == null)
        { 
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null || grassMaterial == null)
        { 
            return;
        }

        grassMaterial.SetVector(CameraPositionID,targetCamera.transform.position);
    }
}
