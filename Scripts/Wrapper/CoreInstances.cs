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
using System.Linq;

namespace SK.Libretro
{
    public sealed class CoreInstances : IEnumerable<Core>
    {
        public static CoreInstances Instance
        {
            get
            {
                _instance ??= new CoreInstances();
                return _instance;
            }
        }

        public int CoreCount => _cores.Count;

        private static CoreInstances _instance;

        private readonly List<Core> _cores = new();
        private readonly object _lock = new();

        public bool Contains(string coreName) => _cores.Any(x => x.Name == coreName);

        public void UpdateCoreOptionValue(string coreName, string key, int index, bool global)
        {
            lock (_lock)
            {
                if (string.IsNullOrWhiteSpace(key))
                    return;

                Core core = _cores.First(x => x.Name == coreName);
                if (core != null)
                {
                    if (global)
                        core.CoreOptions.UpdateValue(key, index);
                    else
                        core.GameOptions.UpdateValue(key, index);

                    core.SerializeOptions(global);
                }
            }
        }

        public (CoreOptions, CoreOptions) this[string coreName]
        {
            get
            {
                lock (_lock)
                {
                    Core core = _cores.First(x => x.Name == coreName);
                    return (core?.CoreOptions, core?.GameOptions);
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

        internal void Add(Core core)
        {
            lock (_lock)
                if (!_cores.Contains(core))
                    _cores.Add(core);
        }

        IEnumerator<Core> IEnumerable<Core>.GetEnumerator() => _cores.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _cores.GetEnumerator();
    }
}
