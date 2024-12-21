/* MIT License

 * Copyright (c) 2021-2022 Skurdt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

using SK.Libretro.Header;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class OptionsHandler
    {
        public Options CoreOptions { get; private set; }
        public Options GameOptions { get; private set; }

        private const uint CORE_OPTIONS_VERSION = 1;

        private readonly object _lock = new();
        private bool _updateVariables = false;

        public bool GetVariable(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_variable outVariable = data.ToStructure<retro_variable>();
            if (outVariable.key.IsNull())
                return false;

            string key = outVariable.key.AsString();
            if (GameOptions is null || !GameOptions.TryGetValue(key, out Option coreOption))
            {
                if (CoreOptions is null)
                {
                    Wrapper.Instance.LogHandler.LogWarning($"Core didn't set its options. Requested key: {key}", nameof(RETRO_ENVIRONMENT.GET_VARIABLE));
                    return false;
                }

                if (!CoreOptions.TryGetValue(key, out coreOption))
                {
                    Wrapper.Instance.LogHandler.LogWarning($"Core option '{key}' not found.", nameof(RETRO_ENVIRONMENT.GET_VARIABLE));
                    return false;
                }
            }

            outVariable.value = Wrapper.Instance.GetUnsafeString(coreOption.CurrentValue);
            Marshal.StructureToPtr(outVariable, data, false);
            return true;
        }

        public bool GetVariableUpdate(IntPtr data)
        {
            if (data.IsNull())
                return false;

            data.Write(_updateVariables);

            if (!_updateVariables)
                return false;

            _updateVariables = false;
            return true;
        }

        public bool GetCoreOptionsVersion(IntPtr data)
        {
            if (data.IsNull())
                return false;

            data.Write(CORE_OPTIONS_VERSION);
            return true;
        }

        public bool SetVariables(IntPtr data)
        {
            if (data.IsNull())
                return false;

            try
            {
                Deserialize();

                retro_variable variable = data.ToStructure<retro_variable>();
                while (variable is not null && variable.key.IsNotNull() && variable.value.IsNotNull())
                {
                    string key     = variable.key.AsString();
                    string value   = variable.value.AsString();
                    string[] split = value.Split(';');
                    if (CoreOptions[key] is null)
                        CoreOptions[key] = split.Length > 3 ? new Option(split) : new Option(key, split);
                    else
                    {
                        if (split.Length > 3)
                            CoreOptions[key].Update(split);
                        else
                            CoreOptions[key].Update(key, split);
                    }

                    data += Marshal.SizeOf(variable);
                    data.ToStructure(variable);
                }

                Serialize();
                return true;
            }
            catch (Exception e)
            {
                Wrapper.Instance.LogHandler.LogException(e);
                return false;
            }
        }

        public bool SetCoreOptions(IntPtr data) => SetCoreOptionsInternal(data);

        public bool SetCoreOptionsIntl(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_core_options_intl intl = data.ToStructure<retro_core_options_intl>();
            bool result = SetCoreOptionsInternal(intl.local);
            if (!result)
                result = SetCoreOptionsInternal(intl.us);
            return result;
        }

        public bool SetCoreOptionsDisplay(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_core_option_display coreOptionDisplay = data.ToStructure<retro_core_option_display>();
            if (coreOptionDisplay.key.IsNull())
                return false;

            string key = coreOptionDisplay.key.AsString();
            CoreOptions[key]?.SetVisibility(coreOptionDisplay.visible);
            return true;
        }

        public void Serialize(bool global = true, bool updateVariables = true)
        {
            lock (_lock)
            {
                if (global)
                {
                    string filePath = $"{Wrapper.OptionsDirectory}/{Wrapper.Instance.Core.Name}.json";
                    Serialize(CoreOptions, filePath);
                }
                else
                {
                    string directoryPath = FileSystem.GetOrCreateDirectory($"{Wrapper.OptionsDirectory}/{Wrapper.Instance.Core.Name}");
                    string filePath      = $"{directoryPath}/{Wrapper.Instance.Game.Name}.json";
                    Serialize(GameOptions, filePath);
                }

                _updateVariables = updateVariables;
            }
        }

        public void Deserialize()
        {
            lock (_lock)
            {
                CoreOptions = Deserialize($"{Wrapper.OptionsDirectory}/{Wrapper.Instance.Core.Name}.json") ?? new Options();
                GameOptions = Deserialize($"{Wrapper.OptionsDirectory}/{Wrapper.Instance.Core.Name}/{Wrapper.Instance.Game.Name}.json") ?? ClassUtilities.DeepCopy(CoreOptions);
            }
        }

        private static void Serialize(Options coreOptions, string path)
        {
            if (coreOptions is null || coreOptions.Count <= 0)
                return;

            SerializableCoreOptions options = new(coreOptions);
            FileSystem.SerializeToJson(options, path);
        }

        private static Options Deserialize(string path)
        {
            if (!FileSystem.FileExists(path))
                return null;

            SerializableCoreOptions options = FileSystem.DeserializeFromJson<SerializableCoreOptions>(path);
            return options is null || options.Options is null || options.Options.Length <= 0
                 ? null
                 : new Options(options);
        }

        private bool SetCoreOptionsInternal(IntPtr data)
        {
            try
            {
                Deserialize();

                if (data.IsNull())
                    return false;

                Type type = typeof(retro_core_option_values);
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                FieldInfo[] fields = type.GetFields(bindingFlags);

                retro_core_option_definition optionDefinition = data.ToStructure<retro_core_option_definition>();
                while (optionDefinition is not null && optionDefinition.key.IsNotNull())
                {
                    string key = optionDefinition.key.AsString();
                    string description = optionDefinition.desc.AsString();
                    string info = optionDefinition.info.AsString();
                    string defaultValue = optionDefinition.default_value.AsString();

                    List<string> possibleValues = new();
                    for (int i = 0; i < fields.Length; ++i)
                    {
                        FieldInfo fieldInfo = fields[i];
                        if (fieldInfo.GetValue(optionDefinition.values) is not retro_core_option_value optionValue || optionValue.value.IsNull())
                            continue;

                        possibleValues.Add(optionValue.value.AsString());
                    }

                    string value = "";
                    if (!string.IsNullOrWhiteSpace(defaultValue))
                        value = defaultValue;
                    else if (possibleValues.Count > 0)
                        value = possibleValues[0];

                    if (CoreOptions[key] is null)
                        CoreOptions[key] = new Option(key, description, info, value, possibleValues.ToArray());
                    else
                        CoreOptions[key].Update(key, description, info, possibleValues.ToArray());

                    data += Marshal.SizeOf(optionDefinition);
                    data.ToStructure(optionDefinition);
                }

                Serialize();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
