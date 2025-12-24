using System;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] private int _damageAmount = 2;
    [SerializeField] private float attackDuration = 0.3f;

    public event EventHandler OnSwordSwing;

    private PolygonCollider2D _attackCollider;
    private bool _isAttacking = false;

    private void Awake()
    {
        _attackCollider = GetComponent<PolygonCollider2D>();
        if (_attackCollider == null)
        {
            Debug.LogError("PolygonCollider2D не найден на Sword!");
        }
    }

    private void Start()
    {
        AttackColliderTurnOff();
    }

    public void Attack()
    {
        if (_isAttacking) return;

        StartCoroutine(AttackRoutine());
        OnSwordSwing?.Invoke(this, EventArgs.Empty);
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        AttackColliderTurnOn();

        yield return new WaitForSeconds(attackDuration);

        AttackColliderTurnOff();
        _isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isAttacking) return;

        if (collision.transform.TryGetComponent(out EnemyEntity enemyEntity))
        {
            enemyEntity.TakeDamage(_damageAmount);
            Debug.Log($"Нанесен урон {_damageAmount} врагу: {collision.name}");
        }
    }

    private void AttackColliderTurnOff()
    {
        if (_attackCollider != null)
        {
            _attackCollider.enabled = false;
        }
    }

    private void AttackColliderTurnOn()
    {
        if (_attackCollider != null)
        {
            _attackCollider.enabled = true;
        }
    }
}