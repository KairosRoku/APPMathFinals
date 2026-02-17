using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefense.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Global UI Settings")]
        public bool ShowCursor = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Ensure correct cursor state on initialization
                UpdateCursorState();
            }
            else if (Instance != this)
            {
                // If a duplicate exists, destroy the new one, not the old boss.
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UpdateCursorState();
            
            // Re-ensure EventSystem exists in every scene
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[UIManager] Created missing EventSystem.");
            }
        }

        public void UpdateCursorState()
        {
            if (ShowCursor)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public void SetCursorVisibility(bool visible)
        {
            ShowCursor = visible;
            UpdateCursorState();
        }

        /// <summary>
        /// Global helper to return to the Main Menu safely.
        /// </summary>
        public void ReturnToMainMenu()
        {
            Time.timeScale = 1;
            SetCursorVisibility(true);
            SceneManager.LoadScene("MainMenu");
        }
    }
}
