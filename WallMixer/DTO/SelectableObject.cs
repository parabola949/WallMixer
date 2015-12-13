namespace WallMixer.DTO
{
    public class SelectableObject <T>
    {
        public bool IsSelected { get; set; }
        public T ObjectData { get; set; }

        public SelectableObject(T objectData)
        {
            ObjectData = objectData;
        }

        public SelectableObject(T ObjectDataProvider, bool isSelected)
        {
            IsSelected = isSelected;
            ObjectData = ObjectData;
        }
    }
}
