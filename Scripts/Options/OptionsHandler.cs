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

        private readonly Wrapper _wrapper;
        private readonly object _lock = new();

        private bool _updateVariables = false;

        public OptionsHandler(Wrapper wrapper) => _wrapper = wrapper;

        public bool GetVariable(IntPtr data)
        {
            if (data.IsNull())
                return false;

            var outVariable = data.ToStructure<retro_variable>();
            if (outVariable.key.IsNull())
                return false;

            var key = outVariable.key.AsString();
            if (GameOptions is null || !GameOptions.TryGetValue(key, out var coreOption))
            {
                if (CoreOptions is null)
                {
                    _wrapper.LogHandler.LogWarning($"Core didn't set its options. Requested key: {key}", $"SK.Libretro.OptionsHandler.GetVariable");
                    return false;
                }

                if (!CoreOptions.TryGetValue(key, out coreOption))
                {
                    _wrapper.LogHandler.LogWarning($"Core option '{key}' not found.", $"SK.Libretro.OptionsHandler.GetVariable");
                    return false;
                }
            }

            outVariable.value = _wrapper.GetUnsafeString(coreOption.CurrentValue);
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

                var variable = data.ToStructure<retro_variable>();
                while (variable is not null && variable.key.IsNotNull() && variable.value.IsNotNull())
                {
                    var key     = variable.key.AsString();
                    var value   = variable.value.AsString();
                    var split = value.Split(';');
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
                _wrapper.LogHandler.LogException(e);
                return false;
            }
        }

        public bool SetCoreOptions(IntPtr data) => SetCoreOptionsInternal(data);

        public bool SetCoreOptionsIntl(IntPtr data)
        {
            if (data.IsNull())
                return false;

            var intl = data.ToStructure<retro_core_options_intl>();
            var result = SetCoreOptionsInternal(intl.local);
            if (!result)
                result = SetCoreOptionsInternal(intl.us);
            return result;
        }

        public bool SetCoreOptionsDisplay(IntPtr data)
        {
            if (data.IsNull())
                return false;

            var coreOptionDisplay = data.ToStructure<retro_core_option_display>();
            if (coreOptionDisplay.key.IsNull())
                return false;

            var key = coreOptionDisplay.key.AsString();
            CoreOptions[key]?.SetVisibility(coreOptionDisplay.visible);
            return true;
        }

        public void Serialize(bool global = true, bool updateVariables = true)
        {
            lock (_lock)
            {
                if (global)
                {
                    var filePath = $"{Wrapper.OptionsDirectory}/{_wrapper.Core.Name}.json";
                    Serialize(CoreOptions, filePath);
                }
                else
                {
                    var directoryPath = FileSystem.GetOrCreateDirectory($"{Wrapper.OptionsDirectory}/{_wrapper.Core.Name}");
                    var filePath      = $"{directoryPath}/{_wrapper.Game.Name}.json";
                    Serialize(GameOptions, filePath);
                }

                _updateVariables = updateVariables;
            }
        }

        public void Deserialize()
        {
            lock (_lock)
            {
                CoreOptions = Deserialize($"{Wrapper.OptionsDirectory}/{_wrapper.Core.Name}.json") ?? new Options();
                GameOptions = Deserialize($"{Wrapper.OptionsDirectory}/{_wrapper.Core.Name}/{_wrapper.Game.Name}.json") ?? ClassUtilities.DeepCopy(CoreOptions);
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

            var options = FileSystem.DeserializeFromJson<SerializableCoreOptions>(path);
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

                var type = typeof(retro_core_option_values);
                var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                var fields = type.GetFields(bindingFlags);

                var optionDefinition = data.ToStructure<retro_core_option_definition>();
                while (optionDefinition is not null && optionDefinition.key.IsNotNull())
                {
                    var key = optionDefinition.key.AsString();
                    var description = optionDefinition.desc.AsString();
                    var info = optionDefinition.info.AsString();
                    var defaultValue = optionDefinition.default_value.AsString();

                    List<string> possibleValues = new();
                    for (var i = 0; i < fields.Length; ++i)
                    {
                        var fieldInfo = fields[i];
                        if (fieldInfo.GetValue(optionDefinition.values) is not retro_core_option_value optionValue || optionValue.value.IsNull())
                            continue;

                        possibleValues.Add(optionValue.value.AsString());
                    }

                    var value = "";
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
