using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{

    public class CameraOrbit : MonoBehaviour
    {

        // Distance the camera is from world zero
        public float distance = 10f;

        // X and Y rotation speed
        public float xSpeed = 120.0f;
        public float ySpeed = 120.0f;

        // X and Y rotation limits
        public float yMin = 15f;
        public float yMax = 80f;

        // Optional look at target
        public Transform target;

        // Current X and Y rotation
        private float x = 0.0f;
        private float y = 0.0f;


        // ------------------------------------------------- //

        private void Start()
        {

            // Get current rotation of camera.
            // Because its going to be represented as mouse coordinates, we need to swap X and Y.
            Vector3 euler = transform.eulerAngles;
            x = euler.y;
            y = euler.x;

        }

        // ------------------------------------------------- //

        private void LateUpdate()
        {
            
            // Is the right mouse button pressed?
            if (Input.GetMouseButton(1))
            {
                // Hide the cursor
                Cursor.visible = false;

                // Get input X and Y offsets
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                // Offset rotation with mouse X and Y offset
                x += mouseX * xSpeed * Time.deltaTime;
                y += mouseY * ySpeed * Time.deltaTime;


            }
            else
            {
                // Show the cursor when not holding button
                Cursor.visible = true;
            }

            // Update transform
            transform.rotation = Quaternion.Euler(y, x, 0);

            if (target == null)
                transform.position = -transform.forward * distance;
            else
                transform.position = target.position - transform.forward * distance;

        }

        // ------------------------------------------------- //

        // ------------------------------------------------- //

    }

}