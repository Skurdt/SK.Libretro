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

        public LogLevel LogLevel                    { get; set; } = LogLevel.Warning;
        public string MainDirectory                 { get; set; } = "";
        public string TempDirectory                 { get; set; } = "";
        public Language Language                    { get; set; } = Language.English;
        public string UserName                      { get; set; } = "Default User";
        public bool UseCoreRotation                 { get; set; }
        public bool CropOverscan                    { get; set; } = true;
        public ILogProcessor LogProcessor           { get; set; }
        public IGraphicsProcessor GraphicsProcessor { get; set; }
        public IAudioProcessor AudioProcessor       { get; set; }
        public IInputProcessor InputProcessor       { get; set; }
        public ILedProcessor LedProcessor           { get; set; }
        public IMessageProcessor MessageProcessor   { get; set; }

        public WrapperSettings(Platform platform) => Platform = platform;
    }
}
