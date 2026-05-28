using System.Collections;
using UnityEngine;

public class PooledParticleAutoReturn : MonoBehaviour
{
    [SerializeField] private float fallbackLifetime = 2f;

    private ParticleSystem[] particleSystems;
    private Coroutine routine;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    private void OnEnable()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ReturnWhenFinished());
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator ReturnWhenFinished()
    {
        if (particleSystems == null || particleSystems.Length == 0)
        {
            yield return new WaitForSecondsRealtime(fallbackLifetime);
        }
        else
        {
            float elapsed = 0f;

            while (elapsed < fallbackLifetime)
            {
                bool anyAlive = false;

                for (int i = 0; i < particleSystems.Length; i++)
                {
                    if (particleSystems[i] != null && particleSystems[i].IsAlive(true))
                    {
                        anyAlive = true;
                        break;
                    }
                }

                if (!anyAlive)
                    break;

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Return(gameObject);
        else
            gameObject.SetActive(false);

        routine = null;
    }
}