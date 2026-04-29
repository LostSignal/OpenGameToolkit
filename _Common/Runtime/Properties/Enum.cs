namespace OGT.Properties
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Enum", menuName = "Scriptable Objects/Enum")]
    public class Enum : ScriptableObject
    {
        [SerializeField]
        private List<EnumValue> enumValues;

        public IReadOnlyList<EnumValue> EnumValues => this.enumValues;

        public int IndexOf(EnumValue enumValue)
        {
            return this.enumValues.IndexOf(enumValue);
        }

#if UNITY_EDITOR
        [InspectorButton]
        private void AddEnumValue()
        {
            if (UnityEditor.VersionControl.Provider.isActive)
            {
                UnityEditor.VersionControl.Provider.Checkout(this, UnityEditor.VersionControl.CheckoutMode.Asset);
            }

            var newEnumValue = new EnumValue() { name = "New Enum Value" };
            UnityEditor.AssetDatabase.AddObjectToAsset(newEnumValue, this);
            this.enumValues.Add(newEnumValue);

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(this));
            UnityEditor.AssetDatabase.Refresh();
        }

#endif
    }
}
