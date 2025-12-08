using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy_kngiht_movement : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    [SerializeField] private GameObject Player;
    
    [Header("Hareket Ayarları")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float chaseDistance = 10f;
    [SerializeField] private float stopDistance = 1.5f;

    // Durum Değişkenleri
    private float distance;
    private bool attack;
    private bool defend;

    // Coroutine Referansı
    private Coroutine combatCoroutine;

    // Component referansları
    private Animator _animator;
    private SpriteRenderer _spriteRenderer; 

    void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Player == null) return;

        AIChase();
        UpdateAnimations();
    }

    private void AIChase()
    {
        distance = Vector2.Distance(transform.position, Player.transform.position);

        // SENARYO 1: Düşman çok uzakta (Görmüyor)
        if (distance >= chaseDistance)
        {
            StopCombat(); 
            _animator.SetFloat("speed", 0);
            return; // Uzaktaysa fonksiyondan çık, aşağıdaki kodlar çalışmasın
        }

        // --- YÖN DÖNDÜRME (Buraya eklendi) ---
        // Düşman seni gördüğü andan itibaren sana dönmeli
        FlipSprite(); 

        // SENARYO 2: Düşman görüyor ama vuracak kadar yakın değil (Kovalama)
        if (distance > stopDistance)
        {
            StopCombat(); 
            transform.position = Vector2.MoveTowards(transform.position, Player.transform.position, speed * Time.deltaTime);
            _animator.SetFloat("speed", speed);
        }
        // SENARYO 3: Düşman dibine girdi (Savaş Başlasın!)
        else 
        {
            _animator.SetFloat("speed", 0);

            if (combatCoroutine == null)
            {
                combatCoroutine = StartCoroutine(CombatPattern());
            }
        }
    }

    // Karakterin yönünü oyuncuya çeviren fonksiyon
    private void FlipSprite()
    {
        // Player'ın X pozisyonu, Düşman'ın X pozisyonundan büyükse (Player sağdaysa)
        if (Player.transform.position.x > transform.position.x)
        {
            // SpriteRenderer'ın flipX'ini kapat (Sağa baksın)
            _spriteRenderer.flipX = false;
        }
        else
        {
            // Player soldaysa, flipX'i aç (Sola baksın)
            _spriteRenderer.flipX = true;
        }
    }

    IEnumerator CombatPattern()
    {
        while (true) 
        {
            // 1. SALDIRI MODU
            attack = true;
            defend = false;
            yield return new WaitForSeconds(3.5f); 

            // 2. SAVUNMA MODU
            attack = false;
            defend = true;
            yield return new WaitForSeconds(2.0f); 
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