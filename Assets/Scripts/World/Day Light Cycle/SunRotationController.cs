using UnityEngine;

public class SunRotationController : MonoBehaviour
{
    [SerializeField]
    private GameObject sun;
    [SerializeField]
    private float dayMinutes = 5f;
    private float _rotationSpeed;

    void Awake()
    {
        _rotationSpeed = 360f / (dayMinutes * 60f);
    }

    void OnEnable()
    {
        transform.eulerAngles = new (0f, 0f, 30f);
    }

    void Update()
    {
        transform.eulerAngles += new Vector3(0f, 0f, (_rotationSpeed * Time.deltaTime) % 360);
        sun.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
}
