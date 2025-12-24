using System;
using UnityEngine;

[SelectionBase]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float movingSpeed = 5f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer characterSprite;
    [SerializeField] private bool flipBasedOnMouse = true;
    [SerializeField] private Animator animator;

    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponHoldPoint; // Создайте пустой GameObject как дочерний для позиции оружия

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private bool isRunning = false;
    private float lastAttackTime = -Mathf.Infinity;
    private bool isAttacking = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rb = GetComponent<Rigidbody2D>();

        // Автоматически находим компоненты если не назначены
        if (characterSprite == null)
            characterSprite = GetComponentInChildren<SpriteRenderer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Создаем точку для оружия если не назначена
        if (weaponHoldPoint == null)
        {
            GameObject weaponPoint = new GameObject("WeaponHoldPoint");
            weaponPoint.transform.SetParent(transform);
            weaponPoint.transform.localPosition = new Vector3(0.5f, 0.1f, 0);
            weaponHoldPoint = weaponPoint.transform;
        }
    }

    private void Start()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPlayerAttack += Player_OnPlayerAttack;
            Debug.Log("Player: Подписан на атаку");
        }
        else
        {
            Debug.LogError("GameInput не найден!");
        }

        // Назначаем точку для оружия
        if (ActiveWeapon.Instance != null)
        {
            ActiveWeapon.Instance.SetWeaponParent(weaponHoldPoint);
        }
    }

    private void Player_OnPlayerAttack(object sender, EventArgs e)
    {
        TryAttack();
    }

    private void Update()
    {
        Vector2 inputVector = GetMovementInput();
        isRunning = inputVector.magnitude > 0.1f;

        // Обновляем анимацию
        UpdateAnimation();

        // Поворот к мыши
        if (flipBasedOnMouse && characterSprite != null)
        {
            LookAtMouse();
        }
    }

    private void FixedUpdate()
    {
        Vector2 inputVector = GetMovementInput();

        if (inputVector.magnitude > 0.01f && !isAttacking)
        {
            rb.MovePosition(rb.position + inputVector * (movingSpeed * Time.fixedDeltaTime));
        }
    }

    private void TryAttack()
    {
        // Проверяем кулдаун атаки
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    private void PerformAttack()
    {
        Debug.Log("Игрок атакует!");

        // Вызываем атаку оружия
        if (ActiveWeapon.Instance != null)
        {
            ActiveWeapon.Instance.GetActiveWeapon().Attack();
        }

        // Визуальная обратная связь
        if (characterSprite != null)
        {
            StartCoroutine(AttackVisualFeedback());
        }

        // Анимация
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    private System.Collections.IEnumerator AttackVisualFeedback()
    {
        isAttacking = true;

        Color originalColor = characterSprite.color;
        characterSprite.color = new Color(1f, 0.5f, 0.5f);

        yield return new WaitForSeconds(0.15f);

        characterSprite.color = originalColor;
        isAttacking = false;
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        animator.SetBool("IsRunning", isRunning);
        animator.SetFloat("Speed", GetMovementInput().magnitude);
    }

    private void LookAtMouse()
    {
        if (Camera.main == null) return;

        Vector3 mouseWorldPos = GameInput.Instance != null
            ? GameInput.Instance.GetMouseWorldPosition()
            : Camera.main.ScreenToWorldPoint(Input.mousePosition);

        mouseWorldPos.z = 0;

        // Поворачиваем спрайт в сторону мыши
        if (mouseWorldPos.x < transform.position.x)
        {
            characterSprite.flipX = true;
        }
        else
        {
            characterSprite.flipX = false;
        }

        // Обновляем позицию оружия
        if (weaponHoldPoint != null)
        {
            Vector3 weaponOffset = new Vector3(0.5f, 0.1f, 0);
            if (characterSprite.flipX)
            {
                weaponOffset.x = -0.5f;
            }
            weaponHoldPoint.localPosition = weaponOffset;
        }
    }

    private Vector2 GetMovementInput()
    {
        if (GameInput.Instance != null)
        {
            return GameInput.Instance.GetMovementVector();
        }

        // Резервный ввод
        Vector2 inputVector = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) inputVector.y += 1;
        if (Input.GetKey(KeyCode.S)) inputVector.y -= 1;
        if (Input.GetKey(KeyCode.A)) inputVector.x -= 1;
        if (Input.GetKey(KeyCode.D)) inputVector.x += 1;

        return inputVector.normalized;
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public Vector3 GetPlayerScreenPosition()
    {
        if (Camera.main == null) return Vector3.zero;
        return Camera.main.WorldToScreenPoint(transform.position);
    }

    public Vector3 GetPlayerWorldPosition()
    {
        return transform.position;
    }

    public SpriteRenderer GetCharacterSprite()
    {
        return characterSprite;
    }

    private void OnDestroy()
    {
        // Отписываемся от события
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPlayerAttack -= Player_OnPlayerAttack;
        }
    }
}