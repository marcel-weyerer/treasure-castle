using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private Image fadePanel;
    [SerializeField] private PlayerMovement playerMovement;

    // Fade Coroutine properties
    private readonly float _fadeTime = 3f;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Stard fading in game view at start
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float currentOpacity = 1f;
        float targetOpacity = 0f;

        var currColor = fadePanel.color;

        float time = 0f;

        while (time < _fadeTime)
        {
            float t = time / _fadeTime;
            float smoothT = Mathf.SmoothStep(currentOpacity, targetOpacity, t);

            currColor.a = smoothT;
            fadePanel.color = currColor;

            time += Time.deltaTime;
            yield return null;
        }

        currColor.a = targetOpacity;
        fadePanel.color = currColor;

        // Enable Player Movement
        playerMovement.enabled = true;

        yield return null;
    }
}
