using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor {
    public sealed class FieldUpdateEvent : EventBase<FieldUpdateEvent> { 
        public string path;
        public object value;

        public static FieldUpdateEvent GetPooled(string path,object newValue,IEventHandler target) {
            var evt = EventBase<FieldUpdateEvent>.GetPooled();
            evt.path = path;
            evt.bubbles = true;
            evt.target = target;

            evt.value = newValue;
            return evt;
        }
    }
}