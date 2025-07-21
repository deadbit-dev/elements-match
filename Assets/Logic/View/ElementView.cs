using UnityEngine;
using System.Collections;
using Scellecs.Morpeh;
using System;

public class ElementView : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private Action destroyCallback;

    private Coroutine idleCoroutine;

    public void SetIdle()
    {
        if (animator == null)
            return;
        animator.Play("Idle", 0);
    }

    public void SetIdleWithDelay(float delay)
    {
        if (animator == null)
            return;
        idleCoroutine = StartCoroutine(EnableWithDelay(delay));
    }

    public void SetDestroy(Action callback = null)
    {
        if (animator == null)
            return;
        destroyCallback = callback;
        animator.Play("Destroy", 0);
        if (idleCoroutine != null)
            StopCoroutine(idleCoroutine);
    }

    public void OnEndDestroy()
    {
        destroyCallback?.Invoke();
        Destroy(this.gameObject);
    }

    private IEnumerator EnableWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetIdle();
    }
}