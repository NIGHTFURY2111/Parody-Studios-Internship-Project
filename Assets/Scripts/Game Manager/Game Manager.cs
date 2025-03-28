using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Central manager for game state, handling checkpoints, timer, score display, and game end conditions.
/// Acts as the core controller for game flow and UI updates.
/// </summary>
public class GameManager : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] double RemainingTimeSeconds;
    [SerializeField] GameObject CheckPointParent;
    [SerializeField] GameObject GameOverPanel;

    /// <summary>
    /// Event triggered when a checkpoint is reached.
    /// The integer parameter represents the number of remaining checkpoints.
    /// </summary>
    public static event Action<int> OnCheckpointReached;
    /// <summary>
    /// Event triggered when the game ends.
    /// The boolean parameter indicates whether the player won (true) or lost (false).
    /// </summary>
    public static event Action<bool> OnGameOver;
    /// <summary>
    /// Gets the number of checkpoints remaining to be collected.
    /// </summary>
    public int checkpointsLeft => checkPoints;

    int checkPoints;

    private void Start()
    {
        checkPoints = CheckPointParent.transform.childCount;
        updateScore();
        OnCheckpointReached += updateScore;
        OnGameOver += GameOver;

    }

    void LateUpdate()
    {
        updateTimer();
    }

    /// <summary>
    /// Called when a player reaches a checkpoint.
    /// Decreases the checkpoint count and triggers win condition if all checkpoints are collected.
    /// </summary>
    public void RegisterCheckpoint()
    {
        checkPoints--;
        OnCheckpointReached?.Invoke(checkPoints);

        if (checkPoints == 0)
        {
            GameWon();
        }
        Debug.Log("Checkpoint reached");
    }

    /// <summary>
    /// Handles game over when the player loses.
    /// Triggers the OnGameOver event with false to indicate loss.
    /// </summary>
    public void GameLost()
    {
        OnGameOver?.Invoke(false);
        Debug.Log("Game Lost");
    }

    /// <summary>
    /// Handles game over when the player wins.
    /// Triggers the OnGameOver event with true to indicate victory.
    /// </summary>
    public void GameWon()
    {
        OnGameOver?.Invoke(true);
        Debug.Log("Game Won");
    }

    /// <summary>
    /// Formats a time value in seconds to a MM:SS string format.
    /// </summary>
    /// <param name="time">Time in seconds to format.</param>
    /// <returns>A formatted string showing minutes and seconds (Time Left: MM:SS).</returns>

    string formatTime(double time)
    {
        decimal minutes = Math.Floor((decimal)time / 60);
        decimal seconds = Math.Floor((decimal)time % 60);
        return string.Format("Time Left: {0:00}:{1:00}", minutes, seconds);
    }


    /// <summary>
    /// Updates the timer display and checks for time-based game over.
    /// Called every frame from Update.
    /// </summary>
    void updateTimer()
    {
        if (RemainingTimeSeconds > 0)
        {
            RemainingTimeSeconds -= Time.deltaTime;
        }
        else if (RemainingTimeSeconds <= 0)
        {
            RemainingTimeSeconds = 0;
            GameLost();
        }
        timerText.text = formatTime(RemainingTimeSeconds);

    }


    /// <summary>
    /// Updates the score display with the current number of remaining checkpoints.
    /// </summary>
    /// <param name="_">Optional parameter for event subscription compatibility. Not used.</param>
    void updateScore(int _ = -1)
    {
        scoreText.text = "Checkpoint Left: " + checkPoints;
    }

    /// <summary>
    /// Displays the game over screen and pauses the game.
    /// </summary>
    /// <param name="GameWon">True if the player won, false if they lost.</param>
    void GameOver(bool GameWon)
    {
        GameOverPanel.SetActive(true);
        Time.timeScale = 0;
        GameOverPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =  GameWon? "You Win" : "You Lost";
    }

}
