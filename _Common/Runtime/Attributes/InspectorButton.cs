namespace OGT
{
    public class InspectorButton : System.Attribute
    {
        public string ButtonName { get; private set; }

        public InspectorButton(string buttonName) => this.ButtonName = buttonName;

        public InspectorButton() => this.ButtonName = null;
    }
}
