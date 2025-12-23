using UnityEngine;

public class enemy_wizard_movement : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    public Transform player;
    public float detectionRange = 10f; // 10 birim saldırı menzili

    [Header("Saldırı Ayarları")]
    public GameObject fireballPrefab;   // Fırlatılacak alev topu prefabı
    public Transform firePoint;         // Alev topunun çıkış noktası
    
    // --- DEĞİŞİKLİK BURADA: Rastgele Aralıklar ---
    public float minAttackCooldown = 1.5f; // En az bekleme süresi
    public float maxAttackCooldown = 3.5f; // En fazla bekleme süresi
    // ---------------------------------------------
    
    private float nextAttackTime = 0f;

    [Header("Bileşenler")]
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // --- MENZİL KONTROLÜ ---
        if (distance <= detectionRange)
        {
            FacePlayer();
            anim.SetBool("isInRange", true);

            // Saldırı zamanı geldiyse
            if (Time.time >= nextAttackTime)
            {
                Attack();
                
                // --- DEĞİŞİKLİK: Bir sonraki saldırı için RASTGELE süre ekle ---
                float randomWait = Random.Range(minAttackCooldown, maxAttackCooldown);
                nextAttackTime = Time.time + randomWait;
                // ---------------------------------------------------------------
            }
        }
        else
        {
            anim.SetBool("isInRange", false);
        }
    }

    void Attack()
    {
        anim.SetTrigger("Attack");

        if (fireballPrefab != null && firePoint != null)
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Alev topunu oluştur (enemy_fireball scripti ile düz gidecek)
            Instantiate(fireballPrefab, firePoint.position, Quaternion.Euler(0f, 0f, angle));
        }
    }

    void FacePlayer()
    {
        if (player.position.x > transform.position.x)
            spriteRenderer.flipX = false; 
        else
            spriteRenderer.flipX = true;
    }

    public void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;

        anim.SetBool("isDie", true);
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        this.enabled = false; 
        Manager.Instance.EnemyDied();
        Destroy(gameObject, 1.2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}