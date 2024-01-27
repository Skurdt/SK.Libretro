using System.Collections.Generic;
using System.Threading;

namespace SK.Libretro
{
    abstract class InstanceHandler<T>
    {
        // IL2CPP does not support marshaling delegates that point to instance methods to native code.
        // Using static method and per instance table.
        private readonly Thread _thread;
        private static Dictionary<Thread, T> instancePerHandle = new Dictionary<Thread, T>();

        public InstanceHandler(T childClass)
        {
            _thread = Thread.CurrentThread;
            lock(instancePerHandle)
            {
                instancePerHandle.Add(_thread, childClass);
            }
        }

        ~InstanceHandler()
        {
            lock(instancePerHandle)
            {
                instancePerHandle.Remove(_thread);
            }
        }

        protected static bool GetInstance(Thread thread, out T instance)
        {
            bool ok;
            lock (instancePerHandle)
            {
                ok = instancePerHandle.TryGetValue(thread, out instance);
            }
            return ok;
        }
    }
}
