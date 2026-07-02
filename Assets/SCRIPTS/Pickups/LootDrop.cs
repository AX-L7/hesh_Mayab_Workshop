using UnityEngine;

public class LootDrop : MonoBehaviour
{
    [Header("Loot Table")]
    [SerializeField] LootEntry[] lootTable;

    [Header("Drop Settings")]
    [SerializeField] float dropForce = 3f;
    [SerializeField] float dropUpForce = 5f;

    [System.Serializable]
    public class LootEntry
    {
        public GameObject pickupPrefab;
        [Range(0f, 1f)] public float dropChance = 0.3f;
    }

    public void DropLoot()
    {
        if (lootTable == null) return;

        foreach (LootEntry entry in lootTable)
        {
            if (Random.value > entry.dropChance) continue;
            if (entry.pickupPrefab == null) continue;

            Vector3 offset = Random.insideUnitSphere * 0.5f;
            offset.y = Mathf.Abs(offset.y);
            GameObject pickup = Instantiate(entry.pickupPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);

            Rigidbody rb = pickup.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 force = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized;
                rb.AddForce(force * dropForce + Vector3.up * dropUpForce, ForceMode.Impulse);
            }
        }
    }
}
