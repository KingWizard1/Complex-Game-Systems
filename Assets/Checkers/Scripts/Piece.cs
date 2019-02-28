using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class Piece : MonoBehaviour
    {
        
        public int x, y;

        public bool isWhite;
        public bool isKing;

        private Animator anim;

        // Use this for initialization
        void Start()
        {
            anim = GetComponentInChildren<Animator>();
        }

        
        public void King()
        {
            isKing = true;

            anim.SetTrigger("King");

        }
    } 
}
