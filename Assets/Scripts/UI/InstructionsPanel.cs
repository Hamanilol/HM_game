using UnityEngine;
using UnityEngine.UI;

namespace HorrorMansion.UI
{
    /// <summary>
    /// Shows a controls/instructions overlay when the scene loads.
    /// Pauses the game and frees the cursor while visible, then dismisses
    /// when the player clicks the close button or presses any key.
    /// </summary>
    public class InstructionsPanel : MonoBehaviour
    {
        [Header("REFERENCES")]
        [Tooltip("Root object of the instructions panel (the dim overlay + window).")]
        [SerializeField] private GameObject panelRoot;
        [Tooltip("Button that closes the panel.")]
        [SerializeField] private Button closeButton;

        [Header("BEHAVIOUR")]
        [Tooltip("Show the panel automatically when the scene loads.")]
        [SerializeField] private bool showOnStart = true;
        [Tooltip("Freeze the game (Time.timeScale = 0) while the panel is visible.")]
        [SerializeField] private bool pauseWhileVisible = true;
        [Tooltip("Allow dismissing the panel by pressing any key.")]
        [SerializeField] private bool dismissOnAnyKey = true;

        private bool _isVisible;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
        }

        private void Start()
        {
            if (showOnStart)
                Show();
            else
                Hide();
        }

        private void Update()
        {
            if (!_isVisible || !dismissOnAnyKey)
                return;

            // Dismiss on any key press (ignore the mouse-down that may belong to the close button).
            if (Input.anyKeyDown)
                Hide();
        }

        // Runs after every other script's Update, so the paused/cursor state wins
        // even if the player controller or other systems change it during the frame.
        private void LateUpdate()
        {
            if (!_isVisible)
                return;

            if (pauseWhileVisible && Time.timeScale != 0f)
                Time.timeScale = 0f;

            if (Cursor.lockState != CursorLockMode.None)
                Cursor.lockState = CursorLockMode.None;

            if (!Cursor.visible)
                Cursor.visible = true;
        }

        public void Show()
        {
            _isVisible = true;

            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (pauseWhileVisible)
                Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Hide()
        {
            _isVisible = false;

            if (panelRoot != null)
                panelRoot.SetActive(false);

            if (pauseWhileVisible)
                Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
