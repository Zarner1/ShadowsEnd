using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Control : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f; // Zıplama gücü
    public int clickCount = 0;

    [Header("Ground Check Settings")]
    public Transform groundCheck; // Yerde olup olmadığını kontrol edecek olan objenin pozisyonu
    public LayerMask groundLayer; // Hangi katmanın "yer" olduğunu belirlemek için
    public float groundCheckRadius = 0.2f; // Yer kontrol çemberinin yarıçapı

    private float currentSpeed = 0.0f;
    private bool isGrounded; // Karakter yerde mi?
    private bool isCrouching; // Karakter yuvarlanıyor mu?
    private float resetTime = 0.2f;
    private float lastClickTime;
    private bool swordSlash;

    // Components
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        lastClickTime = Time.time;
    }

    void Update()
    {
        // YER KONTROLÜ: Her frame'de yerde olup olmadığımızı kontrol et
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // --- YATAY HAREKET ---
        HandleMovement();

        // --- ZIPLAMA ---
        HandleJump();

        // ---YUVARLANMA---
        HandleCrouch();

        // ---SALDIRI---
        HandleAttack();

        // --- ANİMASYON ---
        UpdateAnimations();

        // ---ÖZEL SALDIRI---
        HandleSpecialAttack();

        if (Time.time - lastClickTime >= resetTime)
        {
            clickCount = 0;
        }

    }

    private void HandleMovement()
    {
        if (Input.GetKey(KeyCode.D) && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.S) && !Input.GetMouseButton(0)) // Sağa git
        {
            currentSpeed = moveSpeed;
            _spriteRenderer.flipX = false;
        }
        else if (Input.GetKey(KeyCode.A) && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.S) && !Input.GetMouseButton(0)) // Sola git
        {
            currentSpeed = -moveSpeed;
            _spriteRenderer.flipX = true;
        }
        else // Hiçbir tuşa basılmıyorsa
        {
            currentSpeed = 0.0f;
        }

        // Fiziği güncelle (yatay hareket)
        _rigidbody2D.linearVelocity = new Vector2(currentSpeed, _rigidbody2D.linearVelocity.y);
    }

    private void HandleJump()
    {
        // Eğer Space tuşuna basıldıysa VE karakter yerdeyse
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.S))
        {
            // Rigidbody'ye dikey bir kuvvet uygula
            _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocity.x, jumpForce);
        }
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.S) && isGrounded)
        {
            isCrouching = true;
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            isCrouching = false;
        }
    }

    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickCount++;
            lastClickTime = Time.time;
        }
    }

    private void HandleSpecialAttack()
    {
        if (Input.GetMouseButtonDown(1))
        {
            swordSlash = true;
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            swordSlash = false;
        }
    }

    private void UpdateAnimations()
    {
        // Koşma animasyonu
        _animator.SetFloat("speed", Mathf.Abs(currentSpeed));

        // 'if' kontrolünü buradan kaldırıyoruz.
        // Eğer Animator'de "isJumping" parametresi varsa bu satır çalışır,
        // yoksa Unity bir uyarı verir ve bu satırı görmezden gelir.
        _animator.SetBool("isJumping", !isGrounded);
        _animator.SetBool("isCrouching", isCrouching);
        _animator.SetInteger("clickCount", clickCount);
        _animator.SetBool("isGrounded", isGrounded);
        _animator.SetBool("swordSlash", swordSlash);
    }

    // Editörde yer kontrol çemberini görebilmek için
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}