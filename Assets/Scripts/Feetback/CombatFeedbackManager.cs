using UnityEngine;

public class CombatFeedbackManager : MonoBehaviour
{
    public static CombatFeedbackManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip towerShotClip;
    [SerializeField] private AudioClip projectileHitClip;
    [SerializeField] private AudioClip enemyDeathClip;
    [SerializeField] private AudioClip baseHitClip;
    [SerializeField] private AudioClip baseDestroyClip;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject towerShotVfxPrefab;
    [SerializeField] private GameObject projectileHitVfxPrefab;
    [SerializeField] private GameObject enemyDeathVfxPrefab;
    [SerializeField] private GameObject baseHitVfxPrefab;
    [SerializeField] private GameObject baseDestroyVfxPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayTowerShot(Vector3 position)
    {
        PlayFeedback(towerShotClip, towerShotVfxPrefab, position);
    }

    public void PlayProjectileHit(Vector3 position)
    {
        PlayFeedback(projectileHitClip, projectileHitVfxPrefab, position);
    }

    public void PlayEnemyDeath(Vector3 position)
    {
        PlayFeedback(enemyDeathClip, enemyDeathVfxPrefab, position);
    }

    public void PlayBaseHit(Vector3 position)
    {
        PlayFeedback(baseHitClip, baseHitVfxPrefab, position);
    }

    public void PlayBaseDestroyed(Vector3 position)
    {
        PlayFeedback(baseDestroyClip, baseDestroyVfxPrefab, position);
    }

    private void PlayFeedback(AudioClip clip, GameObject prefab, Vector3 position)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }

        if (prefab == null)
            return;

        GameObject instance = null;

        if (ObjectPool.Instance != null)
        {
            instance = ObjectPool.Instance.Get(prefab, position, Quaternion.identity);
        }
        else
        {
            instance = Instantiate(prefab, position, Quaternion.identity);
        }

        if (instance == null)
            return;

        if (instance.TryGetComponent<PooledParticleAutoReturn>(out _))
            return;

        instance.AddComponent<PooledParticleAutoReturn>();
    }
}