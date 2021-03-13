using System;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor {

    public abstract class BaseTypeFieldProvider : TypeFieldProvider {
        protected delegate bool Converter<TElementValue, TValue>(TElementValue value, out TValue result);
        protected BindableElement CreateField<TElement, TElementValue, TValue>(TElement element, TElementValue initial, Converter<TElementValue, TValue> converter) where TElement : BindableElement, INotifyValueChanged<TElementValue> {

            element.RegisterValueChangedCallback(evt =>
            {
                Debug.Log("e");
                var self = evt.target as BindableElement;
                if (converter.Invoke(evt.newValue, out TValue result)) {
                    using var updateEvent = FieldUpdateEvent.GetPooled(self.bindingPath, result, self); self.SendEvent(updateEvent);
                }
                else {
                    (evt.target as INotifyValueChanged<TElementValue>)?.SetValueWithoutNotify(evt.previousValue);
                }
            });
            element.SetValueWithoutNotify(initial);
            return element;
        }
        protected BindableElement CreateField<TElement, TValue>(TValue initial) where TElement : BindableElement, INotifyValueChanged<TValue>, new() {
            var element = new TElement();
            element.RegisterValueChangedCallback(evt =>
            {
                var self = evt.target as BindableElement;
                using var updateEvent = FieldUpdateEvent.GetPooled(self.bindingPath, evt.newValue, self); self.SendEvent(updateEvent);
            });
            element.SetValueWithoutNotify(initial);
            return element;
        }
    }
    [TypeFieldProviderRule(-100)]
    public class DefaultProviderRule : TypeFieldProviderRule {
        public override bool CreateField(TypeFieldSchema schema, Type type, FieldInfo fieldInfo, object initialValue, out BindableElement element) {
            var provider = schema.GetProvider(type);
            if (provider == null) {
                element = null;
                return false;
            }
            else {
                element = provider.CreateField(type, fieldInfo, initialValue);
                return true;
            }
        }
    }
    /*     public class FixedBufferTypeFieldProviderRule : TypeFieldProviderRule {
            public override bool CreateField(TypeFieldSchema schema, Type type, FieldInfo fieldInfo, object initialValue, out BindableElement element) {
                var attr = 
            }
        } */
    [TypeFieldProviderRule(100)]
    public class EnumTypeFieldProviderRule : TypeFieldProviderRule {
        private BaseField<Enum> CreateField(Type type, FieldInfo fieldInfo, Enum initialValue) {
            BaseField<Enum> element;
            if (type.GetCustomAttribute<FlagsAttribute>() != null) {
                element = new EnumFlagsField(initialValue);
            }
            else {
                element = new EnumField(initialValue);
            }
            element.RegisterValueChangedCallback(evt =>
            {

                var self = evt.target as BindableElement;
                using var updateEvent = FieldUpdateEvent.GetPooled(self.bindingPath, evt.newValue, self); self.SendEvent(updateEvent);
            });
            element.SetValueWithoutNotify(initialValue);
            return element;
        }


        public override bool CreateField(TypeFieldSchema schema, Type type, FieldInfo fieldInfo, object initialValue, out BindableElement element) {
            element = null;
            if (type.IsEnum) {
                element = CreateField(type, fieldInfo, (Enum)initialValue);
                return true;
            }
            else {
                return false;
            }
        }
    }
    [TypeFieldProvider(typeof(sbyte))]
    public class SignedByteTypeFieldProvider : BaseTypeFieldProvider {
        public override BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue) => CreateField(new TextField { isDelayed = true }, ((sbyte)initialValue).ToString(), (string text, out sbyte result) => sbyte.TryParse(text, out result));
    }
    [TypeFieldProvider(typeof(short))]
    public class ShortTypeFieldProvider : BaseTypeFieldProvider {
        public override BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue) => CreateField(new TextField { isDelayed = true }, ((short)initialValue).ToString(), (string text, out short result) => short.TryParse(text, out result));
    }
    [TypeFieldProvider(typeof(int))]
    public class IntTypeFieldProvider : BaseTypeFieldProvider {
        public override BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue) => CreateField(new TextField { isDelayed = true }, ((int)initialValue).ToString(), (string text, out int result) => int.TryParse(text, out result));
    }
    [TypeFieldProvider(typeof(long))]
    public class LongTypeFieldProvider : BaseTypeFieldProvider {
        public override BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue) => CreateField(new TextField { isDelayed = true }, ((long)initialValue).ToString(), (string text, out long result) => long.TryParse(text, out result));
    }
    [TypeFieldProvider(typeof(byte))]
    public class ByteTypeFieldProvider : BaseTypeFieldProvider {
        public override BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue) => CreateField(new TextField { isDelayed = true }, ((byte)initialValue).ToString(), (string text, out byte result) => byte.TryParse(text, out result));
    }
    [TypeFieldProvider(typeof(ushort))]
    public class UnsignedShortTypeFieldProvider : BaseTypeFieldProvider {
        public override BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue) => CreateField(new TextField { isDelayed = true }, ((ushort)initialValue).ToString(), (string text, out ushort result) => ushort.TryParse(text, out result));
    }
    [TypeFieldProvider(typeof(uint))]
    public class UnsignedIntTypeFieldProvider : BaseTypeFieldProvider {
        public override BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue) => CreateField(new TextField { isDelayed = true }, ((uint)initialValue).ToString(), (string text, out uint result) => uint.TryParse(text, out result));
    }
    [TypeFieldProvider(typeof(ulong))]
    public class UnsignedLongTypeFieldProvider : BaseTypeFieldProvider {
        public override BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue) => CreateField(new TextField { isDelayed = true }, ((ulong)initialValue).ToString(), (string text, out ulong result) => ulong.TryParse(text, out result));
    }
    [TypeFieldProvider(typeof(float))]
    public class FloatTypeFieldProvider : BaseTypeFieldProvider {
        public override BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue) => CreateField(new TextField { isDelayed = true }, ((float)initialValue).ToString(), (string text, out float result) => float.TryParse(text, out result));
    }
    [TypeFieldProvider(typeof(double))]
    public class DoubleTypeFieldProvider : BaseTypeFieldProvider {
        public override BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue) => CreateField(new TextField { isDelayed = true }, ((double)initialValue).ToString(), (string text, out double result) => double.TryParse(text, out result));
    }
    [TypeFieldProvider(typeof(decimal))]
    public class DecimalTypeFieldProvider : BaseTypeFieldProvider {
        public override BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue) => CreateField(new TextField { isDelayed = true }, ((decimal)initialValue).ToString(), (string text, out decimal result) => decimal.TryParse(text, out result));
    }
    [TypeFieldProvider(typeof(bool))]
    public class BoolTypeFieldProvider : BaseTypeFieldProvider {
        public override BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue) => CreateField<Toggle, bool>((bool)initialValue);
    }
    [TypeFieldProvider(typeof(char))]
    public class CharTypeFieldProvider : BaseTypeFieldProvider {
        public override BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue) => CreateField(new TextField { isDelayed = true, maxLength = 1 }, ((char)initialValue).ToString(), (string text, out char result) => char.TryParse(text, out result));
    }

}