using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private PlayerMovement playerMovement;

    private LookDirection _currentLookDireciton;

    // Coroutines
    private Coroutine _waitCoroutine;
    private Coroutine _moveCoroutine;

    // Coroutine properties
    private readonly float _waitSeconds = 2f;
    private readonly float _moveDuration = 3f;

    void OnEnable()
    {
        playerMovement.PlayerTurn += HandlePlayerTurn;
    }

    void OnDisable()
    {
        playerMovement.PlayerTurn -= HandlePlayerTurn;
    }

    void Start()
    {
        Vector3 currentPos = transform.localPosition;
        transform.localPosition = new (currentPos.x + 0.5f, currentPos.y, currentPos.z);
    }

    private void HandlePlayerTurn(LookDirection direction)
    {
        _currentLookDireciton = direction;

        // Reset camera position
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);

        Vector3 currentPos = transform.localPosition;
        transform.localPosition = new (0f, currentPos.y, currentPos.z);

        // Wait before offsetting the camera
        if (_waitCoroutine != null)
            StopCoroutine(_waitCoroutine);

        _waitCoroutine = StartCoroutine(WaitAfterTurn());
    }

    private IEnumerator WaitAfterTurn()
    {
        yield return new WaitForSecondsRealtime(_waitSeconds);

        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);

        _moveCoroutine = StartCoroutine(MoveCamera());
    }

    private IEnumerator MoveCamera()
    {
        float offset = _currentLookDireciton == LookDirection.Right ? 0.5f : -0.5f;

        Vector3 currentPos = transform.localPosition;
        Vector3 targetPos = new (currentPos.x + offset, currentPos.y, currentPos.z);

        float time = 0f;

        while (time < _moveDuration)
        {
            float t = Mathf.Clamp01(time / _moveDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.localPosition = Vector3.Lerp(currentPos, targetPos, smoothT);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPos;
    }
}
