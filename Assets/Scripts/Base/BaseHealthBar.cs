using UnityEngine;

public class BaseHealthBar : MonoBehaviour
{
    [SerializeField] private BaseHealth baseHealth;
    [SerializeField] private Transform fillTransform;

    private Vector3 initialScale;

    private void Awake()
    {
        if (baseHealth == null)
            baseHealth = GetComponentInParent<BaseHealth>();

        if (fillTransform == null && transform.childCount > 0)
            fillTransform = transform.GetChild(0);

        if (fillTransform != null)
            initialScale = fillTransform.localScale;
    }

    private void Update()
    {
        if (baseHealth == null || fillTransform == null)
            return;

        float hpPercent = Mathf.Clamp01((float)baseHealth.CurrentHP / Mathf.Max(1, baseHealth.MaxHP));

        Vector3 scale = initialScale;
        scale.x *= hpPercent;
        fillTransform.localScale = scale;
    }
}