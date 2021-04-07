using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace NeroWeNeed.Commons {


    [AttributeUsage(AttributeTargets.Field)]
    public abstract class TypeFilterAttribute : Attribute, IComparable<TypeFilterAttribute> {
        public abstract Type ComparisonType { get; }
        public int CompareTo(TypeFilterAttribute other) {
            return Comparer<string>.Default.Compare(ComparisonType?.AssemblyQualifiedName, other.ComparisonType?.AssemblyQualifiedName);
        }

        public abstract bool IsValid(Type type);
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class BurstFunctionPointerDelegateTypeFilterAttribute : TypeFilterAttribute {
        private static readonly Type[] DllImportCompliantTypes = new Type[] {
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(sbyte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(IntPtr),
            typeof(UIntPtr),
            typeof(Unity.Burst.Intrinsics.v64),
            typeof(Unity.Burst.Intrinsics.v128),
            typeof(Unity.Burst.Intrinsics.v256)
        };
        public override Type ComparisonType => typeof(BurstFunctionPointerDelegateTypeFilterAttribute);

        public override bool IsValid(Type type) {
            if (type.IsSubclassOf(typeof(Delegate))) {
                var method = type.GetMethod("Invoke");
                return method.GetParameters().All(p => IsDllImportCompliant(p.ParameterType)) && (method.ReturnType == typeof(void) || IsDllImportCompliant(method.ReturnType));
            }
            return false;
        }
        private bool IsDllImportCompliant(Type type) {
            if (DllImportCompliantTypes.Any(x => type == x) || type.IsPointer || type.IsByRef) {
                return true;
            }
            else if (type.IsValueType) {
                var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                return fields.Length == 1 && (fields[0].FieldType.IsPointer || fields[0].FieldType == typeof(int));
            }
            return false;
        }
    }

    /// <summary>
    /// Only Show types that have the following type as a supertype.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class SuperTypeFilterAttribute : TypeFilterAttribute {
        private readonly Type type;
        public bool AllowSuperType { get; set; }
        public override Type ComparisonType { get => type; }

        public SuperTypeFilterAttribute(Type type) {
            this.type = type;
        }

        public override bool Equals(object obj) {
            return obj is SuperTypeFilterAttribute attribute &&
                   EqualityComparer<Type>.Default.Equals(type, attribute.type);
        }

        public override int GetHashCode() {
            int hashCode = 1064687083;
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(type);
            return hashCode;
        }

        public override bool IsValid(Type type) {
            return this.type.IsAssignableFrom(type) && (AllowSuperType || !type.Equals(this.type));

        }
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class AttributeTypeFilterAttribute : TypeFilterAttribute {

        public override Type ComparisonType { get; }

        public AttributeTypeFilterAttribute(Type type) {
            this.ComparisonType = type;
        }

        public override bool IsValid(Type type) {
            return type.GetCustomAttribute(this.ComparisonType) != null;
        }
    }

    public sealed class ParameterlessConstructorFilterAttribute : TypeFilterAttribute {

        public override Type ComparisonType { get => typeof(ParameterlessConstructorFilterAttribute); }

        public override bool IsValid(Type type) {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }
    }
    public enum MatchType : byte {
        Equals = 0,
        Regex = 1,
        StartsWith = 2,
        EndsWith = 3,
        Contains = 4
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class ExcludeTypeNameFilterAttribute : TypeFilterAttribute {
        public string name;
        public MatchType matchType;
        public override Type ComparisonType { get => typeof(ExcludeTypeNameFilterAttribute); }

        public ExcludeTypeNameFilterAttribute(string name, MatchType matchType = MatchType.Equals) {
            this.name = name;
            this.matchType = matchType;
        }

        public override bool IsValid(Type type) {
            return matchType switch
            {
                MatchType.Regex => !new Regex(name, RegexOptions.Compiled).IsMatch(type.FullName),
                MatchType.StartsWith => !type.FullName.StartsWith(name),
                MatchType.EndsWith => !type.FullName.EndsWith(name),
                MatchType.Contains => !type.FullName.Contains(name),
                _ => type.FullName != name,
            };
        }
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class ExcludeAssemblyFilterAttribute : TypeFilterAttribute {

        public string name;
        public MatchType matchType;
        public override Type ComparisonType { get => typeof(ExcludeAssemblyFilterAttribute); }

        public ExcludeAssemblyFilterAttribute(string name, MatchType matchType = MatchType.Equals) {
            this.name = name;
            this.matchType = matchType;
        }

        public override bool IsValid(Type type) {
            return matchType switch
            {
                MatchType.Regex => !new Regex(name, RegexOptions.Compiled).IsMatch(type.Assembly.FullName),
                MatchType.StartsWith => !type.Assembly.FullName.StartsWith(name),
                MatchType.EndsWith => !type.Assembly.FullName.EndsWith(name),
                MatchType.Contains => !type.Assembly.FullName.Contains(name),
                _ => type.Assembly.FullName != name,
            };
        }
    }
    public sealed class AssemblyFilterAttribute : TypeFilterAttribute {

        public string name;
        public override Type ComparisonType { get => typeof(AssemblyFilterAttribute); }

        public AssemblyFilterAttribute(string name) {
            this.name = name;
        }

        public override bool IsValid(Type type) {
            return type.Assembly.FullName == name;
        }
    }
    public sealed class UnmanagedFilterAttribute : TypeFilterAttribute {
        public override Type ComparisonType { get => typeof(UnmanagedFilterAttribute); }
        public override bool IsValid(Type type) {
            return UnsafeUtility.IsUnmanaged(type);
        }
    }

    public sealed class BlittableFilterAttribute : TypeFilterAttribute {
        public override Type ComparisonType { get => typeof(BlittableFilterAttribute); }
        public override bool IsValid(Type type) {
            return UnsafeUtility.IsBlittable(type);
        }
    }
    public sealed class ConcreteTypeFilterAttribute : TypeFilterAttribute {
        public override Type ComparisonType { get => typeof(ConcreteTypeFilterAttribute); }

        public override bool IsValid(Type type) {
            return !type.IsGenericTypeDefinition;
        }
    }

}
