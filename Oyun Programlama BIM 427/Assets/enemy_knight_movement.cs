using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy_knight_movement : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    public GameObject Player;
    
    [Header("Hareket Ayarları")]
    [SerializeField] private float speed = 3.5f; // Hızı biraz düşürdüm, daha tok dursun
    [SerializeField] private float chaseDistance = 10f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Saldırı Ayarları")]
    [SerializeField] private int damagePerHit = 10; 
    
    // Animasyon zamanlamaları
    [SerializeField] private float timeToFirstHit = 0.4f;  
    [SerializeField] private float timeToSecondHit = 0.5f; 
    [SerializeField] private float timeToThirdHit = 0.5f;  
    [SerializeField] private float animationEndDelay = 0.5f; 

    [Header("Yapay Zeka (AI) Doğallık Ayarları")]
    [SerializeField] private float minWaitTime = 1.0f; // En az bekleme süresi
    [SerializeField] private float maxWaitTime = 3.0f; // En fazla bekleme süresi
    [Range(0, 100)] [SerializeField] private int defenseChance = 65; // Savunmaya geçme ihtimali (%)

    // Durum Değişkenleri
    private float distance;
    private bool attack;
    private bool defend;
    private bool isDead = false;

    private Coroutine combatCoroutine;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer; 
    private Rigidbody2D _rb;     
    private Collider2D _collider; 

    void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        if (Player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) Player = p;
        }
    }

    void Update()
    {
        if (isDead) return;

        AIChase();
        UpdateAnimations();
    }

    public void ReceiveDamage(float amount)
    {
        if (isDead) return;
        
        if (Player != null && defend)
        {
            bool playerIsToTheRight = Player.transform.position.x > transform.position.x;
            bool facingRight = !_spriteRenderer.flipX;
            bool attackFromFront = (playerIsToTheRight && facingRight) || (!playerIsToTheRight && !facingRight);

            if (attackFromFront)
            {
                Debug.Log("🛡️ Düşman Saldırıyı Blokladı!");
                // Bloklandığında belki biraz geri itilebilir (Knockback) eklenebilir.
                return; 
            }
        }

        health_system myHealth = GetComponent<health_system>();
        if (myHealth != null)
        {
            myHealth.TakeDamage(amount);
            
            // YENİ: Hasar alınca agresifleşsin (Combat coroutine'i resetle)
            // Eğer savunmada değilse ve hasar aldıysa, hemen tepki vermesi için combatı yeniden başlatabiliriz.
            if (!defend && !attack) 
            {
                StopCombat();
                combatCoroutine = StartCoroutine(CombatPattern());
            }
        }
    }

    public void TriggerDeath()
    {
        if (isDead) return; 

        isDead = true;
        StopCombat();

        if (_rb != null)
        {
            _rb.velocity = Vector2.zero;
            _rb.isKinematic = true; 
        }

        if (_collider != null) _collider.enabled = false;
        if (_animator != null) _animator.SetBool("isDead", true);
    }

    private void AIChase()
    {
        if (isDead || Player == null) return;

        distance = Vector2.Distance(transform.position, Player.transform.position);

        if (distance >= chaseDistance)
        {
            StopCombat(); 
            _animator.SetFloat("speed", 0);
            return; 
        }

        // Saldırı veya Savunma anında dönmesin, sadece boşta veya yürürken dönsün
        if (!attack && !defend) 
        {
            FlipSprite();
        }

        if (distance > stopDistance)
        {
            // Eğer savunma yapıyorsa yürümesin, savunmayı bıraksın sonra yürüsün
            if (defend) 
            {
                defend = false; // Oyuncu uzaklaştıysa savunmayı bırakıp kovalasın
            }

            if (!attack) // Saldırırken yürümesin (Kayma sorunu olmaması için)
            {
                transform.position = Vector2.MoveTowards(transform.position, Player.transform.position, speed * Time.deltaTime);
                _animator.SetFloat("speed", speed);
            }
        }
        else 
        {
            _animator.SetFloat("speed", 0);
            if (combatCoroutine == null)
            {
                combatCoroutine = StartCoroutine(CombatPattern());
            }
        }
    }

    private void FlipSprite()
    {
        if (Player.transform.position.x > transform.position.x)
            _spriteRenderer.flipX = false;
        else
            _spriteRenderer.flipX = true;
    }

    // --- EN ÖNEMLİ KISIM: DOĞAL SALDIRI DÖNGÜSÜ ---
    IEnumerator CombatPattern()
    {
        while (true) 
        {
            if (isDead) yield break;

            // 1. KARAR: Saldırıya başla
            attack = true;
            defend = false;

            // Rastgele saldırı sayısı belirle (1 ile 3 arası)
            // 1 gelirse sadece ilk vuruş, 2 gelirse iki vuruş, 3 gelirse full kombo
            int attackCount = Random.Range(1, 4); 

            // -- 1. VURUŞ --
            yield return new WaitForSeconds(timeToFirstHit);
            DealDamage();
            
            // Eğer saldırı sayısı 1'den büyükse devam et
            if (attackCount > 1 && !isDead)
            {
                yield return new WaitForSeconds(timeToSecondHit);
                DealDamage();
            }

            // Eğer saldırı sayısı 2'den büyükse devam et
            if (attackCount > 2 && !isDead)
            {
                yield return new WaitForSeconds(timeToThirdHit);
                DealDamage();
            }

            // Animasyonun bitmesini bekle
            yield return new WaitForSeconds(animationEndDelay);
            
            // Saldırı bitti
            attack = false;

            // 2. KARAR: Sırada ne yapacak? (Savunma mı? Dinlenme mi?)
            // %65 ihtimalle savunma, %35 ihtimalle boş bekleme (açık verme)
            int roll = Random.Range(0, 100);

            if (roll < defenseChance)
            {
                // Savunma Modu
                defend = true;
            }
            else
            {
                // Agresif/Dikkatsiz Mod (Savunma açmıyor, sadece bekliyor)
                defend = false;
            }

            // 3. KARAR: Ne kadar bekleyecek?
            // Her seferinde sabit 2 saniye değil, rastgele bir süre (örn: 1.2 sn ile 2.8 sn arası)
            float waitDuration = Random.Range(minWaitTime, maxWaitTime);
            
            yield return new WaitForSeconds(waitDuration);

            // Döngü başa döner...
        }
    }

    private void DealDamage()
    {
        if (isDead || Player == null) return;

        // Vuruş anında oyuncuya dönük olsun (Son dakika düzeltmesi)
        FlipSprite(); 

        float actualDistance = Vector2.Distance(transform.position, Player.transform.position);

        if (actualDistance <= stopDistance + 1.2f)
        {
            player_health_system playerHealth = Player.GetComponent<player_health_system>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damagePerHit);
            }
        }
    }

    private void StopCombat()
    {
        if (combatCoroutine != null)
        {
            StopCoroutine(combatCoroutine);
            combatCoroutine = null;
        }
        attack = false;
        defend = false;
    }

    private void UpdateAnimations()
    {
        if (isDead) return;
        _animator.SetBool("attack", attack);
        _animator.SetBool("defend", defend);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}