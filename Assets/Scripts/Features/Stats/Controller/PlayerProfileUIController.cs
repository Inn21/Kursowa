using System.Collections.Generic;
using Core.Feature.PlayerStats.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core.Feature.PlayerStats.UI
{
    public class PlayerProfileUIController : MonoBehaviour
    {
        [Header("Основні елементи")]
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private Image _avatarImage;

        [Header("Статистика")]
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Slider _strengthSlider;
        [SerializeField] private TextMeshProUGUI _strengthText;
        [SerializeField] private Slider _intelligenceSlider;
        [SerializeField] private TextMeshProUGUI _intelligenceText;
        
        [Header("Рівень")]
        [SerializeField] private Slider _levelSlider;
        [SerializeField] private TextMeshProUGUI _levelText;
        
        [Header("Аватари")]
        [SerializeField] private List<AvatarSpriteMapping> _avatarSprites;

        [Inject] private readonly PlayerStatsFeature _statsFeature;

        private void OnEnable()
        {
            _statsFeature.OnStatsChanged += UpdateUI;
            _statsFeature.OnAvatarStateChanged += UpdateAvatar;
            UpdateUI(_statsFeature.GetCurrentStats());
        }

        private void OnDisable()
        {
            _statsFeature.OnStatsChanged -= UpdateUI;
            _statsFeature.OnAvatarStateChanged -= UpdateAvatar;
        }

        private void UpdateUI(PlayerStatsData data)
        {
            if (data == null) return;

            _playerNameText.text = data.PlayerName;
            _levelText.text = $"Рівень {data.Level}";

            _healthSlider.maxValue = data.MaxStatValue;
            _healthSlider.value = data.Health;
            _healthText.text = $"Здоров'я {data.Health}/{data.MaxStatValue}";
            
            _strengthSlider.maxValue = data.MaxStatValue;
            _strengthSlider.value = data.Strength;
            _strengthText.text = $"Сила {data.Strength}/{data.MaxStatValue}";
            
            _intelligenceSlider.maxValue = data.MaxStatValue;
            _intelligenceSlider.value = data.Intelligence;
            _intelligenceText.text = $"Інтелект {data.Intelligence}/{data.MaxStatValue}";
            
            _levelSlider.maxValue = data.XpToNextLevel;
            _levelSlider.value = data.CurrentXp;
        }

        private void UpdateAvatar(AvatarState state)
        {
            var mapping = _avatarSprites.Find(s => s.State == state);
            if (mapping != null && mapping.Sprite != null)
            {
                _avatarImage.sprite = mapping.Sprite;
            }
        }
    }
    
    [System.Serializable]
    public class AvatarSpriteMapping
    {
        public AvatarState State;
        public Sprite Sprite;
    }
}
