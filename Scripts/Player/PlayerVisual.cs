using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Aim")]
    [SerializeField] private SpriteRenderer characterSprite;
    [SerializeField] private bool flipBasedOnMouse = true;

    [Header("Components")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private float lastAttackTime = -Mathf.Infinity;
    private bool isAttacking = false;

    public static PlayerController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            
            return;
        }

        Instance = this;
        rb = GetComponent<Rigidbody2D>();

        if (characterSprite == null)
        {
            characterSprite = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        // Подписываемся на событие атаки
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPlayerAttack += Player_OnPlayerAttack;
            Debug.Log("PlayerController: Подписан на событие атаки");
        }
        else
        {
            Debug.LogWarning("GameInput.Instance is null! Создайте объект GameInput на сцене.");
        }
    }

    private void Player_OnPlayerAttack(object sender, System.EventArgs e)
    {
        TryAttack();
    }

    private void Update()
    {
        // Получаем ввод движения
        if (GameInput.Instance != null)
        {
            movementInput = GameInput.Instance.GetMovementVector();
        }
        else
        {
            // Резервный ввод с клавиатуры
            movementInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ).normalized;
        }

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
        // Движение
        if (movementInput.magnitude > 0.1f && !isAttacking)
        {
            rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);
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

        // ВЫЗЫВАЕМ АТАКУ ОРУЖИЯ!
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
        characterSprite.color = new Color(1f, 0.5f, 0.5f); // Светло-красный

        yield return new WaitForSeconds(0.15f);

        characterSprite.color = originalColor;
        isAttacking = false;
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        // Параметр для анимации бега
        bool isMoving = movementInput.magnitude > 0.1f;
        animator.SetBool("IsRunning", isMoving);
        animator.SetFloat("Speed", movementInput.magnitude);
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
    }

    private void OnDestroy()
    {
        // Отписываемся от события
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPlayerAttack -= Player_OnPlayerAttack;
        }
    }

    // Методы для доступа из других скриптов
    public bool IsRunning()
    {
        return movementInput.magnitude > 0.1f;
    }

    public Vector3 GetPlayerScreenPosition()
    {
        if (Camera.main == null) return Vector3.zero;
        return Camera.main.WorldToScreenPoint(transform.position);
    }
}