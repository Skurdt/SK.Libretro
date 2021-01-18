using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    [DisallowMultipleComponent]
    public sealed class RebindingUI : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private GameObject _bindingsScreen;
        [SerializeField] private GameObject _bindingOptionsButton;

        private static readonly string _saveDirectory = $"{Application.streamingAssetsPath}/libretro~/bindings";
        private static readonly string _savePath      = $"{_saveDirectory}/default.json";

        private void Start()
        {
            _bindingsScreen.SetActive(false);
            _bindingOptionsButton.SetActive(true);
        }

        public void ShowInputUI()
        {
            _inputActions.Disable();
            LoadFromDisk();

            _bindingsScreen.SetActive(true);
            _bindingOptionsButton.SetActive(false);
        }

        public void HideInputUI()
        {
            SaveToDisk();
            _bindingOptionsButton.SetActive(true);
            _bindingsScreen.SetActive(false);
            _inputActions.Enable();
        }

        private void SaveToDisk()
        {
            if (!Directory.Exists(_saveDirectory))
                _ = Directory.CreateDirectory(_saveDirectory);

            string json = _inputActions.actionMaps[0].SaveBindingOverridesAsJson();
            if (!string.IsNullOrEmpty(json))
                File.WriteAllText(_savePath, json);
        }

        private void LoadFromDisk()
        {
            if (!File.Exists(_savePath))
                return;

            string json = File.ReadAllText(_savePath);
            if (!string.IsNullOrEmpty(json))
                _inputActions.actionMaps[0].LoadBindingOverridesFromJson(json);
        }
    }
}
