//-----------------------------------------------------------------------
// <copyright file="StudiOpsManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Threading.Tasks;

    public class StudiOpsManager : Manager
    {
#if UNITY_EDITOR || OGT_DEVELOPMENT_BUILD
        [UnityEngine.SerializeField] private string feedbackUrl = string.Empty;
#endif

        protected override Task InitializeManager(Bootloader bootloader)
        {
            this.InitializeStudiOps();
            return Task.CompletedTask;
        }

        private void InitializeStudiOps()
        {
#if (UNITY_EDITOR || OGT_DEVELOPMENT_BUILD) && USING_STUDIOPS

            if (string.IsNullOrEmpty(this.feedbackUrl))
            {
                UnityEngine.Debug.LogWarning("StudiOps Feedback URL is empty, feedback will not work");
            }
            else
            {
                StudiOps.Feedback.StudiOpsFeedback.Initialize(this.feedbackUrl);
            }
#endif
        }
    }
}
