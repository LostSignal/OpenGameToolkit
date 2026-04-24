//-----------------------------------------------------------------------
// <copyright file="DialogManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public sealed class DialogManager :
        Manager
#if UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE
        , IUpdate
#endif
    {
#pragma warning disable 0649
        [SerializeField] private DialogLogic[] onDemandDialogs;
#pragma warning restore 0649

        private readonly Dictionary<System.Type, DialogLogic> dialogTypes = new();
        private readonly LinkedList<Dialog> dialogs = new();
        private readonly List<DialogLogic> instantiatedDialogs = new();

        public static DialogManager Instance
        {
            get
            {
                Debug.LogError("DialogManager.Instance No Longer Supported!");
                return GameObject.FindAnyObjectByType<Bootloader>().FindManager<DialogManager>();
            }
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }

        public void RegisterDialog(DialogLogic dialogLogic)
        {
            this.dialogTypes.Add(dialogLogic.GetType(), dialogLogic);
        }

        public void UnregisterDialog(DialogLogic dialogLogic)
        {
            this.dialogTypes.Remove(dialogLogic.GetType());
        }

        public static void ForceUpdateDialogCameras(Camera newCamera)
        {
            throw new System.NotImplementedException();

            //// foreach (var dialogLogic in DialogManager.Instance.dialogTypes.Values)
            //// {
            ////     dialogLogic.Dialog.ForceUpdateCamera(newCamera);
            //// }
        }

        public static T GetDialog<T>()
            where T : DialogLogic
        {
            //// if (DialogManager.Instance.dialogTypes.TryGetValue(typeof(T), out DialogLogic dialogLogic))
            //// {
            ////     return (T)dialogLogic;
            //// }
            //// 
            //// if (DialogManager.Instance.onDemandDialogs == null)
            //// {
            ////     return null;
            //// }
            //// 
            //// for (int i = 0; i < DialogManager.Instance.onDemandDialogs.Length; i++)
            //// {
            ////     var prefab = DialogManager.Instance.onDemandDialogs[i];
            ////     var dailogLogicComponent = prefab.GetComponent<T>();
            //// 
            ////     if (dailogLogicComponent)
            ////     {
            ////         var newDialog = GameObject.Instantiate(prefab);
            ////         newDialog.gameObject.name = newDialog.gameObject.name.Substring(0, newDialog.gameObject.name.Length - "(Clone)".Length);
            ////         DialogManager.Instance.instantiatedDialogs.Add(newDialog);
            ////         GameObject.DontDestroyOnLoad(newDialog.gameObject);
            ////         return newDialog.GetComponent<T>();
            ////     }
            //// }
            //// 
            //// return null;

            throw new System.NotImplementedException();
        }

        public bool IsTopMostDialog(Dialog dialog)
        {
            return this.dialogs.Last != null && this.dialogs.Last.Value == dialog;
        }

        public void AddDialog(Dialog dialog)
        {
            if (dialog != null && dialog.RegisterForBackButton && this.dialogs.Contains(dialog) == false)
            {
                this.dialogs.AddLast(dialog);
            }
        }

        public void RemoveDialog(Dialog dialog)
        {
            if (dialog != null && dialog.RegisterForBackButton && this.dialogs.Contains(dialog))
            {
                this.dialogs.Remove(dialog);
            }
        }

        public void BackButtonPressed()
        {
            if (this.dialogs.Count > 0)
            {
                this.dialogs.Last.Value.BackButtonPressed();
            }
        }

#if UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE
        public void OnUpdate(float deltaTime)
        {
            // NOTE [bgish]: this catches the Android Back Button
            if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                this.BackButtonPressed();
            }
        }

        private void OnDestroy()
        {
            // Making sure we destory all dialogs we created
            foreach (var dialog in this.instantiatedDialogs)
            {
                if (dialog)
                {
                    GameObject.Destroy(dialog.gameObject);
                }
            }

            this.instantiatedDialogs.Clear();
        }

#endif
    }
}
