using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace _PROJECT.Scripts.Core.InstanceGenerator
{
    public class DiInstanceGenerator
    {
        private readonly DiContainer _container;

        public DiInstanceGenerator(DiContainer container)
        {
            _container = container;
        }

        public TClass CreateScriptInstance<TClass>()
            where TClass : class
        {
            return (TClass)_container.Instantiate(typeof(TClass));
        }

        public TClass CreateScriptInstance<TClass>(IEnumerable<object> extraArgs)
            where TClass : class
        {
            return (TClass)_container.Instantiate(typeof(TClass), extraArgs);
        }

        
        public GameObject CreatePrefabInstance<TPrefab>(TPrefab prefab) where TPrefab : Object
        {
            return _container.InstantiatePrefab(prefab);
        }
        
        public GameObject CreatePrefabInstance<TPrefab>(
            TPrefab prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null) where TPrefab : Object
        {
            return _container.InstantiatePrefab(prefab, position, rotation, parent);
        }

       
        public GameObject CreatePrefabInstance(string path)
        {
            return _container.InstantiatePrefabResource(path);
        }

        
        public TComponent CreatePrefabForComponentInstance<TComponent>(GameObject go) where TComponent : MonoBehaviour
        {
            return _container.InstantiatePrefabForComponent<TComponent>(go);
        }

        public TComponent CreatePrefabForComponentInstance<TComponent>(GameObject go, IEnumerable<object> extraArgs)
            where TComponent : MonoBehaviour
        {
            return _container.InstantiatePrefabForComponent<TComponent>(go, extraArgs);
        }

       
        public TComponent CreatePrefabForComponentInstance<TComponent>(string path) where TComponent : MonoBehaviour
        {
            return _container.InstantiatePrefabResourceForComponent<TComponent>(path);
        }

        public TComponent CreatePrefabForComponentInstance<TComponent>(string path, IEnumerable<object> extraArgs)
            where TComponent : MonoBehaviour
        {
            return _container.InstantiatePrefabResourceForComponent<TComponent>(path, extraArgs);
        }

        
        public TComponent CreateComponentInstance<TComponent>(GameObject go) where TComponent : Component
        {
            return _container.InstantiateComponent<TComponent>(go);
        }

        public TComponent CreateComponentInstance<TComponent>(GameObject go, IEnumerable<object> extraArgs)
            where TComponent : Component
        {
            return _container.InstantiateComponent<TComponent>(go, extraArgs);
        }

        public TComponent CreateComponentOnNewGameObject<TComponent>() where TComponent : Component
        {
            return _container.InstantiateComponentOnNewGameObject<TComponent>();
        }

        public TComponent CreateComponentOnNewGameObject<TComponent>(IEnumerable<object> extraArgs)
            where TComponent : Component
        {
            return _container.InstantiateComponentOnNewGameObject<TComponent>(extraArgs);
        }

        public TScriptable CreateScriptableInstance<TScriptable>(string path) where TScriptable : ScriptableObject
        {
            return _container.InstantiateScriptableObjectResource<TScriptable>(path);
        }

        public TScriptable CreateScriptableInstance<TScriptable>(string path,
            IEnumerable<object> extraArgs) where TScriptable : ScriptableObject
        {
            return _container.InstantiateScriptableObjectResource<TScriptable>(path, extraArgs);
        }
    }
}