using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] float velocidadeX = 5.0f;
    [SerializeField] float velocidadeY = 8.0f;
    [Header("Controle Dano")]
    [SerializeField] float tmpTakingDMG;
    [SerializeField] float magVerDMG;
    [SerializeField] float magHorDMG;
    [Header("Controle Escada")]
    [SerializeField] float velocidadeLadder;
    [Header("Variaveis Dash")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDur;
    [SerializeField] float dashCD;
    
    [Header("Arrow")]
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] GameObject arrowSpawnPoint;
    [SerializeField] float arrowAttackCD;
    
    [SerializeField] Sprite playerOnLadderSprite;
    
    
    GameSession gameSession;
    
    
    //Referencias
    Rigidbody2D playerRb;
    Animator playerAnimator;
    BoxCollider2D boxCollider;
    TrailRenderer playerTr;
    AudioSource audioSourceDMG;
        
    //Internas
    Vector2 playerHorDir;
    bool canDoubleJump;
    bool isTakingDMG;
    bool isDead;
    bool isOnLadder;
    float playerDefaultGravity;
    bool canDash = true;
    bool isDashing;
    bool canShoot = true;
    bool isShooting = false;
    float playerArrowAttackAnimLength = 0;
  
    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        playerDefaultGravity = playerRb.gravityScale;
        playerTr = GetComponent<TrailRenderer>();
        gameSession = GameSession.GameSessionInstance;
        audioSourceDMG = GetComponent<AudioSource>();
        Physics2D.IgnoreLayerCollision(7, 9, false);
        
        foreach (var animationClip in playerAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name == "PlayerArrow")
            {
                playerArrowAttackAnimLength = animationClip.length;
            }
        }
        
    }
    
    // Update is called once per frame
    void Update()
    {
        //if (isDead)
        //  return;
        if (!isTakingDMG && !isDashing && !isShooting)
        {
            Correr();
            if (isOnLadder)
                Climb();
        }
    }

    void Reset()
    {
        SceneManager.LoadScene("Level1");
        gameSession.Reset();
    }
    
    void Correr()
    {
        playerRb.velocity = new Vector2(playerHorDir.x*velocidadeX, playerRb.velocity.y);

        bool isRunning = Mathf.Abs(playerRb.velocity.x) > Mathf.Epsilon;
        playerAnimator.SetBool("isRunning",isRunning);
        if(isRunning)
            FlipSprite();
    }

    void FlipSprite()
    {
        transform.localScale = new Vector2(Mathf.Sign(playerRb.velocity.x) , 1);
    }

    void Climb()
    {
        playerRb.velocity = new Vector2(playerRb.velocity.x,velocidadeLadder*playerHorDir.y);
        bool isClimbing = Mathf.Abs(playerRb.velocity.y) > Mathf.Epsilon;
        playerAnimator.enabled = isClimbing;
    }
    
    public void InitDmgAnimation()
    {
        StartCoroutine(IETakingDmg());
    }

    public void InitPlayerDeath()
    {
        StartCoroutine(IEPlayerDeath());
    }


    IEnumerator IEPlayerDeath()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        playerAnimator.SetBool("isDead",true);
        isDead = true;
        GetComponent<PlayerInput>().enabled = false;
        playerRb.velocity = Vector2.zero;
        Physics2D.IgnoreLayerCollision(7, 9, true);
        Reset();
    }
    
    IEnumerator IETakingDmg()
    {
        isTakingDMG = true;
        playerAnimator.SetBool("isTknDmg",isTakingDMG);
        playerRb.velocity = new Vector2(-magHorDMG*transform.localScale.x,magVerDMG);
        AudioSource.PlayClipAtPoint(audioSourceDMG.clip,transform.position);
        gameSession.ProcessandoVida();
        yield return new WaitForSecondsRealtime(tmpTakingDMG);
        isTakingDMG = false;
        playerAnimator.SetBool("isTknDmg",isTakingDMG);
    }

    IEnumerator IEPlayerShooting()
    {
        playerAnimator.SetBool("isShootingArrow",isShooting);
        playerRb.velocity = Vector2.zero;//jogar parar de andar
        yield return new WaitForSecondsRealtime(playerArrowAttackAnimLength);
        canShoot = true;
        isShooting = false;
        playerAnimator.SetBool("isShootingArrow",isShooting);
        
    }
    
    IEnumerator IEPlayerDashing()
    {
        canDash = false;
        isDashing = true;
        playerRb.gravityScale = 0;
        playerRb.velocity = new Vector2(transform.localScale.x*dashSpeed, 0);
        playerTr.emitting = isDashing;
        yield return new WaitForSecondsRealtime(dashDur);
        isDashing = false;
        playerTr.emitting = isDashing;
        playerRb.gravityScale = playerDefaultGravity;
        yield return new WaitForSecondsRealtime(dashCD);
        canDash = true;
    }
    
    //Metodos de Actions
    void OnMove(InputValue inputValue)
    {
        playerHorDir = inputValue.Get<Vector2>();
    }

    void OnJump(InputValue inputValue)
    {
        if (inputValue.isPressed && 
            boxCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x,velocidadeY);
            canDoubleJump = true;
        }else if (canDoubleJump)
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x,velocidadeY);
            canDoubleJump = false;
        }
    }

    void OnDash(InputValue inputValue)
    {
        if (inputValue.isPressed && canDash)
            StartCoroutine(IEPlayerDashing());
    }

    void OnFire(InputValue inputValue)
    {
        if (inputValue.isPressed && gameSession.PlayerArrow > 0 && canShoot)
        {
            canShoot = false;
            isShooting = true;
            gameSession.ProcessandoTiro();
            GameObject arrow = Instantiate(arrowPrefab,arrowSpawnPoint.transform.position,Quaternion.identity);
            float arrowScale = transform.localScale.x;
            arrow.transform.localScale = new Vector3(arrowScale,1,1);
            arrow.GetComponent<ArrowController>().ArrowDir = arrowScale;
            StartCoroutine(IEPlayerShooting());
        }
    }
    
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Ladder"))
        {
            StopAllCoroutines();
            InterruptDashing();
            isOnLadder = true;
            playerAnimator.SetBool("isOnLadder",isOnLadder);
            playerRb.gravityScale = 0;
            GetComponent<SpriteRenderer>().sprite = playerOnLadderSprite;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isOnLadder = false;
            playerAnimator.SetBool("isOnLadder",isOnLadder);
            playerAnimator.enabled = !isOnLadder;
            playerRb.gravityScale = playerDefaultGravity;
        }
    }

    void InterruptDashing()
    {
        isDashing = false;
        canDash = true;
        playerTr.emitting = isDashing;
    }
    
}