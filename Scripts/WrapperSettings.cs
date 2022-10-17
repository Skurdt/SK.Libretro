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

namespace SK.Libretro
{
    internal sealed class WrapperSettings
    {
        public readonly RuntimePlatform RuntimePlatform;

        public retro_log_level LogLevel { get; init; } = retro_log_level.RETRO_LOG_WARN;
        public string RootDirectory     { get; init; } = "";
        public string UserName          { get; init; } = "Default User";
        public retro_language Language  { get; init; } = retro_language.ENGLISH;
        public bool UseCoreRotation     { get; init; } = false;
        public bool CropOverscan        { get; init; } = true;

        public WrapperSettings(RuntimePlatform runtimePlatform) =>
            RuntimePlatform = runtimePlatform;
    }
}
