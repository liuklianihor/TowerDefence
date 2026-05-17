using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.6f, 0f);

    private Transform target;
    private Camera mainCamera;

    public void Bind(Transform targetTransform)
    {
        target = targetTransform;
        mainCamera = Camera.main;
    }

    public void SetValue(float normalizedValue)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(normalizedValue);
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        transform.position = target.position + offset;

        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
    }
}