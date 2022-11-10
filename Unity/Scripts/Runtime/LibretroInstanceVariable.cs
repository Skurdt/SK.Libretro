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
using UnityEngine;

namespace SK.Libretro.Unity
{
    [CreateAssetMenu(fileName = "LibretroInstanceVariable", menuName = "SK.Libretro/LibretroInstanceVariable")]
    public sealed class LibretroInstanceVariable : ScriptableObject
    {
        [field: NonSerialized] public event Action<LibretroInstance> OnInstanceChanged;

        public LibretroInstance Current
        {
            get => _current;
            set
            {
                if (_current != value)
                {
                    _current = value;
                    OnInstanceChanged?.Invoke(_current);
                }
            }
        }

        public bool DiskHandlerEnabled => Current && Current.DiskHandlerEnabled;

        [NonSerialized] private LibretroInstance _current;

        public void SetInputEnabled(bool enabled)
        {
            if (Current)
                Current.InputEnabled = enabled;
        }

        public void StartContent()
        {
            if (Current)
                Current.StartContent();
        }

        public void PauseContent()
        {
            if (Current)
                Current.PauseContent();
        }

        public void ResumeContent()
        {
            if (Current)
                Current.ResumeContent();
        }

        public void ResetContent()
        {
            if (Current)
                Current.ResetContent();
        }

        public void StopContent()
        {
            if (Current)
                Current.StopContent();
        }

        public void SaveState(int stateSlot)
        {
            if (!Current)
                return;
            
            Current.SetStateSlot(stateSlot);
            Current.SaveStateWithScreenshot();
        }

        public void LoadState(int stateSlot)
        {
            if (!Current)
                return;

            Current.SetStateSlot(stateSlot);
            Current.LoadState();
        }

        public void SetDiskIndex(int index)
        {
            if (Current)
                Current.SetDiskIndex(index);
        }

        public void SaveSRAM()
        {
            if (Current)
                Current.SaveSRAM();
        }

        public void LoadSRAM()
        {
            if (Current)
                Current.LoadSRAM();
        }
    }
}
