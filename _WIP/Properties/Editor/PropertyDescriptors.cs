using Unity.VisualScripting;

namespace OGT.Properties
{
    [Descriptor(typeof(BoolPropertyUnit))]
    public class BoolPropertyUnitDescriptor : UnitDescriptor<BoolPropertyUnit>
    {
        public BoolPropertyUnitDescriptor(BoolPropertyUnit target) : base(target)
        {
        }

        protected override string DefinedSubtitle() =>
            string.IsNullOrEmpty(unit.BoolProperty?.Name) ? string.Empty : unit.BoolProperty.Name;
    }
}
