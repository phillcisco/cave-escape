using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    //Referencias
    AudioSource audioSource;
   
    //Valores internos
    bool notDestroyed = true;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && notDestroyed)
        {
            notDestroyed = false;
            FindObjectOfType<GameSession>().ProcessandoColetaItem();
            AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);
            Destroy(gameObject);
        }
    }
}