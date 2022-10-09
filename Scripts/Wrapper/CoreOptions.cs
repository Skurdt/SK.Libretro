/* MIT License

 * Copyright (c) 2022 Skurdt
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

using System.Collections;
using System.Collections.Generic;

namespace SK.Libretro
{
    [System.Serializable]
    public sealed class CoreOptions : IEnumerable<CoreOption>
    {
        internal int Count => _options.Count;

        private readonly SortedList<string, CoreOption> _options = new();

        internal CoreOptions()
        {
        }

        internal CoreOptions(SerializableCoreOptions options)
        {
            foreach (string option in options.Options)
            {
                CoreOption coreOption = new(option);
                _options.Add(coreOption.Key, coreOption);
            }
        }

        internal void UpdateValue(string key, int index)
        {
            if (_options.TryGetValue(key, out CoreOption option))
                option.Update(index);
        }

        public CoreOption this[int index] => _options.Count > index ? _options.Values[index] : null;

        internal CoreOption this[string key]
        {
            get => !string.IsNullOrWhiteSpace(key) && _options.ContainsKey(key) ? _options[key] : null;
            set
            {
                if (string.IsNullOrWhiteSpace(key))
                    return;
                if (_options.ContainsKey(key))
                    _options[key] = value;
                else
                    _options.Add(key, value);
            }
        }

        IEnumerator<CoreOption> IEnumerable<CoreOption>.GetEnumerator() => _options.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _options.Values.GetEnumerator();
    }
}
