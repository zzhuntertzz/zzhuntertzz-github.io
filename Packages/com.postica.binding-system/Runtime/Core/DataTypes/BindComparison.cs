using System;
using System.ComponentModel;
using UnityEngine;

namespace Postica.BindingSystem
{
    [Serializable]
    public struct BindComparison<T> where T : IComparable<T>
    {
        public enum ComparisonType
        {
            [InspectorName("=")]
            Equals,
            [InspectorName("\u2260")]
            NotEquals,
            [InspectorName(">")]
            GreaterThan,
            [InspectorName("\u2265")]
            GreaterThanOrEquals,
            [InspectorName("<")]
            LessThan,
            [InspectorName("\u2264")]
            LessThanOrEquals
        }

        public ReadOnlyBind<T> value;
        public ComparisonType comparisonType;

        public bool Compare(T other)
        {
            return comparisonType switch
            {
                ComparisonType.Equals => value.Value.Equals(other),
                ComparisonType.NotEquals => !value.Value.Equals(other),
                ComparisonType.GreaterThan => value.Value.CompareTo(other) > 0,
                ComparisonType.GreaterThanOrEquals => value.Value.CompareTo(other) >= 0,
                ComparisonType.LessThan => value.Value.CompareTo(other) < 0,
                ComparisonType.LessThanOrEquals => value.Value.CompareTo(other) <= 0,
                _ => throw new InvalidEnumArgumentException()
            };
        }
        
        public static implicit operator BindComparison<T>(T value)
        {
            return new BindComparison<T>
            {
                value = value.Bind(),
                comparisonType = ComparisonType.Equals
            };
        }
        
        public static implicit operator BindComparison<T>(ReadOnlyBind<T> value)
        {
            return new BindComparison<T>
            {
                value = value,
                comparisonType = ComparisonType.Equals
            };
        }
        
        public static implicit operator BindComparison<T>(ComparisonType comparisonType)
        {
            return new BindComparison<T>
            {
                value = default,
                comparisonType = comparisonType
            };
        }
        
        public static implicit operator BindComparison<T>((ReadOnlyBind<T>, ComparisonType) value)
        {
            return new BindComparison<T>
            {
                value = value.Item1,
                comparisonType = value.Item2
            };
        }
        
        public static implicit operator BindComparison<T>((T, ComparisonType) value)
        {
            return new BindComparison<T>
            {
                value = value.Item1.Bind(),
                comparisonType = value.Item2
            };
        }
        
        public static implicit operator T(BindComparison<T> value)
        {
            return value.value.Value;
        }
    }
}
