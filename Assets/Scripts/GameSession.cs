using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    float playerLives = 3;
    float playerScore;
    
    public float PlayerArrow { get; set; } = 10;


    [SerializeField] TextMeshProUGUI playerLiveText;
    [SerializeField] TextMeshProUGUI playerColetaText;
    [SerializeField] TextMeshProUGUI playerNumberArrow;
    [SerializeField] GameObject panelReset;

    public static GameSession GameSessionInstance { get; private set; }

    
    void Awake()
    {
        if (GameSessionInstance == null)
        {
            GameSessionInstance = FindObjectOfType<GameSession>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //Start is called before the first frame update
    void Start()
    {
        playerLiveText.text = playerLives.ToString();
        playerNumberArrow.text = PlayerArrow.ToString();
    }

    public void ProcessandoVida()
    {
        playerLives--;
        if(playerLives <= 0)
            FindObjectOfType<Player>().InitPlayerDeath();
        playerLiveText.text = playerLives.ToString();
    }

    public void ProcessandoColetaMoeda(int score = 1)
    {
        playerScore += score;
        playerColetaText.text = playerScore.ToString();
    }

    public void ProcessandoTiro()
    {
        PlayerArrow--;
        playerNumberArrow.text = PlayerArrow.ToString();
    }
    public void Reset()
    {
        playerScore = 0;
        playerLives = 3;
        PlayerArrow = 10;
        playerLiveText.text = playerLives.ToString();
        playerColetaText.text = playerScore.ToString();
        StartCoroutine(PanelResetCD());
    }

    IEnumerator PanelResetCD()
    {
        panelReset.SetActive(true);
        yield return new WaitForSecondsRealtime(1.0f);
        panelReset.SetActive(false);
    }
}
