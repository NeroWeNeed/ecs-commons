namespace NeroWeNeed.Commons.Editor {
    public interface IInitializable {
        void OnInit();
    }
    
    
    public interface ISerializationCallback {
        void OnSerialize();
    }
    public interface IDeserializationCallback {
        void OnDeserialize();
    }
}