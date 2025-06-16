using System;

namespace Core.Feature.PlayerStats.Model
{
    [Serializable]
    public class PlayerStatsData
    {
        public string PlayerName;

        public int Strength;
        public int Health;
        public int Intelligence;
        
        public int MaxStatValue;

        public int Level;
        public int CurrentXp;
        public int XpToNextLevel;
    }

    public enum AvatarState
    {
        Sad,
        Normal,
        Happy,
    }
}