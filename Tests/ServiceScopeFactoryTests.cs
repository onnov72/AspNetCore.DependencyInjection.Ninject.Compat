using Microsoft.Extensions.DependencyInjection;
using Ninject;
using Shouldly;
using System;
using Xunit;

namespace AspNetCore.DependencyInjection.Ninject.Compat.Tests
{
    public class ServiceScopeFactoryTests
    {
        [Fact]
        public void Scoped_Objects_Created_In_Different_Scopes_Should_Differ()
        {
            var kernel = new StandardKernel();
            kernel.AddServices(services =>
            {
                services.AddScoped<ScopedFoo>();
            });
            var scopeFactory = kernel.Get<IServiceScopeFactory>();
            var scopedFoo1 = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ScopedFoo>();
            var scopedFoo2 = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ScopedFoo>();

            scopedFoo1.ShouldNotBeSameAs(scopedFoo2);
        }

        [Fact]
        public void Scoped_Objects_Created_In_The_Same_Scopes_Should_Be_The_Same()
        {
            var kernel = new StandardKernel();
            kernel.AddServices(services =>
            {
                services.AddScoped<ScopedFoo>();
            });
            var scopeFactory = kernel.Get<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();
            var scopedFoor1 = scope.ServiceProvider.GetRequiredService<ScopedFoo>();
            var scopedFoo2 = scope.ServiceProvider.GetRequiredService<ScopedFoo>();

            scopedFoor1.ShouldBeSameAs(scopedFoo2);
        }

        [Fact]
        public void Scoped_Objects_Should_Be_Disposed_Together_With_The_Scope()
        {
            var kernel = new StandardKernel();
            kernel.AddServices(services =>
            {
                services.AddScoped<ScopedFoo>();
            });
            var scopeFactory = kernel.Get<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();
            var scopedFoo = scope.ServiceProvider.GetRequiredService<ScopedFoo>();

            scopedFoo.IsDisposed.ShouldBeFalse();
            scope.Dispose();
            scopedFoo.IsDisposed.ShouldBeTrue();
        }
    }

    internal class ScopedFoo : IDisposable
    {
        public void Dispose() => IsDisposed = true;

        public bool IsDisposed { get; set; } = false;
    }
}
