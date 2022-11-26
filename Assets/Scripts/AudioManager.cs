using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{

    public static AudioManager AudioManagerInstance { get; private set; }

    //Referencias
    Slider audioSliderVolume;
    AudioSource audioSourceBG;
    
    [SerializeField]
    Sprite[] btnMuteSprites;
    int btnCurrentSpriteIndex;

    GameObject btnMute;
    
    void Awake()
    {
        if (AudioManagerInstance == null)
        {
            AudioManagerInstance = FindObjectOfType<AudioManager>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
        audioSourceBG = GetComponent<AudioSource>();

        if (PlayerPrefs.HasKey("Volume"))
        {
            audioSourceBG.volume = PlayerPrefs.GetFloat("Volume");
        }
        else
        {
            audioSourceBG.volume = 0.5f;
            PlayerPrefs.SetFloat("Volume",audioSourceBG.volume);
        }
    }

    void OnSceneLoad(Scene scene, LoadSceneMode sceneMode)
    {
        if (scene.buildIndex == 1)//Menu Config
        {
            audioSliderVolume = FindObjectOfType<Slider>();
            audioSliderVolume.value = audioSourceBG.volume;
            audioSliderVolume.onValueChanged.AddListener(delegate { OnSliderValueChanged();});
            
            
            //Para achar o btnMute
            btnMute = GameObject.FindGameObjectWithTag("BTN_MUTE");
            btnCurrentSpriteIndex = audioSourceBG.volume == 0 ? 0 : 1;
            btnMute.GetComponent<Image>().sprite = btnMuteSprites[btnCurrentSpriteIndex];
            btnMute.GetComponent<Button>().onClick.AddListener(delegate { OnClickBtnMute(); });
        }
    }

    void OnClickBtnMute()
    {
        btnCurrentSpriteIndex = (btnCurrentSpriteIndex + 1) % 2;
        btnMute.GetComponent<Image>().sprite = btnMuteSprites[btnCurrentSpriteIndex];
        SaveAudio(btnCurrentSpriteIndex == 0 ? 0 : audioSliderVolume.value);
    }
    
    void OnSliderValueChanged()
    {
        float volume = audioSliderVolume.value;
        btnCurrentSpriteIndex = volume == 0 ? 0 : 1;
        btnMute.GetComponent<Image>().sprite = btnMuteSprites[btnCurrentSpriteIndex];
        SaveAudio(audioSliderVolume.value);
    }

    void SaveAudio(float volume)
    {
        audioSourceBG.volume = volume;
        PlayerPrefs.SetFloat("Volume",volume);
    }
    
}