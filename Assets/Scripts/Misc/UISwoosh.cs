using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(CanvasGroup))]
public class UISwoosh : MonoBehaviour
{

    private RectTransform rect;

    private CanvasGroup canvasGroup;

    private bool hidden = true;

    private Vector2 hiddenPosition;
    private Vector2 shownPosition;

    private Coroutine swooshRoutine;
    private void Start()
    {
        SetPositions();
    }

    void SetUpReferences()
    {
        if(rect != null && canvasGroup != null)
        {
            return;
        }
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }
    public void Show()
    {
        SetPositions();
        gameObject.SetActive(true);
        hidden = false;
        if(swooshRoutine != null)
        {
            StopCoroutine(swooshRoutine);
        }
        swooshRoutine = StartCoroutine(SwooshObjectRoutine(false));
    }
    public void Hide()
    {
        SetPositions();
        gameObject.SetActive(true);
        hidden = true;
        if (swooshRoutine != null)
        {
            StopCoroutine(swooshRoutine);
        }
        swooshRoutine = StartCoroutine(SwooshObjectRoutine(true));
    }

    public void ToggleShowHide()
    {
        SetPositions();
        hidden = !hidden;
        gameObject.SetActive(true);
        if (swooshRoutine != null)
        {
            StopCoroutine(swooshRoutine);
        }
        swooshRoutine = StartCoroutine(SwooshObjectRoutine(hidden));
    }

    void SetPositions()
    {
        SetUpReferences();
        if(shownPosition != Vector2.zero && hiddenPosition != Vector2.zero)
        {
            return;
        }
        shownPosition = rect.anchoredPosition;
        switch (rect.anchorMax)
        {
            case Vector2 v when v.Equals(new Vector2(1f, 0.5f)):
                // Anchored to the right side
                hiddenPosition += Vector2.right * 200f;
                break;
            case Vector2 v when v.Equals(new Vector2(0f, 0.5f)):
                // Anchored to the left side
                hiddenPosition += Vector2.left * 200f;
                break;
            case Vector2 v when v.Equals(new Vector2(0.5f, 1)):
                // Anchored to the top
                hiddenPosition += Vector2.up * 200f;
                break;
            case Vector2 v when v.Equals(new Vector2(0.5f, 0)):
                // Anchored to the bottom
                hiddenPosition += Vector2.down * 200f;
                break;
            default:
                // Anchors are not set up, insert it from the bottom
                hiddenPosition += Vector2.down * 200f;
                break;
        }
    }

    IEnumerator SwooshObjectRoutine(bool hide)
    {
        // Set up references only if needed
        if(rect == null || canvasGroup == null)
        {
            SetUpReferences();
        }

        // The UI that we will swossh
        RectTransform objectToSwoosh = rect;

        // Get the swoosh direction from the RectTransform
        Vector3 startposition = rect.anchoredPosition;
        Vector3 endPosition = shownPosition;

        // Timer
        float timer = 0f;

        // How long to swoosh
        float duration = 0.25f;

        // The value to be used for our lerp
        float value = 0f;

        // Alpha values
        float alphaStart = canvasGroup.alpha;
        float alphaEnd = hide ? 0f : 1f;

        // If we're hiding instead of showing, should end up at hidden position
        if (hide)
        {
            endPosition = hiddenPosition;
        }
        else
        {
            canvasGroup.alpha = 0f;
        }

        // Set the position and turn on the gameObject
        objectToSwoosh.anchoredPosition = startposition;
        objectToSwoosh.gameObject.SetActive(true);

        // Run for duration
        while (timer <= duration)
        {
            value = timer / duration;
            // Interpolate with smoothstep
            if (hide == false)
            {
                value = Mathf.Sin(value * Mathf.PI * 0.5f);
            }
            else
            {
                value = 1f - Mathf.Cos(value * Mathf.PI * 0.5f);
            }

            // Interpolate alpha
            canvasGroup.alpha = Mathf.Lerp(alphaStart, alphaEnd, value);

            // Swoosh position
            objectToSwoosh.anchoredPosition = Vector3.Lerp(startposition, endPosition, value);
            timer += Time.deltaTime;
            yield return null;
        }

        // In case we overshoot, set the position
        objectToSwoosh.anchoredPosition = endPosition;

        // Turn off the game object if we're hiding it
        if (hide)
        {
            objectToSwoosh.gameObject.SetActive(false);
            // Set back to the original position
            objectToSwoosh.anchoredPosition = hiddenPosition;
        }
        yield return null;
    }
}
