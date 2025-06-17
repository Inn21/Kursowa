using System;
using Core.Feature.PlayerStats.Model;
using Core.Feature.Save;
using Features.Tasks;
using Features.Tasks.Model;
using UnityEngine;
using Zenject;

namespace Core.Feature.PlayerStats
{
    public class PlayerStatsFeature : BaseFeature
    {
        public event Action<PlayerStatsData> OnStatsChanged;
        public event Action<AvatarState> OnAvatarStateChanged;

        [Inject] private readonly ISaveFeature _saveFeature;

        private PlayerStatsData _statsData;
        private const string SAVE_KEY = "PlayerStatsTEST2";

        public void Initialize()
        {
            LoadStats();
        }

        public PlayerStatsData GetCurrentStats()
        {
            return _statsData;
        }

        public void AddStat(RewardType type, int amount)
        {
            switch (type)
            {
                case RewardType.Strength:
                    _statsData.Strength = Mathf.Clamp(_statsData.Strength + amount, 0, _statsData.MaxStatValue);
                    break;
                case RewardType.Health:
                    _statsData.Health = Mathf.Clamp(_statsData.Health + amount, 0, _statsData.MaxStatValue);
                    break;
                case RewardType.Intelligence:
                    _statsData.Intelligence = Mathf.Clamp(_statsData.Intelligence + amount, 0, _statsData.MaxStatValue);
                    break;
                case RewardType.Xp:
                    AddXp(amount);
                    break;
            }
            
            UpdateAvatarState();
            OnStatsChanged?.Invoke(_statsData);
            SaveStats();
        }

        private void AddXp(int amount)
        {
            _statsData.CurrentXp += amount;
            while (_statsData.CurrentXp >= _statsData.XpToNextLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            _statsData.CurrentXp -= _statsData.XpToNextLevel;
            _statsData.Level++;
            _statsData.XpToNextLevel = CalculateXpForNextLevel(_statsData.Level);
        }

        private int CalculateXpForNextLevel(int level)
        {
            return 100 * level;
        }

        private void UpdateAvatarState()
        {
            var state = AvatarState.Normal;
            int healthPercentage = (int)((float)_statsData.Health / _statsData.MaxStatValue * 100);

            switch (healthPercentage)
            {
                case < 20:
                    state = AvatarState.Sad;
                    break;
                case >= 20 and < 70:
                    state = AvatarState.Normal;
                    break;
                case >= 70:
                    state = AvatarState.Happy;
                    break;
            }

            OnAvatarStateChanged?.Invoke(state);
        }

        private void SaveStats()
        {
            string json = JsonUtility.ToJson(_statsData);
            _saveFeature.Save(json, SAVE_KEY);
        }

        private void LoadStats()
        {
            if (_saveFeature.IsKeyPresent(SAVE_KEY))
            {
                string json = _saveFeature.Load(SAVE_KEY, "{}");
                _statsData = JsonUtility.FromJson<PlayerStatsData>(json);
            }
            else
            {
                _statsData = new PlayerStatsData
                {
                    PlayerName = "Гравець",
                    Strength = 3,
                    Health = 10,
                    Intelligence = 3,
                    MaxStatValue = 10,
                    Level = 1,
                    CurrentXp = 0,
                    XpToNextLevel = 100
                };
            }
            
            OnStatsChanged?.Invoke(_statsData);
            UpdateAvatarState();
        }
    }
}
