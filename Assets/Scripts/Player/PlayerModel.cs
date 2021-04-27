﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools.Components;
using System;

namespace AgeOfWarBuilders.Entities
{
    public class PlayerModel : MonoBehaviour
    {
        #region D-XZ Vars
        CharacterController controller;
        [SerializeField] Transform cam;
        [SerializeField] float speed = 12f;
        bool Aux_sideMovement;
        public float turnSmoothTime = 0.1f;
        float turnSmoothVelocity;
        Vector3 moveDir;
        float targetangle;
        float angle;
        Vector3 direction;
        #endregion
        #region D-Y Vars
        PlayerComponent_GroundCheck groundcheck;
        Vector3 velocity;
        bool isGrounded;
        [SerializeField] float jumpHeight = 3f;
        [SerializeField] float gravity = -9.81f;
        const float GFORCE = -2;
        #endregion

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            groundcheck = GetComponentInChildren<PlayerComponent_GroundCheck>();
            if (groundcheck == null) throw new System.Exception("No have a [PlayerComponent_GroundCheck], plase add to some child object");
        }
        private void Update()
        {
            #region Movement & Rotation
            Aux_sideMovement = PlayerController.HOLD_Ctrl;

            direction = new Vector3(PlayerController.AXIS_Horizontal, 0f, PlayerController.AXIS_Vertical).normalized;

            if (direction.magnitude >= 0.1f) //esto es un deathzone virtual
            {
               
                // calculo la direccion dependiendo... el input de movimiento y le sumo la rotacion de la camara
                targetangle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                // esto hace un Smooth entre mi rotacion y la rotacion a la que quiero ir, le paso un valor de "Cantidad de movimiento (turnSmoothTime)"
                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, !Aux_sideMovement ? targetangle : cam.eulerAngles.y, ref turnSmoothVelocity, turnSmoothTime);

                // ahora si le paso la rotacion directamente porque ya esta todo precalculado
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                // multiplicar un Quaternion por un Vector3 me da un Vector3
                //¿ porque se le multiplica por Forward ? porque asi cuando me mueva por segunda vez no pierda mi rotacion original
                moveDir = Quaternion.Euler(0, targetangle, 0) * Vector3.forward;

                //al controller solo le tengo que pasar el vector direccion
                controller.Move(moveDir.normalized * speed * Time.deltaTime);
            }
            #endregion
            #region Jump & Gravity
            isGrounded = groundcheck.IsGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            //esto se ejecuta 1 solo frame
            if (PlayerController.PRESS_DOWN_Jump && isGrounded)
            {
                //con esto calculamos la altura en la que vamos a saltar...
                //si le pasamos esto al Move del controller va a hacer que se dispare hasta cierto punto
                //¿cual el la diferencia entre GForce y gravity?
                // GForce es el multiplicador de Fuerza de Gravedad
                // en cambio Gravity el la cantidad y direccion de gravedad
                velocity.y = Mathf.Sqrt(jumpHeight * GFORCE * gravity);
            }

            //la gravedad va a estar todo el tiempo afectando... si el valor de Velocity
            //se dispara repentinamente, esto va a hacer que retroceda
            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);
            #endregion
        }

    }
}