using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ArrowController : MonoBehaviour
{
    [Header("Controle Movimentacao Arrow")] 
    [SerializeField] float velocidadeArrow;

    public float ArrowDir { get; set; } = 1;          
    
    
    //Refs internas
    Rigidbody2D arrowRb;

    void Start()
    {
        arrowRb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        arrowRb.velocity = velocidadeArrow*ArrowDir*Vector2.right;
    }
}
