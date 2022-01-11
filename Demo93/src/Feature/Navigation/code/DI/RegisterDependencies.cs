using Demo93.Feature.Navigation.Controllers;
using Demo93.Feature.Navigation.Repository;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Demo93.Feature.Navigation.DI
{
    public class RegisterDependencies : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<INavigationRepository, NavigationRepository>();
            serviceCollection.AddTransient<NavigationController>();
        }
    }
}