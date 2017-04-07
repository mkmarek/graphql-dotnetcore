namespace GraphQLCore.Type
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class NonNullable
    {
        public static bool Equals<T>(NonNullable<T> n1, NonNullable<T> n2)
            where T : class
        {
            return EqualityComparer<T>.Default.Equals(n1.Value, n2.Value);
        }

        public static Type GetUnderlyingType(Type nonNullableType)
        {
            if (nonNullableType == null)
                throw new ArgumentNullException(nameof(nonNullableType));

            Type result = null;

            var typeInfo = nonNullableType.GetTypeInfo();
            if (typeInfo.IsGenericType && !typeInfo.IsGenericTypeDefinition)
            {
                Type genericType = nonNullableType.GetGenericTypeDefinition();

                if (object.ReferenceEquals(genericType, typeof(NonNullable<>)))
                    result = nonNullableType.GenericTypeArguments[0];
            }

            return result;
        }
    }

    public interface INonNullable
    {
        object GetValue();
    }

    public struct NonNullable<T> : INonNullable
        where T : class
    {
        private T value;

        public NonNullable(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            this.value = value;
        }

        public T Value
        {
            get
            {
                if (this.value == null)
                    throw new NullReferenceException();

                return this.value;
            }
        }

        public object GetValue()
        {
            return this.Value;
        }

        public override bool Equals(object other)
        {
            if (other.GetType() == typeof(NonNullable<T>))
                return this.Value.Equals(((NonNullable<T>)other).Value);

            return this.Value.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        public static implicit operator NonNullable<T>(T value)
        {
            return new NonNullable<T>(value);
        }

        public static implicit operator T(NonNullable<T> value)
        {
            return value.Value;
        }
    }
}
