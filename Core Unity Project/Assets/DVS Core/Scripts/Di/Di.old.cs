//using Dvs.Core.IoC.Exceptions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Dvs.Core.IoC
//{
//    public static class Di
//    {
//        private static List<DiBinding> Bindings = new List<DiBinding>();

//        private static Dictionary<Type, object> TypeEvents 
//            = new Dictionary<Type, object>();

//        private static Dictionary<Type, Action> TypeSignals
//            = new Dictionary<Type, Action>();

//        static Di()
//        {
//            ResetDefaults();
//        }

//        public static void Clear()
//        {
//            Bindings.Clear();
//            TypeEvents.Clear();
//        }

//        public static void ResetDefaults()
//        {
//            Clear();

//            var diSetupType = typeof(DiSetupAttribute);

//            var diSetups = AppDomain.CurrentDomain.GetAssemblies()
//                .SelectMany(s => s.GetTypes())
//                .SelectMany(s => s.GetMethods(System.Reflection.BindingFlags.Static))
//                .Where(x => Attribute.IsDefined(x, diSetupType));

//            diSetups
//                .OrderBy(x =>
//                {
//                    var attrs = x.GetCustomAttributes(diSetupType, true);
//                    return ((DiSetupAttribute)attrs[0]).SortOrder;
//                })
//                .ToList()
//                .ForEach(x => 
//                {
//                    x.Invoke(null, null);
//                });
//        }

//        public static IDiBinding Bind<T>()
//        {
//            var fromType = typeof(T);

//            var match = Bindings.Where((x) => x != null && x.FromType == fromType);

//            DiBinding binding = null;

//            if (match.Any())
//            {
//                binding = match.First();
//            }
//            else
//            {
//                binding = new DiBinding { FromType = typeof(T) };
//                Bindings.Add(binding);
//            }

//            return binding;
//        }
//        public static void TriggerSignal<T>()
//        {
//            var type = typeof(T);
//            var noListeners = !TypeSignals.ContainsKey(type);
//            if (noListeners) return;

//            var action = (Action)TypeSignals[type];
//            action();
//        }

//        public static void TriggerEvent<T>(T message)
//        {
//            var type = typeof(T);
//            var noListeners = !TypeEvents.ContainsKey(type);
//            if (noListeners) return;

//            var action = (Action<T>)TypeEvents[type];
//            action(message);
//        }

//        public static void AttachEventListener<T>(Action<T> listener, bool attach = true)
//        {
//            var type = typeof(T);
//            var noListeners = !TypeEvents.ContainsKey(type);

//            if (attach)
//            {
//                if (noListeners)
//                {
//                    TypeEvents.Add(type, listener);
//                    return;
//                }

//                var action = (Action<T>)TypeEvents[type];
//                action += listener;
//            }
//            else
//            {
//                if (noListeners) return;

//                var action = (Action<T>)TypeEvents[type];
//                action -= listener;

//                if (action == null)
//                {
//                    TypeEvents.Remove(type);
//                }
//                else
//                {
//                    TypeEvents[type] = action;
//                }
//            }
//        }

//        public static T Convert<T>(object from)
//        {
//            var fromType = from.GetType();
//            var toType = typeof(T);
//            var found = Bindings.Where((x) => x.ConversionFrom == fromType && x.ConversionTo == toType);

//            var foundAny = found.Any();
//            if (!foundAny)
//            {

//                throw new NoBindingsFoundException("Requested Type " + fromType.Name + ", but it was never bound.  Please check that there is an associated binding configured");
//            }


//            var binding = found.First();
//            if (binding.Style != BindingStyle.ByConversion)
//            {
//                throw new BindingNotConversionException("Requested to get type " + fromType.Name + ", but it was not bound as a conversion.  Please use Di.Get() instead of Di.Get(...), or change the configuration");
//            }


//            return (T)binding.Convert(from);
//        }

//        public static T Get<T>(object key = null)
//        {
//            var fromType = typeof(T);
//            var found = Bindings.Where((x) => x.FromType == fromType);
//            if (!found.Any())
//            {
//                throw new NoBindingsFoundException("Requested Type " + fromType.Name + ", but it was never bound.  Please check that there is an associated binding configured");
//            }

//            var binding = found.First();
//            if (binding.Style == BindingStyle.ByConversion)
//            {
//                throw new BindingNotConversionException("Requested to get type " + fromType.Name + ", but it was bound as a conversion.  Please use Di.Convert(...) instead of Di.Get(), or change the configuration");
//            }

//            if (key == null)
//            {
//                var result = (T)binding.Get();
//                Inject(result);

//                return result;
//            }
//            else
//            {
//                return (T)binding.GetByKey(key);
//            }
//        }

//        /// <summary>
//        /// Injects values
//        /// </summary>
//        /// <param name="obj">the object to inject</param>
//        /// <param name="used">a list of used types</param>
//        /// <param name="ignoreSetValues">Should it ignore values that already have data?</param>
//        public static void Inject(object obj, List<Type> used = null, bool ignoreSetValues = false)
//        {
//            if (used == null)
//            {
//                used = new List<Type>();
//            }

//            // creates a new reference to the list, so unconnected branches are not affected captured for cyclical dependencies, only children.
//            used = new List<Type>(used);

//            // Cyclical dependencies should only be called out for objects duplicated in the same tree, but not the same branch.
//            var usedOutOfThisLoop = new List<Type>(used);

//            foreach (var property in obj.GetType().GetProperties())
//            {
//                // do not bind if requested not to
//                if (Attribute.IsDefined(property, typeof(DoNotBindAttribute)))
//                {
//                    continue;
//                }

//                // if the property has a setter;
//                if (property.CanWrite)
//                {
//                    if (ignoreSetValues && property.GetValue(obj, null) != null) continue;
//                    var fromType = property.PropertyType;
//                    var found = Bindings.Where((x) => x.FromType == fromType && x.Style != BindingStyle.ByConversion);

//                    // if we have a binding for that type
//                    if (found.Any())
//                    {
//                        var binding = found.First();

//                        // is this has already been used, then we have a cyclical dependency.
//                        if (usedOutOfThisLoop.Contains(fromType))
//                        {
//                            throw new CyclicalDependencyException("There is a cyclical dependency found on type, " + fromType.Name);
//                        }

//                        used.Add(fromType);
//                        var val = binding.Get();
//                        Inject(val, used);

//                        property.SetValue(obj, val, null);
//                    }
//                }
//            }
//        }
//    }
//}