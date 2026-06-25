using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SojaExiles
{
    public class PlayerMovement : MonoBehaviour
    {
        public CharacterController controller;

        [Header("Hareket Hızları")]
        public float walkSpeed = 5f;
        public float sprintSpeed = 8f;
        public float crouchSpeed = 2.5f;

        [Header("Fizik & Zıplama")]
        public float gravity = -19.62f; // Oyunlar için genellikle gerçek yerçekiminin iki katı daha iyi hissettirir
        public float jumpHeight = 1.5f;

        [Header("Eğilme Ayarları")]
        public float normalHeight = 2f;
        public float crouchHeight = 1f;

        Vector3 velocity;
        bool isGrounded;
        bool isCrouching;

        void Update()
        {
            // 1. Zemin Kontrolü (CharacterController'ın kendi özelliğini kullanıyoruz)
            isGrounded = controller.isGrounded;

            if (isGrounded && velocity.y < 0)
            {
                // Karakter yerdeyken onu yere hafifçe bastırıyoruz ki merdiven/yokuş inerken sekmeyesin
                velocity.y = -2f; 
            }

            // 2. Klavye Girdileri (WASD veya Yön Tuşları)
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            // 3. Koşma ve Eğilmeye Göre Hız Belirleme
            float currentSpeed = walkSpeed;

            if (Input.GetKey(KeyCode.LeftShift) && !isCrouching)
            {
                currentSpeed = sprintSpeed; // Shift'e basılıysa koş
            }
            else if (isCrouching)
            {
                currentSpeed = crouchSpeed; // Eğiliyorsa yavaş yürü
            }

            // Karakteri hareket ettir
            controller.Move(move * currentSpeed * Time.deltaTime);

            // 4. Zıplama (Space tuşu)
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                // Fizik formülü: v = karekök(h * -2 * g)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // 5. Eğilme (Sol Control tuşu)
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                isCrouching = true;
                controller.height = crouchHeight; // Karakterin boyunu kısalt
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                isCrouching = false;
                controller.height = normalHeight; // Karakterin boyunu eski haline getir
            }

            // 6. Yerçekimi Uygulaması
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}