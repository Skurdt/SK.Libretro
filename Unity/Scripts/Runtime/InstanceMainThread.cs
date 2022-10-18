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

using Unity.Mathematics;

namespace SK.Libretro.Unity
{
    internal sealed class InstanceMainThread : Instance
    {
        public override bool Running { get; protected set; }

        public override bool Paused { get; protected set; }

        public override int FastForwardFactor
        {
            get => _fastForwardFactor;
            set => _fastForwardFactor = math.clamp(value, 2, 32);
        }

        public override bool FastForward { get; set; }

        public override bool Rewind { get; set; }

        public override bool InputEnabled
        {
            get => _wrapper.InputHandler.Enabled;
            set => _wrapper.InputHandler.Enabled = value;
        }

        protected override string CoreName { get; set; }
        protected override string GamesDirectory { get; set; }
        protected override string[] GameNames { get; set; }

        private int _fastForwardFactor = 8;

        public InstanceMainThread(LibretroInstance instance)
        : base(instance)
        {
        }

        public override void Dispose()
        {
            StopContent();
            _screen.material = _originalMaterial;
        }

        public override void PauseContent()
        {
            if (!Running || Paused)
                return;

            Paused = true;
        }

        public override void ResumeContent()
        {
            if (!Running || !Paused)
                return;

            Paused = false;
        }

        public override void SetStateSlot(string slot)
        {
            if (int.TryParse(slot, out int slotInt))
                _currentStateSlot = slotInt;
        }

        public override void SetStateSlot(int slot) => _currentStateSlot = slot;

        public override void SaveStateWithScreenshot()
        {
            if (_wrapper.SerializationHandler.SaveState(_currentStateSlot, out string screenshotPath))
                TakeScreenshot(screenshotPath);
        }

        public override void SaveStateWithoutScreenshot() => _wrapper.SerializationHandler.SaveState(_currentStateSlot);

        public override void LoadState() => _wrapper.SerializationHandler.LoadState(_currentStateSlot);

        public override void SaveSRAM() => _wrapper.SerializationHandler.SaveSRAM();

        public override void LoadSRAM() => _wrapper.SerializationHandler.LoadSRAM();
    }
}
