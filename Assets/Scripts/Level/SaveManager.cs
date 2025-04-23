using UnityEngine;
using System.IO;
using System;

namespace Rift.Level
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager instance;
        public static SaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SaveManager>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject("SaveManager");
                        instance = obj.AddComponent<SaveManager>();
                    }
                }
                return instance;
            }
        }

        private string savePath;
        private SaveData currentSaveData;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            LoadGame();
        }

        public void SaveGame(LevelManager levelManager, PlayerController player)
        {
            if (currentSaveData == null)
            {
                currentSaveData = new SaveData();
            }

            currentSaveData.currentLevel = levelManager.currentLevelIndex;
            currentSaveData.playerHealth = player.currentHealth;
            currentSaveData.remainingHeals = player.maxHeals; // Using maxHeals as we have a SetHeals method
            currentSaveData.lastSaveTime = DateTime.Now;

            string json = JsonUtility.ToJson(currentSaveData, true);
            File.WriteAllText(savePath, json);
            
            Debug.Log("Game saved successfully!");
        }

        public void LoadGame()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                currentSaveData = JsonUtility.FromJson<SaveData>(json);
                Debug.Log("Game loaded successfully!");
            }
            else
            {
                currentSaveData = new SaveData();
                Debug.Log("No save file found. Starting new game.");
            }
        }

        public SaveData GetSaveData()
        {
            return currentSaveData;
        }

        public void DeleteSave()
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                currentSaveData = new SaveData();
                Debug.Log("Save file deleted.");
            }
        }
    }
} 