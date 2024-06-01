using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Pool;
using YG;

public class GameplayManager : Singleton<GameplayManager>
{
    public const int LENGTH_TO_DESTROY = 3;

    private int score;
    private int highScore;

    [Header("Game Parametrs (Set In Inspector)")]
    [SerializeField] private float cellRadius = 0.25f;
    [SerializeField] private int spawnXBalls = 5;
    [SerializeField] private int spawnYLines = 3;

    [SerializeField] private float levelMoveDownSpeedStart = 0.1f;
    [SerializeField] private float levelMoveDownSpeedTime = 0f;
    [SerializeField] private float levelMoveDownSpeedMax = 3f;
    [SerializeField] private float levelMoveDownSpeedStep =0.1f;
    [SerializeField] private float levelMoveDownSpeedDuration = 15f;

    [SerializeField] private float levelMoveDownSpeed;

    private float gridXStep;
    private float gridYstep;
    private float gridXEvenLineOffset;
    private float ballRadius;
    private bool isSpawnLineOdd = true;
    private float linePos;

    private bool isCanLaunch = true;
    private float timeLastLaunch = 0;
    public float timeBetweenLaunches = 0.5f;

    [Header("Ball Prefab (Set In Inspector)")]
    //    [SerializeField] private Ball ballPrefab;
    [SerializeField] private GameObject ballPrefabGO;

    [Header("Anchors (Set In Inspector)")]
    [SerializeField] private GameObject spawnAnchorGO;
    public static Transform SPAWN_ANCHOR;



    [SerializeField] private GameObject startAnchorGO;
    public static Transform START_ANCHOR;

    [SerializeField] private GameObject startSecondAnchorGO;
    public static Transform START_SECOND_ANCHOR;

    public Trajectory trajectory;

    [Header("Listes(Set Dynamically)")]

    [SerializeField] private List<Ball> listOfBalls;
    [SerializeField] private List<Ball> listOfBallsFirstLine;
    public HashSet<Ball> listOfBallOneColor;
    public HashSet<Ball> listOfLinkedBalls;
    public HashSet<Ball> listOfUnpinnedBalls;
    public HashSet<Ball> listOfMovingBalls;
    private Ball ballForLaunching;
    private Ball ballForLaunchSecond;

    public int countOfCheckedBallOneColor = 0;

    public event Action<float> OnLevelSpeedChange;

    public override void  Awake()
    {
        base.Awake();
        listOfBalls = new List<Ball>();
        listOfBallsFirstLine = new List<Ball>();

        listOfBallOneColor = new HashSet<Ball>();
        listOfLinkedBalls = new HashSet<Ball>();
        listOfUnpinnedBalls = new HashSet<Ball>();
        listOfMovingBalls = new HashSet<Ball>();

        SPAWN_ANCHOR = spawnAnchorGO.transform;
        START_ANCHOR = startAnchorGO.transform;
        START_SECOND_ANCHOR = startSecondAnchorGO.transform;

        PlayerControl.Instance.OnPointerPressed += TryLaunchBall;

        SetGridSteps(cellRadius);
    }

    public void Update()
    {
        if (GameManager.gameState == GameState.Gameplay)
        {
            TrySpawnLineOfBalls();
            CheckLaunchTime();
            CheckLevelSpeedUpdate();
        }
    }

    public void SetGridSteps(float cellRadius)
    {
        gridXStep = cellRadius * Mathf.Sqrt(3);
        gridYstep = cellRadius * 1.5f;
        ballRadius = 0.5f * gridXStep;
        gridXEvenLineOffset = ballRadius;
    }

    #region Score Management
    public void AddScore(int addScore)
    {
        score += addScore;
        GameplayUIManager.Instance.UpdateScore(score);

        if (score > highScore)
        {
            highScore = score;
            GameplayUIManager.Instance.UpdateHighScore(highScore);
        }
    }
    #endregion Score Management

    public void CheckLaunchTime()
    {
        if (GameManager.gameState != GameState.Gameplay || isCanLaunch) return;
        timeLastLaunch += Time.deltaTime;
        if (timeLastLaunch >= timeBetweenLaunches)
        {
            isCanLaunch = true;
            timeLastLaunch = 0;
        }
    }

    public void CheckLevelSpeedUpdate() {
        if (levelMoveDownSpeedTime > levelMoveDownSpeedDuration) {
            levelMoveDownSpeedTime = 0;
            IncreaseLevelSpeed();
        }
        levelMoveDownSpeedTime += Time.deltaTime;
    }

    public void IncreaseLevelSpeed() {
        levelMoveDownSpeed = Mathf.Min(
            levelMoveDownSpeed + levelMoveDownSpeedStep
            , levelMoveDownSpeedMax
            );
        OnLevelSpeedChange(levelMoveDownSpeed);
    }

    public void TrySpawnLineOfBalls() {
        linePos += levelMoveDownSpeed * Time.deltaTime;
        if (linePos >= Mathf.Abs(gridYstep)) {
            listOfBallsFirstLine.Clear();
            linePos = 0;
            float posY = SPAWN_ANCHOR.transform.position.y;
            SpawnBallsLine(posY, true);
            UpdateBallsNeighbors();
        }
    }

    public void StartGame() {
        highScore = GameManager.Instance.highScore;
        score = 0;
        linePos = 0;

        levelMoveDownSpeedTime = 0;
        levelMoveDownSpeed = levelMoveDownSpeedStart;

        isCanLaunch = true;

        ClearAllLists();
        SpawnBallsWave();
        SpawnBallForLaunching();
        SpawnSecondBallForLaunching();
        UpdateBallsNeighbors();
    }

    #region Spawns

    public void SpawnBallsWave() {
        for (int j = 0; j < spawnYLines; j++)
        {
            float posY = SPAWN_ANCHOR.transform.position.y - j * gridYstep;

            bool isFirstLine = false;
            if (j == 0) isFirstLine = true;

            SpawnBallsLine(posY, isFirstLine);

        }
    }

    public void SpawnBallsLine(float posY, bool isFirstLine) {

        int count = spawnXBalls;
        if (!isSpawnLineOdd) count = spawnXBalls - 1;


        for (int i = 0; i < count; i++) {
            float posX = SPAWN_ANCHOR.transform.position.x + i * gridXStep;
            if (!isSpawnLineOdd) posX += gridXEvenLineOffset;

            Vector2 pos = new Vector2(posX, posY);
            SpawnBall(pos, isFirstLine);
        }
        isSpawnLineOdd = !isSpawnLineOdd;
    }

    public void SpawnBall(Vector2 pos, bool isFirstLine)
    {

        Ball newBall = Poolable.TryGetPoolable<Ball>(ballPrefabGO);
        newBall.transform.SetParent(SPAWN_ANCHOR);
        //        Ball newBall = Instantiate(ballPrefab, SPAWN_ANCHOR);

        newBall.SetBall(pos, ballRadius, BallStatus.PartOfLevel, levelMoveDownSpeed, Ball.GetBallRandType());
        listOfBalls.Add(newBall);
        if (isFirstLine)
        {
            listOfBallsFirstLine.Add(newBall);
        }
    }

    public void SpawnBallForLaunching() {

        Ball newBall = Poolable.TryGetPoolable<Ball>(ballPrefabGO);
        newBall.transform.SetParent(START_ANCHOR);
        //        Ball newBall = Instantiate(ballPrefab, START_ANCHOR);
        newBall.SetBall(START_ANCHOR.position, ballRadius, BallStatus.BallForLaunch, levelMoveDownSpeed, Ball.GetBallRandType());
        ballForLaunching = newBall;
        trajectory.SetLineColor(newBall.ballColor);
    }

    public void SpawnSecondBallForLaunching()
    {
        Ball newBall = Poolable.TryGetPoolable<Ball>(ballPrefabGO);
        newBall.transform.SetParent(START_SECOND_ANCHOR);
        //        Ball newBall = Instantiate(ballPrefab, START_ANCHOR);

        newBall.SetBall(START_SECOND_ANCHOR.position, ballRadius, BallStatus.SecondBallForLaunch, levelMoveDownSpeed, Ball.GetBallRandType());
        ballForLaunchSecond = newBall;
        ballForLaunchSecond.StartSpawnBall(BallStatus.SecondBallForLaunch);
    }

    public void MoveSecondBallToFirstPlace() {
        ballForLaunchSecond.StartMoveAtPos(START_ANCHOR.position, BallStatus.BallForLaunch);
        ballForLaunching = ballForLaunchSecond;
        trajectory.SetLineColor(ballForLaunching.ballColor);

        SpawnSecondBallForLaunching();
    }

    public void SwapBallsPositions() {
        ballForLaunchSecond.StartMoveAtPos(START_ANCHOR.position, BallStatus.BallForLaunch);
        ballForLaunching.StartMoveAtPos(START_SECOND_ANCHOR.position, BallStatus.SecondBallForLaunch);

        Ball tBall = ballForLaunching;
        ballForLaunching = ballForLaunchSecond;
        ballForLaunchSecond = tBall;

        trajectory.SetLineColor(ballForLaunching.ballColor);
    }

    #endregion Spawns

    public void UpdateBallsNeighbors() {
//        Debug.Log(listOfBalls.Count);

        Physics2D.SyncTransforms();

        for (int i = 0; i < listOfBalls.Count; i++)
        {
            listOfBalls[i].UpdateListOfNeighbours();
        }
    }

    public void TryLaunchBall() {
        if (GameManager.gameState == GameState.Gameplay && isCanLaunch)
        {
            LaunchBall();
        }
    }

    private void LaunchBall() {
        Vector3 mouseWorldPos = PlayerControl.Instance.GetMouseWorldPos();
        Vector2 direction = new Vector2(
            mouseWorldPos.x - START_ANCHOR.position.x,
            mouseWorldPos.y - START_ANCHOR.position.y
            );

        ballForLaunching.LaunchBall(direction);

        isCanLaunch = false;

        MoveSecondBallToFirstPlace();
    }

    public void ClearListOfBallsOfOneColor() {
        listOfBallOneColor = new HashSet<Ball>();
    }

    public void ClearListOfCheckedBallsOfOneColor()
    {
        countOfCheckedBallOneColor = 0;
    }

    public void TryUnpinBalls() {
        if (listOfBallOneColor.Count >= LENGTH_TO_DESTROY)
        {
            foreach (Ball tBall in listOfBallOneColor)
            {
                tBall.UnpinBall();
            }
            CheckLinksOfRestBalls();
        }
    }

    public void CheckLinksOfRestBalls() {
        listOfLinkedBalls = new HashSet<Ball>();

        for (int i = 0; i < listOfBallsFirstLine.Count; i++) {
            listOfBallsFirstLine[i].CheckLinksWithNeighbours();
        }

        foreach (Ball ball in listOfBalls) {
            if (!listOfLinkedBalls.Contains(ball)
                && ball.ballStatus != BallStatus.UnpinnedBall
                ) {
                ball.UnpinBall();
            }
        }
    }

    public void AddAndCheckCountOfCheckedOneColorBalls()
    {
        countOfCheckedBallOneColor += 1;
        if (listOfBallOneColor.Count == countOfCheckedBallOneColor)
        {
            TryUnpinBalls();
        }
    }

    public void RemoveBallFromListOfBalls(Ball ball) {

        listOfBalls.Remove(ball);
        listOfBallsFirstLine.Remove(ball);
    }

    public void RemoveBallFromListOfUnpinnedBalls(Ball ball) {
//        Debug.Log(ball.name);
        listOfUnpinnedBalls.Remove(ball);
    }

    public void AddBallToListOfBalls(Ball ball) {
        listOfBalls.Add(ball);
    }

    public void GameOver() {
        foreach (Ball tBall in listOfBalls) {
            tBall.SetLevelMoveSpeed(0);
        }

        StartCoroutine(WaitUntilUnpinnedBallsFalls());
    }

    public IEnumerator WaitUntilUnpinnedBallsFalls() {
        while (true) {

            if (listOfUnpinnedBalls.Count == 0) {
                break;
            }

            yield return null;
        }
        GameManager.GAME_OVER(score);
    }

    public void ClearAllLists() {
        listOfBalls.Clear();
        listOfBallsFirstLine.Clear();
        listOfBallOneColor.Clear();
        countOfCheckedBallOneColor = 0;
        listOfLinkedBalls.Clear();
        listOfUnpinnedBalls.Clear();
        listOfMovingBalls.Clear();
    }

    public void RestartGame() {
        RemoveAllBallsFromField();
    }

    public void RemoveAllBallsFromField() {

        Stack<Ball> removeBin = new Stack<Ball>();

        foreach (Ball tball in listOfBalls)
        {
            removeBin.Push(tball);
        }

        foreach (Ball tBall in listOfMovingBalls)
        {
            removeBin.Push(tBall);
        }
        removeBin.Push(ballForLaunching);
        removeBin.Push(ballForLaunchSecond);
        ballForLaunching = null;
        ballForLaunchSecond = null;
        while (true) {
            if (removeBin.Count == 0) break;
            Ball tBall = removeBin.Pop();
            tBall.ImidialtyDestriyBall();
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (PlayerControl.instanceExists)
        {
            PlayerControl.Instance.OnPointerPressed -= TryLaunchBall;
        }
    }
}


