using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] double RemainingTimeSeconds;
    [SerializeField] GameObject CheckPointParent;
    [SerializeField] GameObject GameOverPanel;


    public static event Action<int> OnCheckpointReached;
    public static event Action<bool> OnGameOver;
    public int checkpointsLeft => checkPoints;

    int checkPoints;

    private void Start()
    {
        checkPoints = CheckPointParent.transform.childCount;
        updateScore();
        OnCheckpointReached += updateScore;
        OnGameOver += GameOver;

    }

    void Update()
    {
        updateTimer();
    }

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

    public void GameLost()
    {
        OnGameOver?.Invoke(false);
        Debug.Log("Game Lost");
    }

    public void GameWon()
    {
        OnGameOver?.Invoke(true);
        Debug.Log("Game Won");
    }


    string formatTime(double time)
    {
        decimal minutes = Math.Floor((decimal)time / 60);
        decimal seconds = Math.Floor((decimal)time % 60);
        return string.Format("Time Left: {0:00}:{1:00}", minutes, seconds);
    }

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

    void updateScore(int _ = -1)
    {
        scoreText.text = "Checkpoint Left: " + checkPoints;
    }

    void GameOver(bool GameWon)
    {
        GameOverPanel.SetActive(true);
        Time.timeScale = 0;
        GameOverPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =  GameWon? "You Win" : "You Lost";
    }

}
