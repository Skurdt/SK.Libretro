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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SK.Libretro
{
    public sealed class CoreInstances : IEnumerable<Wrapper>
    {
        public static CoreInstances Instance
        {
            get
            {
                if (_instance is null)
                    lock (_lock)
                        _instance ??= new CoreInstances();
                return _instance;
            }
        }

        public int CoreCount => _wrappers.Count;

        private static CoreInstances _instance;
        private static readonly object _lock = new();

        private readonly List<Wrapper> _wrappers = new();

        public bool Contains(string coreName) => _wrappers.Any(x => x.Core.Name.Equals(coreName, StringComparison.OrdinalIgnoreCase));

        public void UpdateCoreOptionValue(string coreName, string key, int index, bool global)
        {
            lock (_lock)
            {
                if (string.IsNullOrWhiteSpace(key))
                    return;

                Wrapper wrapper = _wrappers.First(x => x.Core.Name.Equals(coreName, StringComparison.OrdinalIgnoreCase));
                if (wrapper is not null)
                {
                    if (global)
                        wrapper.Options.CoreOptions.UpdateValue(key, index);
                    else
                        wrapper.Options.GameOptions.UpdateValue(key, index);

                    wrapper.Options.Serialize(global);
                }
            }
        }

        public (Options, Options) this[string coreName]
        {
            get
            {
                lock (_lock)
                {
                    Wrapper wrapper = _wrappers.First(x => x.Core.Name.Equals(coreName, StringComparison.OrdinalIgnoreCase));
                    return (wrapper?.Options.CoreOptions, wrapper?.Options.GameOptions);
                }
            }
        }

        //public (CoreOptions, CoreOptions) this[int index]
        //{
        //    get
        //    {
        //        lock (_lock)
        //        {
        //            if (_cores.Count == 0 || index < 0 || index > _cores.Count)
        //                return (null, null);

        //            Core core = _cores[index];
        //            return (core?.CoreOptions, core?.GameOptions);
        //        }
        //    }
        //}

        internal void Add(Wrapper wrapper)
        {
            lock (_lock)
                if (!_wrappers.Contains(wrapper))
                    _wrappers.Add(wrapper);
        }

        internal void Remove(Wrapper wrapper)
        {
            lock (_lock)
                if (_wrappers.Contains(wrapper))
                    _ = _wrappers.Remove(wrapper);
        }

        IEnumerator<Wrapper> IEnumerable<Wrapper>.GetEnumerator() => _wrappers.GetEnumerator();
        public IEnumerator GetEnumerator() => GetEnumerator();
    }
}
