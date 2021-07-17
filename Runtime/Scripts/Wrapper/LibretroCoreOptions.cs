/* MIT License

 * Copyright (c) 2020 Skurdt
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

using SK.Libretro.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SK.Libretro
{
    [Serializable]
    internal sealed class LibretroCoreOptions
    {
        [Serializable]
        public sealed class LibretroCoreOptionsList
        {
            public List<LibretroCoreOptions> Cores = new List<LibretroCoreOptions>();
        }

        public static LibretroCoreOptionsList CoreOptionsList { get; private set; }

        public string CoreName      = "";
        public List<string> Options = new List<string>();

        public static void LoadCoreOptionsFile()
        {
            CoreOptionsList = FileSystem.DeserializeFromJson<LibretroCoreOptionsList>(LibretroWrapper.CoreOptionsFile);
            CoreOptionsList ??= new LibretroCoreOptionsList();
        }

        public static void SaveCoreOptionsFile()
        {
            if (CoreOptionsList is null || CoreOptionsList.Cores.Count == 0)
                return;

            CoreOptionsList.Cores = CoreOptionsList.Cores.OrderBy(x => x.CoreName).ToList();
            for (int i = 0; i < CoreOptionsList.Cores.Count; ++i)
                CoreOptionsList.Cores[i].Options.Sort();
            _ = FileSystem.SerializeToJson(CoreOptionsList, LibretroWrapper.CoreOptionsFile);
        }
    }
}
