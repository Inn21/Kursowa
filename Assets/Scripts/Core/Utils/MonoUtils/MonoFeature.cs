#region

using System;
using System.Collections;
using System.Collections.Generic;
using _PROJECT.Scripts.Core.Utils.MonoUtils;
using Core.Feature;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Core.Utils.MonoUtils
{
    public class MonoFeature : BaseFeature
    {
        private const int SlowUpdateRate = 10; // TODO: Add to config when ServiceConfig created

        private readonly float _unscaledFixedTimeStep = 0.1f;

        private readonly List<WaitForFramesHandler> _waitForNextFrameLateUpdate = new();

        private readonly List<WaitForFramesHandler> _waitForNextFrameUpdate = new();
        private bool _applicationQuitting;

        private MonoBehaviour _coroutineRunner;

        private bool _disposed;
        private MonoServiceInjector _monoServiceInjector;
        private float _perSecondUpdateTimeSum;
        private int _slowUpdateFrameCount;
        private float _slowUpdateTimeSum;
        private float _unscaledFixedTimeAccumulator;
        public event Action<float> OnUnscaledFixedUpdate;

        public event Action<float> OnUpdate;
        public event Action<float> OnFixedUpdate;
        public event Action<float> OnSlowUpdate;
        public event Action<float> OnLateUpdate;

        public event Action<float> OnPerSecondUpdate;

        public event Action<bool> OnApplicationFocusEvent;

        public event Action<bool> OnApplicationPauseEvent;

        public event Action OnApplicationQuitEvent;

        public void Initialize()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayerModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnPlayerModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode) OnApplicationQuit();
        }
#endif

        internal void Update()
        {
            var deltaTime = Time.deltaTime;
            UnscaledUpdate();
            OnUpdate?.Invoke(deltaTime);
            WaitForFrameUpdate(deltaTime);
            PerSecondUpdate(deltaTime);
            SlowUpdate(deltaTime);
        }

        private void UnscaledUpdate()
        {
            _unscaledFixedTimeAccumulator += Time.unscaledDeltaTime;
            while (_unscaledFixedTimeAccumulator >= _unscaledFixedTimeStep)
            {
                OnUnscaledFixedUpdate?.Invoke(_unscaledFixedTimeStep);
                _unscaledFixedTimeAccumulator -= _unscaledFixedTimeStep;
            }
        }

        internal void FixedUpdate()
        {
            OnFixedUpdate?.Invoke(Time.fixedDeltaTime);
        }

        private void WaitForFrameUpdate(float deltaTime)
        {
            if (_waitForNextFrameUpdate.Count == 0) return;

            var frameCount = Time.frameCount;
            for (var i = 0; i < _waitForNextFrameUpdate.Count; i++)
            {
                var handler = _waitForNextFrameUpdate[i];

                if (handler.TriggerFrame <= frameCount)
                {
                    handler.Action?.Invoke();
                    _waitForNextFrameUpdate.RemoveAt(i);
                    i--;
                }
            }
        }

        private void PerSecondUpdate(float deltaTime)
        {
            _perSecondUpdateTimeSum += deltaTime;
            if (!(_perSecondUpdateTimeSum >= 1f)) return;

            OnPerSecondUpdate?.Invoke(_perSecondUpdateTimeSum);
            _perSecondUpdateTimeSum = 0f;
        }

        private void SlowUpdate(float deltaTime)
        {
            _slowUpdateTimeSum += deltaTime;
            _slowUpdateFrameCount++;

            // Check if enough frames passed for the next slow update
            if (_slowUpdateFrameCount < SlowUpdateRate) return;

            OnSlowUpdate?.Invoke(_slowUpdateTimeSum);
            _slowUpdateTimeSum = 0;
            _slowUpdateFrameCount = 0;
        }

        public void WaitForUpdateFrames(int frames, Action callback)
        {
            var triggerFrame = Time.frameCount + frames;
            var handler = new WaitForFramesHandler(triggerFrame, callback);
            _waitForNextFrameUpdate.Add(handler);
        }

        public void WaitForLateUpdateFrames(int frames, Action callback)
        {
            var triggerFrame = Time.frameCount + frames;
            var handler = new WaitForFramesHandler(triggerFrame, callback);
            _waitForNextFrameLateUpdate.Add(handler);
        }

        public void RegisterSlowUpdate(Action<float> controlledUpdate)
        {
            if (_disposed) return;

            OnSlowUpdate += controlledUpdate;
        }

        public void UnregisterSlowUpdate(Action<float> controlledUpdate)
        {
            if (_disposed) return;

            OnSlowUpdate -= controlledUpdate;
        }

        internal void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            OnLateUpdate?.Invoke(deltaTime);
            WaitForFrameLateUpdate(deltaTime);
        }

        private void WaitForFrameLateUpdate(float deltaTime)
        {
            if (_waitForNextFrameLateUpdate.Count == 0) return;

            for (var i = 0; i < _waitForNextFrameLateUpdate.Count; i++)
            {
                var handler = _waitForNextFrameLateUpdate[i];
                if (handler.TriggerFrame <= Time.frameCount)
                {
                    handler.Action?.Invoke();
                    _waitForNextFrameLateUpdate.RemoveAt(i);
                    i--;
                }
            }
        }

        internal void OnApplicationFocus(bool hasFocus)
        {
            OnApplicationFocusEvent?.Invoke(hasFocus);
        }

        internal void OnApplicationPause(bool pauseStatus)
        {
            OnApplicationPauseEvent?.Invoke(pauseStatus);
        }

        public async void OnApplicationQuit()
        {
            if (_applicationQuitting) return;

            _applicationQuitting = true;
            OnApplicationQuitEvent?.Invoke();
            DestroyMonoInjector();
        }

        private void DestroyMonoInjector()
        {
            if (_monoServiceInjector != null)
            {
#if UNITY_EDITOR
                Object.DestroyImmediate(_monoServiceInjector.gameObject);
#else
                Object.Destroy(_monoServiceInjector.gameObject);
#endif
            }
        }


        public override void Dispose()
        {
            base.Dispose();
            _disposed = true;
            OnUpdate = null;
            OnSlowUpdate = null;
            OnLateUpdate = null;
            OnPerSecondUpdate = null;
            OnApplicationFocusEvent = null;
            OnApplicationPauseEvent = null;
            OnApplicationQuitEvent = null;
            OnUnscaledFixedUpdate = null;

            _coroutineRunner.StopAllCoroutines();
            _coroutineRunner = null;

            DestroyMonoInjector();
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            if (!_coroutineRunner) _coroutineRunner = GameObject.Find("Client").GetComponent<MonoBehaviour>();

            return _coroutineRunner.StartCoroutine(routine);
        }


        public void StopCoroutine(Coroutine routine)
        {
            if (!_coroutineRunner) _coroutineRunner = GameObject.Find("Client").GetComponent<MonoBehaviour>();

            _coroutineRunner.StopCoroutine(routine);
        }


        private class WaitForFramesHandler
        {
            public readonly Action Action;
            public readonly int TriggerFrame;

            public WaitForFramesHandler(int triggerFrame, Action action)
            {
                TriggerFrame = triggerFrame;
                Action = action;
            }
        }
    }
}