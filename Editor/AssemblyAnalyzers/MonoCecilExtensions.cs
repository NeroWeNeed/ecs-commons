using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NeroWeNeed.Commons.AssemblyAnalyzers.Editor {
    public static class MonoCecilExtensions {
        public static bool HasAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider) where TAttribute : Attribute {
            var name = typeof(TAttribute).FullName;
            return attributeProvider.HasCustomAttributes && attributeProvider.CustomAttributes.Any(attr => attr.AttributeType.FullName == name);
        }
        public static string AssemblyQualifiedName(this TypeDefinition definition) {
            return definition.FullName + ", " + definition.Module.Assembly.FullName;
        }
        public static CustomAttribute GetAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider) where TAttribute : Attribute {
            var name = typeof(TAttribute).FullName;
            return attributeProvider.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.FullName == name);
        }
        public static CustomAttribute[] GetAttributes<TAttribute>(this ICustomAttributeProvider attributeProvider) where TAttribute : Attribute {
            var name = typeof(TAttribute).FullName;
            return attributeProvider.CustomAttributes.Where(attr => attr.AttributeType.FullName == name).ToArray();
        }
        public static bool HasInterface<TInterface>(this TypeDefinition definition) {
            var name = typeof(TInterface).FullName;
            return definition.HasInterfaces && definition.Interfaces.Any(i => i.InterfaceType.FullName == name);

        }

        public static bool IsUnmanaged(this TypeDefinition definition) {
            if (definition.IsPrimitive || definition.IsEnum) {
                return true;
            }
            else if (definition.IsValueType) {
                if (!definition.HasFields) {
                    return true;
                }
                var definitions = new List<TypeDefinition>() { definition };
                return definition.Fields.All(field =>
                {
                    try {
                        var def = field.FieldType.Resolve();
                        return def.IsUnmanaged(definitions);
                    }
                    catch (Exception) {
                        return false;
                    }
                });
            }
            else {
                return false;
            }

        }
        private static bool IsUnmanaged(this TypeDefinition definition, List<TypeDefinition> definitions) {
            if (definitions.Contains(definition)) {
                return true;
            }
            else if (definition.IsPrimitive || definition.IsEnum) {
                definitions.Add(definition);
                return true;
            }
            else if (definition.IsValueType) {
                definitions.Add(definition);
                if (!definition.HasFields) {
                    return true;
                }
                return definition.Fields.All(field =>
                {
                    try {
                        var def = field.FieldType.Resolve();
                        return def.IsUnmanaged(definitions);
                    }
                    catch (Exception) {
                        return false;
                    }

                    
                });
            }
            else {
                return false;
            }

        }

        public static bool IsTypeOf(this TypeReference self, string @namespace, string name) {
            if (self.Name == name) {
                return self.Namespace == @namespace;
            }
            return false;
        }
        public static bool IsTypeOf(this TypeReference self, Type other) => self.FullName == other.FullName;
    }
}