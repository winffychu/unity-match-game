using System;
using System.Collections.Generic;
using UnityEngine;

namespace MemoryMatch.Data
{
    [Serializable]
    public class LevelProgress
    {
        public int levelId;
        public bool isUnlocked;
        public bool isCompleted;
        public int bestScore;
        public int bestStars;
        public int bestCombo;
        public int playCount;
        public string lastPlayTime;

        public LevelProgress(int id)
        {
            levelId = id;
            isUnlocked = id == 0;
            isCompleted = false;
            bestScore = 0;
            bestStars = 0;
            bestCombo = 0;
            playCount = 0;
            lastPlayTime = DateTime.MinValue.ToString();
        }
    }

    [Serializable]
    public class PlayerProgressData
    {
        public int totalStars;
        public int highestUnlockedLevel;
        public List<LevelProgress> levelProgresses = new List<LevelProgress>();
        public int totalMatches;
        public int totalGamesPlayed;
        public int highestCombo;
        public int totalPlayTimeSeconds;
        public float masterVolume = 1f;
        public float bgmVolume = 0.7f;
        public float sfxVolume = 1f;
        public bool vibrationEnabled = true;

        public LevelProgress GetLevelProgress(int levelId)
        {
            return levelProgresses.Find(lp => lp.levelId == levelId);
        }

        public void InitializeLevelProgress(int totalLevels)
        {
            for (int i = 0; i < totalLevels; i++)
            {
                if (GetLevelProgress(i) == null)
                {
                    levelProgresses.Add(new LevelProgress(i));
                }
            }
        }
    }

    public static class SaveSystem
    {
        private static readonly string SAVE_FOLDER = "/SaveData/";
        private static readonly string SAVE_FILE = "player_progress.json";

        private static string GetSavePath()
        {
            string path = Application.persistentDataPath + SAVE_FOLDER;
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            return path + SAVE_FILE;
        }

        public static void SaveProgress(PlayerProgressData data)
        {
            try
            {
                string jsonData = JsonUtility.ToJson(data, true);
                System.IO.File.WriteAllText(GetSavePath(), jsonData);
                Debug.Log("玩家进度已保存");
            }
            catch (Exception e)
            {
                Debug.LogError($"保存失败: {e.Message}");
            }
        }

        public static PlayerProgressData LoadProgress()
        {
            string path = GetSavePath();
            if (System.IO.File.Exists(path))
            {
                try
                {
                    string jsonData = System.IO.File.ReadAllText(path);
                    PlayerProgressData data = JsonUtility.FromJson<PlayerProgressData>(jsonData);
                    Debug.Log("玩家进度已加载");
                    return data ?? CreateNewProgress();
                }
                catch (Exception e)
                {
                    Debug.LogError($"加载存档失败: {e.Message}");
                    return CreateNewProgress();
                }
            }
            return CreateNewProgress();
        }

        public static PlayerProgressData CreateNewProgress()
        {
            return new PlayerProgressData();
        }

        public static void DeleteProgress()
        {
            string path = GetSavePath();
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                Debug.Log("存档已删除");
            }
        }

        public static bool SaveExists()
        {
            return System.IO.File.Exists(GetSavePath());
        }
    }
}
