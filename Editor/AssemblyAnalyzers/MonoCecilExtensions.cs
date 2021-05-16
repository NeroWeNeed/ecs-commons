using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UnityEngine;

namespace NeroWeNeed.Commons.AssemblyAnalyzers.Editor
{
    public static class MonoCecilExtensions
    {
        public static bool HasAttribute(this ICustomAttributeProvider attributeProvider, Type tAttribute)
        {
            var name = tAttribute.FullName;
            return attributeProvider.HasCustomAttributes && attributeProvider.CustomAttributes.Any(attr => attr.AttributeType.FullName == name);
        }
        public static bool HasAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider) where TAttribute : Attribute
        {
            var name = typeof(TAttribute).FullName;
            return attributeProvider.HasCustomAttributes && attributeProvider.CustomAttributes.Any(attr => attr.AttributeType.FullName == name);
        }
        public static string AssemblyQualifiedName(this TypeDefinition definition)
        {
            return definition.FullName + ", " + definition.Module.Assembly.FullName;
        }
        public static string AssemblyQualifiedName(this TypeReference reference, bool withGenerics = false)
        {
            return ((withGenerics && reference.IsGenericInstance) ? reference.FullName : (string.IsNullOrWhiteSpace(reference.Namespace) ? reference.Name : reference.Namespace + '.' + reference.Name)) + ", " + reference.Resolve().Module.Assembly.FullName;
        }
        public static CustomAttribute GetAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider) where TAttribute : Attribute
        {
            var name = typeof(TAttribute).FullName;
            return attributeProvider.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.FullName == name);
        }
        public static TProperty GetProperty<TProperty>(this CustomAttribute attribute, string name, TProperty defaultValue)
        {
            foreach (var property in attribute.Properties)
            {
                if (property.Name == name)
                {
                    return (TProperty)property.Argument.Value;
                }
            }
            return defaultValue;
        }
        public static TArgument GetArgument<TArgument>(this CustomAttribute attribute, int index)
        {
            return (TArgument)attribute.ConstructorArguments[index].Value;
        }
        public static TArgument[] GetArgumentArray<TArgument>(this CustomAttribute attribute, int index)
        {
            return Array.ConvertAll((CustomAttributeArgument[])attribute.ConstructorArguments[index].Value, a => (TArgument)a.Value);
        }

        public static CustomAttribute[] GetAttributes<TAttribute>(this ICustomAttributeProvider attributeProvider) where TAttribute : Attribute
        {
            var name = typeof(TAttribute).FullName;
            return attributeProvider.CustomAttributes.Where(attr => attr.AttributeType.FullName == name).ToArray();
        }
        public static bool HasInterface<TInterface>(this TypeDefinition definition)
        {
            var name = typeof(TInterface).FullName;
            return definition.HasInterfaces && definition.Interfaces.Any(i => i.InterfaceType.FullName == name);
        }
        public static bool HasInterface(this TypeDefinition definition, TypeDefinition @interface)
        {
            var name = @interface.FullName;
            return definition.HasInterfaces && definition.Interfaces.Any(i => i.InterfaceType.FullName == name);
        }
        public static bool HasInterface(this TypeDefinition definition, Type @interface)
        {
            var name = @interface.FullName;
            return definition.HasInterfaces && definition.Interfaces.Any(i => i.InterfaceType.FullName == name);
        }
        public static MethodReference MakeHostInstanceGeneric(
                                          this MethodReference self,
                                          params TypeReference[] args)
        {
            var reference = new MethodReference(
                self.Name,
                self.ReturnType,
                self.DeclaringType.MakeGenericInstanceType(args))
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

            foreach (var parameter in self.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            foreach (var genericParam in self.GenericParameters)
            {
                reference.GenericParameters.Add(new GenericParameter(genericParam.Name, reference));
            }

            return reference;
        }
        public static bool IsUnmanaged(this TypeDefinition definition)
        {
            if (definition.IsPrimitive || definition.IsEnum)
            {
                return true;
            }
            else if (definition.IsValueType)
            {
                if (!definition.HasFields)
                {
                    return true;
                }
                var definitions = new List<TypeDefinition>() { definition };
                return definition.Fields.All(field =>
                {
                    try
                    {
                        var def = field.FieldType.Resolve();
                        return def.IsUnmanaged(definitions);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                });
            }
            else
            {
                return false;
            }

        }
        private static bool IsUnmanaged(this TypeDefinition definition, List<TypeDefinition> definitions)
        {
            if (definitions.Contains(definition))
            {
                return true;
            }
            else if (definition.IsPrimitive || definition.IsEnum)
            {
                definitions.Add(definition);
                return true;
            }
            else if (definition.IsValueType)
            {
                definitions.Add(definition);
                if (!definition.HasFields)
                {
                    return true;
                }
                return definition.Fields.All(field =>
                {
                    try
                    {
                        var def = field.FieldType.Resolve();
                        return def.IsUnmanaged(definitions);
                    }
                    catch (Exception)
                    {
                        return false;
                    }


                });
            }
            else
            {
                return false;
            }

        }

        public static bool IsTypeOf(this TypeReference self, string @namespace, string name)
        {
            if (self.Name == name)
            {
                return self.Namespace == @namespace;
            }
            return false;
        }
        public static bool IsTypeOf(this TypeReference self, Type other) => self.FullName == other.FullName;
        public static SerializableMethod ToSerializableMethod(this MethodReference self)
        {
            return new SerializableMethod
            {
                container = self.DeclaringType.ToSerializableType(),
                name = self.Name
            };
        }
        public static SerializableMethod ToSerializableMethod(this MethodDefinition self)
        {
            return new SerializableMethod
            {
                container = self.DeclaringType.ToSerializableType(),
                name = self.Name
            };
        }
        public static SerializableType ToSerializableType(this TypeReference self)
        {
            if (self.IsGenericInstance)
            {
                var genericInstanceSelf = (GenericInstanceType)self;
                var generics = new List<string>();
                foreach (var genericTypeReference in genericInstanceSelf.GenericArguments)
                {
                    ToSerializableType(genericTypeReference, generics);
                }
                return new SerializableType(self.AssemblyQualifiedName(), generics.ToArray());
            }
            else
            {
                return new SerializableType(self.AssemblyQualifiedName());
            }
        }
        private static void ToSerializableType(TypeReference current, List<string> generics)
        {
            generics.Add(current.AssemblyQualifiedName());
            if (current.IsGenericInstance)
            {
                var genericCurrent = (GenericInstanceType)current;
                foreach (var genericTypeReference in genericCurrent.GenericArguments)
                {
                    ToSerializableType(genericTypeReference, generics);
                }

            }


        }
    }
}