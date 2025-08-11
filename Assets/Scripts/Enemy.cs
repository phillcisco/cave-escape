using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{   
    
    //Serializaveis
    [SerializeField] float enemyVelocidade = 2.0f;
    [SerializeField] GameObject poofVFX;
    [SerializeField] AudioClip clip;
    
    //Referencias
    Rigidbody2D enemyRb;
    // Start is called before the first frame update
    void Start()
    {
        enemyRb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Movimentacao();
    }

    void Movimentacao()
    {
        enemyRb.linearVelocity = new Vector2(enemyVelocidade,0);
    }

    void FlipSprite()
    {
        transform.localScale = new Vector2(-Mathf.Sign(enemyRb.linearVelocity.x) , 1);
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        enemyVelocidade = -enemyVelocidade;
        FlipSprite();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            if (col.collider as BoxCollider2D)
            {
                DestroyEnemy();   
            }
            else
            {
                col.gameObject.GetComponent<Player>().InitDmgAnimation();
            }
        }
    }

    public void DestroyEnemy()
    {
        GameObject vfxClone = Instantiate(poofVFX, transform.position, Quaternion.identity);
        Destroy(vfxClone,1.2f);
        Destroy(gameObject);
        AudioSource.PlayClipAtPoint(clip,transform.position,2);
    }
}