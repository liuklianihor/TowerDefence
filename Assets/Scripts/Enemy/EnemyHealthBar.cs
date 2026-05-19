using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Transform fillTransform;

    private Vector3 initialScale;

    private void Awake()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<EnemyHealth>();

        if (fillTransform == null)
            fillTransform = transform;

        initialScale = fillTransform.localScale;
    }

    private void Update()
    {
        if (enemyHealth == null)
            return;

        float hpPercent =
            (float)enemyHealth.CurrentHP / enemyHealth.MaxHP;

        hpPercent = Mathf.Clamp01(hpPercent);

        Vector3 scale = initialScale;
        scale.x *= hpPercent;

        fillTransform.localScale = scale;
    }
}