using UnityEngine;
using UnityEngine.AI;
using KnightAdventure.Utils;
using static UnityEngine.Video.VideoPlayer;
using System;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private State startingState;
    [SerializeField] private float roamingDistanceMax = 7f;
    [SerializeField] private float roamingDistanceMin = 3f;
    [SerializeField] private float roamingTimerMax = 2f;
    
    [SerializeField] private bool _isChasingEnemy = false;
    private float _chasingDistance = 4f;
    private float _chasingSpeedMyltiplayer = 2f;


    [SerializeField] private bool _isAttackChasingEnemy = false;
    private float _attackingDistance = 2f;
    private float _attackRate = 2f;
    private float _nextAttackTime = 0f;
   
    
    private NavMeshAgent navMeshAgent;
    private State _currentState;
    private float roamingTimer;
    private Vector3 roamPosition;
    private Vector3 startingPosition;


    private float _roamingSpeed;
    private float _chasingSpeed;
    private float _chasingSpeedMultiplayer;
    private float attackingDistance;
    private bool _isAttackingEnemy;


    public event System.EventHandler OnEnemyAttack;
    
    
    
    public EnemyAI(State state )
    {
        _currentState = state;
    }
    public enum State
    {
       Idle,
        Roaming,
        Chasing,
        Attacking,
        Death
    }

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

      
        if (navMeshAgent != null)
        {
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;
            _roamingSpeed = navMeshAgent.speed;
            _chasingSpeed = navMeshAgent.speed * _chasingSpeedMultiplayer;
            if (!NavMesh.CalculatePath(transform.position, transform.position, NavMesh.AllAreas, new NavMeshPath()))
            {
                Debug.LogError("NavMesh not properly set up!");
            }
        }

        _currentState = startingState;
        startingPosition = transform.position;
    }

 
    
    
    private void Start()
    {
        roamingTimer = roamingTimerMax;
    }

    private void Update()
    {
        StateHandler();
    }

    private void StateHandler()
    {
        switch (_currentState)
        {

            case State.Roaming:
                roamingTimer -= Time.deltaTime;
                if (roamingTimer <= 0)
                {
                    Roaming();
                    roamingTimer = roamingTimerMax;
                }
                break;
            case State.Chasing:
                ChasingTarget();
                GetCurrentState();
                break;
            case State.Attacking:
                AttackingTarget();
                CheckCurrentState();
                
                break;
            case State.Death:
                break;
            default:
            case State.Idle:
                break;
        }
    }

    private void AttackingTarget()
    {
       if(Time.time > _nextAttackTime)
        {
            OnEnemyAttack?.Invoke(this, EventArgs.Empty);

            _nextAttackTime = Time.time + _attackRate;
        }
        
        
        
    }

    private void CheckCurrentState()
    {
        throw new System.NotImplementedException();
    }

    private void ChasingTarget()
    {
        navMeshAgent.SetDestination(Player.Instance.transform.position);

    }
    
    private State GetCurrentState()
    {

        float distanceToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);
        State newstate = State.Roaming;

        if (_isChasingEnemy)
        {
            if (distanceToPlayer <= _chasingDistance)
            {
                newstate = State.Chasing;
            }
        }
        if (_isAttackingEnemy)
        {
            if (distanceToPlayer <= attackingDistance)
            {
                newstate = State.Attacking;
            }
        }
        
        
        
        
        if (newstate != _currentState)
        {
             if (newstate == State.Chasing)
            {
                navMeshAgent.ResetPath();
                navMeshAgent.speed = _chasingSpeed;
            }else  if (newstate == State.Roaming)
            {
                roamingTimer = 0f;
                navMeshAgent.speed = _roamingSpeed;
            } else if (newstate == State.Attacking)
            {
                navMeshAgent.ResetPath();
            }


                _currentState = newstate;

        }


        return newstate; 
    }
   
    
    public bool IsRunning()
    {
        if (navMeshAgent.velocity == Vector3.zero)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    
    
    
    
    
    
    
    
    private void Roaming()
    {
        if (navMeshAgent == null) return;

        roamPosition = GetRoamingPosition();
        ChangeFacingDirection(transform.position, roamPosition);

        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(roamPosition);
        }
    }

    private Vector3 GetRoamingPosition()
    {
        Vector3 randomDir = Utils.GetRandomDir();
        float randomDistance = Random.Range(roamingDistanceMin, roamingDistanceMax);
        Vector3 targetPosition = startingPosition + randomDir * randomDistance;

    
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 2.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return startingPosition + randomDir * roamingDistanceMin;
    }

    private void ChangeFacingDirection(Vector3 sourcePosition, Vector3 targetPosition)
    {
        if (sourcePosition.x > targetPosition.x)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}