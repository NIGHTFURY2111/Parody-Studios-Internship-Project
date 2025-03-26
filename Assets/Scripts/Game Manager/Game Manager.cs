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

    int checkPoints;

    public static event Action<int> OnCheckpointReached;
    public static event Action<int> OnGameLost;
    public static event Action<int> OnGameWon;


    private void Start()
    {
        checkPoints = CheckPointParent.transform.childCount;
        updateScore();
        OnCheckpointReached += updateScore;

    }
    void Update()
    {
        updateTimer();
    }

    public void RegisterCheckpoint()
    {
        checkPoints--;
        OnCheckpointReached?.Invoke(checkPoints);
        Debug.Log("Checkpoint reached EVENT RECIEVED");
    }

    public void GameLost()
    {
        OnGameLost?.Invoke(checkPoints);
        Debug.Log("Game Lost EVENT RECIEVED");
    }

    public void GameWon()
    {
        OnGameWon?.Invoke(checkPoints);
        Debug.Log("Game Won EVENT RECIEVED");
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

}
