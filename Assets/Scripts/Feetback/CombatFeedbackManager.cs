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
    [SerializeField] private AudioClip towerPlaceClip;
    [SerializeField] private AudioClip newRoundClip;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioClip defeatClip;

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

    public void PlayClip(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlayTowerShot(Vector3 position)
    {
        PlayClip(towerShotClip);
        PlayVfx(towerShotVfxPrefab, position);
    }

    public void PlayProjectileHit(Vector3 position)
    {
        PlayClip(projectileHitClip);
        PlayVfx(projectileHitVfxPrefab, position);
    }

    public void PlayEnemyDeath(Vector3 position)
    {
        PlayClip(enemyDeathClip);
        PlayVfx(enemyDeathVfxPrefab, position);
    }

    public void PlayBaseHit(Vector3 position)
    {
        PlayClip(baseHitClip);
        PlayVfx(baseHitVfxPrefab, position);
    }

    public void PlayBaseDestroyed(Vector3 position)
    {
        PlayClip(baseDestroyClip);
        PlayVfx(baseDestroyVfxPrefab, position);
    }

    public void PlayTowerPlace()
    {
        PlayClip(towerPlaceClip);
    }

    public void PlayNewRound()
    {
        PlayClip(newRoundClip);
    }

    public void PlayVictory()
    {
        PlayClip(victoryClip);
    }

    public void PlayDefeat()
    {
        PlayClip(defeatClip);
    }

    private void PlayVfx(GameObject prefab, Vector3 position)
    {
        if (prefab == null)
            return;

        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Get(prefab, position, Quaternion.identity);
        else
            Instantiate(prefab, position, Quaternion.identity);
    }
}