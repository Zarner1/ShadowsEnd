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
    public GameObject arrowPrefab; // Unity'de oluşturduğun ok prefabı
    public Transform firePoint;    // Okun çıkacağı nokta
    public float fireRate = 1.5f;  // Atış hızı
    private float nextFireTime = 0f;

    [Header("Ok Fizik Ayarları (YENİ)")]
    public float launchForce = 15f;  // Fırlatma gücü
    public float arcModifier = 0.5f; // Havaya dikme oranı (Kavis miktarı)

    [Header("Bileşenler")]
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;      
    private Collider2D col;      

    // --- Ölüm Kontrolü ---
    private bool isDead = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        FlipTowardsPlayer();

        // 1. DURUM: Karakter çok uzakta -> KOVALA
        if (distanceToPlayer > stopDistance)
        {
            MoveTowardsPlayer();
            
            anim.SetFloat("speed", moveSpeed); 
            anim.SetBool("meleeAttack", false);
            anim.SetBool("rangedAttack", false);
        }
        // 2. DURUM: Menzilde -> OK AT (Ranged)
        else if (distanceToPlayer <= stopDistance && distanceToPlayer > meleeDistance)
        {
            StopMoving();

            anim.SetFloat("speed", 0f);
            anim.SetBool("rangedAttack", true);
            anim.SetBool("meleeAttack", false);

            // Ateş Etme Mantığı
            if (Time.time >= nextFireTime)
            {
                ShootArrow();
                nextFireTime = Time.time + fireRate;
            }
        }
        // 3. DURUM: Çok yakında -> KILIÇ ÇEK (Melee)
        else if (distanceToPlayer <= meleeDistance)
        {
            StopMoving();

            anim.SetFloat("speed", 0f);
            anim.SetBool("meleeAttack", true);
            anim.SetBool("rangedAttack", false);
        }
    }

    // --- GÜNCELLENMİŞ: Eğimli Atış Fonksiyonu ---
    void ShootArrow()
    {
        if (arrowPrefab != null && firePoint != null && player != null)
        {
            // 1. Oku oluştur
            GameObject newArrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
            
            // 2. Okun Rigidbody'sine ulaş
            Rigidbody2D arrowRb = newArrow.GetComponent<Rigidbody2D>();

            if (arrowRb != null)
            {
                // 3. Yön Hesaplama
                // Oyuncuya olan düz vektörü bul
                Vector2 direction = (player.position - firePoint.position).normalized;

                // Mesafeye göre ne kadar havaya dikeceğimizi hesapla
                float distance = Vector2.Distance(firePoint.position, player.position);
                
                // Y eksenine (yukarı) biraz ekleme yapıyoruz (arcModifier ile kontrol edilir)
                // 0.1f çarpanı mesafenin etkisini dengeler
                Vector2 arcDirection = new Vector2(direction.x, direction.y + (distance * 0.1f * arcModifier)).normalized;

                // 4. Gücü Uygula
                arrowRb.velocity = arcDirection * launchForce;
            }
        }
        else
        {
            Debug.LogWarning("Arrow Prefab, Fire Point veya Player eksik!");
        }
    }

    public void TriggerDeath()
    {
        if (isDead) return;

        isDead = true;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
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
        if(rb != null) rb.velocity = Vector2.zero;
    }

    void FlipTowardsPlayer()
    {
        if (isDead) return;
        if (spriteRenderer == null) return;

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