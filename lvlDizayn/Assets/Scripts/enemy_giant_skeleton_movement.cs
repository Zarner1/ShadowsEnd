using System.Collections;
using UnityEngine;

public class enemy_giant_skeleton_movement : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    public Transform player;

    [Header("Yapay Zeka (AI) Ayarları")]
    public float detectionRange = 10f; // 10 birimden önce uyanmaz
    public float moveSpeed = 1.5f;     // Hantal, yavaş yürüyüş
    public float stopDistance = 2.0f;  // Oyuncunun dibine kadar girmesin

    [Header("Saldırı Ayarları")]
    public int attackDamage = 40;       // Yüksek hasar
    
    // --- DEĞİŞİKLİK BURADA: Sabit süre yerine aralık ---
    public float minAttackCooldown = 2.0f; // En az ne kadar beklesin?
    public float maxAttackCooldown = 4.5f; // En fazla ne kadar beklesin?
    // ---------------------------------------------------

    private float nextAttackTime = 0f;
    private bool isAwake = false;      
    private bool isAttacking = false;
    private bool isDead = false;

    [Header("Bileşenler")]
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

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

        // --- 1. UYANMA KONTROLÜ ---
        if (!isAwake)
        {
            if (distance <= detectionRange) isAwake = true;
            else
            {
                StopMoving();
                anim.SetBool("isRunning", false);
                return;
            }
        }

        // --- 2. SALDIRI HALİNDEYSE ---
        if (isAttacking)
        {
            StopMoving();
            anim.SetBool("isRunning", false);
            return;
        }

        FlipTowardsPlayer();

        // --- 3. HAREKET VE SALDIRI ---
        if (distance > stopDistance)
        {
            MoveTowardsPlayer();
            anim.SetBool("isRunning", true);
            anim.SetBool("isAttacking", false);
        }
        else
        {
            StopMoving();
            anim.SetBool("isRunning", false);

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(HeavyAttackRoutine());
            }
        }
    }

    IEnumerator HeavyAttackRoutine()
    {
        isAttacking = true;

        // Animasyon parametrelerini ayarla
        anim.SetBool("isAttacking", true); 
        anim.SetBool("isRunning", false);
        anim.SetTrigger("Attack"); 
        
        // Animasyonun bitmesini bekle (Tahmini animasyon süresi)
        yield return new WaitForSeconds(1.0f); 

        // --- DEĞİŞİKLİK BURADA: Rastgele Soğuma Süresi ---
        float randomCooldown = Random.Range(minAttackCooldown, maxAttackCooldown);
        nextAttackTime = Time.time + randomCooldown;
        // -------------------------------------------------

        isAttacking = false;
        anim.SetBool("isAttacking", false);
    }

    // Animasyon Event'i bu fonksiyonu çağıracak
    public void HitTarget()
    {
        DealDamage();
    }

    void DealDamage()
    {
        if (player != null && Vector2.Distance(transform.position, player.position) <= stopDistance + 1.5f)
        {
            player_health_system playerHealth = player.GetComponent<player_health_system>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage, transform);
            }
        }
    }

    void MoveTowardsPlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }

    void StopMoving()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    void FlipTowardsPlayer()
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

        StopAllCoroutines();
        StopMoving();

        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        anim.SetBool("isDie", true);     
        anim.SetBool("isRunning", false); 
        anim.SetBool("isAttacking", false);
        
        this.enabled = false; 
        Manager.Instance.EnemyDied();
        Destroy(gameObject, 1.2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}