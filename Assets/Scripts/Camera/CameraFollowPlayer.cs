using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{ 
    [SerializeField] private Transform target; 
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f); 

    private void LateUpdate() 
    {
        if (target == null)
        {
            return;
        }
    
        transform.position = target.position + offset;
    } 
}
