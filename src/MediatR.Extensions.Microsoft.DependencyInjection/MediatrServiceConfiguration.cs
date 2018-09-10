namespace MediatR
{
    using System;

    public class MediatrServiceConfiguration
    {
        public Type MediatorImpl { get; private set; }
        public Lifetime Lifetime { get; private set; }

        public MediatrServiceConfiguration()
        {
            MediatorImpl = typeof(Mediator);
            Lifetime = Lifetime.Scopped;
        }

        public MediatrServiceConfiguration Using<TMediator>() where TMediator : IMediator
        {
            MediatorImpl = typeof(TMediator);
            return this;
        }

        public MediatrServiceConfiguration AsSingleton()
        {
            Lifetime = Lifetime.Singleton;
            return this;
        }

        public MediatrServiceConfiguration AsScopped()
        {
            Lifetime = Lifetime.Scopped;
            return this;
        }

        public MediatrServiceConfiguration AsTransient()
        {
            Lifetime = Lifetime.Transient;
            return this;
        }
    }

    public enum Lifetime
    {
        Singleton,
        Scopped,
        Transient
    }
}