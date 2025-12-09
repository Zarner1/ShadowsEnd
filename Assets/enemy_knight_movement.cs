using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy_knight_movement : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    public GameObject Player;
    
    [Header("Hareket Ayarları")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float chaseDistance = 10f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Saldırı Ayarları")]
    [SerializeField] private int damagePerHit = 5; 
    
    // Animasyonun içindeki vuruş zamanlamaları
    [SerializeField] private float timeToFirstHit = 0.4f;  
    [SerializeField] private float timeToSecondHit = 0.5f; 
    [SerializeField] private float timeToThirdHit = 0.5f;  
    [SerializeField] private float animationEndDelay = 0.5f; 

    // Durum Değişkenleri
    private float distance;
    private bool attack;
    private bool defend; // Savunma durumunu tutan değişken

    // --- YENİ: Ölüm Kontrolü ---
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
    }

    void Update()
    {
        if (isDead) return;

        if (Player == null) return;
        AIChase();
        UpdateAnimations();
    }

    // --- YENİ: Oyuncu vurduğunda bu fonksiyon çalışacak ---
    public void ReceiveDamage(float amount)
    {
        // 1. Eğer ölü ise hasar alamaz
        if (isDead) return;

        // 2. Eğer SAVUNMA yapıyorsa hasarı engelle
        if (defend)
        {
            Debug.Log("Düşman Saldırıyı Blokladı!");
            // İstersen buraya bir bloklama sesi veya efekti ekleyebilirsin
            return; 
        }

        // 3. Savunma yoksa canı azalt
        // Düşmanın kendi üzerindeki health_system scriptini bul
        health_system myHealth = GetComponent<health_system>();
        if (myHealth != null)
        {
            myHealth.TakeDamage(amount);
            
            // Opsiyonel: Hasar alma animasyonunu tetikle
            // if(_animator != null) _animator.SetTrigger("Hurt");
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

        if (_collider != null)
        {
            _collider.enabled = false;
        }

        if (_animator != null)
        {
            _animator.SetBool("isDead", true);
        }
    }

    private void AIChase()
    {
        if (isDead) return;
        if (Player == null) return;

        distance = Vector2.Distance(transform.position, Player.transform.position);

        if (distance >= chaseDistance)
        {
            StopCombat(); 
            _animator.SetFloat("speed", 0);
            return; 
        }

        FlipSprite(); 

        if (distance > stopDistance)
        {
            StopCombat(); 
            transform.position = Vector2.MoveTowards(transform.position, Player.transform.position, speed * Time.deltaTime);
            _animator.SetFloat("speed", speed);
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
        if (isDead) return; 

        if (Player == null) return;
        if (Player.transform.position.x > transform.position.x)
            _spriteRenderer.flipX = false;
        else
            _spriteRenderer.flipX = true;
    }

    IEnumerator CombatPattern()
    {
        while (true) 
        {
            if (isDead) yield break;

            // SALDIRI MODU
            attack = true;
            defend = false;

            for (int i = 0; i < 2; i++)
            {
                if (isDead) yield break; 

                yield return new WaitForSeconds(timeToFirstHit); 
                DealDamage();

                yield return new WaitForSeconds(timeToSecondHit);
                DealDamage();

                yield return new WaitForSeconds(timeToThirdHit);
                DealDamage();

                yield return new WaitForSeconds(animationEndDelay);
            }

            // SAVUNMA MODU (Bu süre içinde ReceiveDamage gelirse hasar almayacak)
            attack = false;
            defend = true; 
            
            yield return new WaitForSeconds(2.0f); 
        }
    }

    private void DealDamage()
    {
        if (isDead) return; 
        if (Player == null) return;

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