using Microsoft.Extensions.DependencyInjection;
using Ninject;
using Xunit;

namespace AspNetCore.DependencyInjection.Ninject.Compat.Tests
{
    public class KernelServiceCollectionExtensionsTests
    {
        [Fact]
        public void KernelShouldReturnFooWhenAskedForIFoo()
        {
            IKernel kernel = new StandardKernel();

            kernel.AddServices(services => services.AddTransient<IFoo, Foo>());

            Assert.Equal(Foo.MagicString, kernel.Get<IFoo>().Id);
        }

        [Fact]
        public void KernelShouldReturnDifferentInstancesWhenAskedTwiceForTransientScopedServices()
        {
            IKernel kernel = new StandardKernel();

            kernel.AddServices(services => services.AddTransient<IFoo, Foo>());

            IFoo number1 = kernel.Get<IFoo>();
            IFoo number2 = kernel.Get<IFoo>();

            Assert.NotEqual(number1, number2);
        }

        [Fact]
        public void KernelShouldReturnTheSameInstanceWhenAskedTwiceForSingletonScopedServices()
        {
            IKernel kernel = new StandardKernel();

            kernel.AddServices(services => services.AddSingleton<IFoo, Foo>());

            IFoo number1 = kernel.Get<IFoo>();
            IFoo number2 = kernel.Get<IFoo>();

            Assert.Equal(number1, number2);
        }

        [Fact]
        public void KernelShouldInjectInnerServices()
        {
            IKernel kernel = new StandardKernel();

            kernel.AddServices(services =>
            {
                services.AddSingleton<IFoo, Foo>();
                services.AddSingleton<IBar, Bar>();
            });

            Assert.Equal(Foo.MagicString, kernel.Get<IBar>().MyFoo.Id);
        }

        [Fact]
        public void KernelShouldInjectInnerServicesOneWay()
        {
            IKernel kernel = new StandardKernel();
            kernel.Bind<IFoo>().To<Foo>();

            kernel.AddServices(services =>
            {
                services.AddSingleton<IBar, Bar>();
            });

            Assert.Equal(Foo.MagicString, kernel.Get<IBar>().MyFoo.Id);
        }

        [Fact]
        public void KernelShouldInjectInnerServicesOrAnother()
        {
            IKernel kernel = new StandardKernel();
            kernel.Bind<IBar>().To<Bar>();

            kernel.AddServices(services =>
            {
                services.AddSingleton<IFoo, Foo>();
            });

            Assert.Equal(Foo.MagicString, kernel.Get<IBar>().MyFoo.Id);
        }

        [Fact]
        public void KernelShouldInjectInnerServicesToMethodsToo()
        {
            IKernel kernel = new StandardKernel();

            kernel.AddServices(services =>
            {
                services.AddTransient<IFoo>(serviceProvider => new Foo());
                services.AddTransient<IBar>(serviceProvider => new Bar(serviceProvider.GetService<IFoo>()));
            });

            Assert.Equal(Foo.MagicString, kernel.Get<IBar>().MyFoo.Id);
        }

        [Fact]
        public void KernelShouldUseConstantsToo()
        {
            IKernel kernel = new StandardKernel();

            kernel.AddServices(services =>
            {
                services.Add(new ServiceDescriptor(typeof(int), 3));
            });

            Assert.Equal(3, kernel.Get<int>());
        }
    }

    internal interface IFoo
    {
        string Id { get; }
    }

    internal class Foo : IFoo
    {
        public const string MagicString = "I am Foo";
        public string Id { get; } = MagicString;
    }

    internal interface IBar
    {
        IFoo MyFoo { get; }
    }

    internal class Bar : IBar
    {
        public Bar(IFoo foo)
        {
            MyFoo = foo;
        }

        public IFoo MyFoo { get; }
    }
}
