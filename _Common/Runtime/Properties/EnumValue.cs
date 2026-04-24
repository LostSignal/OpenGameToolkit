
namespace OGT.Properties
{
    using UnityEngine;

    public class EnumValue : ScriptableObject
    {
        [SerializeField] private string displayName;

        public string Name => this.name;

        public string DisplayName => this.displayName;
    }
}
