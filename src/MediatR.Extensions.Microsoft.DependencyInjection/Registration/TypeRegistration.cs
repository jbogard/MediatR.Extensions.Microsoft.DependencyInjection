using System;

namespace MediatR.Registration
{
    public class TypeRegistration
    {
        public TypeRegistration(Type interfaceType, bool singleImplementation = false)
        {
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException("Only interface types are allowed");
            }

            InterfaceType = interfaceType;
            SingleImplementation = singleImplementation;
        }

        public Type InterfaceType { get; }
        public bool SingleImplementation { get; }

        protected bool Equals(TypeRegistration other)
        {
            return InterfaceType == other.InterfaceType;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TypeRegistration) obj);
        }

        public override int GetHashCode()
        {
            return InterfaceType.GetHashCode();
        }

        public static bool operator ==(TypeRegistration? left, TypeRegistration? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TypeRegistration? left, TypeRegistration? right)
        {
            return !Equals(left, right);
        }
    }
}