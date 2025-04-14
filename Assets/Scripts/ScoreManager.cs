using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public AudioSource hitSFX;
    public AudioSource missSFX;
    public TMPro.TextMeshPro scoreText;
    static int Score;
    void Start()
    {
        Instance = this;
        Score = 0;
    }
    public static void Hit()
    {
        Score += 1;
        Instance.hitSFX.Play();
    }
    public static void Miss()
    {
        Instance.missSFX.Play();
    }
    private void Update()
    {
        scoreText.text = Score.ToString();
    }
}
