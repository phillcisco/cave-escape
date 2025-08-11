using System;
using System.Collections;
using MobileControl;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utils;

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
#if (UNITY_ANDROID || UNITY_IOS) && !(UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER)
    MobileJoystick joystick;
#endif
    
    
    GameSession gameSession;
    
    
    //Referencias
    Rigidbody2D playerRb;
    Animator playerAnimator;
    BoxCollider2D boxCollider;
    TrailRenderer playerTr;
    AudioSource audioSourceDMG;
    PlayerTrail _playerTrail;
    
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
    bool isShooting;
    float playerArrowAttackAnimLength = 0;

    void Awake()
    {
        _playerTrail = GetComponent<PlayerTrail>();
    }

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
#if (UNITY_ANDROID || UNITY_IOS) && !(UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER)
        joystick = GameObject.FindWithTag(Constants.TAG_MOBILE_JOYSTICK).GetComponent<MobileJoystick>();
        joystick.OnMove += Move;
#endif
        
    }

    

    // Update is called once per frame
    void Update()
    {
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
        playerRb.linearVelocity = new Vector2(playerHorDir.x*velocidadeX, playerRb.linearVelocity.y);

        bool isRunning = Mathf.Abs(playerRb.linearVelocity.x) > Mathf.Epsilon;
        playerAnimator.SetBool("isRunning",isRunning);
        if(isRunning)
            FlipSprite();
    }

    void FlipSprite()
    {
        transform.localScale = new Vector2(Mathf.Sign(playerRb.linearVelocity.x) , 1);
    }

    void Climb()
    {
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x,velocidadeLadder*playerHorDir.y);
        bool isClimbing = Mathf.Abs(playerRb.linearVelocity.y) > Mathf.Epsilon;
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
        playerRb.linearVelocity = Vector2.zero;
        Physics2D.IgnoreLayerCollision(7, 9, true);
        Reset();
    }
    
    IEnumerator IETakingDmg()
    {
        isTakingDMG = true;
        playerAnimator.SetBool("isTknDmg",isTakingDMG);
        playerRb.linearVelocity = new Vector2(-magHorDMG*transform.localScale.x,magVerDMG);
        AudioSource.PlayClipAtPoint(audioSourceDMG.clip,transform.position);
        gameSession.ProcessandoVida();
        yield return new WaitForSecondsRealtime(tmpTakingDMG);
        isTakingDMG = false;
        playerAnimator.SetBool("isTknDmg",isTakingDMG);
    }

    IEnumerator IEPlayerShooting()
    {
        playerAnimator.SetBool("isShootingArrow",isShooting);
        playerRb.linearVelocity = Vector2.zero;//jogar parar de andar
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
        playerRb.linearVelocity = new Vector2(transform.localScale.x*dashSpeed, 0);
        playerTr.emitting = isDashing;
        _playerTrail.StartTrailVFX();
        yield return new WaitForSecondsRealtime(dashDur);
        isDashing = false;
        playerTr.emitting = isDashing;
        playerRb.gravityScale = playerDefaultGravity;
        yield return new WaitForSecondsRealtime(dashCD);
        canDash = true;
        _playerTrail.DestroyTrails();
    }
    
    //Metodos de Actions
    
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
    void OnMove(InputValue inputValue)
    {
        //float xClamped = Mathf.Abs(inputValue.Get<Vector2>().x) > 0.5 ? inputValue.Get<Vector2>().x : 0;
        //float yClamped = Mathf.Abs(inputValue.Get<Vector2>().y) > 0.5 ? inputValue.Get<Vector2>().y : 0;
        playerHorDir = inputValue.Get<Vector2>();
    }
#elif (UNITY_ANDROID || UNITY_IOS)
    void Move(Vector2 input)
    {
        playerHorDir = input;
    }
#endif
    void OnJump(InputValue inputValue)
    {
        if (inputValue.isPressed && 
            boxCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x,velocidadeY);
            canDoubleJump = true;
        }else if (canDoubleJump)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x,velocidadeY);
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
//IEnumerator IEPlayerTakingDMG()
    // {
    //     mov = new Vector2(-transform.localScale.x*3, 10);
    //     _playerRb.velocity = new Vector2(mov.x,mov.y);
    //     yield return new WaitForSecondsRealtime(0.005f);
    //     mov = Vector2.zero;
    //     yield return new WaitForSecondsRealtime(1.5f);
    //     _playerAnimator.SetBool("isTakingDMG", false);
    //     GetComponent<PlayerInput>().ActivateInput();
    //     Physics2D.IgnoreLayerCollision(6,9,false);
    //     Physics2D.IgnoreLayerCollision(6,10,false);
    // }
