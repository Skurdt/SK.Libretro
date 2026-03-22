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
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class OpenGLRenderProxySDL : HardwareRenderProxy
    {
        private IntPtr _window = IntPtr.Zero;
        private IntPtr _context = IntPtr.Zero;

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

        private IntPtr _readbackContext = IntPtr.Zero;
        private IntPtr _primaryFence = IntPtr.Zero;
        private uint _readbackSrcFbo = 0;
        private uint _readbackFlipFbo = 0;

        public OpenGLRenderProxySDL(Wrapper wrapper, retro_hw_render_callback hwRenderCallback)
        : base(wrapper, hwRenderCallback)
        {
        }

        [HandleProcessCorruptedStateExceptions]
        public override bool Init(int width, int height)
        {
            try
            {
                if (!CreateNativeWindow(width, height))
                {
                    DeInit();
                    return false;
                }

                if (!CreateNativeContext())
                {
                    DeInit();
                    return false;
                }

                if (!SDL.GL_MakeCurrent(_window, _context))
                {
                    DeInit();
                    return false;
                }

                GL.GenTextures(1, out _fboTex);
                _wrapper.LogHandler.LogInfo($"Generated color texture: {_fboTex}", "SK.Libretro.OpenGLRenderProxySDL.Init");
                GL.BindTexture(GL.TEXTURE_2D, _fboTex);
                _wrapper.LogHandler.LogInfo($"TexImage2D: target=0x{GL.TEXTURE_2D:X}, level=0, internalFormat=0x{GL.BGRA:X}, width={width}, height={height}, border=0, format=0x{GL.BGRA:X}, type=0x{GL.UNSIGNED_BYTE:X}", "SK.Libretro.OpenGLRenderProxySDL.Init");
                GL.TexImage2D(GL.TEXTURE_2D, 0, GL.BGRA, width, height, 0, GL.BGRA, GL.UNSIGNED_BYTE, IntPtr.Zero);
                GL.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
                GL.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);

                GL.GenFramebuffers(1, out _fbo);
                _wrapper.LogHandler.LogInfo($"Generated framebuffer: {_fbo}", "SK.Libretro.OpenGLRenderProxySDL.Init");
                GL.BindFramebuffer(GL.FRAMEBUFFER, _fbo);
                GL.FramebufferTexture2D(GL.FRAMEBUFFER, GL.COLOR_ATTACHMENT0, GL.TEXTURE_2D, _fboTex, 0);
                _wrapper.LogHandler.LogInfo($"Attached texture {_fboTex} to COLOR_ATTACHMENT0", "SK.Libretro.OpenGLRenderProxySDL.Init");
                var drawBuf = GL.COLOR_ATTACHMENT0;
                GL.DrawBuffers(1, ref drawBuf);

                if (Depth)
                {
                    GL.GenRenderbuffers(1, out _fboRbo);
                    _wrapper.LogHandler.LogInfo($"Generated renderbuffer: {_fboRbo}", "SK.Libretro.OpenGLRenderProxySDL.Init");
                    GL.BindRenderbuffer(GL.RENDERBUFFER, _fboRbo);
                    if (Stencil)
                    {
                        _wrapper.LogHandler.LogInfo($"RenderbufferStorage: target=0x{GL.RENDERBUFFER:X}, internalFormat=0x{GL.DEPTH24_STENCIL8:X}, width={width}, height={height}", "SK.Libretro.OpenGLRenderProxySDL.Init");
                        GL.RenderbufferStorage(GL.RENDERBUFFER, GL.DEPTH24_STENCIL8, width, height);
                        GL.FramebufferRenderbuffer(GL.FRAMEBUFFER, GL.DEPTH_ATTACHMENT, GL.RENDERBUFFER, _fboRbo);
                        GL.FramebufferRenderbuffer(GL.FRAMEBUFFER, GL.STENCIL_ATTACHMENT, GL.RENDERBUFFER, _fboRbo);
                        _wrapper.LogHandler.LogInfo($"Attached renderbuffer {_fboRbo} to DEPTH_ATTACHMENT and STENCIL_ATTACHMENT", "SK.Libretro.OpenGLRenderProxySDL.Init");
                    }
                    else
                    {
                        _wrapper.LogHandler.LogInfo($"RenderbufferStorage: target=0x{GL.RENDERBUFFER:X}, internalFormat=0x{GL.DEPTH_COMPONENT24:X}, width={width}, height={height}", "SK.Libretro.OpenGLRenderProxySDL.Init");
                        GL.RenderbufferStorage(GL.RENDERBUFFER, GL.DEPTH_COMPONENT24, width, height);
                        GL.FramebufferRenderbuffer(GL.FRAMEBUFFER, GL.DEPTH_ATTACHMENT, GL.RENDERBUFFER, _fboRbo);
                        _wrapper.LogHandler.LogInfo($"Attached renderbuffer {_fboRbo} to DEPTH_ATTACHMENT", "SK.Libretro.OpenGLRenderProxySDL.Init");
                    }
                }

                var fbStatus = GL.CheckFramebufferStatus(GL.FRAMEBUFFER);
                if (fbStatus != GL.FRAMEBUFFER_COMPLETE)
                {
                    var statusMsg = fbStatus switch
                    {
                        var s when s == GL.FRAMEBUFFER_UNDEFINED => "FRAMEBUFFER_UNDEFINED",
                        var s when s == GL.FRAMEBUFFER_INCOMPLETE_ATTACHMENT => "FRAMEBUFFER_INCOMPLETE_ATTACHMENT",
                        var s when s == GL.FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT => "FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT",
                        var s when s == GL.FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER => "FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER",
                        var s when s == GL.FRAMEBUFFER_INCOMPLETE_READ_BUFFER => "FRAMEBUFFER_INCOMPLETE_READ_BUFFER",
                        var s when s == GL.FRAMEBUFFER_UNSUPPORTED => "FRAMEBUFFER_UNSUPPORTED",
                        var s when s == GL.FRAMEBUFFER_INCOMPLETE_MULTISAMPLE => "FRAMEBUFFER_INCOMPLETE_MULTISAMPLE",
                        var s when s == GL.FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS => "FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS",
                        _ => $"Unknown status: 0x{fbStatus:X}"
                    };
                    _wrapper.LogHandler.LogError($"Framebuffer incomplete: {statusMsg} (0x{fbStatus:X})", "SK.Libretro.OpenGLRenderProxySDL.Init");
                    DeInit();
                    return false;
                }
    
                GL.BindTexture(GL.TEXTURE_2D, 0);
                GL.BindFramebuffer(GL.FRAMEBUFFER, 0);

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
            catch (SEHException ex)
            {
                _wrapper.LogHandler.LogException(ex);
                if (ex.InnerException is not null)
                    _wrapper.LogHandler.LogException(ex.InnerException);
            }
            catch (Exception e)
            {
                _wrapper.LogHandler.LogException(e);
            }

            return false;
        }

        public override bool ReadbackFrame(uint width, uint height, ref byte[] textureData)
        {
            if (_window.IsNull() || _context.IsNull())
                return false;

            if (!SDL.GL_MakeCurrent(_window, _context))
                return false;

            if (!InitReadbackResources((int)width, (int)height))
                return false;

            if (_readbackContext.IsNull() || _readbackSrcFbo == 0 || _readbackFlipFbo == 0)
                return false;

            var size = textureData.Length;
            if (size == 0)
                return false;

            // === Primary context: place a fence after all rendering and flush to submit to the GPU.
            // glFlush is non-blocking; it just ensures the driver sends its command batch to the GPU
            // so the readback context can wait on the fence without an implicit stall.
            if (_primaryFence.IsNotNull())
            {
                GL.DeleteSync(_primaryFence);
                PointerUtilities.SetToNull(ref _primaryFence);
            }
            _primaryFence = GL.FenceSync(GL.SYNC_GPU_COMMANDS_COMPLETE, 0);
            GL.Flush();

            // === Switch to the dedicated readback context. ===
            if (!SDL.GL_MakeCurrent(_window, _readbackContext))
                return false;

            // GPU-side wait: the readback context's command queue will not proceed past this point
            // until the primary context's fence is signalled. The CPU is never stalled.
            GL.WaitSync(_primaryFence, 0, GL.TIMEOUT_IGNORED);

            var writePBO = _pbos[_pboIndex];
            GL.BindBuffer(GL.PIXEL_PACK_BUFFER, writePBO);

            // Blit core render target into flip target with Y-inversion, then read into PBO.
            GL.BindFramebuffer(GL.READ_FRAMEBUFFER, _readbackSrcFbo);
            GL.BindFramebuffer(GL.DRAW_FRAMEBUFFER, _readbackFlipFbo);
            GL.BlitFramebuffer(0, 0, (int)width, (int)height, 0, 0, (int)width, (int)height, GL.COLOR_BUFFER_BIT, GL.NEAREST);
            GL.BindFramebuffer(GL.READ_FRAMEBUFFER, _readbackFlipFbo);
            GL.ReadPixels(0, 0, (int)width, (int)height, GL.BGRA, GL.UNSIGNED_BYTE, IntPtr.Zero);
            GL.BindFramebuffer(GL.READ_FRAMEBUFFER, 0);
            GL.BindFramebuffer(GL.DRAW_FRAMEBUFFER, 0);

            // Fence so we can detect when this PBO's data is CPU-readable.
            if (_pboFences[_pboIndex].IsNotNull())
            {
                GL.DeleteSync(_pboFences[_pboIndex]);
                _pboFences[_pboIndex] = IntPtr.Zero;
            }
            _pboFences[_pboIndex] = GL.FenceSync(GL.SYNC_GPU_COMMANDS_COMPLETE, 0);
            GL.Flush();

            var readIndex = (_pboIndex + 1) % NUM_PBOS;
            _pboIndex = (_pboIndex + 1) % NUM_PBOS;

            // Non-blocking check: if the oldest PBO's data is already available, copy it out.
            var success = false;
            if (_pboFences[readIndex].IsNotNull())
            {
                var waitResult = GL.ClientWaitSync(_pboFences[readIndex], 0, 0);
                if (waitResult == GL.ALREADY_SIGNALED || waitResult == GL.CONDITION_SATISFIED)
                {
                    if (_pboMapped[readIndex].IsNotNull())
                        Marshal.Copy(_pboMapped[readIndex], textureData, 0, size);

                    GL.DeleteSync(_pboFences[readIndex]);
                    _pboFences[readIndex] = IntPtr.Zero;
                    success = true;
                }
            }

            GL.BindBuffer(GL.PIXEL_PACK_BUFFER, 0);

            // Restore primary context so the core can render on the next retro_run().
            _ = SDL.GL_MakeCurrent(_window, _context);
            return success;
        }

        protected override void DeInit()
        {
            try
            {
                if (_context.IsNull())
                    return;

                var madeCurrent = false;
                if (_context.IsNotNull() && _window.IsNotNull())
                {
                    madeCurrent = SDL.GL_MakeCurrent(_window, _context);
                    if (!madeCurrent)
                        _wrapper.LogHandler.LogWarning("Failed to make context current; will avoid GL cleanup to prevent crash.", "SK.Libretro.OpenGLRenderProxySDL.DeInit");
                }

                if (madeCurrent)
                {
                    DestroyReadbackContext();
                    DisposeReadbackResources();

                    if (_fbo != 0)
                    {
                        GL.DeleteFramebuffers(1, ref _fbo);
                        _fbo = 0;
                    }
                    if (_fboTex != 0)
                    {
                        GL.DeleteTextures(1, ref _fboTex);
                        _fboTex = 0;
                    }
                    if (_fboRbo != 0)
                    {
                        GL.DeleteRenderbuffers(1, ref _fboRbo);
                        _fboRbo = 0;
                    }

                    // ContextDestroy?.Invoke();

                    GL.Finish();

                    DestroyNativeContext();
                }
                else
                {
                    _wrapper.LogHandler.LogWarning("Skipping GL API cleanup because context couldn't be made current.", "SK.Libretro.OpenGLRenderProxySDL.Shutdown");
                    PointerUtilities.SetToNull(ref _context);

                    for (var i = 0; i < NUM_PBOS; ++i)
                        PointerUtilities.SetToNull(ref _pboFences[i]);
                    _pboSize = 0;
                    _pboIndex = 0;

                    _fbo = 0;
                    _fboTex = 0;
                    _fboRbo = 0;
                }

                DestroyNativeWindow();

		        _wrapper.LogHandler.LogInfo("Shutdown complete.", "SK.Libretro.OpenGLRenderProxySDL.Shutdown");
            }
            catch (Exception e)
            {
                _wrapper.LogHandler.LogException(e);
            }
        }

        protected override IntPtr GetCurrentFrameBufferCall() => _fbo != 0 ? (IntPtr)_fbo : (IntPtr)0;

        protected override IntPtr GetProcAddressCall(string functionName) => SDL.GL_GetProcAddress(functionName);

        private bool CreateNativeWindow(int width, int height)
        {
            if (!SDL.InitSubSystem(SDL.INIT_VIDEO))
            {
                _wrapper.LogHandler.LogError("Failed to initialize SDL Video subsystem: " + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.CreateNativeWindow");
                return false;
            }

            var uniqueTitle = $"LibretroGL_{GetHashCode()}_{DateTime.UtcNow.Ticks}";
            _window = SDL.CreateWindow(uniqueTitle, width, height, SDL.WINDOW_HIDDEN | SDL.WINDOW_OPENGL);
            if (_window.IsNull())
            {
                _wrapper.LogHandler.LogError("Failed to create SDL Window : " + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.CreateNativeWindow");
                return false;
            }
            return true;
        }

        private void DestroyNativeWindow()
        {
            if (_window.IsNotNull())
            {
                SDL.DestroyWindow(_window);
                PointerUtilities.SetToNull(ref _window);
            }

            SDL.QuitSubSystem(SDL.INIT_VIDEO);
        }

        private bool CreateNativeContext()
        {
            _ = SDL.GL_SetAttribute(SDL.GLAttr.RED_SIZE, 8);
            _ = SDL.GL_SetAttribute(SDL.GLAttr.GREEN_SIZE, 8);
            _ = SDL.GL_SetAttribute(SDL.GLAttr.BLUE_SIZE, 8);
            _ = SDL.GL_SetAttribute(SDL.GLAttr.ALPHA_SIZE, 8);
            _ = SDL.GL_SetAttribute(SDL.GLAttr.DEPTH_SIZE, 24);
            _ = SDL.GL_SetAttribute(SDL.GLAttr.STENCIL_SIZE, 8);
            _ = SDL.GL_SetAttribute(SDL.GLAttr.DOUBLEBUFFER, 1);
            _ = SDL.GL_SetAttribute(SDL.GLAttr.CONTEXT_MAJOR_VERSION, 3);
            _ = SDL.GL_SetAttribute(SDL.GLAttr.CONTEXT_MINOR_VERSION, 3);
            _ = SDL.GL_SetAttribute(SDL.GLAttr.CONTEXT_PROFILE_MASK, SDL.GL_CONTEXT_PROFILE_CORE);

            _context = SDL.GL_CreateContext(_window);
            if (_context.IsNull())
            {
                _wrapper.LogHandler.LogError("Failed to create SDL GL context: " + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.CreateNativeContext");
                return false;
            }

            if (!SDL.GL_MakeCurrent(_window, _context))
            {
                _wrapper.LogHandler.LogError("Failed to make SDL GL context current: " + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.CreateNativeContext");
                return false;
            }

            try
            {
                GL.LoadFunctions();
            }
            catch (Exception e)
            {
                _wrapper.LogHandler.LogException(e);
                return false;
            }

            if (!SDL.GL_MakeCurrent(_window, IntPtr.Zero))
                _wrapper.LogHandler.LogWarning("Failed to unset current context after initialization: " + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.CreateNativeContext");

            return true;
        }

        private void DestroyNativeContext()
        {
            if (_context.IsNull())
                return;

            if (!SDL.GL_DestroyContext(_context))
                _wrapper.LogHandler.LogError("Failed to destroy SDL GL context: " + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.DestroyNativeContext");
            PointerUtilities.SetToNull(ref _context);
        }

        private bool CreateReadbackContext()
        {
            if (_window.IsNull() || _context.IsNull())
            {
                _wrapper.LogHandler.LogError("Primary context must exist first.", "SK.Libretro.OpenGLRenderProxySDL.CreateReadbackContext");
                return false;
            }

            if (!SDL.GL_MakeCurrent(_window, _context))
            {
                _wrapper.LogHandler.LogError("Failed to make primary context current." + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.CreateReadbackContext");
                return false;
            }

            _ = SDL.GL_SetAttribute(SDL.GLAttr.SHARE_WITH_CURRENT_CONTEXT, 1);
            _readbackContext = SDL.GL_CreateContext(_window);
            _ = SDL.GL_SetAttribute(SDL.GLAttr.SHARE_WITH_CURRENT_CONTEXT, 0);

            if (_readbackContext.IsNull())
            {
                _wrapper.LogHandler.LogError("SDL_GL_CreateContext (shared) failed." + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.CreateReadbackContext");
                return false;
            }

            // Create the readback context's FBOs while it is current.
            if (!RecreateReadbackFBOs())
            {
                DestroyReadbackContext();
                return false;
            }

            // Restore primary context.
            if (!SDL.GL_MakeCurrent(_window, _context))
            {
                _wrapper.LogHandler.LogError("Failed to restore primary context." + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.CreateReadbackContext");
                DestroyReadbackContext();
                return false;
            }

            _wrapper.LogHandler.LogInfo("Shared readback context created.", "SK.Libretro.OpenGLRenderProxySDL.CreateReadbackContext");
            return true;
        }

        private void DestroyReadbackContext()
        {
            if (_readbackContext.IsNull())
                return;

            // Sync objects are shared; delete the primary fence from whichever context is current.
            if (_primaryFence.IsNotNull())
            {
                GL.DeleteSync(_primaryFence);
                PointerUtilities.SetToNull(ref _primaryFence);
            }

            if (!SDL.GL_MakeCurrent(_window, _readbackContext))
            {
                _wrapper.LogHandler.LogError("Failed to make readback context current for cleanup." + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.DestroyReadbackContext");
                return;
            }

            if (_readbackSrcFbo != 0)
            {
                GL.DeleteFramebuffers(1, ref _readbackSrcFbo);
                _readbackSrcFbo = 0;
            }
            if (_readbackFlipFbo != 0)
            {
                GL.DeleteFramebuffers(1, ref _readbackFlipFbo);
                _readbackFlipFbo = 0;
            }

            if (!SDL.GL_MakeCurrent(_window, IntPtr.Zero))
                _wrapper.LogHandler.LogError("Failed to unset current context for cleanup." + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.DestroyReadbackContext");
            if (!SDL.GL_DestroyContext(_readbackContext))
                _wrapper.LogHandler.LogError("Failed to destroy readback context." + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.DestroyReadbackContext");
            PointerUtilities.SetToNull(ref _readbackContext);

            if (!SDL.GL_MakeCurrent(_window, _context))
                _wrapper.LogHandler.LogError("Failed to restore primary context after cleanup." + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.DestroyReadbackContext");

            _wrapper.LogHandler.LogInfo("Shared readback context destroyed.", "SK.Libretro.OpenGLRenderProxySDL.DestroyReadbackContext");
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
                    GL.GenBuffers(NUM_PBOS, pbosPtr);
            }

            for (var i = 0; i < NUM_PBOS; ++i)
            {
                GL.BindBuffer(GL.PIXEL_PACK_BUFFER, _pbos[i]);
                GL.BufferStorage(GL.PIXEL_PACK_BUFFER, _pboSize, IntPtr.Zero, GL.MAP_READ_BIT | GL.MAP_PERSISTENT_BIT | GL.DYNAMIC_STORAGE_BIT);
                _pboMapped[i] = GL.MapBufferRange(GL.PIXEL_PACK_BUFFER, 0, _pboSize, GL.MAP_READ_BIT | GL.MAP_PERSISTENT_BIT);
                if (_pboMapped[i].IsNull())
                    _wrapper.LogHandler.LogWarning("Persistent PBO mapping failed for index " + i, "SK.Libretro.OpenGLRenderProxySDL.InitReadbackResources");
            }

            GL.BindBuffer(GL.PIXEL_PACK_BUFFER, 0);

            _pboIndex = 0;
            for (var i = 0; i < NUM_PBOS; ++i)
                PointerUtilities.SetToNull(ref _pboFences[i]);

            // Create a flip framebuffer + texture so we can blit the FBO vertically flipped
            if (_flipFboTex != 0)
            {
                GL.DeleteTextures(1, ref _flipFboTex);
                _flipFboTex = 0;
            }
            if (_flipFbo != 0)
            {
                GL.DeleteFramebuffers(1, ref _flipFbo);
                _flipFbo = 0;
            }

            GL.GenTextures(1, out _flipFboTex);
            GL.BindTexture(GL.TEXTURE_2D, _flipFboTex);
            GL.TexImage2D(GL.TEXTURE_2D, 0, GL.BGRA, width, height, 0, GL.BGRA, GL.UNSIGNED_BYTE, IntPtr.Zero);
            GL.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
            GL.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
            GL.BindTexture(GL.TEXTURE_2D, 0);

            GL.GenFramebuffers(1, out _flipFbo);
            GL.BindFramebuffer(GL.FRAMEBUFFER, _flipFbo);
            GL.FramebufferTexture2D(GL.FRAMEBUFFER, GL.COLOR_ATTACHMENT0, GL.TEXTURE_2D, _flipFboTex, 0);
            {
                var drawBuf = GL.COLOR_ATTACHMENT0;
                GL.DrawBuffers(1, ref drawBuf);
            }
            if (GL.CheckFramebufferStatus(GL.FRAMEBUFFER) != GL.FRAMEBUFFER_COMPLETE)
            {
                _wrapper.LogHandler.LogError("Failed to create flip framebuffer; aborting readback initialization.", "SK.Libretro.OpenGLRenderProxySDL.InitReadbackResources");
                GL.DeleteFramebuffers(1, ref _flipFbo);
                GL.DeleteTextures(1, ref _flipFboTex);
                _flipFbo = 0;
                _flipFboTex = 0;
                GL.BindBuffer(GL.PIXEL_PACK_BUFFER, 0);
                // Cleanup everything we created earlier for readback
                DisposeReadbackResources();
                return false;
            }
            // restore default binding
            GL.BindFramebuffer(GL.FRAMEBUFFER, 0);

            if (_readbackContext.IsNotNull())
            {
                if (!RecreateReadbackFBOs())
                {
                    GL.DeleteFramebuffers(1, ref _flipFbo);
                    GL.DeleteTextures(1, ref _flipFboTex);
                    _flipFbo = 0;
                    _flipFboTex = 0;
                    GL.BindBuffer(GL.PIXEL_PACK_BUFFER, 0);
                    // Cleanup everything we created earlier for readback
                    DisposeReadbackResources();
                    return false;
                }
                
                if (!SDL.GL_MakeCurrent(_window, _context))
                {
                    DeInit();
                    return false;
                }
            }

            return true;
        }

        private void DisposeReadbackResources()
        {
            for (var i = 0; i < NUM_PBOS; ++i)
            {
                if (_pboFences[i].IsNotNull())
                {
                    GL.DeleteSync(_pboFences[i]);
                    PointerUtilities.SetToNull(ref _pboFences[i]);
                }
            }

            if (_pbos[0] != 0)
            {
                for (var i = 0; i < NUM_PBOS; ++i)
                {
                    if (_pboMapped[i].IsNotNull())
                    {
                        GL.BindBuffer(GL.PIXEL_PACK_BUFFER, _pbos[i]);
                        GL.UnmapBuffer(GL.PIXEL_PACK_BUFFER);
                        PointerUtilities.SetToNull(ref _pboMapped[i]);
                    }
                }
                GL.BindBuffer(GL.PIXEL_PACK_BUFFER, 0);

                unsafe
                {
                    fixed (uint* pbosPtr = _pbos)
                        GL.DeleteBuffers(NUM_PBOS, pbosPtr);
                }

                for (var i = 0; i < NUM_PBOS; ++i)
                    _pbos[i] = 0;
            }

            if (_flipFbo != 0)
            {
                GL.DeleteFramebuffers(1, ref _flipFbo);
                _flipFbo = 0;
            }
            if (_flipFboTex != 0)
            {
                GL.DeleteTextures(1, ref _flipFboTex);
                _flipFboTex = 0;
            }

            _pboSize = 0;
            _pboIndex = 0;
        }

        private bool RecreateReadbackFBOs()
        {
            if (_readbackContext.IsNull() || _fboTex == 0 || _flipFboTex == 0)
                return false;

            if (!SDL.GL_MakeCurrent(_window, _readbackContext))
            {
                _wrapper.LogHandler.LogError("Failed to make readback context current." + SDL.GetError(), "SK.Libretro.OpenGLRenderProxySDL.RecreateReadbackFBOs");
                return false;
            }

            if (_readbackSrcFbo != 0)
            {
                GL.DeleteFramebuffers(1, ref _readbackSrcFbo);
                _readbackSrcFbo = 0;
            }
            if (_readbackFlipFbo != 0)
            {
                GL.DeleteFramebuffers(1, ref _readbackFlipFbo);
                _readbackFlipFbo = 0;
            }

            // Source FBO: reads from the core's render target (shared m_fbo_tex).
            GL.GenFramebuffers(1, out _readbackSrcFbo);
            GL.BindFramebuffer(GL.FRAMEBUFFER, _readbackSrcFbo);
            GL.FramebufferTexture2D(GL.FRAMEBUFFER, GL.COLOR_ATTACHMENT0, GL.TEXTURE_2D, _fboTex, 0);
            if (GL.CheckFramebufferStatus(GL.FRAMEBUFFER) != GL.FRAMEBUFFER_COMPLETE)
            {
                _wrapper.LogHandler.LogError("Source FBO is incomplete.", "SK.Libretro.OpenGLRenderProxySDL.RecreateReadbackFBOs");
                GL.BindFramebuffer(GL.FRAMEBUFFER, 0);
                return false;
            }
            GL.BindFramebuffer(GL.FRAMEBUFFER, 0);

            // Flip FBO: Y-inversion target (shared m_flip_tex).
            GL.GenFramebuffers(1, out _readbackFlipFbo);
            GL.BindFramebuffer(GL.FRAMEBUFFER, _readbackFlipFbo);
            GL.FramebufferTexture2D(GL.FRAMEBUFFER, GL.COLOR_ATTACHMENT0, GL.TEXTURE_2D, _flipFboTex, 0);
            var drawBuf = GL.COLOR_ATTACHMENT0;
            GL.DrawBuffers(1, ref drawBuf);
            if (GL.CheckFramebufferStatus(GL.FRAMEBUFFER) != GL.FRAMEBUFFER_COMPLETE)
            {
                _wrapper.LogHandler.LogError("Flip FBO is incomplete.", "SK.Libretro.OpenGLRenderProxySDL.RecreateReadbackFBOs");
                GL.BindFramebuffer(GL.FRAMEBUFFER, 0);
                return false;
            }
            GL.BindFramebuffer(GL.FRAMEBUFFER, 0);

            // Leave readback context current; caller is responsible for restoring the primary.
            return true;
        }
    }
}
