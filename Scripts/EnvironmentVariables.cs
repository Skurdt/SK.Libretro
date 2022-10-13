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

using SK.Libretro.Header;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class EnvironmentVariables
    {
        public int Rotation { get; private set; }
        public int PerformanceLevel { get; private set; }
        public bool SupportNoGame { get; private set; }
        public bool SupportsAchievements { get; private set; }
        public bool HwAccelerated { get; set; }

        public readonly List<retro_subsystem_info> SubsystemInfo = new();

        // Values: 0,  1,   2,   3
        // Result: 0, 90, 180, 270 degrees
        public void SetRotation(int rotation) => Rotation = rotation * 90;

        public void SetPerformanceLevel(int performanceLevel) => PerformanceLevel = performanceLevel;

        public void SetSupportNoGame(bool supportNoGame) => SupportNoGame = supportNoGame;

        public void SetSubsystemInfo(ref IntPtr data)
        {
            SubsystemInfo.Clear();

            retro_subsystem_info subsystemInfo = data.ToStructure<retro_subsystem_info>();
            while (!subsystemInfo.desc.IsNull())
            {
                SubsystemInfo.Add(subsystemInfo);

                data += Marshal.SizeOf(subsystemInfo);
                data.ToStructure(subsystemInfo);
            }
        }

        public void SetSupportsAchievements(bool supportsAchievements) => SupportsAchievements = supportsAchievements;
    }
}
