using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_archer_movement : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    public Transform player;

    [Header("Fark Etme (Aggro) Ayarı")]
    public float detectionRange = 10f; // Karakter 10 birim yakına gelmezse hareket etmez

    [Header("Hareket Ayarları")]
    public float moveSpeed = 3f;
    public float stopDistance = 5f;
    public float meleeDistance = 1.5f;

    [Header("Saldırı Sonrası Rastgele Bekleme")]
    public float minWaitTime = 0.3f; 
    public float maxWaitTime = 0.7f; 

    [Header("Ranged (Ok) Ayarları")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float arrowReleaseDelay = 0.8f;
    public float rangedAnimDuration = 1.167f;
    public float launchForce = 15f;
    public float arcModifier = 0.5f;

    [Header("Melee (Çoklu Vuruş) Ayarları")]
    public Transform meleeAttackPoint;
    public float meleeAttackRange = 0.8f;
    public float damagePerHit = 10f;
    public float meleeAnimDuration = 2.0f;
    public List<float> meleeHitTimings;

    [Header("Bileşenler")]
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D col;

    // Durum Kontrolü
    private bool isDead = false;
    private bool isAttacking = false; 

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        // Mesafeyi hesapla
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // --- YENİ EKLENEN KISIM: 10 Birim Kontrolü ---
        // Eğer oyuncu fark etme menzilinden (10 birim) uzaktaysa HİÇBİR ŞEY YAPMA.
        if (distanceToPlayer > detectionRange)
        {
            StopMoving();
            anim.SetFloat("speed", 0f); // Idle animasyonuna geç
            return; // Aşağıdaki kodları çalıştırma
        }
        // ---------------------------------------------

        if (isAttacking) 
        {
            StopMoving();
            return; 
        }

        FlipTowardsPlayer();

        // 1. DURUM: KOVALA
        if (distanceToPlayer > stopDistance)
        {
            MoveTowardsPlayer();
            anim.SetFloat("speed", moveSpeed);
            anim.SetBool("rangedAttack", false);
            anim.SetBool("meleeAttack", false);
        }
        // 2. DURUM: OK AT (Ranged)
        else if (distanceToPlayer <= stopDistance && distanceToPlayer > meleeDistance)
        {
            StopMoving();
            anim.SetFloat("speed", 0f);
            StartCoroutine(RangedAttackRoutine());
        }
        // 3. DURUM: KILIÇ ÇEK (Melee)
        else if (distanceToPlayer <= meleeDistance)
        {
            StopMoving();
            anim.SetFloat("speed", 0f);
            StartCoroutine(MeleeAttackRoutine());
        }
    }

    IEnumerator MeleeAttackRoutine()
    {
        isAttacking = true; 
        anim.SetBool("meleeAttack", true);
        anim.SetBool("rangedAttack", false);

        float timer = 0f; 
        int hitIndex = 0; 

        if (meleeHitTimings != null && meleeHitTimings.Count > 0)
        {
            meleeHitTimings.Sort();
            while (hitIndex < meleeHitTimings.Count)
            {
                float nextHitTime = meleeHitTimings[hitIndex];
                float waitTime = nextHitTime - timer;

                if (waitTime > 0)
                {
                    yield return new WaitForSeconds(waitTime);
                    timer += waitTime;
                }
                MeleeDamageCalculation();
                hitIndex++;
            }
        }

        float remainingTime = meleeAnimDuration - timer;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        anim.SetBool("meleeAttack", false);

        float randomWait = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(randomWait);

        isAttacking = false; 
    }

    void MeleeDamageCalculation()
    {
        Vector2 attackPos = meleeAttackPoint != null ? meleeAttackPoint.position : transform.position;
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPos, meleeAttackRange);

        foreach (Collider2D hitPlayer in hitPlayers)
        {
            if (hitPlayer.CompareTag("Player"))
            {
                 player_health_system playerHealth = hitPlayer.GetComponent<player_health_system>(); 
                 if (playerHealth != null) playerHealth.TakeDamage(damagePerHit, transform);
            }
        }
    }

    IEnumerator RangedAttackRoutine()
    {
        isAttacking = true; 
        anim.SetBool("rangedAttack", true);
        anim.SetBool("meleeAttack", false);

        yield return new WaitForSeconds(arrowReleaseDelay);
        ShootArrow();
        yield return new WaitForSeconds(rangedAnimDuration - arrowReleaseDelay);

        anim.SetBool("rangedAttack", false);

        float randomWait = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(randomWait);

        isAttacking = false; 
    }

    void ShootArrow()
    {
        if (arrowPrefab != null && firePoint != null && player != null)
        {
            GameObject newArrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D arrowRb = newArrow.GetComponent<Rigidbody2D>();

            if (arrowRb != null)
            {
                Vector2 direction = (player.position - firePoint.position).normalized;
                float distance = Vector2.Distance(firePoint.position, player.position);
                Vector2 arcDirection = new Vector2(direction.x, direction.y + (distance * 0.1f * arcModifier)).normalized;
                arrowRb.linearVelocity = arcDirection * launchForce;
            }
        }
    }

    public void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines(); 

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (col != null) col.enabled = false;
        if (anim != null)
        {
            anim.SetBool("isDead", true);
            anim.SetBool("meleeAttack", false);
            anim.SetBool("rangedAttack", false);
            anim.SetFloat("speed", 0);
        }
        this.enabled = false;
        Manager.Instance.EnemyDied();
        Destroy(gameObject, 1.2f);
    }

    void MoveTowardsPlayer()
    {
        if (isDead) return;
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
    }

    void StopMoving()
    {
        if(rb != null) rb.linearVelocity = Vector2.zero;
    }

    void FlipTowardsPlayer()
    {
        if (isDead || spriteRenderer == null) return;
        if (player.position.x > transform.position.x) spriteRenderer.flipX = false;
        else if (player.position.x < transform.position.x) spriteRenderer.flipX = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance); 
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeDistance); 
        
        // Fark etme mesafesini göstermek için:
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}