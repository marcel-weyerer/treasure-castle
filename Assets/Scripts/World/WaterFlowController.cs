using UnityEngine;

public class WaterFlowController : MonoBehaviour
{
    [Header("Water Layers")]
    [SerializeField]
    private Material waterBaseMaterial;
    [SerializeField]
    private Material waterTopMaterial;
    [SerializeField]
    private Material waterSpecularMaterial;

    [Header("Water Speed")]
    [SerializeField]
    private float baseSpeed = 0.2f;
    [SerializeField]
    private float topSpeed = 0.4f;
    [SerializeField]
    private float specularSpeed = 0.6f;

    private Vector2 baseOffset = new (0f, 0f);
    private Vector2 topOffset = new (0f, 0f);
    private Vector2 specularOffset = new (0f, 0f);

    void Start()
    {
        if (waterBaseMaterial == null)
            Debug.LogError("No water water layer found.");
        if (waterTopMaterial == null)
            Debug.LogError("No water top layer found.");
        if (waterSpecularMaterial == null)
            Debug.LogError("No water reflection layer found.");
    }

    void Update()
    {
        baseOffset.x -= baseSpeed * Time.deltaTime;
        topOffset.x -= topSpeed * Time.deltaTime;
        specularOffset.x -= specularSpeed * Time.deltaTime;

        waterBaseMaterial.SetTextureOffset("_BaseMap", baseOffset);
        waterTopMaterial.SetTextureOffset("_BaseMap", topOffset);
        waterSpecularMaterial.SetTextureOffset("_BaseMap", specularOffset);
    }
}
