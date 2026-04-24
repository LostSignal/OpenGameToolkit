
namespace OGT
{
    using System;
    using System.Collections.Generic;

    public interface IHasHidableComponents
    {
#if UNITY_EDITOR
        IEnumerable<Type> GetHidableComponents();

        bool AreComponentsHidden { get; set; }
#endif
    }
}
