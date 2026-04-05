using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Cost Properties")]
    [SerializeField]
    private int interactionCost;
    [SerializeField]
    private GameObject costPrefab;
    [SerializeField]
    private float costYOffset = 0f;

    protected bool isInteractable = true;

    // All created cost prefabs
    private readonly List<GameObject> _costPrefabs = new();

    // Coroutine parameters
    private Coroutine _spawnCoroutine;
    private readonly float _showDuration = 0.1f;
    private readonly float _spawnDelay = 0.01f;

    public int InteractionCost => interactionCost;
    public bool IsInteractable => isInteractable;

    // Called by the player
    public abstract void Interact();

    public virtual void OnEnterRange()
    {
        if (interactionCost <= 10 && (interactionCost % 2) != 0)
            _spawnCoroutine = StartCoroutine(ShowCostSequence(CostPositions.UnevenUnderTen));
        else if (interactionCost <= 10)
            _spawnCoroutine = StartCoroutine(ShowCostSequence(CostPositions.EvenUnderTen));
        else if ((interactionCost % 2) != 0)
            _spawnCoroutine = StartCoroutine(ShowCostSequence(CostPositions.UnevenOverTen));
        else
            _spawnCoroutine = StartCoroutine(ShowCostSequence(CostPositions.EvenOverTen));
    }
    public virtual void OnExitRange()
    {
        if (_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);

        if (_costPrefabs.Count == 0)
            return;

        foreach (GameObject go in _costPrefabs)
        {
            Destroy(go);
        }
    }

    private IEnumerator ShowCostSequence(Vector3[] positions)
    {
        int startIndex;
        int endIndex;

        if (interactionCost > 10)
        {
            int bottomCount = 2 * ((interactionCost + 1) / 4);
            int topCount = interactionCost - bottomCount;

            startIndex = (10 - bottomCount) / 2;
            endIndex = (startIndex + bottomCount) - 1;

            for (int i = startIndex; i <= endIndex; i++)
            {
                Vector3 endPos = transform.localPosition + positions[i] + new Vector3(0f, costYOffset, 0f);

                StartCoroutine(ShowCostPrefabs(endPos));

                yield return new WaitForSeconds(_spawnDelay);
            }

            startIndex = ((positions.Length - 10) - topCount) / 2 + 10;
            endIndex = (startIndex + topCount) - 1;

            for (int i = startIndex; i <= endIndex; i++)
            {
                Vector3 endPos = transform.localPosition + positions[i] + new Vector3(0f, costYOffset, 0f);

                StartCoroutine(ShowCostPrefabs(endPos));

                yield return new WaitForSeconds(_spawnDelay);
            }
        }
        else
        {
            startIndex = (positions.Length - interactionCost) / 2;
            endIndex = (startIndex + interactionCost) - 1;

            for (int i = startIndex; i <= endIndex; i++)
            {
                Vector3 endPos = transform.localPosition + positions[i] + new Vector3(0f, costYOffset, 0f);

                StartCoroutine(ShowCostPrefabs(endPos));

                yield return new WaitForSeconds(_spawnDelay);
            }
        }
    }

    private IEnumerator ShowCostPrefabs(Vector3 endPos)
    {
        Vector3 spawnPos = endPos - new Vector3(0f, 0.5f, 0f);

        GameObject newCostPrefab = Instantiate(costPrefab, spawnPos, Quaternion.identity);
        newCostPrefab.transform.SetParent(GameObject.FindGameObjectWithTag("CostParent").transform);
        _costPrefabs.Add(newCostPrefab);

        float time = Time.deltaTime;

        while (time < _showDuration)
        {
            if (newCostPrefab == null)
                yield break;

            float t = time / _showDuration;
            newCostPrefab.transform.localPosition = Vector3.Lerp(spawnPos, endPos, t);

            time += Time.deltaTime;
            yield return null;
        }

        if (newCostPrefab != null)
            newCostPrefab.transform.position = endPos;
    }
}