/*
Copyright 2015 Pim de Witte All Rights Reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SK.Libretro.Unity
{
    internal sealed class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;
        private static readonly Queue<Action> _executionQueue = new();
        private static readonly SemaphoreSlim _executionQueueLock = new(1, 1);

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

        public void Update()
        {
            _executionQueueLock.Wait();
            try
            {
                while (_executionQueue.Count > 0)
                    _executionQueue.Dequeue().Invoke();
            }
            finally
            {
                _ = _executionQueueLock.Release();
            }
        }

        public static void Enqueue(IEnumerator action)
        {
            _executionQueueLock.Wait();
            try
            {
                _executionQueue.Enqueue(() => _instance.StartCoroutine(action));
            }
            finally
            {
                _ = _executionQueueLock.Release();
            }
        }

        public static void Enqueue(Action action) => Enqueue(ActionWrapper(action));

        public static Task EnqueueAsync(Action action)
        {
            TaskCompletionSource<bool> tcs = new();

            void WrappedAction()
            {
                try
                {
                    action();
                    _ = tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    _ = tcs.TrySetException(ex);
                }
            }

            Enqueue(ActionWrapper(WrappedAction));
            return tcs.Task;
        }

        private static IEnumerator ActionWrapper(Action a)
        {
            a();
            yield return null;
        }
    }
}
