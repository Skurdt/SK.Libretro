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

namespace SK.Libretro
{
    public sealed class WrapperSettings
    {
        public readonly Platform Platform;

        public LogLevel LogLevel                    { get; init; } = LogLevel.Warning;
        public string MainDirectory                 { get; init; } = "";
        public string TempDirectory                 { get; init; } = "";
        public Language Language                    { get; init; } = Language.English;
        public string UserName                      { get; init; } = "Default User";
        public bool UseCoreRotation                 { get; init; }
        public bool CropOverscan                    { get; init; } = true;
        public ILogProcessor LogProcessor           { get; init; }
        public IGraphicsProcessor GraphicsProcessor { get; init; }
        public IAudioProcessor AudioProcessor       { get; init; }
        public IInputProcessor InputProcessor       { get; init; }
        public ILedProcessor LedProcessor           { get; init; }
        public IMessageProcessor MessageProcessor   { get; init; }

        public WrapperSettings(Platform platform) => Platform = platform;
    }
}
