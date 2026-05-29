using UnityEngine;

[CreateAssetMenu(menuName = "Tower Defense/Tower Data", fileName = "TowerData")]
public class TowerData : ScriptableObject
{
    [Header("Identity")]
    public string towerName = "Archer";

    [Header("Economy")]
    [Min(0)] public int cost = 100;

    [Header("Limits")]
    [Tooltip("Maximum number of towers of this type that can be placed at the same time. Use 0 for unlimited.")]
    [Min(0)] public int maxCount = 0;

    [Header("Combat")]
    [Min(0.1f)] public float range = 3f;
    [Min(0.05f)] public float cooldown = 1f;
    [Min(1)] public int damage = 1;
    [Min(0.1f)] public float projectileSpeed = 8f;

    [Header("Special")]
    public TowerAttackMode attackMode = TowerAttackMode.SingleTarget;
    public TowerTargetMode targetMode = TowerTargetMode.Progress;
    [Min(0f)] public float splashRadius = 0f;
    [Range(0.1f, 1f)] public float slowMultiplier = 0.5f;
    [Min(0f)] public float slowDuration = 1.5f;

    [Header("Visuals")]
    public Sprite icon;
    public TowerBase towerPrefab;
    public GameObject projectilePrefab;

    [Header("Audio")]
    public AudioClip attackClip;
}