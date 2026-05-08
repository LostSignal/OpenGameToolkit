//-----------------------------------------------------------------------
// <copyright file="BackendMessages.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGT;

namespace Lost
{
    public static class BackendMessages
    {
        public static void ShowInsufficientCurrency(this PanelManager panelManager, Action yesAction, Action noAction)
        {
            var yesNoDialog = panelManager.GetPanel<NewMessageBox>();
            yesNoDialog.ShowYesNo("Not Enough Currency", "Not enough currency to make this purchase.", yesAction, noAction);
        }
    }
}
