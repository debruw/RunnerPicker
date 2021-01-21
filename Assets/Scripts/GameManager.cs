using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TapticPlugin;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance { get { return _instance; } }

    public int currentLevel = 1;
    int MaxLevelNumber = 10;
    public bool isGameStarted, isInSlowMotion;
    public PlayerController Player;
    public GameObject Crates;

    #region UI Elements
    public Transform canvas;
    public GameObject WinPanel, LosePanel, InGamePanel;
    public Button VibrationButton, TapToStartButton;
    public Sprite on, off;
    public Text LevelText;
    public GameObject TapToLoadButton;
    public GameObject Tutorial1Canvas;
    #endregion

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        if (!PlayerPrefs.HasKey("VIBRATION"))
        {
            PlayerPrefs.SetInt("VIBRATION", 1);
            VibrationButton.GetComponent<Image>().sprite = on;
        }
        else
        {
            if (PlayerPrefs.GetInt("VIBRATION") == 1)
            {
                VibrationButton.GetComponent<Image>().sprite = on;
            }
            else
            {
                VibrationButton.GetComponent<Image>().sprite = off;
            }
        }
        currentLevel = PlayerPrefs.GetInt("LevelId");
        LevelText.text = "Level " + currentLevel;
    }

    IEnumerator WaitAndGameWin()
    {
        SoundManager.Instance.StopAllSounds();
        SoundManager.Instance.playSound(SoundManager.GameSounds.Win);
        yield return new WaitForSeconds(.5f);
        Ship.transform.DORotate(new Vector3(0, -45, 0), 5);
        Ship.transform.DOMove(ShipTarget.position, 20);
        yield return new WaitForSeconds(1f);
        if (PlayerPrefs.GetInt("VIBRATION") == 1)
            TapticManager.Impact(ImpactFeedback.Light);
        currentLevel++;
        PlayerPrefs.SetInt("LevelId", currentLevel);
        WinPanel.SetActive(true);
        InGamePanel.SetActive(false);
    }

    public IEnumerator WaitAndGameLose()
    {
        SoundManager.Instance.playSound(SoundManager.GameSounds.Lose);
        yield return new WaitForSeconds(.5f);
        if (PlayerPrefs.GetInt("VIBRATION") == 1)
            TapticManager.Impact(ImpactFeedback.Light);

        LosePanel.SetActive(true);
        InGamePanel.SetActive(false);
    }

    public void TapToNextButtonClick()
    {
        if (currentLevel > MaxLevelNumber)
        {
            int rand = Random.Range(1, MaxLevelNumber);
            if (rand == PlayerPrefs.GetInt("LastRandomLevel"))
            {
                rand = Random.Range(1, MaxLevelNumber);
            }
            else
            {
                PlayerPrefs.SetInt("LastRandomLevel", rand);
            }
            SceneManager.LoadScene("Level" + rand);
        }
        else
        {
            SceneManager.LoadScene("Level" + currentLevel);
        }
    }

    public void TapToTryAgainButtonClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VibrateButtonClick()
    {
        if (PlayerPrefs.GetInt("VIBRATION").Equals(1))
        {//Vibration is on
            PlayerPrefs.SetInt("VIBRATION", 0);
            VibrationButton.GetComponent<Image>().sprite = off;
        }
        else
        {//Vibration is off
            PlayerPrefs.SetInt("VIBRATION", 1);
            VibrationButton.GetComponent<Image>().sprite = on;
        }

        if (PlayerPrefs.GetInt("VIBRATION") == 1)
            TapticManager.Impact(ImpactFeedback.Light);
    }

    public void TapToStartButtonClick()
    {
        Player.GetComponent<Animator>().SetTrigger("Start");
        isGameStarted = true;
        if (GameManager.Instance.currentLevel == 1)
        {
            Tutorial1Canvas.SetActive(true);
        }
    }
    public GameObject Ship;
    public Transform ShipTarget;
    public void TapToLoadButtonClick()
    {
        Player.GetComponent<PlayerController>().LoadObjsWithTime();
        Crates.SetActive(true);

        StartCoroutine(WaitAndGameWin());
    }
}
