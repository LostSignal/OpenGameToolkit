namespace OGT.Properties
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "New Enum Value", menuName = "OGT/Properties/Enum Value")]
    public class EnumValue : Resource
    {
        [SerializeField] private string displayName;

        public string Name => this.name;

        public string DisplayName => this.displayName;
    }
}
