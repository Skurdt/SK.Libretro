using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace SK.Libretro.Unity
{
    internal sealed class MainThreadDispatcher : MonoBehaviour
    {
        public static MainThreadDispatcher Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogError("No MainThreadDispatcher found in scene.");
                return _instance;
            }
        }

        private static readonly Queue<Action> _actions = new Queue<Action>();
        private static MainThreadDispatcher _instance = null;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Update()
        {
            if (_instance == null)
                return;

            lock (_actions)
                while (_actions.Count > 0)
                    _actions.Dequeue().Invoke();
        }

        public void Enqueue(IEnumerator action)
        {
            if (_instance == null)
                return;

            lock (_actions)
                _actions.Enqueue(() => StartCoroutine(action));
        }

        public void Enqueue(Action action) => Enqueue(ActionWrapper(action));

        public void Enqueue<T1>(Action<T1> action, T1 param1) => Enqueue(ActionWrapper(action, param1));

        public void Enqueue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2) => Enqueue(ActionWrapper(action, param1, param2));

        public void Enqueue<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3) => Enqueue(ActionWrapper(action, param1, param2, param3));

        public void Enqueue<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4) => Enqueue(ActionWrapper(action, param1, param2, param3, param4));

        private IEnumerator ActionWrapper(Action action)
        {
            action();
            yield return null;
        }

        private IEnumerator ActionWrapper<T1>(Action<T1> action, T1 param1)
        {
            action(param1);
            yield return null;
        }

        private IEnumerator ActionWrapper<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
        {
            action(param1, param2);
            yield return null;
        }

        private IEnumerator ActionWrapper<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
        {
            action(param1, param2, param3);
            yield return null;
        }

        private IEnumerator ActionWrapper<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            action(param1, param2, param3, param4);
            yield return null;
        }
    }
}
