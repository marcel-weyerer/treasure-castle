using UnityEngine;

public class ReflectionController : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;

    private void LateUpdate()
    {
        Vector3 pos = mainCamera.position;
        transform.position = new (pos.x, transform.position.y, transform.position.z);
    }
}