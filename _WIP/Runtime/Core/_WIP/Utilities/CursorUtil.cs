//-----------------------------------------------------------------------
// <copyright file="CursorUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    public static class CursorUtil
    {
        public static void SetCursorLockedAndHidden()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public static void SetCursorUnlockedAndVisible()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
