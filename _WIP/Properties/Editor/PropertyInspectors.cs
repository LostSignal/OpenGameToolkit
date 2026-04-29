namespace OGT.Properties
{
    using Unity.VisualScripting;
    using UnityEditor;
    using UnityEngine;

    public abstract class PropertyInspector<T> : Inspector where T : Property, new()
    {
        public PropertyInspector(Metadata metadata) : base(metadata) {}

        protected override float GetHeight(float width, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 8;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            var property = (Property)metadata.value;

            if (property == null)
            {
                property = new T();
                metadata.value = property;
            }

            var changed = new EditorGUI.ChangeCheckScope();

            using (changed)
            {
                PropertyPropertyDrawer.Draw(property, position);
            }

            if (changed.changed)
            {
                metadata.value = property;
            }
        }
    }


    [Inspector(typeof(BoolProperty))]
    public class BoolPropertyInspector : PropertyInspector<BoolProperty>
    {
        public BoolPropertyInspector(Metadata metadata) : base(metadata) {}
    }

    [Inspector(typeof(IntProperty))]
    public class IntPropertyInspector : PropertyInspector<IntProperty>
    {
        public IntPropertyInspector(Metadata metadata) : base(metadata) {}
    }

    [Inspector(typeof(FloatProperty))]
    public class FloatPropertyInspector : PropertyInspector<FloatProperty>
    {
        public FloatPropertyInspector(Metadata metadata) : base(metadata) {}
    }

    [Inspector(typeof(StringProperty))]
    public class StringPropertyInspector : PropertyInspector<StringProperty>
    {
        public StringPropertyInspector(Metadata metadata) : base(metadata) {}
    }

    [Inspector(typeof(EnumProperty))]
    public class EnumPropertyInspector : PropertyInspector<EnumProperty>
    {
        public EnumPropertyInspector(Metadata metadata) : base(metadata) {}
    }
}
