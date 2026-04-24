#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="KeyboardManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class KeyboardManager
    {
        private Dictionary<Key, KeyboardButton> buttons = new Dictionary<Key, KeyboardButton>();

        public void Update()
        {
            foreach (KeyValuePair<Key, KeyboardButton> pair in this.buttons)
            {
                pair.Value.Update();
            }
        }

        public KeyboardButton GetButton(Key keyCode)
        {
            KeyboardButton button;

            if (this.buttons.TryGetValue(keyCode, out button) == false)
            {
                button = new KeyboardButton(keyCode);
                button.Update();
                this.buttons.Add(keyCode, button);
            }

            return button;
        }
    }
}
