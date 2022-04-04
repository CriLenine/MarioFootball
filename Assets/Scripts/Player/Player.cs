using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum PlayerState
    {
        Moving,
        Tackling,
        Headbutting,
        Shooting,
        Falling,
        Stunned
    }

    [SerializeField]
    private PlayerSpecs _specs;

    private Animator _animator;
    private Rigidbody _rgdb;
    private NavMeshAgent _agent;

    public PlayerBrain IABrain { get; private set; }

    public PlayerState State { get; private set; }
    public Team Team { get; private set; }
    public Team Enemies => Team == Field.Team1 ? Field.Team2 : Field.Team1;

    public bool CanGetBall => !IsStunned && State != PlayerState.Headbutting && !HasBall;
    public bool IsStunned => State == PlayerState.Stunned || State == PlayerState.Falling;

    public bool HasBall => Field.Ball.transform.parent == transform;
    public bool IsDoped { get; private set; }
    public bool CanMove => State == PlayerState.Moving;

    public bool IsPiloted { get; set; } = false;
    public bool IsNavDriven { get; set; } = false;

    private float _dashSpeed;
    private Vector3 _dashEndPoint;

    private Action _waitingAction = null;

    #region Debug

    public void SetActive(bool value)
    {
        _debugOnly = !value;
    }

    public bool _debugOnly = false;
    private bool _isRetard => GameManager.EnemiesAreRetard && Team == Field.Team2 && Time.timeSinceLevelLoad < 2f;
    public PlayerState state;
    public bool isPiloted;
    public bool hasBall;
    public Vector3 input;

    #endregion

    #region Constructor

    public static Player CreatePlayer(Player prefab, Team team, bool isGoalKeeper = false)
    {
        Player player = Instantiate(prefab);

        Component brain = player.gameObject.AddComponent(isGoalKeeper ? team.GoalBrainType : team.TeamBrainType);

        player.IABrain = (PlayerBrain)player.GetComponent(brain.GetType());

        player.Team = team;

        return player;
    }
    #endregion

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rgdb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _rgdb.mass = _specs.Weight;
        gameObject.name += " " + _specs.Name;
    }

    private void Update()
    {
        state = State;
        isPiloted = IsPiloted;
        hasBall = HasBall;

        if (_debugOnly || _isRetard)
            return;

        _rgdb.angularVelocity = Vector3.zero;
        _rgdb.velocity = Vector3.zero;

        if (State != PlayerState.Moving)
        {
            _rgdb.position = Vector3.MoveTowards(_rgdb.position, _dashEndPoint, _dashSpeed * Time.deltaTime);
            input = Vector3.up;

            return;
        }

        if (IsNavDriven)
            return;

        //if (IsNavDriven && Vector3.Distance(transform.position, _agent.destination) <= 0.1f)

        Action action;

        if (_waitingAction)
            action = _waitingAction;
        else if (IsPiloted)
            action = Team.Brain.GetAction();
        else
            action = IABrain.GetAction();

        input = action.Direction;

        _agent.isStopped = !IsNavDriven && IsPiloted;

        if (action.DirectionalAction)
        {
            Vector3 direction = ComputeDirection(action);

            Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            _rgdb.rotation = Quaternion.Slerp(_rgdb.rotation, rotation, 25f * Time.deltaTime);

            if (action.WaitForRotation && Quaternion.Angle(_rgdb.rotation, rotation) > 5f)
            {
                _waitingAction = action;

                return;
            }
        }

        _waitingAction = null;
        MakeAction(action);
    }

    private void MakeAction(Action action)
    {
        Vector3 direction = ComputeDirection(action);

        if (action)
            _animator.SetBool("Idle", false);
        else
        {
            _animator.SetBool("Idle", true);
            _animator.SetBool("Run", false);
        }

        switch (action.ActionType)
        {
            case Action.Type.Move:
                _rgdb.position += direction * 2f * _specs.Speed * Time.deltaTime;
                _animator.SetBool("Run", true);

                break;

            case Action.Type.MoveTo:
                _agent.SetDestination(action.Position);
                _animator.SetBool("Run", true);

                break;

            case Action.Type.Shoot:
                if (HasBall)
                {
                    Shoot();
                    _animator.SetTrigger("Strike");
                }

                break;

            case Action.Type.Throw:
                Debug.Log("Throw");

                break;

            case Action.Type.Headbutt:
                Headbutt(direction);

                break;

            case Action.Type.Tackle:
                Tackle(direction);

                break;

            case Action.Type.ChangePlayer:
                Debug.Log("ChangePlayer");

                break;

            case Action.Type.Dribble:
                Debug.Log("Dribble");
                _animator.SetTrigger("Spin");

                break;

            case Action.Type.LobPass:
                if (HasBall)
                {
                    LobPass(direction);
                    _animator.SetTrigger("Pass");
                }

                break;

            case Action.Type.Pass:
                if (HasBall)
                {
                    DirectPass(direction);
                    _animator.SetTrigger("Pass");
                }

                break;
        }
    }

    private Vector3 ComputeDirection(Action action)
    {
        Vector3 direction = action.Direction != Vector3.zero ? action.Direction : transform.forward;
        direction = Field.Transform.TransformDirection(direction);

        if (action.ActionType == Action.Type.Shoot)
        {
            direction = Enemies.transform.position - transform.position;
            direction.y = 0f;
        }

        return direction.normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();

        if (Field.Ball == ball && !HasBall)
            ball.Take(transform);

        if (other.tag == "Wall")
            Stun();

        Player player = other.GetComponent<Player>();

        if (player && player.State == PlayerState.Tackling)
        {
            Fall((_rgdb.position - player.transform.position).normalized);

            //if (!HasBall) OnHitWithNoBall();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Wall")
            ResetState();

        Player player = other.GetComponent<Player>();

        if (player && player.State == PlayerState.Tackling)
            Fall((_rgdb.position - player.transform.position).normalized);
    }

    #region Shoot

    private void Shoot()
    {
        Transform goal = Enemies.transform;

        //float angle = Vector3.SignedAngle(transform.forward, goal.forward, Vector3.up);
        //angle = -(Mathf.Abs(angle) - 180f);

        float distance = Vector3.Distance(transform.position, goal.position);

        if (distance > Field.Width / 2f)
            MissedShoot(goal);
        else
        {
            if (Random.value > _specs.Accuracy + 1000f)
                GoalPostShoot(goal);
            else
                ShootOnTarget(goal);
        }
    }

    private void MissedShoot(Transform goal)
    {
        float sign = Mathf.Sign(Random.value - 0.5f);
        float range = Random.Range(Field.GoalWidth / 2f, Field.Height / 2f);

        Field.Ball.Shoot(goal.position + goal.right * sign * range, 33f);

        // Debug
        float distance = Vector3.Distance(goal.position, _rgdb.position);
        string direction = sign < 0 ? "gauche" : "droite";
        //Debug.Log($"Distance > 45m ({distance}) - Tir non cadr� � {direction} ({range}m).");
    }

    private void GoalPostShoot(Transform goal)
    {
        float sign = Mathf.Sign(Random.value - 0.5f);

        Field.Ball.Shoot(goal.position + goal.right * sign * Field.GoalWidth / 2f, 33f);

        // Debug
        float distance = Vector3.Distance(goal.position, _rgdb.position);
        string direction = sign < 0 ? "gauche" : "droit";
        //Debug.Log($"Distance < 45m ({distance}) - Tir sur poteau {direction}.");
    }

    private void ShootOnTarget(Transform goal)
    {
        float x = Random.Range(-1, 2);
        float y = Random.Range(-1, 2);

        Vector3 endPosition = goal.position;
        endPosition += goal.right * x * Field.GoalWidth * 0.85f / 2f;
        endPosition += goal.up * y * Field.GoalHeight * 0.85f / 2f;

        Vector3 interpolator = (_rgdb.position + endPosition) / 2f;
        interpolator -= Vector3.Project(endPosition - _rgdb.position, goal.right);

        Field.Ball.Shoot(endPosition, interpolator, 33f);

        // Debug
        float distance = Vector3.Distance(goal.position, _rgdb.position);
        //Debug.Log($"Distance < 45m ({distance}) - Tir cadr� ({x} ; {y}).");
    }

    #endregion

    #region Pass

    private void DirectPass(Vector3 direction)
    {
        Player mate = FindMateInRange(direction, 90f);

        if (mate)
        {
            Field.Ball.Pass(mate, 16f);

            //Debug.Log("Passe directe vers " + direction.ToString());
        }
        else
            LobPass(direction);
    }

    private void LobPass(Vector3 direction)
    {
        Field.Ball.LobPass(direction);

        //Debug.Log("Passe lobée vers " + direction.ToString());
    }

    #endregion

    #region FindMate

    private Player FindMateInRange(Vector3 direction, float range, bool standOut = false)
    {
        return FindPlayerInRange(Team, direction, range, standOut);
    }

    private Player FindEnemyInRange(Vector3 direction, float range, bool standOut = false)
    {
        return FindPlayerInRange(Enemies, direction, range, standOut);
    }

    private Player FindPlayerInRange(Team team, Vector3 direction, float range, bool standOut)
    {
        (Player player, float angle) best = (null, 0f);

        foreach (Player player in team.Players)
        {
            if (player == this)
                continue;

            float angle = Vector3.Angle(direction, player.transform.position - transform.position);

            if (angle <= range && (!standOut || IsPlayerStandOut(player)))
                if (best.player == null || angle < best.angle)
                    best = (player, angle);
        }

        return best.player;
    }

    private bool IsPlayerStandOut(Player player)
    {
        Vector3 direction = player.transform.position - transform.position;

        Debug.DrawRay(transform.position + Vector3.up, direction, Color.red, 10f);
        if (Physics.Raycast(transform.position + Vector3.up, direction, out RaycastHit hit, Mathf.Infinity, 1 << gameObject.layer))
        {
            Player target = hit.transform.GetComponent<Player>();

            if (target == player)
                return true;
        }

        return false;
    }

    #endregion

    #region Special

    private void Headbutt(Vector3 direction)
    {
        State = PlayerState.Headbutting;
        _animator.SetTrigger("Electrocuted");

        Dash(direction, 3f, 0.2f);
    }

    private void Tackle(Vector3 direction)
    {
        State = PlayerState.Tackling;
        _animator.SetTrigger("Tackled");

        Dash(direction, 8f, 1.2f, 0.5f);
    }

    private void Fall(Vector3 direction)
    {
        State = PlayerState.Falling;
        _animator.SetTrigger("isTackled");

        if (HasBall)
            Field.Ball.Free();

        Dash(direction, 4f, 1.5f, 2f);
    }

    private void Stun(float duration = 2f)
    {
        State = PlayerState.Stunned;
        _animator.SetTrigger("Electrocuted");

        Dash(Vector3.zero, 0f, duration);
    }

    private void Dash(Vector3 direction, float distance, float time, float standUpDelay = 0f)
    {
        _waitingAction = null;

        _animator.SetBool("Idle", false);
        _animator.SetBool("Run", false);

        if (direction == Vector3.zero)
            _dashSpeed = 0f;
        else
        {
            _dashSpeed = distance / time;
            _dashEndPoint = _rgdb.position + direction * distance;
        }

        Invoke(nameof(ResetState), time + standUpDelay);
    }

    private void ResetState()
    {
        State = PlayerState.Moving;

        CancelInvoke();
    }

    #endregion
}
