using AspNetCore.DependencyInjection.Ninject.Compat;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ninject
{
    public static class KernelServiceCollectionExtensions
    {
        public static void AddServices(this IKernel ninjectKernel, Action<IServiceCollection> registerServices)
        {
            if (ninjectKernel == null)
            {
                throw new ArgumentNullException(nameof(ninjectKernel));
            }

            if (registerServices == null)
            {
                throw new ArgumentNullException(nameof(registerServices));
            }

            ServiceCollection serviceCollection = new ServiceCollection();
            registerServices(serviceCollection);

            ninjectKernel.Load(new ServiceCollectionNinjectModule(serviceCollection));
        }
    }
}
