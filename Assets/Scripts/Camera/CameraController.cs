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
    private Coroutine _returnCameraCoroutine;

    // Coroutine properties
    private readonly float _waitSeconds = 2f;
    private readonly float _moveDuration = 3f;
    private readonly float _returnDuration = 0.3f;

    private void OnEnable()
    {
        playerMovement.PlayerTurn += HandlePlayerTurn;
    }

    private void OnDisable()
    {
        playerMovement.PlayerTurn -= HandlePlayerTurn;
    }

    private void Start()
    {
        // Init camera position
        Vector3 currentPos = transform.localPosition;
        transform.localPosition = new (currentPos.x + 0.5f, currentPos.y, currentPos.z);
    }

    /// <summary>
    /// Handles a player turn.
    /// It returns camera to neutral position and starts a timer at the end of which the camera is moved further in look direction.
    /// </summary>
    /// <param name="direction">New current look direction of player</param>
    private void HandlePlayerTurn(LookDirection direction)
    {
        _currentLookDireciton = direction;

        // Stop moving the camera
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);

        // Reset camera position
        if (_returnCameraCoroutine != null)
            StopCoroutine(_returnCameraCoroutine);
        _returnCameraCoroutine = StartCoroutine(ReturnCamera());

        // Wait before offsetting the camera in look direction
        if (_waitCoroutine != null)
            StopCoroutine(_waitCoroutine);
        _waitCoroutine = StartCoroutine(WaitAfterTurn());
    }

    /// <summary>
    /// Waits for given seconds before starting to move the camera further in look direction.
    /// </summary>
    private IEnumerator WaitAfterTurn()
    {
        yield return new WaitForSecondsRealtime(_waitSeconds);

        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveCamera());
    }

    /// <summary>
    /// Moves the camera smoothly to an offset in look direction.
    /// </summary>
    private IEnumerator MoveCamera()
    {
        // Offset in current look direction
        float offset = _currentLookDireciton == LookDirection.Right ? 0.5f : -0.5f;

        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = new (startPos.x + offset, startPos.y, startPos.z);

        float time = 0f;

        while (time < _moveDuration)
        {
            float t = Mathf.Clamp01(time / _moveDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.localPosition = Vector3.Lerp(startPos, targetPos, smoothT);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPos;
    }

    /// <summary>
    /// Returns the camera to neutral position.
    /// </summary>
    private IEnumerator ReturnCamera()
    {
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = new (0f, startPos.y, startPos.z);

        float time = 0f;

        while (time < _returnDuration)
        {
            float t = time / _returnDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.localPosition = Vector3.Lerp(startPos, targetPos, smoothT);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPos;
    }
}
