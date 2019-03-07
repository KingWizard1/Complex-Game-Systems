using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{

    public class Piece : MonoBehaviour
    {

        public bool isWhite, isKing;

        public Vector2Int cell, oldCell;

        

        private Animator anim;

        // ------------------------------------------------- //

        void Awake()
        {

            anim = GetComponent<Animator>();

        }
        
        // ------------------------------------------------- //

        void Update()
        {

        }

        // ------------------------------------------------- //

        public void King()
        {
            // Set new state
            isKing = true;

            // Trigger kinged animation
            anim.SetTrigger("King");

        }

        // ------------------------------------------------- //

    }

}