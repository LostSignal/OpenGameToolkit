
namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(MonoBehaviour), editorForChildClasses: true, isFallback = false)]
    public class DefaultMonoBehaviourEditor : UnityEditor.Editor
    {
        private static readonly Dictionary<Type, List<MethodInfo>> MethodInfoCache = new();
        private static readonly ValidationReport Report = new ValidationReport();

        private void OnEnable()
        {
            UpdateHidableComponents(this.target);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DrawDefaultContent(this.target);
        }

        public static void DrawDefaultContent(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            GUILayout.Space(5);

            bool didDraw = false;
            didDraw |= DrawInspectorButtons(target);
            didDraw |= DrawShowHideComponents(target);
            didDraw |= DrawValidateButton(target);
            didDraw |= ValidateGameObjectsPrefabsAndScriptableObjects(target);

            if (didDraw)
            {
                GUILayout.Space(5);
            }
        }

        public static bool DrawInspectorButtons(UnityEngine.Object target)
        {
            if (MethodInfoCache.TryGetValue(target.GetType(), out List<MethodInfo> methodsList) == false)
            {
                methodsList = new List<MethodInfo>();

                foreach (var method in target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    var parameters = method.GetParameters();
                    var inspectorButton = method.GetCustomAttributes(typeof(InspectorButton), true).FirstOrDefault() as InspectorButton;

                    if (parameters == null || parameters.Length == 0 && inspectorButton != null)
                    {
                        methodsList.Add(method);
                    }
                }

                MethodInfoCache.Add(target.GetType(), methodsList);
            }

            foreach (var method in methodsList)
            {
                var inspectorButton = method.GetCustomAttributes(typeof(InspectorButton), false).FirstOrDefault() as InspectorButton;

                if (GUILayout.Button(inspectorButton.ButtonName ?? ObjectNames.NicifyVariableName(method.Name)))
                {
                    method.Invoke(target, null);
                }
            }

            return methodsList.Count > 0;
        }

        public static bool DrawShowHideComponents(UnityEngine.Object target)
        {
            // Adding extra options if it implements IHasHidableComponents
            if (target is IHasHidableComponents hidableComponents)
            {
                GUILayout.Space(10);

                if (GUILayout.Button(hidableComponents.AreComponentsHidden ? "Show Components" : "Hide Components"))
                {
                    hidableComponents.AreComponentsHidden = !hidableComponents.AreComponentsHidden;
                    UpdateHidableComponents(target);
                }

                return true;
            }

            return false;
        }

        public static bool DrawValidateButton(UnityEngine.Object target)
        {
            if (target is IValidate validate)
            {
                if (GUILayout.Button("Validate"))
                {
                    ValidateGameObjectsPrefabsAndScriptableObjects(target, true);
                }

                return true;
            }

            return false;
        }

        public static bool ValidateGameObjectsPrefabsAndScriptableObjects(UnityEngine.Object target, bool printErrors = false)
        {
            if (target is IValidate validate)
            {
                bool isSceneObject = target.EditorIsSceneObject();

                if (printErrors)
                {
                    // Run full validation on object
                    Validation.Validation.ValidateObjects(new List<UnityEngine.Object> { (target as MonoBehaviour).gameObject }, isSceneObject);
                }
                else
                {
                    // Just run validate and ignore all errors
                    Report.Errors.Clear();
                    validate.Validate(Report, isSceneObject);
                    Report.Errors.Clear();
                }

                return true;
            }

            return false;
        }

        public static void UpdateHidableComponents(UnityEngine.Object target)
        {
            var haveHideFlagsChanged = false;
            var behaviour = (target as Behaviour);

            if (behaviour == null || behaviour.enabled == false)
            {
                return;
            }

            var gameObject = (target as Component)?.gameObject;

            if (gameObject == null)
            {
                return;
            }

            if (target is IHasHidableComponents hidableComponents)
            {
                var newHideFlags = hidableComponents.AreComponentsHidden ? HideFlags.HideInInspector : HideFlags.None;

                foreach (var componentType in hidableComponents.GetHidableComponents())
                {
                    var component = gameObject.GetComponent(componentType);

                    if (component && component.hideFlags != newHideFlags)
                    {
                        component.hideFlags = newHideFlags;
                        haveHideFlagsChanged = true;
                    }
                }
            }

            if (haveHideFlagsChanged && EditorWindow.focusedWindow != null)
            {
                EditorUtility.SetDirty(EditorWindow.focusedWindow);
            }
        }
    }
}
