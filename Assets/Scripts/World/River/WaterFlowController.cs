using UnityEngine;

public class WaterFlowController : MonoBehaviour
{
    [Header("Water Layer Planes")]
    [SerializeField] private Transform waterBasePlane;
    [SerializeField] private Transform waterTopPlane;
    [SerializeField] private Transform waterSpecularPlane;

    [Header("Water Layer Materials")]
    [SerializeField] private Material waterBaseMaterial;
    [SerializeField] private Material waterTopMaterial;
    [SerializeField] private Material waterSpecularMaterial;

    [Header("Water Speed")]
    [SerializeField] private float baseSpeed = 0.2f;
    [SerializeField] private float topSpeed = 0.4f;
    [SerializeField] private float specularSpeed = 0.6f;

    private Vector2 baseOffset = new (0f, 0f);
    private Vector2 topOffset = new (0f, 0f);
    private Vector2 specularOffset = new (0f, 0f);

    private void Start()
    {
        if (waterBaseMaterial == null)
            Debug.LogError("No water water layer found.");
        if (waterTopMaterial == null)
            Debug.LogError("No water top layer found.");
        if (waterSpecularMaterial == null)
            Debug.LogError("No water reflection layer found.");
    }

    private void Update()
    {
        baseOffset.x -= baseSpeed * Time.deltaTime;
        topOffset.x -= topSpeed * Time.deltaTime;
        specularOffset.x -= specularSpeed * Time.deltaTime;

        waterBaseMaterial.SetTextureOffset("_BaseMap", baseOffset);
        waterTopMaterial.SetTextureOffset("_BaseMap", topOffset);
        waterSpecularMaterial.SetTextureOffset("_BaseMap", specularOffset);
    }

    public void ScaleRiver(float worldWidth)
    {
        // Scale river planes depending on world size
        waterBasePlane.localScale = new Vector3(worldWidth, waterBasePlane.localScale.y, waterBasePlane.localScale.z);
        waterTopPlane.localScale = new Vector3(worldWidth, waterTopPlane.localScale.y, waterTopPlane.localScale.z);
        waterSpecularPlane.localScale = new Vector3(worldWidth, waterSpecularPlane.localScale.y, waterSpecularPlane.localScale.z);

        // Choose tiling accoring to scale
        waterBaseMaterial.mainTextureScale = new Vector2(worldWidth, waterBaseMaterial.mainTextureScale.y);
        waterTopMaterial.mainTextureScale = new Vector2(worldWidth, waterTopMaterial.mainTextureScale.y);
        waterSpecularMaterial.mainTextureScale = new Vector2(worldWidth, waterSpecularMaterial.mainTextureScale.y);
    }
}
