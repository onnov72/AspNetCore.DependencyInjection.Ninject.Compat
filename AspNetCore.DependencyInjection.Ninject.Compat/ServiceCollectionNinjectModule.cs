using Microsoft.Extensions.DependencyInjection;
using Ninject.Syntax;
using Ninject.Web.Common;
using System;
using Ninject;
using Ninject.Modules;

namespace AspNetCore.DependencyInjection.Ninject.Compat
{
    public class ServiceCollectionNinjectModule : NinjectModule
    {
        private readonly IServiceCollection _serviceCollection;

        public ServiceCollectionNinjectModule(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }

        public override void Load()
        {
            Bind<IServiceProvider>().To<NinjectServiceProvider>().InSingletonScope();
            foreach (ServiceDescriptor serviceDescriptor in _serviceCollection)
            {
                IBindingToSyntax<object> binding = Bind(serviceDescriptor.ServiceType);
                IBindingWhenInNamedWithOrOnSyntax<object> binded;
                if (serviceDescriptor.ImplementationInstance != null)
                {
                    binded = binding.ToConstant(serviceDescriptor.ImplementationInstance);
                }
                else
                {
                    if (serviceDescriptor.ImplementationType != null)
                    {
                        binded = binding.To(serviceDescriptor.ImplementationType);
                    }
                    else
                    {
                        binded = binding.ToMethod(ctx => serviceDescriptor.ImplementationFactory(new NinjectServiceProvider(ctx.Kernel)));
                    }
                }
                switch (serviceDescriptor.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        binded.InSingletonScope();
                        break;
                    case ServiceLifetime.Scoped:
                        binded.InRequestScope();
                        break;
                    case ServiceLifetime.Transient:
                        binded.InTransientScope();
                        break;
                    default:
                        break;
                }
            }
        }

        private class NinjectServiceProvider : IServiceProvider
        {
            private readonly IKernel _kernel;

            public NinjectServiceProvider(IKernel kernel)
            {
                _kernel = kernel;
            }

            public object GetService(Type serviceType)
            {
                return _kernel.Get(serviceType);
            }
        }
    }
}