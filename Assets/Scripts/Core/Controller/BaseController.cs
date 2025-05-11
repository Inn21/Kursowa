using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Controller
{
    public abstract class BaseController<TConfig> : MonoBehaviour where TConfig : BaseControllerConfig
    {
        [FormerlySerializedAs("config")] [FoldoutGroup("Config")][SerializeField][InlineEditor] protected TConfig _config;
        [FoldoutGroup("Config")][SerializeField] protected bool Autoconfiguration = false;
        [FoldoutGroup("Config")] private const string ConfigsFolder = "Configs";

        [FoldoutGroup("Config")]
        [Button("TryAutoconfigure", ButtonSizes.Medium)]
        public void TryAutoconfigure()
        {
            if (_config == null)
            {
                string configName = typeof(TConfig).Name;
                _config = Resources.Load<TConfig>($"{ConfigsFolder}/{configName}");

#if UNITY_EDITOR
                if (_config != null)
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        private void OnValidate()
        {
            if (Autoconfiguration && _config == null)
            {
                TryAutoconfigure();
            }

        }

        public void SetConfig(BaseControllerConfig newConfig)
        {
            if (newConfig is TConfig typedConfig)
            {
                _config = typedConfig;
            }
            else
            {
                Debug.LogError($"Invalid config type. Expected {typeof(TConfig)}, got {newConfig.GetType()}");
            }
        }

        public TConfig GetConfig()
        {
            return _config;
        }
    }
}
