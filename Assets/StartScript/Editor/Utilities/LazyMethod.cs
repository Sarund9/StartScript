using System;
using System.Reflection;

namespace StartScript
{
    struct LazyMethod
    {
        MethodInfo method;
        Type methodType;
        string methodName;
        BindingFlags flags;

        public static LazyMethod From<T>(
            string methodName, BindingFlags flags) => new LazyMethod
            {
                methodType = typeof(T),
                methodName = methodName,
                flags = flags,
            };

        public MethodInfo Method => GetMethod();

        private MethodInfo GetMethod()
        {
            if (method == null)
            {
                method = methodType.GetMethod(methodName, flags);
            }
            return method;
        }

        public object Invoke(object instance, params object[] @params)
        {
            return Method.Invoke(instance, @params);
        }
    }
}
