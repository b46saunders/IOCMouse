using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace IOC
{

    public class InterfaceBinding
    {
        private readonly Func<object> _bindingFunction;
        private Dictionary<string,IObjectResolver> _constructorArguments = new Dictionary<string, IObjectResolver>();

        private bool _singletonScope;

        private object _singletonObject;

        public InterfaceBinding()
        {
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

        public InterfaceBinding WithConstructorArguments(Dictionary<string,object> constructorArguments)
        {
            
            return this;
        }

        public InterfaceBinding WithConstructorArguments(string name,object argument)
        {

            return this;
        }

        public void c()
        {
            Console.WriteLine($"Binding [{typeof(TInterface).Name}] to [{typeof(TBinding).Name}]");
            //only handle 1 constructor
            var constructor = typeof(TBinding).GetConstructors().First();

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
                            $"Unable to set binding for [{typeof(TInterface).FullName}] to {typeof(TBinding).FullName} because parameter number {i + 1} of type {parameters[i].ParameterType} has not been bound yet.");
                    }
                    args.Add(binding);

                }
                else
                {
                    throw new Exception("BARFF - non interface parameters cannot be resolved yet :(");
                }

            }
            Func<object> func = () => constructor.Invoke(args.ToArray());
            var interfaceBinding = new InterfaceBinding(func);


            _bindings[typeof(TInterface).GetHashCode()] = interfaceBinding;
        }
    }
    public static class Injector
    {
        private static ConcurrentDictionary<int, InterfaceBinding> _bindings = new ConcurrentDictionary<int, InterfaceBinding>();
        public static IReadOnlyDictionary<int, InterfaceBinding> Bindings => new Dictionary<int, InterfaceBinding>(_bindings); 


        public static InterfaceBinding Bind<TInterface, TBinding>() where TBinding : TInterface
        {
            InterfaceBinding createdBinding = new InterfaceBinding();
            if (!_bindings.TryAdd(typeof(TInterface).GetHashCode(), createdBinding))
            {
                throw new Exception($"A binding for {typeof(TInterface).FullName} has already been set to {typeof(TBinding).FullName}");
            }



            return createdBinding;
        }


        public static T Resolve<T>()
        {
            if(!_bindings.Any()) throw new Exception("No bindings have been setup");

            InterfaceBinding binding;
            if (_bindings.TryGetValue(typeof(T).GetHashCode(), out binding))
            {
                return (T)binding.ResolveBinding<T>();
            }
            throw new Exception($"Failed to resolve type: {typeof(T).FullName}");
        }
    }
}
