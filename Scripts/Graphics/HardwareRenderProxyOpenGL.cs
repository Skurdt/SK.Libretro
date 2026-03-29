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
using System;
using System.Runtime.InteropServices;
using static SK.Libretro.SDL.Header;
using static SK.Libretro.SDL.GLContext;
using static SK.Libretro.GL.Header;

namespace SK.Libretro
{
    internal sealed class HardwareRenderProxyOpenGL : HardwareRenderProxy
    {
        private SDL.WindowOpenGL _window = null;

        private uint _fboTex = 0;
        private uint _fbo = 0;
        private uint _fboRbo = 0;

        private const int NUM_PBOS = 3;
        private readonly uint[] _pbos = new uint[NUM_PBOS];
        private readonly IntPtr[] _pboFences = new IntPtr[NUM_PBOS];
        private int _pboIndex = 0;
        private uint _pboSize = 0;
        private readonly IntPtr[] _pboMapped = new IntPtr[NUM_PBOS];

        private uint _flipFboTex = 0;
        private uint _flipFbo = 0;

        private SDL.GLContext _readbackContext = null;
        private IntPtr _primaryFence = IntPtr.Zero;
        private uint _readbackSrcFbo = 0;
        private uint _readbackFlipFbo = 0;

        public HardwareRenderProxyOpenGL(Wrapper wrapper, retro_hw_render_callback hwRenderCallback)
        : base(wrapper, hwRenderCallback)
        {
        }

        public override bool Init(int width, int height)
        {
            try
            {
                try
                {
                    var uniqueTitle = $"HardwareRenderProxyOpenGL_{GetHashCode()}_{DateTime.UtcNow.Ticks}";
                    _window = new SDL.WindowOpenGL(uniqueTitle, width, height, 3, 3);
                }
                catch (Exception e)
                {
                    _wrapper.LogHandler.LogException(e);
                    DeInit();
                    return false;
                }

                if (!_window.MakeCurrent())
                {
                    DeInit();
                    return false;
                }

                glGenTextures(1, out _fboTex);
                _wrapper.LogHandler.LogInfo($"Generated color texture: {_fboTex}", "SK.Libretro.HardwareRenderProxyOpenglInit");
                glBindTexture(GL_TEXTURE_2D, _fboTex);
                _wrapper.LogHandler.LogInfo($"TexImage2D: target=0x{GL_TEXTURE_2D:X}, level=0, internalFormat=0x{GL_BGRA:X}, width={width}, height={height}, border=0, format=0x{GL_BGRA:X}, type=0x{GL_UNSIGNED_BYTE:X}", "SK.Libretro.HardwareRenderProxyOpenglInit");
                glTexImage2D(GL_TEXTURE_2D, 0, GL_BGRA, width, height, 0, GL_BGRA, GL_UNSIGNED_BYTE, IntPtr.Zero);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

                glGenFramebuffers(1, out _fbo);
                _wrapper.LogHandler.LogInfo($"Generated framebuffer: {_fbo}", "SK.Libretro.HardwareRenderProxyOpenglInit");
                glBindFramebuffer(GL_FRAMEBUFFER, _fbo);
                glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, _fboTex, 0);
                _wrapper.LogHandler.LogInfo($"Attached texture {_fboTex} to COLOR_ATTACHMENT0", "SK.Libretro.HardwareRenderProxyOpenglInit");
                var drawBuf = GL_COLOR_ATTACHMENT0;
                glDrawBuffers(1, ref drawBuf);

                if (Depth)
                {
                    glGenRenderbuffers(1, out _fboRbo);
                    _wrapper.LogHandler.LogInfo($"Generated renderbuffer: {_fboRbo}", "SK.Libretro.HardwareRenderProxyOpenglInit");
                    glBindRenderbuffer(GL_RENDERBUFFER, _fboRbo);
                    if (Stencil)
                    {
                        _wrapper.LogHandler.LogInfo($"RenderbufferStorage: target=0x{GL_RENDERBUFFER:X}, internalFormat=0x{GL_DEPTH24_STENCIL8:X}, width={width}, height={height}", "SK.Libretro.HardwareRenderProxyOpenglInit");
                        glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, width, height);
                        glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, _fboRbo);
                        glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_STENCIL_ATTACHMENT, GL_RENDERBUFFER, _fboRbo);
                        _wrapper.LogHandler.LogInfo($"Attached renderbuffer {_fboRbo} to DEPTH_ATTACHMENT and STENCIL_ATTACHMENT", "SK.Libretro.HardwareRenderProxyOpenglInit");
                    }
                    else
                    {
                        _wrapper.LogHandler.LogInfo($"RenderbufferStorage: target=0x{GL_RENDERBUFFER:X}, internalFormat=0x{GL_DEPTH_COMPONENT24:X}, width={width}, height={height}", "SK.Libretro.HardwareRenderProxyOpenglInit");
                        glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT24, width, height);
                        glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, _fboRbo);
                        _wrapper.LogHandler.LogInfo($"Attached renderbuffer {_fboRbo} to DEPTH_ATTACHMENT", "SK.Libretro.HardwareRenderProxyOpenglInit");
                    }
                }

                var fbStatus = glCheckFramebufferStatus(GL_FRAMEBUFFER);
                if (fbStatus != GL_FRAMEBUFFER_COMPLETE)
                {
                    var statusMsg = fbStatus switch
                    {
                        var s when s == GL_FRAMEBUFFER_UNDEFINED => "FRAMEBUFFER_UNDEFINED",
                        var s when s == GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT => "FRAMEBUFFER_INCOMPLETE_ATTACHMENT",
                        var s when s == GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT => "FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT",
                        var s when s == GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER => "FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER",
                        var s when s == GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER => "FRAMEBUFFER_INCOMPLETE_READ_BUFFER",
                        var s when s == GL_FRAMEBUFFER_UNSUPPORTED => "FRAMEBUFFER_UNSUPPORTED",
                        var s when s == GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE => "FRAMEBUFFER_INCOMPLETE_MULTISAMPLE",
                        var s when s == GL_FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS => "FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS",
                        _ => $"Unknown status: 0x{fbStatus:X}"
                    };
                    _wrapper.LogHandler.LogError($"Framebuffer incomplete: {statusMsg} (0x{fbStatus:X})", "SK.Libretro.HardwareRenderProxyOpenglInit");
                    DeInit();
                    return false;
                }
    
                glBindTexture(GL_TEXTURE_2D, 0);
                glBindFramebuffer(GL_FRAMEBUFFER, 0);

                if (!InitReadbackResources(width, height))
                {
                    DeInit();
                    return false;
                }

                if (!CreateReadbackContext())
                {
                    DeInit();
                    return false;
                }

                ContextReset?.Invoke();

                return true;
            }
            catch (Exception e)
            {
                _wrapper.LogHandler.LogException(e);
            }

            return false;
        }

        public override void Resize(int width, int height)
        {
        }

        public override bool ReadbackFrame(uint width, uint height, ref byte[] textureData)
        {
            if (_window is null)
                return false;

            if (!_window.MakeCurrent())
                return false;

            if (!InitReadbackResources((int)width, (int)height))
                return false;

            if (_readbackContext is null || _readbackSrcFbo == 0 || _readbackFlipFbo == 0)
                return false;

            var size = textureData.Length;
            if (size == 0)
                return false;

            if (_primaryFence.IsNotNull())
            {
                glDeleteSync(_primaryFence);
                PointerUtilities.SetToNull(ref _primaryFence);
            }
            _primaryFence = glFenceSync(GL_SYNC_GPU_COMMANDS_COMPLETE, 0);
            glFlush();

            if (!_readbackContext.MakeCurrent())
                return false;

            glWaitSync(_primaryFence, 0, GL_TIMEOUT_IGNORED);

            var writePBO = _pbos[_pboIndex];
            glBindBuffer(GL_PIXEL_PACK_BUFFER, writePBO);

            glBindFramebuffer(GL_READ_FRAMEBUFFER, _readbackSrcFbo);
            glBindFramebuffer(GL_DRAW_FRAMEBUFFER, _readbackFlipFbo);
            glBlitFramebuffer(0, 0, (int)width, (int)height, 0, 0, (int)width, (int)height, GL_COLOR_BUFFER_BIT, GL_NEAREST);
            glBindFramebuffer(GL_READ_FRAMEBUFFER, _readbackFlipFbo);
            glReadPixels(0, 0, (int)width, (int)height, GL_BGRA, GL_UNSIGNED_BYTE, IntPtr.Zero);
            glBindFramebuffer(GL_READ_FRAMEBUFFER, 0);
            glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0);

            if (_pboFences[_pboIndex].IsNotNull())
            {
                glDeleteSync(_pboFences[_pboIndex]);
                _pboFences[_pboIndex] = IntPtr.Zero;
            }
            _pboFences[_pboIndex] = glFenceSync(GL_SYNC_GPU_COMMANDS_COMPLETE, 0);
            glFlush();

            var readIndex = (_pboIndex + 1) % NUM_PBOS;
            _pboIndex = (_pboIndex + 1) % NUM_PBOS;

            var success = false;
            if (_pboFences[readIndex].IsNotNull())
            {
                var waitResult = glClientWaitSync(_pboFences[readIndex], 0, 0);
                if (waitResult == GL_ALREADY_SIGNALED || waitResult == GL_CONDITION_SATISFIED)
                {
                    if (_pboMapped[readIndex].IsNotNull())
                        Marshal.Copy(_pboMapped[readIndex], textureData, 0, size);

                    glDeleteSync(_pboFences[readIndex]);
                    _pboFences[readIndex] = IntPtr.Zero;
                    success = true;
                }
            }

            glBindBuffer(GL_PIXEL_PACK_BUFFER, 0);

            return _window.MakeCurrent() && success;
        }

        public override void CallContextDestroy()
        {
            
        }

        public override void DeInit()
        {
            try
            {
                if (_window is null)
                    return;

                if (_window.MakeCurrent())
                {
                    DestroyReadbackContext();
                    DisposeReadbackResources();

                    if (_fbo != 0)
                    {
                        glDeleteFramebuffers(1, ref _fbo);
                        _fbo = 0;
                    }
                    if (_fboTex != 0)
                    {
                        glDeleteTextures(1, ref _fboTex);
                        _fboTex = 0;
                    }
                    if (_fboRbo != 0)
                    {
                        glDeleteRenderbuffers(1, ref _fboRbo);
                        _fboRbo = 0;
                    }

                    // ContextDestroy?.Invoke();

                    glFinish();
                }
                else
                {
                    _wrapper.LogHandler.LogWarning("Failed to make context current; will avoid GL cleanup to prevent crash.", "SK.Libretro.HardwareRenderProxyOpenglDeInit");

                    for (var i = 0; i < NUM_PBOS; ++i)
                        _pboFences[i] = IntPtr.Zero;

                    _pboSize = 0;
                    _pboIndex = 0;

                    _fbo = 0;
                    _fboTex = 0;
                    _fboRbo = 0;
                }

                _window.Dispose();
                _window = null;

		        _wrapper.LogHandler.LogInfo("Shutdown complete.", "SK.Libretro.HardwareRenderProxyOpenglShutdown");
            }
            catch (Exception e)
            {
                _wrapper.LogHandler.LogException(e);
            }
        }

        protected override IntPtr GetCurrentFrameBufferCall() => _fbo != 0 ? (IntPtr)_fbo : (IntPtr)0;

        protected override IntPtr GetProcAddressCall(IntPtr functionName) => SDL_GL_GetProcAddress(functionName.AsString());

        private bool CreateReadbackContext()
        {
            if (_window is null)
            {
                _wrapper.LogHandler.LogError("Primary context must exist first.", "SK.Libretro.HardwareRenderProxyOpenglCreateReadbackContext");
                return false;
            }

            if (!_window.MakeCurrent())
            {
                _wrapper.LogHandler.LogError("Failed to make primary context current." + SDL_GetError(), "SK.Libretro.HardwareRenderProxyOpenglCreateReadbackContext");
                return false;
            }

            try
            {
                _readbackContext = new SDL.GLContext(_window, 3, 3, true);
            }
            catch (Exception e)
            {
                _wrapper.LogHandler.LogException(e, "SK.Libretro.HardwareRenderProxyOpenglCreateReadbackContext");
                return false;
            }

            if (!RecreateReadbackFBOs())
            {
                DestroyReadbackContext();
                return false;
            }

            if (!_window.MakeCurrent())
            {
                _wrapper.LogHandler.LogError("Failed to restore primary context." + SDL_GetError(), "SK.Libretro.HardwareRenderProxyOpenglCreateReadbackContext");
                DestroyReadbackContext();
                return false;
            }

            _wrapper.LogHandler.LogInfo("Shared readback context created.", "SK.Libretro.HardwareRenderProxyOpenglCreateReadbackContext");
            return true;
        }

        private void DestroyReadbackContext()
        {
            if (_readbackContext is null)
                return;

            if (_primaryFence.IsNotNull())
            {
                glDeleteSync(_primaryFence);
                PointerUtilities.SetToNull(ref _primaryFence);
            }

            if (!_readbackContext.MakeCurrent())
            {
                _wrapper.LogHandler.LogError("Failed to make readback context current for cleanup." + SDL_GetError(), "SK.Libretro.HardwareRenderProxyOpenglDestroyReadbackContext");
                return;
            }

            if (_readbackSrcFbo != 0)
            {
                glDeleteFramebuffers(1, ref _readbackSrcFbo);
                _readbackSrcFbo = 0;
            }
            if (_readbackFlipFbo != 0)
            {
                glDeleteFramebuffers(1, ref _readbackFlipFbo);
                _readbackFlipFbo = 0;
            }

            if (!_window.ClearCurrent())
                _wrapper.LogHandler.LogError("Failed to unset current context for cleanup." + SDL_GetError(), "SK.Libretro.HardwareRenderProxyOpenglDestroyReadbackContext");

            try
            {
                _readbackContext.Dispose();
                _readbackContext = null;
            }
            catch (Exception e)
            {
                _wrapper.LogHandler.LogException(e, "SK.Libretro.HardwareRenderProxyOpenglDestroyReadbackContext");
            }

            if (!_window.MakeCurrent())
                _wrapper.LogHandler.LogError("Failed to restore primary context after cleanup." + SDL_GetError(), "SK.Libretro.HardwareRenderProxyOpenglDestroyReadbackContext");

            _wrapper.LogHandler.LogInfo("Shared readback context destroyed.", "SK.Libretro.HardwareRenderProxyOpenglDestroyReadbackContext");
        }

        private bool InitReadbackResources(int width, int height)
        {
            if (width <= 0 || height <= 0)
                return false;

            var newSize = (uint)(width * height * 4);
            if (newSize == _pboSize && _pbos[0] != 0)
                return true;

            DisposeReadbackResources();

            _pboSize = newSize;

            unsafe
            {
                fixed (uint* pbosPtr = _pbos)
                    glGenBuffers(NUM_PBOS, pbosPtr);
            }

            for (var i = 0; i < NUM_PBOS; ++i)
            {
                glBindBuffer(GL_PIXEL_PACK_BUFFER, _pbos[i]);
                glBufferStorage(GL_PIXEL_PACK_BUFFER, _pboSize, IntPtr.Zero, GL_MAP_READ_BIT | GL_MAP_PERSISTENT_BIT | GL_DYNAMIC_STORAGE_BIT);
                _pboMapped[i] = glMapBufferRange(GL_PIXEL_PACK_BUFFER, 0, _pboSize, GL_MAP_READ_BIT | GL_MAP_PERSISTENT_BIT);
                if (_pboMapped[i].IsNull())
                    _wrapper.LogHandler.LogWarning("Persistent PBO mapping failed for index " + i, "SK.Libretro.HardwareRenderProxyOpenglInitReadbackResources");
            }

            glBindBuffer(GL_PIXEL_PACK_BUFFER, 0);

            _pboIndex = 0;
            for (var i = 0; i < NUM_PBOS; ++i)
                PointerUtilities.SetToNull(ref _pboFences[i]);

            if (_flipFboTex != 0)
            {
                glDeleteTextures(1, ref _flipFboTex);
                _flipFboTex = 0;
            }
            if (_flipFbo != 0)
            {
                glDeleteFramebuffers(1, ref _flipFbo);
                _flipFbo = 0;
            }

            glGenTextures(1, out _flipFboTex);
            glBindTexture(GL_TEXTURE_2D, _flipFboTex);
            glTexImage2D(GL_TEXTURE_2D, 0, GL_BGRA, width, height, 0, GL_BGRA, GL_UNSIGNED_BYTE, IntPtr.Zero);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            glBindTexture(GL_TEXTURE_2D, 0);

            glGenFramebuffers(1, out _flipFbo);
            glBindFramebuffer(GL_FRAMEBUFFER, _flipFbo);
            glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, _flipFboTex, 0);
            var drawBuf = GL_COLOR_ATTACHMENT0;
            glDrawBuffers(1, ref drawBuf);

            if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
            {
                _wrapper.LogHandler.LogError("Failed to create flip framebuffer; aborting readback initialization.", "SK.Libretro.HardwareRenderProxyOpenglInitReadbackResources");
                glDeleteFramebuffers(1, ref _flipFbo);
                glDeleteTextures(1, ref _flipFboTex);
                _flipFbo = 0;
                _flipFboTex = 0;
                glBindBuffer(GL_PIXEL_PACK_BUFFER, 0);
                DisposeReadbackResources();
                return false;
            }

            glBindFramebuffer(GL_FRAMEBUFFER, 0);

            if (_readbackContext is null)
                return true;

            if (!RecreateReadbackFBOs())
            {
                glDeleteFramebuffers(1, ref _flipFbo);
                glDeleteTextures(1, ref _flipFboTex);
                _flipFbo = 0;
                _flipFboTex = 0;
                glBindBuffer(GL_PIXEL_PACK_BUFFER, 0);
                DisposeReadbackResources();
                return false;
            }

            if (!_window.MakeCurrent())
            {
                DeInit();
                return false;
            }

            return true;
        }

        private void DisposeReadbackResources()
        {
            for (var i = 0; i < NUM_PBOS; ++i)
            {
                if (_pboFences[i].IsNotNull())
                {
                    glDeleteSync(_pboFences[i]);
                    PointerUtilities.SetToNull(ref _pboFences[i]);
                }
            }

            if (_pbos[0] != 0)
            {
                for (var i = 0; i < NUM_PBOS; ++i)
                {
                    if (_pboMapped[i].IsNotNull())
                    {
                        glBindBuffer(GL_PIXEL_PACK_BUFFER, _pbos[i]);
                        glUnmapBuffer(GL_PIXEL_PACK_BUFFER);
                        PointerUtilities.SetToNull(ref _pboMapped[i]);
                    }
                }
                glBindBuffer(GL_PIXEL_PACK_BUFFER, 0);

                unsafe
                {
                    fixed (uint* pbosPtr = _pbos)
                        glDeleteBuffers(NUM_PBOS, pbosPtr);
                }

                for (var i = 0; i < NUM_PBOS; ++i)
                    _pbos[i] = 0;
            }

            if (_flipFbo != 0)
            {
                glDeleteFramebuffers(1, ref _flipFbo);
                _flipFbo = 0;
            }
            if (_flipFboTex != 0)
            {
                glDeleteTextures(1, ref _flipFboTex);
                _flipFboTex = 0;
            }

            _pboSize = 0;
            _pboIndex = 0;
        }

        private bool RecreateReadbackFBOs()
        {
            if (_readbackContext is null || _fboTex == 0 || _flipFboTex == 0)
                return false;

            if (!_readbackContext.MakeCurrent())
            {
                _wrapper.LogHandler.LogError("Failed to make readback context current." + SDL_GetError(), "SK.Libretro.HardwareRenderProxyOpenglRecreateReadbackFBOs");
                return false;
            }

            if (_readbackSrcFbo != 0)
            {
                glDeleteFramebuffers(1, ref _readbackSrcFbo);
                _readbackSrcFbo = 0;
            }
            if (_readbackFlipFbo != 0)
            {
                glDeleteFramebuffers(1, ref _readbackFlipFbo);
                _readbackFlipFbo = 0;
            }

            glGenFramebuffers(1, out _readbackSrcFbo);
            glBindFramebuffer(GL_FRAMEBUFFER, _readbackSrcFbo);
            glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, _fboTex, 0);
            if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
            {
                _wrapper.LogHandler.LogError("Source FBO is incomplete.", "SK.Libretro.HardwareRenderProxyOpenglRecreateReadbackFBOs");
                glBindFramebuffer(GL_FRAMEBUFFER, 0);
                return false;
            }
            glBindFramebuffer(GL_FRAMEBUFFER, 0);

            glGenFramebuffers(1, out _readbackFlipFbo);
            glBindFramebuffer(GL_FRAMEBUFFER, _readbackFlipFbo);
            glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, _flipFboTex, 0);
            var drawBuf = GL_COLOR_ATTACHMENT0;
            glDrawBuffers(1, ref drawBuf);
            if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
            {
                _wrapper.LogHandler.LogError("Flip FBO is incomplete.", "SK.Libretro.HardwareRenderProxyOpenglRecreateReadbackFBOs");
                glBindFramebuffer(GL_FRAMEBUFFER, 0);
                return false;
            }
            glBindFramebuffer(GL_FRAMEBUFFER, 0);

            return true;
        }
    }
}
