using System;
using System.Collections.Generic;
using _PROJECT.Scripts.Core.InstanceGenerator;
using Core.Feature;
using UnityEngine;
using Zenject;

namespace Features.Ui
{
    public enum UI_Priority
    {
        VeryLow,
        Low,
        Middle,    
        High,
        VeryHigh,
        Overlay
    }

    public class RuntimeUIFeature : BaseFeature
    {
        [Inject] private DiInstanceGenerator _diInstanceGenerator;

        private const string CommonUIFolder = "UI/Common";
        private const string PopUpUIFolder = "UI/PopUp";
        private GameObject _uiCanvasObject;

        private Dictionary<UI_Priority, Transform> _uiPriorityContainers;

        public void Initialize()
        {
            _uiCanvasObject = _diInstanceGenerator.CreatePrefabInstance("UI/--- Runtime UI ---");
            InitializeUIContainers();
            InitializeCommonUI();
        }
        private void InitializeUIContainers()
        {
            _uiPriorityContainers = new Dictionary<UI_Priority, Transform>();
            foreach (UI_Priority priority in Enum.GetValues(typeof(UI_Priority)))
            {
                GameObject container = _diInstanceGenerator.CreatePrefabInstance($"{CommonUIFolder}/CommonUIContainer");
                container.name = priority.ToString() + "Container";
                container.transform.SetParent(_uiCanvasObject.transform, false);
                _uiPriorityContainers.Add(priority, container.transform);
            }
        }

        private void InitializeCommonUI()
        {
        }

        public GameObject GenerateUIElement(string resourcePaths, UI_Priority priority = UI_Priority.Middle)
        {
            var newObject = _diInstanceGenerator.CreatePrefabInstance(resourcePaths);
            if (_uiPriorityContainers != null && _uiPriorityContainers.ContainsKey(priority))
            {
                newObject.transform.SetParent(_uiPriorityContainers[priority], false);
            }
            else
            {
                newObject.transform.SetParent(_uiCanvasObject.transform, false);
            }
            return newObject;
        }
        
    }
}