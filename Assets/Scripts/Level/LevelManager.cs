using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Rift.Level
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Configuration")]
        public List<LevelData> levels = new List<LevelData>();
        public int currentLevelIndex = 0;

        [Header("References")]
        public WeaponSwitchController weaponController;
        public PlayerController playerController;

        private bool isTransitioning = false;

        private void Start()
        {
            // Find references if not assigned
            if (weaponController == null)
            {
                weaponController = FindObjectOfType<WeaponSwitchController>();
            }
            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
            }

            // Load saved game or start new game
            SaveData saveData = SaveManager.Instance.GetSaveData();
            if (saveData != null)
            {
                currentLevelIndex = saveData.currentLevel;
                LoadLevel(currentLevelIndex);
                playerController.currentHealth = saveData.playerHealth;
                playerController.SetHeals(saveData.remainingHeals);
            }
            else
            {
                LoadLevel(currentLevelIndex);
            }
        }

        public void LoadLevel(int levelIndex)
        {
            if (isTransitioning) return;
            StartCoroutine(LoadLevelCoroutine(levelIndex));
        }

        private IEnumerator LoadLevelCoroutine(int levelIndex)
        {
            isTransitioning = true;

            // Start transition out
            yield return StartCoroutine(TransitionManager.Instance.FadeOut());

            if (levelIndex < 0 || levelIndex >= levels.Count)
            {
                Debug.LogError("Invalid level index!");
                isTransitioning = false;
                yield break;
            }

            currentLevelIndex = levelIndex;
            LevelData levelData = levels[levelIndex];

            // Configure player
            if (playerController != null)
            {
                playerController.maxHealth = levelData.playerMaxHealth;
                playerController.currentHealth = levelData.playerMaxHealth;
                playerController.maxHeals = levelData.maxHeals;
                playerController.SetHeals(levelData.maxHeals);
            }

            // Equip level weapons
            if (weaponController != null)
            {
                // Instantiate new weapon instances
                GameObject gunInstance = Instantiate(levelData.gunPrefab);
                GameObject meleeInstance = Instantiate(levelData.meleeWeaponPrefab);

                // Equip the weapons
                weaponController.EquipLevelWeapons(gunInstance, meleeInstance);
            }

            // Save game progress
            SaveManager.Instance.SaveGame(this, playerController);

            // Start transition in
            yield return StartCoroutine(TransitionManager.Instance.FadeIn());

            isTransitioning = false;
            Debug.Log($"Loaded level: {levelData.levelName}");
        }

        public void NextLevel()
        {
            if (currentLevelIndex < levels.Count - 1)
            {
                LoadLevel(currentLevelIndex + 1);
            }
            else
            {
                Debug.Log("Game completed!");
                // Handle game completion
            }
        }

        public void RestartLevel()
        {
            LoadLevel(currentLevelIndex);
        }

        public void CompleteLevel()
        {
            SaveData saveData = SaveManager.Instance.GetSaveData();
            if (saveData != null)
            {
                saveData.lastCompletedLevel = currentLevelIndex;
                SaveManager.Instance.SaveGame(this, playerController);
            }

            // Show level complete UI
            Debug.Log($"Completed level: {levels[currentLevelIndex].levelName}");
            
            // Auto-progress to next level after a delay
            StartCoroutine(AutoNextLevel());
        }

        private IEnumerator AutoNextLevel()
        {
            yield return new WaitForSeconds(2f); // Wait before loading next level
            NextLevel();
        }
    }
} 