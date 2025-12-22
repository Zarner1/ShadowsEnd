using System.Collections;
using UnityEngine;

public class enemy_king_movement : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    public Transform player;
    public float detectionRange = 15f; 

    [Header("Hareket Ayarları")]
    public float moveSpeed = 4f;       
    public float stopDistance = 1.5f;  

    [Header("Saldırı Zamanlamaları (Toplam Süreler)")]
    public float normalAttackDuration = 2.5f; 
    public float hardAttackDuration = 5.0f;   
    public float teleportDuration = 1.5f;     

    [Header("Normal Saldırı (Alan Hasarı - Bloklanamaz)")]
    public int normalDamage = 25;
    public float normalHitTime = 1.5f; // Animasyonun kaçıncı saniyesinde vursun?
    public float areaDamageRadius = 1.0f; 
    public Transform attackPoint;         

    [Header("Ağır Saldırı (2 Hamleli - Bloklanabilir)")]
    // 1. Hamle Ayarları
    public int hardHit1Damage = 20;
    public float hardHit1Time = 2.0f; // 5 saniyelik animasyonun 2. saniyesinde vursun
    
    // 2. Hamle Ayarları (Bitirici)
    public int hardHit2Damage = 40;
    public float hardHit2Time = 4.0f; // 5 saniyelik animasyonun 4. saniyesinde vursun
    
    // İtme Gücü (2. vuruşta daha fazla itecek)
    public float hardAttackKnockback = 10f; 

    [Header("Zamanlama Ayarları")]
    public float actionCooldown = 2.0f; 
    private float nextActionTime = 0f;

    // Durum Değişkenleri
    private bool isDead = false;
    private bool isBusy = false; 
    private bool isAwake = false;

    [Header("Bileşenler")]
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D col;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (isDead || player == null) return;
        if (isBusy) { StopMoving(); return; }

        float distance = Vector2.Distance(transform.position, player.position);

        if (!isAwake)
        {
            if (distance <= detectionRange) isAwake = true;
            else { StopMoving(); anim.SetFloat("isWalking", 0f); return; }
        }

        FlipTowardsPlayer();

        if (distance > stopDistance)
        {
            MoveTowardsPlayer();
            anim.SetFloat("isWalking", 1f);
        }
        else
        {
            StopMoving();
            anim.SetFloat("isWalking", 0f);
            if (Time.time >= nextActionTime) DecideNextAction();
        }
    }

    void DecideNextAction()
    {
        int roll = Random.Range(0, 100);
        if (roll < 40) StartCoroutine(NormalAttackRoutine());
        else if (roll < 70) StartCoroutine(HardAttackRoutine());
        else StartCoroutine(TeleportRoutine());
    }

    // --- 1. NORMAL SALDIRI (TEK HAMLE - ALAN HASARI) ---
    IEnumerator NormalAttackRoutine()
    {
        isBusy = true; 
        anim.SetBool("isAttacking", true);
        anim.SetFloat("isWalking", 0f);

        // Vuruş anına kadar bekle
        yield return new WaitForSeconds(normalHitTime);
        
        DealAreaDamageUnblockable(); 

        // Animasyonun geri kalanını bekle
        yield return new WaitForSeconds(normalAttackDuration - normalHitTime);

        anim.SetBool("isAttacking", false);
        isBusy = false; 
        nextActionTime = Time.time + actionCooldown;
    }

    // --- 2. AĞIR SALDIRI (İKİ HAMLELİ) ---
    IEnumerator HardAttackRoutine()
    {
        isBusy = true; 
        anim.SetBool("isHardAttacking", true);
        anim.SetFloat("isWalking", 0f);

        // --- 1. HAMLE ---
        // İlk vuruş zamanına kadar bekle (Örn: 2.0 sn)
        yield return new WaitForSeconds(hardHit1Time);
        
        // İlk hasarı ver (Normal itme gücü)
        DealDamageWithKnockback(hardHit1Damage, 3.5f, hardAttackKnockback);

        // --- 2. HAMLE ---
        // İkinci vuruş zamanı ile birinci arasındaki fark kadar bekle
        // (Örn: 4.0 - 2.0 = 2.0 sn daha bekle)
        yield return new WaitForSeconds(hardHit2Time - hardHit1Time);

        // İkinci hasarı ver (Daha yüksek hasar + %50 Daha fazla itme gücü)
        DealDamageWithKnockback(hardHit2Damage, 3.5f, hardAttackKnockback * 1.5f);

        // --- BİTİŞ ---
        // Animasyonun toplam süresinden kalan zamanı bekle
        yield return new WaitForSeconds(hardAttackDuration - hardHit2Time);

        anim.SetBool("isHardAttacking", false);
        isBusy = false; 
        // Kral yorulduğu için biraz daha uzun bekle
        nextActionTime = Time.time + actionCooldown + 1.0f; 
    }

    // --- 3. IŞINLANMA ---
    IEnumerator TeleportRoutine()
    {
        isBusy = true; 
        anim.SetBool("isTeleporting", true);
        if(col != null) col.enabled = false; 

        yield return new WaitForSeconds(teleportDuration * 0.5f);

        Vector2 teleportPos;
        float offset = (player.localScale.x > 0) ? -2f : 2f; 
        teleportPos = new Vector2(player.position.x + offset, transform.position.y);
        transform.position = teleportPos;

        yield return new WaitForSeconds(teleportDuration * 0.5f);

        if(col != null) col.enabled = true;
        anim.SetBool("isTeleporting", false);
        isBusy = false; 
        nextActionTime = Time.time + 0.5f; 
    }

    // --- HASAR FONKSİYONLARI ---

    void DealAreaDamageUnblockable()
    {
        Vector2 center = (attackPoint != null) ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(center, areaDamageRadius);

        foreach (Collider2D obj in hitObjects)
        {
            if (obj.CompareTag("Player"))
            {
                player_health_system playerHealth = obj.GetComponent<player_health_system>();
                if (playerHealth != null)
                {
                    // attacker = null -> Bloklanamaz
                    playerHealth.TakeDamage(normalDamage, null); 
                }
            }
        }
    }

    void DealDamageWithKnockback(int dmgAmount, float range, float knockback)
    {
        if (player != null && Vector2.Distance(transform.position, player.position) <= range)
        {
            player_health_system playerHealth = player.GetComponent<player_health_system>();
            if (playerHealth != null)
            {
                // Hasar, Saldırgan, İtme Gücü -> Bloklanabilir ve Fırlatır
                playerHealth.TakeDamage(dmgAmount, transform, knockback);
            }
        }
    }

    // --- HAREKET VE FİZİK ---

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
        if (isBusy) return;

        // Eğer AttackPoint atanmamışsa hata vermemesi için kontrol
        if (attackPoint == null) return;

        // AttackPoint'in merkeze olan uzaklığını al (Mutlak değer kullanıyoruz ki hep pozitif olsun)
        float distanceFromCenter = Mathf.Abs(attackPoint.localPosition.x);

        if (player.position.x > transform.position.x)
        {
            // --- SAĞA BAKIYOR ---
            spriteRenderer.flipX = false;
            
            // AttackPoint'i sağ tarafa al (Pozitif X)
            attackPoint.localPosition = new Vector3(distanceFromCenter, attackPoint.localPosition.y, attackPoint.localPosition.z);
        }
        else
        {
            // --- SOLA BAKIYOR ---
            spriteRenderer.flipX = true;

            // AttackPoint'i sol tarafa al (Negatif X)
            attackPoint.localPosition = new Vector3(-distanceFromCenter, attackPoint.localPosition.y, attackPoint.localPosition.z);
        }
    }

    public void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;
        isBusy = true; 

        StopAllCoroutines();
        StopMoving();

        if (col != null) col.enabled = false;
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        anim.SetBool("isDead", true);
        anim.SetFloat("isWalking", 0f);
        anim.SetBool("isAttacking", false);
        anim.SetBool("isHardAttacking", false);
        anim.SetBool("isTeleporting", false);
        
        this.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = new Color(1, 0.5f, 0); 
        Vector2 center = (attackPoint != null) ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Gizmos.DrawWireSphere(center, areaDamageRadius);
    }
}