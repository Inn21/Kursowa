#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Engine.Core.Services.MonoUtils
{
    public abstract class BaseMonoBehaviour : MonoBehaviour
    {
        private Dictionary<Type, Component> _cachedComponents;

        private GameObject _cachedGameObject;
        private RectTransform _cachedRectTransform;
        private Transform _cachedTransform;
        private bool _hasCachedVitalComponents;

        public GameObject GameObject => CacheGameObject();
        public Transform Transform => CacheTransform();
        public RectTransform RectTransform => CacheRectTransform();
        public bool IsDestroyed { get; private set; }

        private void OnDestroy()
        {
            OnBaseMonoDestroy();

            IsDestroyed = true;

            _cachedGameObject = null;
            _cachedTransform = null;
            _cachedRectTransform = null;
            _cachedComponents = null;
        }

        protected virtual void OnBaseMonoDestroy()
        {
        }

        #region Get Components API

        public TComponent GetCachedComponent<TComponent>() where TComponent : Component
        {
            if (IsDestroyed) return null;

            const int capacity = 4;

            _cachedComponents ??= new Dictionary<Type, Component>(capacity);

            if (!_cachedComponents.TryGetValue(typeof(TComponent), out var tComponent))
            {
                tComponent = GetComponent<TComponent>();
                _cachedComponents.Add(typeof(TComponent), tComponent);
            }

            return tComponent as TComponent;
        }

        #endregion

        #region Cache Vital Objects

        private GameObject CacheGameObject()
        {
            if (IsDestroyed) return null;

            TryCacheVitalComponents();
            return _cachedGameObject;
        }

        private Transform CacheTransform()
        {
            if (IsDestroyed) return null;

            TryCacheVitalComponents();
            return _cachedTransform;
        }

        private RectTransform CacheRectTransform()
        {
            if (IsDestroyed) return null;

            TryCacheVitalComponents();
            return _cachedRectTransform;
        }

        private void TryCacheVitalComponents()
        {
            if (_hasCachedVitalComponents) return;

            _hasCachedVitalComponents = true;
            _cachedGameObject = gameObject;
            _cachedTransform = transform;
            _cachedRectTransform = _cachedTransform as RectTransform;
        }

        #endregion
    }
}