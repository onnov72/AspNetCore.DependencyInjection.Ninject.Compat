using Microsoft.Extensions.DependencyInjection;
using Ninject.Syntax;
using Ninject.Web.Common;
using System;
using Ninject;
using Ninject.Modules;
using Ninject.Activation.Blocks;

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
            Bind<IServiceScopeFactory>().To<NinjectServiceScopeFactory>().InSingletonScope();
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

        private class NinjectServiceScopeFactory : IServiceScopeFactory
        {
            private readonly IKernel _kernel;

            public NinjectServiceScopeFactory(IKernel kernel)
            {
                _kernel = kernel;
            }

            public IServiceScope CreateScope() => new NinjectServiceScope(_kernel);

            private sealed class NinjectServiceScope : IServiceScope
            {
                private readonly IActivationBlock _activationBlock;

                public NinjectServiceScope(IKernel kernel)
                {
                    _activationBlock = kernel.BeginBlock();
                    ServiceProvider = new NinjectBlockServiceProvider(_activationBlock);
                }

                public IServiceProvider ServiceProvider { get; }

                public void Dispose() => _activationBlock.Dispose();

                private class NinjectBlockServiceProvider : IServiceProvider
                {
                    private IActivationBlock _activationBlock;

                    public NinjectBlockServiceProvider(IActivationBlock activationBlock)
                    {
                        _activationBlock = activationBlock;
                    }

                    public object GetService(Type serviceType) => _activationBlock.Get(serviceType);
                }
            }
        }
    }
}
