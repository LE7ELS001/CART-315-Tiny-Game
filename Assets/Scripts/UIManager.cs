using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Numerics;
using UnityEngine.Rendering;


public class UIManager : MonoBehaviour
{
    public Text Score;
    public Text HighScore;
    public Text GameOver;

    private int m_Score = 0;
    private int m_BestScore = 0;

    public Slider Force;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameOver.enabled = false;
        m_Score = 0;
        m_BestScore = PlayerPrefs.GetInt("HighScore");
        RefreshScore();

    }

    // Update is called once per frame
    void Update()
    {

    }

    void RefreshScore()
    {
        string str = string.Format("Score : {0}", m_Score);
        Score.text = str;
        str = string.Format("High Score : {0}", m_BestScore);
        HighScore.text = str;
    }

    public void AddScore(int sc = 1)
    {
        m_Score += sc;
        if (m_Score > m_BestScore)
        {
            m_BestScore = m_Score;
            PlayerPrefs.SetInt("HighScore", m_BestScore);
        }
        RefreshScore();
    }

    public void SetGameOver(bool isEnd = true)
    {
        GameOver.enabled = isEnd;
    }

    public void onReStartClick()
    {
        SceneManager.LoadScene("Game");
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }

    public void ShowForce(float force, float maxForce)
    {
        Force.minValue = 0;
        Force.maxValue = maxForce;
        Force.value = force;
        UnityEngine.Vector3 fw = Camera.main.transform.forward * -1;
        fw.y = 0;

        Force.transform.forward = fw;
    }

    public void SetForceShow(bool show = true)
    {
        Force.enabled = show;
        Force.gameObject.SetActive(show);
    }
}
