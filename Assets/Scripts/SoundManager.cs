using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    public static SoundManager Instance { get { return _instance; } }

    public AudioSource[] gameSoundsList;


    //Sound Enums
    public enum GameSounds
    {
        Music,
        Lose,
        Win,
        Collect,
        Splash
    }

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
        DontDestroyOnLoad(gameObject);
        if (!PlayerPrefs.HasKey("SOUND"))
        {
            PlayerPrefs.SetInt("SOUND", 1);
        }
        playSound(GameSounds.Music);
    }


    public void playSound(GameSounds soundType)
    {
        if (PlayerPrefs.GetInt("SOUND").Equals(1))
        {
            gameSoundsList[(int)soundType].Play();
        }
    }

    public void stopSound(GameSounds soundType)
    {
        gameSoundsList[(int)soundType].Stop();
    }

    public void StopAllSounds()
    {
        for (int i = 1; i < gameSoundsList.Length; i++)
        {
            stopSound((GameSounds)i);
        }
    }
}
