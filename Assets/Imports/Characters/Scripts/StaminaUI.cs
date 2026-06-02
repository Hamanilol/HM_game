using UnityEngine;
using UnityEngine.UI;
using Abdulrahman.PlayerSystem;

namespace Abdulrahman.PlayerSystem
{
    public class StaminaUI : MonoBehaviour
    {
        [Header("References")]
        public PlayerController playerController;
        public Slider staminaSlider;
        public CanvasGroup canvasGroup;

        [Header("Settings")]
        public float fadeSpeed = 5f;
        public bool hideWhenFull = true;

        private void Start()
        {
            if (playerController == null)
                playerController = Object.FindAnyObjectByType<PlayerController>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (staminaSlider != null)
            {
                staminaSlider.maxValue = playerController.MaxStamina;
                staminaSlider.value = playerController.CurrentStamina;
            }
        }

        private void Update()
        {
            if (playerController == null || staminaSlider == null) return;

            staminaSlider.value = playerController.CurrentStamina;

            bool shouldShow = playerController.CurrentState == PlayerController.MovementState.Running || 
                             (hideWhenFull && playerController.CurrentStamina < playerController.MaxStamina);

            float targetAlpha = shouldShow ? 1f : 0f;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            }
        }
    }
}