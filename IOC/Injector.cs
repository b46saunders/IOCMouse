using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace IOC
{

    
    public static class Injector
    {
        private static ConcurrentDictionary<Type, InterfaceBinding> _bindings = new ConcurrentDictionary<Type, InterfaceBinding>();
        public static IReadOnlyDictionary<Type, InterfaceBinding> Bindings => new Dictionary<Type, InterfaceBinding>(_bindings); 


        public static InterfaceBinding Bind<TInterface, TBinding>() where TBinding : TInterface
        {
            InterfaceBinding createdBinding = new InterfaceBinding(typeof(TInterface),typeof(TBinding));
            if (!_bindings.TryAdd(typeof(TInterface), createdBinding))
            {
                throw new Exception($"A binding for {typeof(TInterface).FullName} has already been set to {typeof(TBinding).FullName}");
            }
            return createdBinding;
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

        public class InterfaceBinding
        {
            private Func<object> _bindingFunction;
            private readonly Type _resolveType;
            private readonly Type _interfaceType;
            private Dictionary<string, Func<object>> _constructorArguments = new Dictionary<string, Func<object>>();

            private bool _singletonScope;

            private object _singletonObject;

            public InterfaceBinding(Type interfaceType, Type resolveType)
            {
                _interfaceType = interfaceType;
                _resolveType = resolveType;
            }

            public object ResolveBinding<T>()
            {
                if (!_singletonScope)
                {
                    Console.WriteLine($"Newing up a [{typeof(T).Name}]");
                    return _bindingFunction.Invoke();
                }
                if (_singletonObject == null)
                {
                    _singletonObject = _bindingFunction.Invoke();

                }
                Console.WriteLine($"Fetching Singleton of [{typeof(T).Name}] - Hash({_singletonObject.GetHashCode()})");
                return _singletonObject;
            }

            public InterfaceBinding InSingletonScope(bool singleton = true)
            {
                _singletonScope = singleton;
                return this;
            }
            public InterfaceBinding WithConstructorArguments(Dictionary<string, object> constructorArguments)
            {
                foreach (var constructorArgument in constructorArguments)
                {
                    _constructorArguments.Add(constructorArgument.Key, () => constructorArgument.Value);
                }
                return this;
            }
            public InterfaceBinding WithConstructorArguments(string name, object argument)
            {
                _constructorArguments.Add(name, () => argument);
                return this;
            }

            public void BuildBinding()
            {
                Console.WriteLine($"Building binding [{_interfaceType.Name}] to [{_resolveType.Name}]");
                //get constructor with most parameters
                var constructor = _resolveType.GetConstructors().OrderByDescending(a => a.GetParameters().Length).First();

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
                                $"Unable to set binding for [{_resolveType.FullName}] to {_interfaceType.FullName} because parameter number {i + 1} of type {parameters[i].ParameterType} has not been bound yet.");
                        }
                        args.Add(binding);

                    }
                    else
                    {
                        var resolvedParameter = GetConstructorParameter(parameters[i]);
                        if (resolvedParameter == null)
                        {
                            throw new ArgumentNullException(
                                $"Could resolve parameter: {parameters[i].Name} for type: {_resolveType.FullName} when trying to bind: {_interfaceType.FullName}");
                        }
                        args.Add(resolvedParameter);
                    }
                }
                _bindingFunction = () => constructor.Invoke(args.ToArray());
            }

            private object GetConstructorParameter(ParameterInfo parameterInfo)
            {
                Func<object> providedObjectOut;
                if (_constructorArguments.TryGetValue(parameterInfo.Name, out providedObjectOut))
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
