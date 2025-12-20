using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy_knight_movement : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    public GameObject Player;
    
    [Header("Hareket Ayarları")]
    [SerializeField] private float speed = 3.5f; 
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
    [SerializeField] private float minWaitTime = 1.0f; 
    [SerializeField] private float maxWaitTime = 3.0f; 
    [Range(0, 100)] [SerializeField] private int defenseChance = 65; 

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
                Debug.Log("🛡 Düşman Saldırıyı Blokladı!");
                return; 
            }
        }

        health_system myHealth = GetComponent<health_system>();
        if (myHealth != null)
        {
            myHealth.TakeDamage(amount);
            
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
            _rb.linearVelocity = Vector2.zero; 
            _rb.bodyType = RigidbodyType2D.Kinematic;
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

        if (!attack && !defend) 
        {
            FlipSprite();
        }

        if (distance > stopDistance)
        {
            if (defend) 
            {
                defend = false; 
            }

            if (!attack) 
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

    IEnumerator CombatPattern()
    {
        while (true) 
        {
            if (isDead) yield break;

            attack = true;
            defend = false;

            int attackCount = Random.Range(1, 4); 

            yield return new WaitForSeconds(timeToFirstHit);
            DealDamage();
            
            if (attackCount > 1 && !isDead)
            {
                yield return new WaitForSeconds(timeToSecondHit);
                DealDamage();
            }

            if (attackCount > 2 && !isDead)
            {
                yield return new WaitForSeconds(timeToThirdHit);
                DealDamage();
            }

            yield return new WaitForSeconds(animationEndDelay);
            
            attack = false;

            int roll = Random.Range(0, 100);

            if (roll < defenseChance)
            {
                defend = true;
            }
            else
            {
                defend = false;
            }

            float waitDuration = Random.Range(minWaitTime, maxWaitTime);
            
            yield return new WaitForSeconds(waitDuration);
        }
    }

    private void DealDamage()
    {
        if (isDead || Player == null) return;

        FlipSprite(); 

        float actualDistance = Vector2.Distance(transform.position, Player.transform.position);

        if (actualDistance <= stopDistance + 1.2f)
        {
            player_health_system playerHealth = Player.GetComponent<player_health_system>();

            if (playerHealth != null)
            {
                // DEĞİŞİKLİK BURADA: Hasar verirken 'transform' (kendisi) gönderiliyor.
                playerHealth.TakeDamage(damagePerHit, transform);
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