#region

using Core.Utils.MonoUtils;
using UnityEngine;
using Zenject;

#endregion

namespace _PROJECT.Scripts.Core.Utils.MonoUtils
{
    public class MonoServiceInjector : MonoBehaviour
    {
        [Inject] private MonoFeature _monoService;

        private void Update()
        {
#if UNITY_EDITOR
            if (_monoService == null) return;
#endif
            _monoService.Update();
        }

        private void FixedUpdate()
        {
#if UNITY_EDITOR
            if (_monoService == null) return;
#endif
            _monoService.FixedUpdate();
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (_monoService == null) return;
#endif
            _monoService.LateUpdate();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
#if UNITY_EDITOR
            if (_monoService == null) return;
#endif
            _monoService.OnApplicationFocus(hasFocus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
#if UNITY_EDITOR
            if (_monoService == null) return;
#endif
            _monoService.OnApplicationPause(pauseStatus);
        }

        private async void OnApplicationQuit()
        {
#if UNITY_EDITOR
            if (_monoService == null) return;
#endif
            _monoService.OnApplicationQuit();
        }
    }
}