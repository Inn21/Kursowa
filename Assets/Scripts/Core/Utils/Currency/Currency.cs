using System;
using System.Collections.Generic;
using _PROJECT.Scripts.Core.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Utils.Currency
{
    [CreateAssetMenu(fileName = "Currency", menuName = "Currency")]
    public class Currency : ScriptableObject
    {
        public Enums.CurrencyType Type;
        public Sprite defaultSprite;
        public Vector2 IconScale = Vector2.one;

        [SerializeField] private List<SceneSpritePair> sceneSprites;
        public GameObject defaultParticlePrefab;
        [SerializeField] private List<SceneParticlePair> sceneParticlePrefabs;

        public Sprite GetSpriteForCurrentScene()
        {
            var currentSceneName = SceneManager.GetActiveScene().name;

            foreach (var pair in sceneSprites)
                if (pair.sceneName == currentSceneName)
                    return pair.sprite;

            return defaultSprite;
        }
        public GameObject GetParticleForCurrentScene()
        {
            var currentSceneName = SceneManager.GetActiveScene().name;

            foreach (var pair in sceneParticlePrefabs)
                if (pair.sceneName == currentSceneName)
                    return pair.gameObject;

            return defaultParticlePrefab;
        }
    }

    [Serializable]
    public struct SceneSpritePair
    {
        public string sceneName;
        public Sprite sprite;
    }

    [Serializable]
    public struct SceneParticlePair
    {
        public string sceneName;
        public GameObject gameObject;
    }
}