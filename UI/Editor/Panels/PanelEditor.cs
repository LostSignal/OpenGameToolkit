namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(Panel))]
    public class PanelEditor : OGT.Editor
    {
        protected override void NewOnInspectorGUI()
        {
            // Drawing Canvas Properties
            var canvas = this.GetComponent<Canvas>();

            this.Foldout("Canvas", () =>
            {
                this.Space(10);

                // NOTE [bgish]: Whether you use it or not, it should be set so you can switch between render modes
                if (canvas.worldCamera == null)
                {
                    canvas.worldCamera = Camera.main;
                }

                this.DrawProperty(canvas, "renderMode");

                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    this.DrawProperty(canvas, "sortingOrder", "Sort Order");
                }
                else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    this.DrawProperty(canvas, "worldCamera");
                    this.DrawProperty(canvas, "planeDistance");
                    this.DrawProperty(canvas, "sortingOrder", "Order in Layer");
                    this.DrawPropertyAsSortingLayer(canvas, "sortingLayerID");
                }

                this.Space(10);
            }, true);

            // Drawing Showable Properties
            var showable = this.GetComponent<Showable>();

            this.Foldout("Showable", () =>
            {
                this.Space(10);
                this.DrawMember(showable, "showOnStart");
                this.DrawMember(showable, "show");
                this.DrawMember(showable, "idle");
                this.DrawMember(showable, "hide");

                this.Space(10);

                using (new IndentLevelScope(1))
                {
                    this.DrawMember(showable, "enableOnShow");
                    this.DrawMember(showable, "disableOnHide");
                }

                this.Space(10);

                using (new FoldoutScope(1, "Events", out bool visible))
                {
                    if (visible)
                    {
                        this.DrawMember(showable, "onShowStart");
                        this.DrawMember(showable, "onShowEnd");
                        this.DrawMember(showable, "onHideStart");
                        this.DrawMember(showable, "onHideEnd");
                    }
                }

                this.Space(10);
            }, true);

            // Drawing Panel Properties
            var panel = this.target as Panel;

            this.Foldout("Panel", () =>
            {
                this.Space(10);
                this.DrawMember(panel, "registerForBackButtonPressed");
                this.DrawMember(panel, "backButtonAction");
                GUILayout.Space(10);
                this.DrawMember(panel, "onBackButtonPressed");
                this.Space(10);
            }, true);

            if (Application.isPlaying)
            {
                GUILayout.Space(20);

                if (GUILayout.Button("Show"))
                {
                    showable.Show();
                }

                if (GUILayout.Button("Hide"))
                {
                    showable.Hide();
                }

                GUILayout.Space(20);
            }
        }
    }
}
