using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ninject;
using Xunit;

namespace AspNetCore.DependencyInjection.Ninject.Compat.Tests
{
    public class LoggingSupportTests
    {
        [Fact]
        public void ShouldGetObjectThatRequiresLogging()
        {
            IKernel kernel = new StandardKernel();

            kernel.AddServices(services =>
            {
                // Below addition to the services is to resolve an issue that is caused by the differences
                // of Ninject and MS.Extensions.DI 
                // Ninject will take the constructor with the most params, whether it can resolve these or not
                // MS DI will take the constructor with the most params it can resolve.
                services.AddSingleton<ILoggerFactory>(ctx =>
                {
                    var providers = ctx.GetServices<ILoggerProvider>();
                    var filterOptions = ctx.GetService<IOptionsMonitor<LoggerFilterOptions>>();
                    return new LoggerFactory(providers, filterOptions);
                });

                services.AddLogging(builder =>
                {
                    builder.AddDebug();
                });
                services.AddTransient<IFoo, LogFoo>();
            });
            kernel.Bind<IExternalScopeProvider>().ToConstant((IExternalScopeProvider)null);

            Assert.Equal(Foo.MagicString, kernel.Get<IFoo>().Id);
        }
    }

    public class LogFoo : IFoo
    {
        public LogFoo(ILoggerFactory loggerFactory)
        {
            loggerFactory.CreateLogger<LogFoo>().LogInformation("This worked {Id}", Id);
        }

        public string Id => Foo.MagicString;
    }
}
