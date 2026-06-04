using UnityEngine;
using System.Collections.Generic;

public class TargetBuilder : MonoBehaviour
{
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private Transform spawnParent;
    [SerializeField] private int poolSize = 8;
    [SerializeField] private float minLifeTime = 2.0f;
    [SerializeField] private float maxLifeTime = 4.0f;

    [Header("Spawn Area")]
    [SerializeField] private float spawnMinX = -7f;
    [SerializeField] private float spawnMaxX = 7f;
    [SerializeField] private float visibleY = 1f;
    [SerializeField] private float hiddenY = -3f;

    private Queue<Target> pool = new Queue<Target>();
    private List<Target> activeTargets = new List<Target>();
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = GetComponent<GameManager>();
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewTarget();
        }
    }

    private void CreateNewTarget()
    {
        GameObject obj = Instantiate(targetPrefab, spawnParent);
        obj.SetActive(false);
        Target target = obj.GetComponent<Target>();
        pool.Enqueue(target);
    }

    public Target Build()
    {
        if (pool.Count == 0)
        {
            CreateNewTarget();
        }

        Target target = pool.Dequeue();
        target.gameObject.SetActive(true);
        activeTargets.Add(target);

        float lifeTime = Random.Range(minLifeTime, maxLifeTime);
        float randomX = Random.Range(spawnMinX, spawnMaxX);

        Vector3 visiblePos = new Vector3(randomX, visibleY, 0f);
        Vector3 hiddenPos = new Vector3(randomX, hiddenY, 0f);

        target.Initialize(
            hitCallback: OnTargetHit,
            escapedCallback: OnTargetEscaped,
            lifeDuration: lifeTime,
            visiblePos: visiblePos,
            hiddenPos: hiddenPos
        );

        return target;
    }

    private void OnTargetHit(Target target)
    {
        activeTargets.Remove(target);
        gameManager.OnTargetHit();
        StartCoroutine(ReturnToPoolDelayed(target, 0.5f));
    }

    private void OnTargetEscaped(Target target)
    {
        activeTargets.Remove(target);
        StartCoroutine(ReturnToPoolDelayed(target, 0.3f));
    }

    private System.Collections.IEnumerator ReturnToPoolDelayed(Target target, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(target);
    }

    public void ReturnToPool(Target target)
    {
        target.ResetTarget();
        target.gameObject.SetActive(false);
        pool.Enqueue(target);
    }

    public void ReturnAllToPool()
    {
        foreach (var target in activeTargets.ToArray())
        {
            target.ResetTarget();
            target.gameObject.SetActive(false);
            pool.Enqueue(target);
        }
        activeTargets.Clear();
    }
}