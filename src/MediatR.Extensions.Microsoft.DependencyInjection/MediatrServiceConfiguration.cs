using Microsoft.Extensions.DependencyInjection;

namespace MediatR
{
    using System;

    public class MediatRServiceConfiguration
    {
        public Type MediatorImplementationType { get; private set; }
        public ServiceLifetime Lifetime { get; private set; }

        public MediatRServiceConfiguration()
        {
            MediatorImplementationType = typeof(Mediator);
            Lifetime = ServiceLifetime.Transient;
        }

        public MediatRServiceConfiguration Using<TMediator>() where TMediator : IMediator
        {
            MediatorImplementationType = typeof(TMediator);
            return this;
        }

        public MediatRServiceConfiguration AsSingleton()
        {
            Lifetime = ServiceLifetime.Singleton;
            return this;
        }

        public MediatRServiceConfiguration AsScoped()
        {
            Lifetime = ServiceLifetime.Scoped;
            return this;
        }

        public MediatRServiceConfiguration AsTransient()
        {
            Lifetime = ServiceLifetime.Transient;
            return this;
        }
    }
}