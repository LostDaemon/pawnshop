using System.Collections;
using TMPro;
using UnityEngine;

//TODO: NOT USED
[RequireComponent(typeof(CanvasGroup))]
public class SpeechPopupController : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private float _visibleDuration = 2f;
    [SerializeField] private float _fadeDuration = 1f;

    private CanvasGroup _canvasGroup;
    private Coroutine _currentRoutine;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }

    public void ShowMessage(string message)
    {
        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _text.text = message;
        _canvasGroup.alpha = 1f;
        _currentRoutine = StartCoroutine(FadeOutAfterDelay());
    }

    private IEnumerator FadeOutAfterDelay()
    {
        yield return new WaitForSeconds(_visibleDuration);

        float timer = 0f;
        while (timer < _fadeDuration)
        {
            float t = timer / _fadeDuration;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            timer += Time.deltaTime;
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _currentRoutine = null;
    }
}