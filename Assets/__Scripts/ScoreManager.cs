using UnityEngine;
using UnityEngine.UI;   // ← REQUIRED for Legacy Text UI

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public Text scoreText;   // ← THIS IS THE LEGACY UI TEXT
    private int score = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        score = 0;
        scoreText.text = "Score: 0";   // updates UI at start
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;  // updates UI every time
        Debug.Log("Score updated: " + score);
    }
}
