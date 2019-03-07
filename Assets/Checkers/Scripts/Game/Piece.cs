using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool isWhite, isKing;
    public Vector2Int cell, oldCell;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();

    }
    public void King()
    {
        isKing = true;
        anim.SetTrigger("King");
    }
}
