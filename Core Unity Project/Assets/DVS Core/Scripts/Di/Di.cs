using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dvs.Core.IoC
{
    public static class Di
    {
        public static class Injector
        {
            static Injector()
            {
                ResetDefaults();
            }

            public static void ResetDefaults()
            {
                Di.Accessor.FullReset();

                var diSetupType = typeof(DiSetupAttribute);

                var diSetups = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .SelectMany(s => s.GetMethods(System.Reflection.BindingFlags.Static))
                    .Where(x => Attribute.IsDefined(x, diSetupType));

                diSetups
                    .OrderBy(x =>
                    {
                        var attrs = x.GetCustomAttributes(diSetupType, true);
                        return ((DiSetupAttribute)attrs[0]).SortOrder;
                    })
                    .ToList()
                    .ForEach(x =>
                    {
                        x.Invoke(null, null);
                    });
            }

            /// <summary>
            /// Injects values
            /// </summary>
            /// <param name="obj">the object to inject</param>
            /// <param name="used">a list of used types</param>
            /// <param name="ignoreSetValues">Should it ignore values that already have data?</param>
            public static void Inject(object obj, List<Type> used = null, bool ignoreSetValues = false)
            {
                if (used == null)
                {
                    used = new List<Type>();
                }

                // creates a new reference to the list, so unconnected branches are not affected captured for cyclical dependencies, only children.
                used = new List<Type>(used);

                // Cyclical dependencies should only be called out for objects duplicated in the same tree, but not the same branch.
                var usedOutOfThisLoop = new List<Type>(used);

                foreach (var property in obj.GetType().GetProperties())
                {
                    // do not bind if requested not to
                    if (Attribute.IsDefined(property, typeof(DoNotBindAttribute)))
                    {
                        continue;
                    }

                    // if the property has a setter;
                    if (property.CanWrite)
                    {
                        if (ignoreSetValues && property.GetValue(obj, null) != null) continue;
                        var fromType = property.PropertyType;
                        var found = Di.Accessor.GetObjectByType(fromType);
                        //var found = Bindings.Where((x) => x.FromType == fromType && x.Style != BindingStyle.ByConversion);

                        // if we have a binding for that type
                        //if (found.Any())
                        if (found != null)
                        {
                            var binding = found;

                            // is this has already been used, then we have a cyclical dependency.
                            if (usedOutOfThisLoop.Contains(fromType))
                            {
                                throw new CyclicalDependencyException("There is a cyclical dependency found on type, " + fromType.Name);
                            }

                            used.Add(fromType);
                            Inject(found, used);

                            property.SetValue(obj, found, null);
                        }
                    }
                }
            }
        }

        public abstract class Accessor
        {
            internal abstract void Reset<T>() where T : class;
            internal abstract T GetForInjection<T>() where T : class;

            private enum DiObjectReturnHandler
            {
                Factory,
                Singleton
            }

            private struct DiAccessor
            {
                public Type Type;
                public DiObjectReturnHandler Handler;
                public Accessor Accessor;
                public T ReturnObject<T>() where T : class
                {

                    if (Handler == DiObjectReturnHandler.Factory)
                    {
                        return Di<T>.Factory.Get();
                    }

                    if (Handler == DiObjectReturnHandler.Singleton)
                    {
                        return Di<T>.Singleton.Get();
                    }

                    return null;
                }
            }

            private static Dictionary<Type, DiAccessor> Accessors = new Dictionary<Type, DiAccessor>();

            internal static void AddAccessor(Accessor accessor, Type type)
            {
                if (Accessors.ContainsKey(type))
                {
                    throw new DiTypeAlreadyConfiguredException(
                        "The type " + type.FullName + " was attempted to be configured for both Singleton and Factory");
                }

                var da = new DiAccessor();
                da.Accessor = accessor;
                da.Type = type;
                var objType = accessor.GetType();
                if (objType.IsSubclassOf(typeof(Di<>.Factory))) da.Handler = DiObjectReturnHandler.Factory;
                if (objType.IsSubclassOf(typeof(Di<>.Singleton))) da.Handler = DiObjectReturnHandler.Singleton;
                Accessors.Add(type, da);
            }

            public static void FullReset()
            {
                foreach (var a in Accessors)
                {
                    var mName = nameof(a.Value.Accessor.Reset);
                    var aType = a.Value.Accessor.GetType();
                    var aTypeGeneric = aType.GetMethod(mName,BindingFlags.Static | BindingFlags.NonPublic);
                    aTypeGeneric.Invoke(a.Value.Accessor, null);
                }
            }
            public static object GetObjectByType(Type type)
            {
                var method = typeof(Di.Accessor).GetMethod(nameof(GetObject));
                var gMethod = method.MakeGenericMethod(type);
                return gMethod.Invoke(null, null);
            }

            public static T GetObject<T>() where T : class
            {
                var type = typeof(T);
                var routeKnown = Accessors.ContainsKey(type);

                if (routeKnown)
                {
                    var accessor = Accessors[type];
                    return accessor.ReturnObject<T>();
                }

                var isInterface = type.IsInterface;
                if (isInterface)
                {
                    throw new Exception("Di.GetObject<" + type.FullName + ">() has no bound accessors.  ");
                }

                return Di<T>.Factory.Get();
            }

            public static void Trigger(Type type)
            {
                var method = typeof(Di<>.Signal).GetMethod("Trigger");
                var gMethod = method.MakeGenericMethod(type);
                gMethod.Invoke(null, null);
            }
        }


    }

    public static class Di<T> where T : class
    {


        public class Factory : Di.Accessor
        {
            private static Func<T> _Setup;

            private static bool _HasConstructor = false;

            private static Type _BoundType = null;

            private static Func<T, T> _Cleanup = (x) => x;
            private static Func<T, T> _Restore = (x) => x;

            private static List<T> _Pool = new List<T>();

            private static bool _UsePool = false;

            static Factory()
            {
                Di.Accessor.AddAccessor(new Di<T>.Factory(), typeof(T));
            }

            public static bool UsePool
            {
                get
                {
                    return _UsePool;
                }
                set
                {
                    _UsePool = value;
                }
            }

            public static T Get()
            {
                if (UsePool)
                {
                    return GetFromPool();
                }

                return CreateInstance();
            }

            public static void SetConstructor(Func<T> constructor)
            {
                _Setup = constructor;
                _HasConstructor = true;
            }

            public static void Bind<T>()
            {
                _BoundType = typeof(T);
            }

            public static void Bind(Type type)
            {
                _BoundType = type;
            }

            private static T CreateInstance()
            {
                if (_HasConstructor)
                {
                    return _Setup();
                }

                var hasBinding = _BoundType != null;
                if (hasBinding)
                {
                    return (T)Activator.CreateInstance(_BoundType);
                }

                if (typeof(T).IsInterface)
                {
                    throw new DiNotConfiguredException("Di<" + typeof(T).Name + ">.Factory.Get() was called, but that interface has not been configured.");
                }

                return (T)Activator.CreateInstance<T>();
            }

            private static T GetFromPool()
            {
                if (_Pool.Count > 0)
                {
                    int index = _Pool.Count - 1;
                    var obj = _Pool[index];
                    _Pool.RemoveAt(index);
                    return _Restore(obj);
                }

                return _Restore(CreateInstance());
            }

            public static void SetPoolCleanup(Func<T, T> cleanup)
            {
                _Cleanup = cleanup;
            }

            public static void SetPoolRestore(Func<T, T> restore)
            {
                _Restore = restore;
            }

            public static void ReturnToPool(T obj)
            {
                _Pool.Add(_Cleanup(obj));
            }

            internal static void Reset()
            {
                _Setup = null;
                _HasConstructor = false;
                _Cleanup = (x) => x;
                _Restore = (x) => x;
                _Pool.Clear();
                _UsePool = false;
                _BoundType = null;
            }

            internal override void Reset<T1>()
            {
                Di<T1>.Factory.Reset();
            }

            internal override T1 GetForInjection<T1>()
            {
                return Di<T1>.Factory.Get();
            }
        }

        public class Singleton : Di.Accessor
        {
            private static T _Value;

            private static Type _BoundType = null;

            private static Func<T> _Setup;

            private static bool _HasConstructor = false;

            private static Action<T> _OnChange = (x) => { };

            static Singleton()
            {
                Di.Accessor.AddAccessor(new Di<T>.Singleton(), typeof(T));
            }

            public static T Get()
            {
                if (_Value == null)
                {
                    _Value = CreateInstance();
                }

                return _Value;
            }

            private static T CreateInstance()
            {
                if (_HasConstructor)
                {
                    return _Setup();
                }

                var hasType = _BoundType != null;
                if (hasType)
                {
                    return (T)Activator.CreateInstance(_BoundType);
                }

                if (typeof(T).IsInterface)
                {
                    throw new DiNotConfiguredException("Di<" + typeof(T).Name + ">.Singleton.Get() was called, but that interface has not been configured.");
                }

                return (T)Activator.CreateInstance<T>();
            }

            public static void WireUpListener(Action<T> listener, bool wireUp = true)
            {
                if (wireUp)
                {
                    _OnChange += listener;
                }
                else
                {
                    _OnChange -= listener;
                }
            }

            public static void SetConstructor(Func<T> constructor)
            {
                _HasConstructor = true;
                _Setup = constructor;
            }

            public static void Bind<FROM>()
            {
                _BoundType = typeof(FROM);
            }

            internal static void Reset()
            {
                _Value = null;
                _BoundType = null;
                _Setup = null;
                _HasConstructor = false;
                _OnChange = (x) => { };
            }

            internal override void Reset<T1>()
            {
                Di<T1>.Singleton.Reset();
            }

            internal override T1 GetForInjection<T1>()
            {
                throw new NotImplementedException();
            }
        }

        public class Convert
        {
            private static Dictionary<Type, Func<object, T>> _Routes
                = new Dictionary<Type, Func<object, T>>();

            public static void SetRoute<FROM>(Func<FROM, T> route) where FROM : class
            {
                _Routes[typeof(FROM)] = (Func<object, T>)
                     ((x) => { return route((FROM)x); });
            }

            public static T From<FROM>(FROM obj) where FROM : class
            {
                if (_Routes.ContainsKey(typeof(T)))
                {
                    return (T)_Routes[typeof(FROM)](obj);
                }

                throw new DiConvertNotConfiguredException("" +
                    "Requested to convert from "
                    + typeof(FROM).Name
                    + " to "
                    + typeof(T).Name
                    + ", but no converstion has every been set for this route.");
            }
        }

        public class Signal
        {
            private static Action _OnTrigger = () => { };

            public static void WireUpListener(Action listener, bool wireUp = true)
            {
                if (wireUp)
                {
                    _OnTrigger += listener;
                }
                else
                {
                    _OnTrigger -= listener;
                }
            }

            public static void Trigger()
            {
                _OnTrigger();
            }
        }
    }
}
