using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour
{
    public enum TargetState { Appearing, Moving, Hit, Escaped }
    public TargetState State { get; private set; }

    private System.Action<Target> onHitCallback;
    private System.Action<Target> onEscapedCallback;
    private float lifetime;
    private Coroutine lifeCoroutine;
    private SpriteRenderer spriteRenderer;
    private Collider2D targetCollider;

    private Vector3 hiddenPosition;
    private Vector3 visiblePosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetCollider = GetComponent<Collider2D>();
    }

    public void Initialize(
        System.Action<Target> hitCallback,
        System.Action<Target> escapedCallback,
        float lifeDuration,
        Vector3 visiblePos,
        Vector3 hiddenPos)
    {
        onHitCallback = hitCallback;
        onEscapedCallback = escapedCallback;
        lifetime = lifeDuration;

        hiddenPosition = hiddenPos;
        visiblePosition = visiblePos;

        State = TargetState.Appearing;
        targetCollider.enabled = false;

        transform.position = hiddenPosition;
        SetAlpha(0f);

        StartCoroutine(AppearRoutine());
    }

    private IEnumerator AppearRoutine()
    {
        float elapsed = 0f;
        float duration = 0.4f;
        Vector3 startPos = hiddenPosition;
        Vector3 targetPos = visiblePosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = EaseOutBack(t);

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            SetAlpha(Mathf.Lerp(0f, 1f, t));
            yield return null;
        }

        transform.position = targetPos;
        SetAlpha(1f);

        State = TargetState.Moving;
        targetCollider.enabled = true;

        lifeCoroutine = StartCoroutine(LifeTimer());
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifetime);
        Escape();
    }

    private void Escape()
    {
        if (State != TargetState.Moving && State != TargetState.Appearing)
            return;

        State = TargetState.Escaped;
        targetCollider.enabled = false;

        if (lifeCoroutine != null)
            StopCoroutine(lifeCoroutine);

        StopAllCoroutines();
        StartCoroutine(EscapeRoutine());
    }

    private IEnumerator EscapeRoutine()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = hiddenPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = EaseInBack(t);

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            SetAlpha(Mathf.Lerp(1f, 0f, t));
            yield return null;
        }

        transform.position = targetPos;
        SetAlpha(0f);

        onEscapedCallback?.Invoke(this);
    }

    private float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }

    private void OnMouseDown()
    {
        if (State != TargetState.Moving && State != TargetState.Appearing)
            return;

        State = TargetState.Hit;
        targetCollider.enabled = false;

        if (lifeCoroutine != null)
            StopCoroutine(lifeCoroutine);

        StopAllCoroutines();
        StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = originalScale * (1f + 0.3f * Mathf.Sin(t * Mathf.PI));
            SetAlpha(1f - t);
            transform.position += Vector3.up * Time.deltaTime * 2f;
            yield return null;
        }

        transform.localScale = originalScale;
        SetAlpha(0f);

        onHitCallback?.Invoke(this);
    }

    private void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    public void ResetTarget()
    {
        StopAllCoroutines();
        transform.localScale = Vector3.one;
        SetAlpha(0f);
        targetCollider.enabled = false;
        State = TargetState.Escaped;
    }
}