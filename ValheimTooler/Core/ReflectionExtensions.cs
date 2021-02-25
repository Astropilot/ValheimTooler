using System.Reflection;

namespace ValheimTooler.Core
{
    public static class ReflectionExtensions
    {
        public static T GetFieldValue<T>(this object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj);
        }

        public static void SetFieldValue<T>(this object obj, string name, object value)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            field?.SetValue(obj, value);
        }

        public static object CallMethod(this object obj, string methodName, params object[] args)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var mi = obj.GetType().GetMethod(methodName, bindingFlags);
            if (mi != null)
            {
                return mi.Invoke(obj, args);
            }
            return null;
        }
    }
}
