using UnityEngine;
using UnityEditor;
using OGT.Properties;
using Unity.VisualScripting;

public class PropertiesEditorWindow : EditorWindow
{
    private BoolProperty boolProperty;
    private Properties properties;
    private GenericMenu genericMenu;
    private string[] options;
    private string selectedItem = "";

    //[MenuItem("Window/Custom Popup")]
    public static void Show(Properties properties, Rect dropDownButtonRect)
    {
        Rect screenRect = GUIUtility.GUIToScreenRect(dropDownButtonRect);

        var window = GetWindow<PropertiesEditorWindow>(true, string.Empty, true);
        window.titleContent = GUIContent.none;
        window.SetProperties(properties);

        window.position = new Rect(screenRect.x, screenRect.y + screenRect.height, 200, 150);
        window.ShowPopup();

        // window.ShowAsDropDownWithKeyboardFocus(new Rect(screenRect.x, screenRect.y + screenRect.height, 200, 150), new Vector2(300.0f, 50.0f));
    }

    private void SetProperties(Properties props)
    {
        this.properties = props;
        this.options = this.properties.GetPropertyNames(typeof(bool));
        this.genericMenu = new GenericMenu();

        foreach (string option in options)
        {
            string[] parts = option.Split('.');
            string menuPath = string.Join("/", parts);
            this.genericMenu.AddItem(new GUIContent(menuPath), false, OnItemSelected, option);
        }
    }

    private void OnLostFocus()
    {
        // Optionally close automatically when user clicks elsewhere
        Close();
    }

    void OnGUI()
    {
        //if (EditorGUILayout.DropdownButton(new GUIContent(string.IsNullOrEmpty(selectedItem) ? "Select Item" : selectedItem), FocusType.Keyboard))
        //{
        this.genericMenu.ShowAsContext();
        //}

        //// GUILayout.Label("Click away to close this window.", EditorStyles.boldLabel);
        ////
        //// if (GUILayout.Button("Click me!"))
        //// {
        ////     Debug.Log("Button clicked inside the popup.");
        //// }
    }

    private void OnItemSelected(object item)
    {
        selectedItem = (string)item;
        Debug.Log($"Selected: {selectedItem}");
    }
}
