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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace SK.Libretro.Unity
{
    internal sealed class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly ConcurrentQueue<Func<ValueTask>> _executionQueue = new();
        private static MainThreadDispatcher _instance;

        public static void Construct()
        {
            if (_instance)
                return;

            _instance = FindObjectOfType<MainThreadDispatcher>();
            if (!_instance)
            {
                GameObject obj = new("MainThreadDispatcher");
                _instance = obj.AddComponent<MainThreadDispatcher>();
                DontDestroyOnLoad(obj);
            }
        }

        private async void Update()
        {
            while (_executionQueue.Count > 0)
                if (_executionQueue.TryDequeue(out Func<ValueTask> action))
                    await action();
        }

        public static void Enqueue(Func<ValueTask> action) => _executionQueue.Enqueue(async () => await action());

        public static void Enqueue(Action action) => Enqueue(async () =>
        {
            action();
            await Task.Yield();
        });
    }
}
