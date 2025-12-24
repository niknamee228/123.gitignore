using UnityEngine;

public class ActiveWeapon : MonoBehaviour
{
    public static ActiveWeapon Instance { get; private set; }

    [SerializeField] private Sword sword;

    private Transform weaponParent;
    private SpriteRenderer playerSprite;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Если sword не назначен в инспекторе, ищем его
        if (sword == null)
        {
            sword = GetComponentInChildren<Sword>();
        }
    }

    public void SetWeaponParent(Transform parent)
    {
        weaponParent = parent;
        if (weaponParent != null && sword != null)
        {
            sword.transform.SetParent(weaponParent);
            sword.transform.localPosition = Vector3.zero;
            sword.transform.localRotation = Quaternion.identity;

            // Получаем спрайт игрока
            if (Player.Instance != null)
            {
                playerSprite = Player.Instance.GetCharacterSprite();
            }
        }
    }

    public Sword GetActiveWeapon()
    {
        return sword;
    }

    private void LateUpdate() // Используем LateUpdate для обновления ПОСЛЕ всех движений
    {
        if (sword == null || Player.Instance == null) return;

        FollowMousePosition();
    }

    private void FollowMousePosition()
    {
        if (Camera.main == null || Player.Instance == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3 playerPos = Player.Instance.GetPlayerWorldPosition();

        // Определяем направление к мыши
        Vector3 direction = mouseWorldPos - playerPos;

        // Поворачиваем оружие (только если оно не привязано к точке)
        if (weaponParent == null)
        {
            // Вычисляем угол
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Применяем вращение
            sword.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Отражение по X если смотрим влево
            if (playerSprite != null)
            {
                Vector3 scale = sword.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (playerSprite.flipX ? -1 : 1);
                sword.transform.localScale = scale;
            }
        }
        else
        {
            // Если оружие привязано к точке, просто следуем за направлением взгляда игрока
            if (playerSprite != null)
            {
                // Синхронизируем отражение с игроком
                SpriteRenderer weaponSprite = sword.GetComponent<SpriteRenderer>();
                if (weaponSprite != null)
                {
                    weaponSprite.flipY = playerSprite.flipX;
                }

                // Корректируем позицию
                sword.transform.localPosition = new Vector3(
                    playerSprite.flipX ? -0.3f : 0.3f,
                    0.1f,
                    0
                );
            }
        }
    }
}