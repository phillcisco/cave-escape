using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    float playerLives = 3;
    float playerScore;

    [SerializeField] TextMeshProUGUI playerLiveText;
    [SerializeField] TextMeshProUGUI playerColetaText;
    
    // Start is called before the first frame update
    void Start()
    {
        playerLiveText.text = playerLives.ToString();
    }

    public void ProcessandoVida()
    {
        playerLives--;
        if(playerLives <= 0)
            FindObjectOfType<Player>().InitPlayerDeath();
        playerLiveText.text = playerLives.ToString();
    }

    public void ProcessandoColetaItem(int score = 1)
    {
        playerScore += score;
        playerColetaText.text = playerScore.ToString();
    }
}
