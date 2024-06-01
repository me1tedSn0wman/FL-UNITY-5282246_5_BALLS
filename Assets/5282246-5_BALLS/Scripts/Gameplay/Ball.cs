using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;
using Utils.Pool;

public enum BallStatus { 
    None,
    PartOfLevel,
    MovingToDirection,
    MovingToGridPos,
    MovingToPos,
    BallForLaunch,
    SecondBallForLaunch,
    SpawningBall,
    UnpinnedBall,
    Disappear
}

public enum BallType { 
    Green, 
    Blue,
    Yellow,
    Red, 
}

public class Ball : MonoBehaviour
{
    [Header("Ball Definition (Set Dynamically)")]
    [SerializeField] protected BallStatus _ballStatus;
    public BallStatus ballStatus
    {
        get { return _ballStatus; }
        set {
            if (value != _ballStatus)
            {
                switch (value)
                {
                    case BallStatus.None:
                    case BallStatus.PartOfLevel:
                    case BallStatus.MovingToGridPos:
                        rigidbody.isKinematic = true;
                        gameObject.layer = 6;
                        break;
                    case BallStatus.MovingToDirection:
                        rigidbody.isKinematic = false;
                        rigidbody.gravityScale = 0f;
                        gameObject.layer = 11;
                        break;
                    case BallStatus.BallForLaunch:
                        gameObject.layer = 7;
                        break;

                    case BallStatus.SecondBallForLaunch:
                        gameObject.layer = 7;
                        break;
                    case BallStatus.UnpinnedBall:
                        gameObject.layer = 8;
                        rigidbody.isKinematic = false;
                        rigidbody.gravityScale = 1f;
                        break;
                    case BallStatus.SpawningBall:
                        gameObject.layer = 7;
                        break;
                }
            }
            _ballStatus = value;
        }
    }
    public BallStatus nextBallStatus;

    [SerializeField] protected BallType _ballType;
    public BallType ballType {
        get { return _ballType; }
        set {
            switch (value) {
                case BallType.Green:
                    spriteRenderer.sprite = greenBall;
                    ballColor = Color.green;
                    break;
                case BallType.Blue:
                    spriteRenderer.sprite = blueBall;
                    ballColor = Color.cyan;
                    break;
                case BallType.Yellow:
                    spriteRenderer.sprite = yellowBall;
                    ballColor = Color.yellow;
                    break;
                case BallType.Red:
                    spriteRenderer.sprite = redBall;
                    ballColor = Color.red;
                    break;
            }
            _ballType = value;
        }
    }

    public Color ballColor { get; private set; }

    [Header("Ball Definition (Set In Editor)")]

    [SerializeField] protected Sprite greenBall;
    [SerializeField] protected Sprite blueBall;
    [SerializeField] protected Sprite yellowBall;
    [SerializeField] protected Sprite redBall;

    [Header("Physical Definition(Set Dynamically)")]

    protected float ballRadius;
    protected CircleCollider2D circleCollider;
    protected new Rigidbody2D rigidbody;

    [Header("Move characteristics")]

    [SerializeField] protected float moveTimeDuration = 0.1f;
    protected float timeStartMove = -1f;
    protected float levelMoveDownSpeed = 0.2f;

    protected List<Vector3> pts;

    public virtual Vector3 posToMove {
        set {
            timeStartMove = Time.time;
            pts = new List<Vector3>() { transform.position, value };
        }
    }

    public virtual Vector3 locPosToMove
    {
        set
        {
            timeStartMove = Time.time;
            pts = new List<Vector3>() { transform.localPosition, value };
        }
    }

    [SerializeField] private float launchedMoveSpeed = 0.2f;
    private Vector3 moveToDirection;

    [Header("Balls Component")]

    [SerializeField] protected GameObject spiteRendererGO;
    protected SpriteRenderer spriteRenderer;

    public List<Ball> neighbours;

    [Header("Spawn Characterisctics")]
    private float timeStartSpawning = -1f;
    [SerializeField] private float timeSpawnDuration = 0.3f;


    [Header("Disapper Characteristics")]
    private float timeStartDisappear = -1f;
    [SerializeField] private float timeDurationDisappear = 0.5f;

  


    [Header("Audio")]
    [SerializeField] protected AudioClip unpinAudioClip;
    [SerializeField] protected AudioClip launchAudioClip;
    [SerializeField] protected AudioClip impactAudioClip;

    public void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = spiteRendererGO.GetComponent<SpriteRenderer>();

        GameplayManager.Instance.OnLevelSpeedChange += SetLevelMoveSpeed;
    }

    public void Update()
    {
        if (GameManager.gameState == GameState.Gameplay)
        {
            switch (ballStatus)
            {
                case BallStatus.SpawningBall:
                    SpawnBall();
                    break;
                case BallStatus.MovingToPos:
                    MoveToPos();
                    break;
                case BallStatus.MovingToDirection:
                    MoveToDirection();
                    break;
                case BallStatus.MovingToGridPos:
                    MoveAtLocPos();
                    break;
                case BallStatus.PartOfLevel:
                    LevelMove();
                    break;
                case BallStatus.Disappear:
                    DisapearBall();
                    break;
                default:
                    break;
            }
        }
    }

    public void SetBall(Vector2 pos, float ballRadius, BallStatus ballStatus, float levelMoveDownSpeed, BallType ballType)
    {
        transform.position = pos;
        this.ballType = ballType;
        this.ballRadius = ballRadius;
        circleCollider.radius = ballRadius;
        this.levelMoveDownSpeed = levelMoveDownSpeed;

        this.ballStatus = ballStatus;
    }

    public void RemoveFromNeighbour(Ball ball)
    {
        neighbours.Remove(ball);
    }

    public void AddToNeighbour(Ball ball)
    {
        if (!neighbours.Contains(ball))
        {
            neighbours.Add(ball);
        }
    }

    #region Ball Movings
    public void SetLevelMoveSpeed(float value)
    {
        levelMoveDownSpeed = value;
    }

    public void LevelMove()
    {

        Vector3 tMove = Vector3.down * levelMoveDownSpeed * Time.deltaTime;
        transform.Translate(tMove);
    }

    public void LaunchBall(Vector2 direction)
    {
        transform.localScale = Vector3.one;
        GameplayAudioManager.SoundPlayOneShot(launchAudioClip);
        GameplayManager.Instance.listOfMovingBalls.Add(this);
        StartMoveToDirection(direction);
    }

    public void StartMoveToDirection(Vector2 direction)
    {
        ballStatus = BallStatus.MovingToDirection;
        SetMoveToDirection(direction);
    }

    public void SetMoveToDirection(Vector2 direction)
    {
        moveToDirection = launchedMoveSpeed * direction.normalized;
    }

    public void MoveToDirection()
    {
        transform.Translate(moveToDirection * Time.deltaTime);
    }

    public void StartMoveAtGridPos(Vector3 newPos) {
        ballStatus = BallStatus.MovingToGridPos;
        locPosToMove = newPos;
    }

    public void MoveAtLocPos() {
        if (timeStartMove == -1) return;
        float deltaTime = (Time.time - timeStartMove) / moveTimeDuration;
        deltaTime = Mathf.Clamp01(deltaTime);
        Vector3 tPos = Utils.Util.Bezier(deltaTime, pts);
        transform.localPosition = tPos;

        if (deltaTime == 1)
        {
            timeStartMove = -1;
            EndMoveAtGridPos();
        }
    }

    public void StartMoveAtPos(Vector3 pos, BallStatus nextBallStatus)
    {
        transform.localScale = Vector3.one;
        ballStatus = BallStatus.MovingToPos;
        this.nextBallStatus = nextBallStatus;
        posToMove = pos;
    }

    public void MoveToPos() {
        if (timeStartMove == -1) return;
        float deltaTime = (Time.time - timeStartMove) / moveTimeDuration;
        deltaTime = Mathf.Clamp01(deltaTime);
        Vector3 tPos = Utils.Util.Bezier(deltaTime, pts);
        transform.position = tPos;

        if (deltaTime == 1)
        {
            timeStartMove = -1;
            EndMoveAtPos();
        }
    }


    public void EndMoveAtPos()
    {
        ballStatus = nextBallStatus;
    }

    public void EndMoveAtGridPos()
    {
        ballStatus = BallStatus.PartOfLevel;
        gameObject.transform.SetParent(GameplayManager.SPAWN_ANCHOR);

        GameplayManager.Instance.AddBallToListOfBalls(this);
        GameplayManager.Instance.listOfMovingBalls.Remove(this);
        UpdateListOfNeighbours();

        for (int i = 0; i < neighbours.Count; i++) {
            neighbours[i].AddToNeighbour(this);
        }

        StartCheckNeighbours();
    }



    #endregion Ball Movings

    #region Func When arrive to grid Pos

    public void UpdateListOfNeighbours()
    {
        if (ballStatus != BallStatus.PartOfLevel) return;

        neighbours.Clear();

        int layerMask = 1 << 6;

        Vector2 pos = new Vector2(transform.position.x, transform.position.y);

        Collider2D[] arrayOfColliders = Physics2D.OverlapCircleAll(pos, ballRadius * 1.5f, layerMask);  // Physics2D.OverlapCircleNonAlloc ?
                                                                                                        //        Debug.Log(pos + "___"+ cellSize+"___" + arrayOfColliders.Length + "___" + circleCollider.transform.position);
        for (int i = 0; i < arrayOfColliders.Length; i++)
        {

            Ball tBall = arrayOfColliders[i].GetComponent<Ball>();
            if (tBall != null && tBall != this && tBall.ballStatus == BallStatus.PartOfLevel)
            {
                neighbours.Add(tBall);
            }
        }
    }

    public Vector3 GetClosestGridPosToCell(Vector3 cellToPos) {
        List<Vector3> slots = new List<Vector3>();

        slots.Add(new Vector3(cellToPos.x - ballRadius, cellToPos.y + ballRadius * Mathf.Sqrt(3)));
        slots.Add(new Vector3(cellToPos.x + ballRadius, cellToPos.y + ballRadius * Mathf.Sqrt(3)));

        slots.Add(new Vector3(cellToPos.x + 2 * ballRadius, cellToPos.y));
        slots.Add(new Vector3(cellToPos.x - 2 * ballRadius, cellToPos.y));

        slots.Add(new Vector3(cellToPos.x - ballRadius, cellToPos.y - ballRadius * Mathf.Sqrt(3)));
        slots.Add(new Vector3(cellToPos.x + ballRadius, cellToPos.y - ballRadius * Mathf.Sqrt(3)));


        Vector3 tClosePos = new Vector3(0, 0);
        float tCloseDist = Mathf.Infinity;
        for (int i = 0; i < slots.Count; i++) {
            float tDist = (slots[i] - transform.localPosition).magnitude;

            //Debug.Log(tDist +"___" +slots[i] + "___" + cellToPos);

            if (tDist < tCloseDist)
            {
                tClosePos = slots[i];
                tCloseDist = tDist;
            }
            
        }

        return tClosePos;
    }

    public Vector3 GetClosestGridPosToCell() {
        return GetClosestGridPosToCell(Vector3.zero);
    }

    /*
     Check if we created a group of balls of one color
     */
    public void StartCheckNeighbours()
    {
        GameplayManager.Instance.ClearListOfBallsOfOneColor();
        GameplayManager.Instance.ClearListOfCheckedBallsOfOneColor();
        CheckNeighbours();
    }

    public void CheckNeighbours()
    {
        GameplayManager.Instance.listOfBallOneColor.Add(this);

        for (int i = 0; i < neighbours.Count; i++)
        {
            Ball neighBall = neighbours[i];
            if (
                neighBall.ballType == ballType
                && !GameplayManager.Instance.listOfBallOneColor.Contains(neighBall)
                )
            {
                neighBall.CheckNeighbours();
            }
        }

        GameplayManager.Instance.AddAndCheckCountOfCheckedOneColorBalls();  /// TODO rename it something normal
    }

    public void UnpinBall() {
        ballStatus = BallStatus.UnpinnedBall;

        int force = Random.Range(0, 100);
        Vector2 dir = new Vector2(
            Random.Range(-1f,1f),
            Random.Range(-1f,1f)
            );
        rigidbody.AddForce(dir * force);

        GameplayAudioManager.SoundPlayOneShot(unpinAudioClip);

        //        GameplayManager.Instance.RemoveBallFromListOfBalls(this);

        GameplayManager.Instance.listOfUnpinnedBalls.Add(this);
        for (int i = 0; i < neighbours.Count; i++) {
            neighbours[i].RemoveFromNeighbour(this);
        }
        neighbours.Clear();
    }

    public void CheckLinksWithNeighbours()
    {
        GameplayManager.Instance.listOfLinkedBalls.Add(this);
        for (int i = 0; i < neighbours.Count; i++)
        {
            if (!GameplayManager.Instance.listOfLinkedBalls.Contains(neighbours[i]))
            {
                neighbours[i].CheckLinksWithNeighbours();
            }
        }
    }

    #endregion Func When arrive to grid Pos

    public void OnMouseUpAsButton()
    {
        if (ballStatus == BallStatus.SecondBallForLaunch) {
            GameplayManager.Instance.SwapBallsPositions();
        }
    }


    #region OnCollision
    public void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D other = collision.collider;

        if (
            ballStatus == BallStatus.MovingToDirection

            )
        {
            if (other.CompareTag("Wall"))
            {
                GameplayAudioManager.SoundPlayOneShot(impactAudioClip);
                Vector3 newMoveDirection = new Vector3(-moveToDirection.x, moveToDirection.y, moveToDirection.z);
                SetMoveToDirection(newMoveDirection);
            }
            if (other.CompareTag("Ball"))
            {
                Ball othBall = other.GetComponent<Ball>();
                if (othBall.ballStatus == BallStatus.PartOfLevel)
                {

                    GameplayAudioManager.SoundPlayOneShot(impactAudioClip);
                    gameObject.transform.SetParent(other.transform);
                    Vector3 tLocPos = GetClosestGridPosToCell();
                    StartMoveAtGridPos(tLocPos);
                }
                else
                {
                    GameplayAudioManager.SoundPlayOneShot(impactAudioClip);
                    Vector3 newMoveDirection = new Vector3(-moveToDirection.x, moveToDirection.y, moveToDirection.z);
                    SetMoveToDirection(newMoveDirection);
                }
            }
        }
    }

    #endregion OnCollision

    public void StartSpawnBall(BallStatus nextBallStatus) {

        ballStatus = BallStatus.SpawningBall;
        this.nextBallStatus = nextBallStatus;
        timeStartSpawning = Time.time;
        transform.localScale = new Vector3(0, 0, 0);
    }

    public void SpawnBall()
    {
        if (Time.time > timeStartSpawning + timeSpawnDuration)
        {
            transform.localScale = Vector3.one;
            ballStatus = nextBallStatus;
            return;
        }

        float locSc = Mathf.Clamp01((Time.time - timeStartSpawning) / timeSpawnDuration);

        transform.localScale = new Vector3(locSc, locSc, locSc);
    }

    #region Destoy Ball

    public void DestroyBall()
    {
        GameplayManager.Instance.RemoveBallFromListOfBalls(this);
        GameplayManager.Instance.RemoveBallFromListOfUnpinnedBalls(this);
        GameplayManager.Instance.AddScore(1);

        ballStatus = BallStatus.Disappear;
        timeStartDisappear = Time.time;
        DisapearBall();
    }

    public void DisapearBall()
    {
        if (Time.time > timeStartDisappear + timeDurationDisappear)
        {
            ReturnToPool();
            //Destroy(this.gameObject);
            return;
        }

        float locSc = Mathf.Clamp01(1 - (Time.time - timeStartDisappear) / timeDurationDisappear);

        transform.localScale = new Vector3(locSc, locSc, locSc);
    }

    public void ImidialtyDestriyBall()
    {
        GameplayManager.Instance.RemoveBallFromListOfBalls(this);
        GameplayManager.Instance.RemoveBallFromListOfUnpinnedBalls(this);

        ReturnToPool();
        //Destroy(this.gameObject);
    }

    public void ReturnToPool() {

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = Vector3.one;

        rigidbody.isKinematic = true;

        neighbours.Clear();
        Poolable.TryPool(this.gameObject);
    }

    public void OnDestroy()
    {
        GameplayManager.Instance.OnLevelSpeedChange -= SetLevelMoveSpeed;
    }

    #endregion Destoy Ball

    #region Static Functions

    public static BallType GetBallRandType() {
        BallType[] ballTypes = {
            BallType.Green,
            BallType.Blue,
            BallType.Yellow,
            BallType.Red,
        };
        int ind = Random.Range(0,ballTypes.Length);
        return ballTypes[ind];
    }

    public static BallType GetBallType(int ind)
    {
        BallType[] ballTypes = {
            BallType.Green,
            BallType.Blue,
            BallType.Yellow,
            BallType.Red,
        };
        return ballTypes[ind];
    }

    #endregion Static Functions 

    #region Draw In Editor
    public void OnDrawGizmos()
    {
        for (int i = 0; i < neighbours.Count; i++)
        {
            Gizmos.DrawLine(this.transform.position, neighbours[i].transform.position);
        }
    }

    #endregion Draw In Editor

}
