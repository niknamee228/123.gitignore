using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    [System.Serializable]
    public class InputSettings
    {
        public KeyCode moveUp = KeyCode.W;
        public KeyCode moveDown = KeyCode.S;
        public KeyCode moveLeft = KeyCode.A;
        public KeyCode moveRight = KeyCode.D;
        public KeyCode attackPrimary = KeyCode.Mouse0;
        public KeyCode attackSecondary = KeyCode.Space;
        public KeyCode interact = KeyCode.E;
        public KeyCode jump = KeyCode.Space;
    }

    [SerializeField] private InputSettings settings = new InputSettings();

    public event EventHandler OnPlayerAttack;
    public event EventHandler OnPlayerInteract;
    public event EventHandler OnPlayerJump;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Проверяем ввод каждый кадр
        if (Input.GetKeyDown(settings.attackPrimary) || Input.GetKeyDown(settings.attackSecondary))
        {
            OnPlayerAttack?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetKeyDown(settings.interact))
        {
            OnPlayerInteract?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetKeyDown(settings.jump))
        {
            OnPlayerJump?.Invoke(this, EventArgs.Empty);
        }
    }

    public Vector2 GetMovementVector()
    {
        Vector2 input = Vector2.zero;

        // Основные клавиши
        if (Input.GetKey(settings.moveUp)) input.y += 1;
        if (Input.GetKey(settings.moveDown)) input.y -= 1;
        if (Input.GetKey(settings.moveLeft)) input.x -= 1;
        if (Input.GetKey(settings.moveRight)) input.x += 1;

        // Стрелки как альтернатива
        if (Input.GetKey(KeyCode.UpArrow)) input.y += 1;
        if (Input.GetKey(KeyCode.DownArrow)) input.y -= 1;
        if (Input.GetKey(KeyCode.LeftArrow)) input.x -= 1;
        if (Input.GetKey(KeyCode.RightArrow)) input.x += 1;

        // Нормализуем если длина больше 1
        return input.magnitude > 1 ? input.normalized : input;
    }

    public bool GetKey(KeyCode key)
    {
        return Input.GetKey(key);
    }

    public bool GetKeyDown(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }

    public bool GetKeyUp(KeyCode key)
    {
        return Input.GetKeyUp(key);
    }

    public Vector3 GetMousePosition()
    {
        return Input.mousePosition;
    }

    public Vector3 GetMouseWorldPosition()
    {
        if (Camera.main == null) return Vector3.zero;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    // Методы для проверки конкретных действий
    public bool IsAttacking()
    {
        return Input.GetKey(settings.attackPrimary) || Input.GetKey(settings.attackSecondary);
    }

    public bool IsMoving()
    {
        return GetMovementVector().magnitude > 0.1f;
    }

    // Изменение настроек управления
    public void RebindKey(string action, KeyCode newKey)
    {
        switch (action.ToLower())
        {
            case "moveup": settings.moveUp = newKey; break;
            case "movedown": settings.moveDown = newKey; break;
            case "moveleft": settings.moveLeft = newKey; break;
            case "moveright": settings.moveRight = newKey; break;
            case "attack": settings.attackPrimary = newKey; break;
            case "interact": settings.interact = newKey; break;
            case "jump": settings.jump = newKey; break;
        }
    }
}