using SK.Libretro.Header;
using System;
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class OpenGLRenderProxyAndroid : HardwareRenderProxy
    {
        private IntPtr _eglDisplay = IntPtr.Zero;
        private IntPtr _eglSurface = IntPtr.Zero;
        private IntPtr _eglContext = IntPtr.Zero;
        private IntPtr _eglConfig = IntPtr.Zero;

        public OpenGLRenderProxyAndroid(retro_hw_render_callback hwRenderCallback, IntPtr nativeWindow)
        : base(hwRenderCallback)
            => _windowHandle = nativeWindow;

        public override bool Init()
        {
            if (_windowHandle == IntPtr.Zero)
                return false;

            // Initialize EGL
            int[] configAttribs =
            {
                EGL_RED_SIZE, 8,
                EGL_GREEN_SIZE, 8,
                EGL_BLUE_SIZE, 8,
                EGL_ALPHA_SIZE, 8,
                EGL_DEPTH_SIZE, 16,
                EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
                EGL_NONE
            };

            int[] contextAttribs =
            {
                EGL_CONTEXT_CLIENT_VERSION, 2,
                EGL_NONE
            };

            _eglDisplay = eglGetDisplay(EGL_DEFAULT_DISPLAY);
            if (_eglDisplay == IntPtr.Zero)
                return false;

            if (!eglInitialize(_eglDisplay, out _, out _))
                return false;

            if (!eglChooseConfig(_eglDisplay, configAttribs, out _eglConfig, 1, out int numConfigs) || numConfigs < 1)
                return false;

            _eglSurface = eglCreateWindowSurface(_eglDisplay, _eglConfig, _windowHandle, IntPtr.Zero);
            if (_eglSurface == IntPtr.Zero)
                return false;

            _eglContext = eglCreateContext(_eglDisplay, _eglConfig, IntPtr.Zero, contextAttribs);
            return _eglContext != IntPtr.Zero && eglMakeCurrent(_eglDisplay, _eglSurface, _eglSurface, _eglContext);
        }

        public override void PollEvents()
        {
            // No events to poll on Android in this context
        }

        public override void SwapBuffers() => eglSwapBuffers(_eglDisplay, _eglSurface);

        protected override void DeInit()
        {
            _ = eglMakeCurrent(_eglDisplay, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            _ = eglDestroyContext(_eglDisplay, _eglContext);
            _ = eglDestroySurface(_eglDisplay, _eglSurface);
            _ = eglTerminate(_eglDisplay);
        }

        protected override IntPtr GetCurrentFrameBufferCall() => IntPtr.Zero;

        protected override IntPtr GetProcAddressCall(string functionName) => eglGetProcAddress(functionName);

        // EGL constants
        private const int EGL_DEFAULT_DISPLAY = 0;
        private const int EGL_OPENGL_ES2_BIT = 4;
        private const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;
        private const int EGL_NONE = 0x3038;
        private const int EGL_RED_SIZE = 0x3024;
        private const int EGL_GREEN_SIZE = 0x3023;
        private const int EGL_BLUE_SIZE = 0x3022;
        private const int EGL_ALPHA_SIZE = 0x3021;
        private const int EGL_DEPTH_SIZE = 0x3025;
        private const int EGL_RENDERABLE_TYPE = 0x3040;

        [DllImport("android")] private static extern IntPtr ANativeWindow_fromSurface(IntPtr jniEnv, IntPtr surface);
        // P/Invoke declarations for EGL functions
        [DllImport("libEGL.so")] private static extern IntPtr eglGetDisplay(int display_id);
        [DllImport("libEGL.so")] private static extern bool eglInitialize(IntPtr dpy, out int major, out int minor);
        [DllImport("libEGL.so")] private static extern bool eglChooseConfig(IntPtr dpy, int[] attrib_list, out IntPtr config, int config_size, out int num_config);
        [DllImport("libEGL.so")] private static extern IntPtr eglCreateWindowSurface(IntPtr dpy, IntPtr config, IntPtr win, IntPtr attrib_list);
        [DllImport("libEGL.so")] private static extern IntPtr eglCreateContext(IntPtr dpy, IntPtr config, IntPtr share_context, int[] attrib_list);
        [DllImport("libEGL.so")] private static extern bool eglMakeCurrent(IntPtr dpy, IntPtr draw, IntPtr read, IntPtr ctx);
        [DllImport("libEGL.so")] private static extern bool eglSwapBuffers(IntPtr dpy, IntPtr surface);
        [DllImport("libEGL.so")] private static extern bool eglDestroySurface(IntPtr dpy, IntPtr surface);
        [DllImport("libEGL.so")] private static extern bool eglDestroyContext(IntPtr dpy, IntPtr ctx);
        [DllImport("libEGL.so")] private static extern bool eglTerminate(IntPtr dpy);
        [DllImport("libEGL.so")] private static extern IntPtr eglGetProcAddress(string procname);
    }
}
