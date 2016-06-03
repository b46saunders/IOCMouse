using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IOC
{
    public static class Injector
    {
        public static bool LoggingEnabled = false;
        private static ConcurrentDictionary<Type, InterfaceBinding> _bindings = new ConcurrentDictionary<Type, InterfaceBinding>();

        private static void Log(string message)
        {
            if (LoggingEnabled)
            {
                Console.WriteLine(message);
            }
        }

        public static InterfaceBinding Bind<TInterface, TBinding>() where TBinding : TInterface
        {
            var createdBinding = new InterfaceBinding(typeof(TInterface),typeof(TBinding));
            TryAddBinding<TInterface,TBinding>(createdBinding);
            return createdBinding;
        }

        public static InterfaceBinding Bind<TInterface,TBinding>(TInterface instance) where TBinding : TInterface
        {
            var binding = new InterfaceBinding(typeof (TInterface), typeof (TBinding), () => instance);
            TryAddBinding<TInterface, TBinding>(binding);
            Log($"Adding instance binding for type: {typeof(TInterface).FullName}");
            return binding;
        }

        private static void TryAddBinding<TInterface, TBinding>(InterfaceBinding binding)
        {
            if (!_bindings.TryAdd(typeof(TInterface), binding))
            {
                throw new InvalidOperationException($"A binding for {typeof(TInterface).FullName} has already been set to {typeof(TBinding).FullName}");
            }
        }

        public static T Resolve<T>()
        {
            if(!_bindings.Any()) throw new Exception("No bindings have been setup");

            InterfaceBinding binding;
            if (_bindings.TryGetValue(typeof(T), out binding))
            {
                return (T)binding.ResolveBinding<T>();
            }
            throw new Exception($"Failed to resolve type: {typeof(T).FullName}");
        }

        public sealed class InterfaceBinding
        {
            private Func<object> bindingFunction;
            private readonly Type resolveType;
            private readonly Type interfaceType;
            private readonly Dictionary<string, Func<object>> constructorArguments = new Dictionary<string, Func<object>>();
            private bool singletonScope;
            private bool bindingSet;
            private object singletonObject;

            public InterfaceBinding(Type interfaceType, Type resolveType)
            {
                this.interfaceType = interfaceType;
                this.resolveType = resolveType;
                bindingSet = false;
            }

            public InterfaceBinding(Type interfaceType, Type resolveType,Func<object> instance)
            {
                this.interfaceType = interfaceType;
                this.resolveType = resolveType;
                bindingSet = true;
                singletonScope = true;
                bindingFunction = instance;
            }

            public object ResolveBinding<T>()
            {
                if (!bindingSet)
                {
                    InternalResolve();
                }
                if (!singletonScope)
                {
                    Log($"Newing up a [{typeof(T).Name}]");
                    return bindingFunction.Invoke();
                }
                if (singletonObject == null)
                {
                    singletonObject = bindingFunction.Invoke();

                }
                Log($"Fetching Singleton of [{typeof(T).Name}] - Hash({singletonObject.GetHashCode()})");
                return singletonObject;

            }

            public InterfaceBinding InSingletonScope(bool singleton = true)
            {
                singletonScope = singleton;
                return this;
            }
            public InterfaceBinding WithConstructorArguments(Dictionary<string, object> constructorArgs)
            {
                foreach (var constructorArgument in constructorArgs)
                {
                    constructorArguments.Add(constructorArgument.Key, () => constructorArgument.Value);
                }
                return this;
            }
            public InterfaceBinding WithConstructorArguments(string name, object argument)
            {
                constructorArguments.Add(name, () => argument);
                return this;
            }

            public void InternalResolve()
            {
                Log($"Building binding [{interfaceType.Name}] to [{resolveType.Name}]");
                //get constructor with most parameters
                var constructor = resolveType.GetConstructors().OrderByDescending(a => a.GetParameters().Length).First();

                //each parameter should be an interface if we are trying to resolve down the tree
                //when we see a parameter that is not an interface we need to handle resolution in some way
                //resolve each of these paramters using the resolve method
                var parameters = constructor.GetParameters().ToArray();
                var args = new List<object>();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType.IsInterface)
                    {
                        var resolveMethod = typeof(Injector).GetMethod("Resolve",
                            BindingFlags.Public | BindingFlags.Static);
                        var generic = resolveMethod.MakeGenericMethod(parameters[i].ParameterType);
                        var binding = generic.Invoke(null, null);
                        if (binding == null)
                        {
                            throw new Exception(
                                $"Unable to set binding for [{resolveType.FullName}] to {interfaceType.FullName} because parameter number {i + 1} of type {parameters[i].ParameterType} has not been bound yet.");
                        }
                        args.Add(binding);

                    }
                    else
                    {
                        var resolvedParameter = GetConstructorParameter(parameters[i]);
                        if (resolvedParameter == null)
                        {
                            throw new ArgumentNullException(
                                $"Could resolve parameter: {parameters[i].Name} for type: {resolveType.FullName} when trying to resolve: {interfaceType.FullName}. Try providing constructor arguments when binding type: {interfaceType.FullName}");
                        }
                        args.Add(resolvedParameter);
                    }
                }
                bindingFunction = () => constructor.Invoke(args.ToArray());
                bindingSet = true;
            }

            private object GetConstructorParameter(ParameterInfo parameterInfo)
            {
                Func<object> providedObjectOut;
                if (constructorArguments.TryGetValue(parameterInfo.Name, out providedObjectOut))
                {
                    return providedObjectOut();
                }
                if (parameterInfo.HasDefaultValue)
                {
                    return parameterInfo.DefaultValue;
                }
                return null;
            }
        }
    }
}
