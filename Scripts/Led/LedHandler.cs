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
using System.Collections.Generic;
using System.Threading;

namespace SK.Libretro
{
    internal sealed class LedHandler
    {
        private readonly ILedProcessor _processor;

        private readonly retro_set_led_state_t _setLedState;

        private readonly Thread _thread;
        private static Dictionary<Thread, LedHandler> instancePerHandle = new Dictionary<Thread, LedHandler>();

        public LedHandler(ILedProcessor processor)
        {
            _processor   = processor ?? new NullLedProcessor();
            _setLedState = SetState;
            _thread = Thread.CurrentThread;
            lock(instancePerHandle)
            {
                instancePerHandle.Add(_thread, this);
            }
        }

        ~LedHandler()
        {
            lock(instancePerHandle)
            {
                instancePerHandle.Remove(_thread);
            }
        }

        private static LedHandler GetInstance(Thread thread)
        {
            LedHandler instance;
            bool ok;
            lock (instancePerHandle)
            {
                ok = instancePerHandle.TryGetValue(thread, out instance);
            }
            return ok ? instance : null;
        }

        public class MonoPInvokeCallbackAttribute : System.Attribute
        {
            private Type type;
            public MonoPInvokeCallbackAttribute(Type t) { type = t; }
        }

        private delegate void SetStateCallbackDelegate(IntPtr data);
        [MonoPInvokeCallbackAttribute(typeof(SetStateCallbackDelegate))]
        public static void SetState(int led, int state)
        { 
            LedHandler instance = GetInstance(Thread.CurrentThread);
            instance._processor.SetState(led, state);
        }

        public bool GetLedInterface(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_led_interface ledInterface = data.ToStructure<retro_led_interface>();
            ledInterface.set_led_state = _setLedState.GetFunctionPointer();
            Marshal.StructureToPtr(ledInterface, data, true);
            return true;
        }
    }
}
