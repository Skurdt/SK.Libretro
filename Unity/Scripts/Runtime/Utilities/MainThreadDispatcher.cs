using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK.Libretro.Unity
{
    internal sealed class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;
        private readonly Queue<Action> _actions = new();

        private void Awake()
        {
            if (!_instance)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Update()
        {
            if (!_instance)
                return;

            lock (_actions)
                while (_actions.Count > 0)
                    _actions.Dequeue().Invoke();
        }

        public static void Enqueue(IEnumerator action)
        {
            if (!_instance)
            {
                Debug.LogError("No MainThreadDispatcher found in scene.");
                return;
            }

            lock (_instance._actions)
                _instance._actions.Enqueue(() => _instance.StartCoroutine(action));
        }

        public static void Enqueue(Action action) => Enqueue(ActionWrapper(action));

        public static void Enqueue<T1>(Action<T1> action, T1 param1) => Enqueue(ActionWrapper(action, param1));

        public static void Enqueue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2) => Enqueue(ActionWrapper(action, param1, param2));

        public static void Enqueue<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3) => Enqueue(ActionWrapper(action, param1, param2, param3));

        public static void Enqueue<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4) => Enqueue(ActionWrapper(action, param1, param2, param3, param4));

        private static IEnumerator ActionWrapper(Action action)
        {
            action();
            yield return null;
        }

        private static IEnumerator ActionWrapper<T1>(Action<T1> action, T1 param1)
        {
            action(param1);
            yield return null;
        }

        private static IEnumerator ActionWrapper<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
        {
            action(param1, param2);
            yield return null;
        }

        private static IEnumerator ActionWrapper<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
        {
            action(param1, param2, param3);
            yield return null;
        }

        private static IEnumerator ActionWrapper<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            action(param1, param2, param3, param4);
            yield return null;
        }
    }
}
