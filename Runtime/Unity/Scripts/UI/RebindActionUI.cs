using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    public class RebindActionUI : MonoBehaviour
    {
        [Serializable]
        public class UpdateBindingUIEvent : UnityEvent<RebindActionUI, string, string, string>
        {
        }

        [Serializable]
        public class InteractiveRebindEvent : UnityEvent<RebindActionUI, InputActionRebindingExtensions.RebindingOperation>
        {
        }

        [SerializeField] private InputActionReference _action;
        [SerializeField] private string _bindingId;
        [SerializeField] private InputBinding.DisplayStringOptions _displayStringOptions;
        [SerializeField] private TMP_Text _actionLabel;
        [SerializeField] private TMP_Text _bindingText;
        [SerializeField] private GameObject _rebindOverlay;
        [SerializeField] private TMP_Text _rebindText;
        [SerializeField] private UpdateBindingUIEvent _updateBindingUIEvent;
        [SerializeField] private InteractiveRebindEvent _rebindStartEvent;
        [SerializeField] private InteractiveRebindEvent _rebindStopEvent;

        public InputActionReference ActionReference
        {
            get => _action;
            set
            {
                _action = value;
                UpdateActionLabel();
                UpdateBindingDisplay();
            }
        }

        public string BindingId
        {
            get => _bindingId;
            set
            {
                _bindingId = value;
                UpdateBindingDisplay();
            }
        }

        public InputBinding.DisplayStringOptions DisplayStringOptions
        {
            get => _displayStringOptions;
            set
            {
                _displayStringOptions = value;
                UpdateBindingDisplay();
            }
        }

        public TMP_Text ActionLabel
        {
            get => _actionLabel;
            set
            {
                _actionLabel = value;
                UpdateActionLabel();
            }
        }

        public TMP_Text BindingText
        {
            get => _bindingText;
            set
            {
                _bindingText = value;
                UpdateBindingDisplay();
            }
        }

        public TMP_Text RebindPrompt
        {
            get => _rebindText;
            set => _rebindText = value;
        }

        public GameObject RebindOverlay
        {
            get => _rebindOverlay;
            set => _rebindOverlay = value;
        }

        public UpdateBindingUIEvent OnUpdateBindingUI
        {
            get
            {
                if (_updateBindingUIEvent == null)
                    _updateBindingUIEvent = new UpdateBindingUIEvent();
                return _updateBindingUIEvent;
            }
        }

        public InteractiveRebindEvent StartRebindEvent
        {
            get
            {
                if (_rebindStartEvent == null)
                    _rebindStartEvent = new InteractiveRebindEvent();
                return _rebindStartEvent;
            }
        }

        public InteractiveRebindEvent StopRebindEvent
        {
            get
            {
                if (_rebindStopEvent == null)
                    _rebindStopEvent = new InteractiveRebindEvent();
                return _rebindStopEvent;
            }
        }

        public InputActionRebindingExtensions.RebindingOperation OngoingRebind { get; private set; }

        private static List<RebindActionUI> _rebindActionUIs;

        public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
        {
            bindingIndex = -1;

            action = _action != null ? _action.action : null;
            if (action == null)
                return false;

            if (string.IsNullOrEmpty(_bindingId))
                return false;

            Guid bindingId = new Guid(_bindingId);
            bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
            if (bindingIndex == -1)
            {
                Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
                return false;
            }

            return true;
        }

        public void UpdateBindingDisplay()
        {
            string displayString = string.Empty;
            string deviceLayoutName = default;
            string controlPath = default;

            InputAction action = _action != null ? _action.action : null;
            if (action != null)
            {
                int bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == _bindingId);
                if (bindingIndex != -1)
                    displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, DisplayStringOptions);
            }

            if (_bindingText != null)
                _bindingText.text = displayString;

            _updateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
        }

        public void ResetToDefault()
        {
            if (!ResolveActionAndBinding(out InputAction action, out int bindingIndex))
                return;

            if (action.bindings[bindingIndex].isComposite)
            {
                for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                    action.RemoveBindingOverride(i);
            }
            else
            {
                action.RemoveBindingOverride(bindingIndex);
            }
            UpdateBindingDisplay();
        }

        public void StartInteractiveRebind()
        {
            if (!ResolveActionAndBinding(out InputAction action, out int bindingIndex))
                return;

            if (action.bindings[bindingIndex].isComposite)
            {
                int firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                    PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
            }
            else
            {
                PerformInteractiveRebind(action, bindingIndex);
            }
        }

        private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            OngoingRebind?.Cancel();

            void CleanUp()
            {
                OngoingRebind?.Dispose();
                OngoingRebind = null;
            }

            OngoingRebind = action.PerformInteractiveRebinding(bindingIndex)
                .WithTimeout(5f)
                .OnCancel(
                    operation =>
                    {
                        _rebindStopEvent?.Invoke(this, operation);
                        if (_rebindOverlay != null)
                            _rebindOverlay.SetActive(false);
                        UpdateBindingDisplay();
                        CleanUp();
                    })
                .OnComplete(
                    operation =>
                    {
                        if (_rebindOverlay != null)
                            _rebindOverlay.SetActive(false);
                        _rebindStopEvent?.Invoke(this, operation);
                        UpdateBindingDisplay();
                        CleanUp();

                        if (allCompositeParts)
                        {
                            int nextBindingIndex = bindingIndex + 1;
                            if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                                PerformInteractiveRebind(action, nextBindingIndex, true);
                        }
                    });

            string partName = default;
            if (action.bindings[bindingIndex].isPartOfComposite)
                partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

            if (_rebindOverlay != null)
                _rebindOverlay.SetActive(true);
            if (_rebindText != null)
            {
                string text = !string.IsNullOrEmpty(OngoingRebind.expectedControlType)
                    ? $"{partName}Waiting for {OngoingRebind.expectedControlType} input..."
                    : $"{partName}Waiting for input...";
                _rebindText.text = text;
            }

            if (_rebindOverlay == null && _rebindText == null && _rebindStartEvent == null && _bindingText != null)
                _bindingText.text = "<Waiting...>";

            _rebindStartEvent?.Invoke(this, OngoingRebind);

            _ = OngoingRebind.Start();
        }

        protected void OnEnable()
        {
            if (_rebindActionUIs == null)
                _rebindActionUIs = new List<RebindActionUI>();
            _rebindActionUIs.Add(this);
            if (_rebindActionUIs.Count == 1)
                InputSystem.onActionChange += OnActionChange;

            UpdateBindingDisplay();
        }

        protected void OnDisable()
        {
            OngoingRebind?.Dispose();
            OngoingRebind = null;

            _ = _rebindActionUIs.Remove(this);
            if (_rebindActionUIs.Count == 0)
            {
                _rebindActionUIs = null;
                InputSystem.onActionChange -= OnActionChange;
            }
        }

        private static void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged)
                return;

            InputAction action = obj as InputAction;
            InputActionMap actionMap = action != null ? action.actionMap ?? obj as InputActionMap : null;
            InputActionAsset actionAsset = actionMap?.asset != null ? obj as InputActionAsset : null;

            for (int i = 0; i < _rebindActionUIs.Count; ++i)
            {
                RebindActionUI component = _rebindActionUIs[i];
                InputAction referencedAction = component.ActionReference != null ? component.ActionReference.action : null;
                if (referencedAction == null)
                    continue;

                if (referencedAction == action ||
                    referencedAction.actionMap == actionMap ||
                    referencedAction.actionMap?.asset == actionAsset)
                    component.UpdateBindingDisplay();
            }
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            UpdateActionLabel();
            UpdateBindingDisplay();
        }
#endif

        private void UpdateActionLabel()
        {
            if (_actionLabel != null)
            {
                InputAction action = _action != null ? _action.action : null;
                _actionLabel.text  = action != null ? action.name : string.Empty;
            }
        }
    }
}
