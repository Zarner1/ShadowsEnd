using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_archer_movement : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    public Transform player; 

    [Header("Hareket Ayarları")]
    public float moveSpeed = 3f; 
    public float stopDistance = 5f; 
    public float meleeDistance = 1.5f; 

    [Header("Saldırı Ayarları")]
    public GameObject arrowPrefab; 
    public Transform firePoint;    
    public float attackCooldown = 3.0f;  // İsteğin üzerine 3 saniye yapıldı
    private float nextFireTime = 0f;

    [Header("Animasyon Zamanlaması")]
    public float arrowReleaseDelay = 0.8f; // Animasyon başladıktan kaç sn sonra ok çıksın? (Deneyerek bulabilirsin)
    public float totalAnimDuration = 1.167f; // Senin belirttiğin animasyon süresi

    [Header("Ok Fizik Ayarları")]
    public float launchForce = 15f;  
    public float arcModifier = 0.5f; 

    [Header("Bileşenler")]
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;       
    private Collider2D col;       

    // Durum Kontrolü
    private bool isDead = false;
    private bool isAttacking = false; // Şu an saldırı animasyonu oynuyor mu?

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

        // Eğer şu an saldırı yapıyorsa (animasyon oynuyorsa) hareket etmesin veya dönmesin
        if (isAttacking) 
        {
            StopMoving();
            return; 
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        FlipTowardsPlayer();

        // 1. DURUM: Karakter çok uzakta -> KOVALA
        if (distanceToPlayer > stopDistance)
        {
            MoveTowardsPlayer();
            
            anim.SetFloat("speed", moveSpeed); 
            anim.SetBool("rangedAttack", false);
            anim.SetBool("meleeAttack", false);
        }
        // 2. DURUM: Menzilde -> OK AT (Ranged)
        else if (distanceToPlayer <= stopDistance && distanceToPlayer > meleeDistance)
        {
            StopMoving();
            anim.SetFloat("speed", 0f);

            // Zamanı geldiyse saldırıyı başlat
            if (Time.time >= nextFireTime)
            {
                StartCoroutine(RangedAttackRoutine());
                // Bir sonraki saldırı için şu an + 3 saniye ekle
                nextFireTime = Time.time + attackCooldown;
            }
        }
        // 3. DURUM: Çok yakında -> KILIÇ ÇEK (Melee)
        else if (distanceToPlayer <= meleeDistance)
        {
            StopMoving();
            anim.SetFloat("speed", 0f);
            
            // Melee için de benzer bir zamanlama eklenebilir ama şimdilik bool ile bıraktım
            anim.SetBool("meleeAttack", true);
            anim.SetBool("rangedAttack", false);
        }
    }

    // --- YENİ: Saldırı Rutini (Coroutine) ---
    IEnumerator RangedAttackRoutine()
    {
        isAttacking = true; // Saldırı başladı, hareketi kilitle

        // 1. Animasyonu başlat
        anim.SetBool("rangedAttack", true);
        anim.SetBool("meleeAttack", false);

        // 2. Okun yaydan fırlama anına kadar bekle (Örn: animasyonun 8. karesi)
        yield return new WaitForSeconds(arrowReleaseDelay);

        // 3. Oku fiziksel olarak oluştur
        ShootArrow();

        // 4. Animasyonun geri kalanının bitmesini bekle
        // (Toplam süre - geçen süre)
        yield return new WaitForSeconds(totalAnimDuration - arrowReleaseDelay);

        // 5. Animasyonu kapat ve hareketi serbest bırak
        anim.SetBool("rangedAttack", false);
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

                // Unity 6+ linearVelocity, eski sürümler için velocity
                arrowRb.linearVelocity = arcDirection * launchForce;
            }
        }
    }

    public void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;
        // Ölürse tüm coroutine'leri durdur (Saldırı yarıda kalsın)
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

        if (player.position.x > transform.position.x)
            spriteRenderer.flipX = false;
        else if (player.position.x < transform.position.x)
            spriteRenderer.flipX = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance); 
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeDistance); 
    }
}