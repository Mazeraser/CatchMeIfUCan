using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text endMessageText;

    [Header("UI Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;

    [Header("Game Settings")]
    [SerializeField] private int winScore = 10;
    [SerializeField] private float gameDuration = 30f;
    [SerializeField] private float spawnIntervalMin = 1.0f;
    [SerializeField] private float spawnIntervalMax = 2.5f;

    [Header("UI In-Game")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;

    private int currentScore;
    private float remainingTime;
    private bool isGameActive;

    private TargetBuilder targetBuilder;
    private SpawnTargetCommand spawnCommand;
    private AddScoreCommand addScoreCommand;

    private void Awake()
    {
        targetBuilder = GetComponent<TargetBuilder>();
        spawnCommand = new SpawnTargetCommand();
        addScoreCommand = new AddScoreCommand();
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    private void Start()
    {
        ShowWelcomeScreen();
    }

    private void ShowWelcomeScreen()
    {
        welcomePanel.SetActive(true);
        gamePanel.SetActive(false);
        endPanel.SetActive(false);
    }

    public void StartGame()
    {
        currentScore = 0;
        remainingTime = gameDuration;
        isGameActive = true;
        UpdateScoreDisplay();

        welcomePanel.SetActive(false);
        gamePanel.SetActive(true);
        endPanel.SetActive(false);

        StopAllCoroutines();
        targetBuilder.ReturnAllToPool();

        StartCoroutine(SpawnRoutine());
        StartCoroutine(TimerRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (isGameActive)
        {
            yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
            if (isGameActive)
                spawnCommand.Execute(targetBuilder);
        }
    }

    private IEnumerator TimerRoutine()
    {
        while (remainingTime > 0 && isGameActive)
        {
            remainingTime -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(remainingTime).ToString();

            if (remainingTime <= 5f)
            {
                timerText.color = remainingTime % 0.5f < 0.25f ? Color.red : Color.white;
            }

            yield return null;
        }

        if (isGameActive)
            EndGame(false);
    }

    public void OnTargetHit()
    {
        if (!isGameActive) return;

        addScoreCommand.Execute(AddScore);
        UpdateScoreDisplay();

        if (currentScore >= winScore)
            EndGame(true);
    }

    private void AddScore(int points)
    {
        currentScore += points;
    }

    private void UpdateScoreDisplay()
    {
        scoreText.text = $"Score: {currentScore}/{winScore}";
    }

    private void EndGame(bool won)
    {
        timerText.color = Color.white; 
        isGameActive = false;
        StopAllCoroutines();
        StartCoroutine(EndGameSequence(won));
    }

    private IEnumerator EndGameSequence(bool won)
    {
        yield return new WaitForSeconds(0.5f);

        gamePanel.SetActive(false);
        endPanel.SetActive(true);

        endMessageText.text = won ? "Ты выиграл! Молодец!" : "Ну и ну! Время кончилось";
    }

    public void RestartGame()
    {
        StartGame();
    }
}