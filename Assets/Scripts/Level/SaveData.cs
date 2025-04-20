using UnityEngine;
using System;

namespace Rift.Level
{
    [System.Serializable]
    public class SaveData
    {
        public int lastCompletedLevel = 0;
        public int currentLevel = 0;
        public float playerHealth = 100f;
        public int remainingHeals = 3;
        public DateTime lastSaveTime;
        
        // Add any other data you want to save
    }
} 