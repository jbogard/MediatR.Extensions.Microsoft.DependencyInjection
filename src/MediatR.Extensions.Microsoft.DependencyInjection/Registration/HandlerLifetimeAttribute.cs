using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Extensions.Microsoft.DependencyInjection.Registration
{

    [AttributeUsage(AttributeTargets.Class)]
    public class HandlerLifetimeAttribute : Attribute
    {
        public HandlerLifetimeAttribute(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }

        public ServiceLifetime Lifetime { get; }
    }
}
