using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeAnimator : MonoBehaviour
{
    [SerializeField]
    private Animator eyeAnimator;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        eyeAnimator.SetTrigger("Blink");
        yield return new WaitForSeconds(Random.Range(3f, 10f));
        StartCoroutine(BlinkRoutine());
    }
}
