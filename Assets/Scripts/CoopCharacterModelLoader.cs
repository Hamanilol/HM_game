using UnityEngine;

namespace Abdulrahman.PlayerSystem
{
    /// <summary>
    /// Activates the correct body model (Female / Male) for a co-op player based on
    /// the character chosen on the Character Select screen, and repoints the
    /// PlayerController's bodyAnimator to the active model's Animator so movement
    /// animations play on the visible body.
    ///
    /// Character index convention (matches CharacterSelector.characters order):
    ///   0 = Female, 1 = Male.
    /// </summary>
    [DefaultExecutionOrder(-50)] // Run before PlayerController so bodyAnimator is set early.
    public class CoopCharacterModelLoader : MonoBehaviour
    {
        [Header("Body Models")]
        [Tooltip("Body model used when the Female character (index 0) is chosen.")]
        public GameObject femaleModel;
        [Tooltip("Body model used when the Male character (index 1) is chosen.")]
        public GameObject maleModel;

        [Header("Player Identity")]
        [Tooltip("True for Player 2 (reads Player2Character), false for Player 1 (reads Player1Character).")]
        public bool isPlayer2 = false;

        [Header("References")]
        [Tooltip("The PlayerController whose bodyAnimator should be driven by the active model.")]
        public PlayerController playerController;

        private const string KeyP1 = "Player1Character";
        private const string KeyP2 = "Player2Character";

        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            string key = isPlayer2 ? KeyP2 : KeyP1;
            int chosen = PlayerPrefs.GetInt(key, isPlayer2 ? 1 : 0); // sensible defaults: P1 female, P2 male

            bool useMale = (chosen == 1);
            GameObject active = useMale ? maleModel : femaleModel;
            GameObject inactive = useMale ? femaleModel : maleModel;

            if (inactive != null) inactive.SetActive(false);
            if (active != null)
            {
                active.SetActive(true);

                var animator = active.GetComponent<Animator>();
                if (animator == null) animator = active.GetComponentInChildren<Animator>(true);
                if (animator != null && playerController != null)
                    playerController.bodyAnimator = animator;
            }

            Debug.Log("[CoopCharacterModelLoader] " + name + " (isPlayer2=" + isPlayer2 +
                      ") chose index " + chosen + " -> " + (useMale ? "Male" : "Female"));
        }
    }
}