
namespace OGT.Properties
{
    using Unity.VisualScripting;

    [UnitCategory("OGT")]
    [UnitTitle("Bool Property")]
    public sealed class BoolPropertyUnit : Unit, IStart
    {
        private GraphReference graphReference;

        [Serialize]
        [Inspectable]
        public BoolProperty BoolProperty;

        [DoNotSerialize]
        public ControlOutput OnStartTrue { get; private set; }

        [DoNotSerialize]
        public ControlOutput OnStartFalse { get; private set; }

        [DoNotSerialize]
        public ControlOutput OnFalseToTrue { get; private set; }

        [DoNotSerialize]
        public ControlOutput OnTrueToFalse { get; private set; }

        public override void Instantiate(GraphReference instance)
        {
            base.Instantiate(instance);
            this.graphReference = instance;
            this.BoolProperty ??= new BoolProperty();

            ActivationManager.Register(this);
        }

        public override void Uninstantiate(GraphReference instance)
        {
            base.Uninstantiate(instance);
            this.graphReference = null;
        }

        protected override void Definition()
        {
            this.isControlRoot = true;

            this.OnStartTrue = this.ControlOutput(nameof(this.OnStartTrue));
            this.OnStartFalse = this.ControlOutput(nameof(this.OnStartFalse));
            this.OnFalseToTrue = this.ControlOutput(nameof(this.OnFalseToTrue));
            this.OnTrueToFalse = this.ControlOutput(nameof(this.OnTrueToFalse));
        }

        protected override void AfterDefine()
        {
            base.AfterDefine();

            if (this.BoolProperty != null)
            {
                this.BoolProperty.OnChange += this.OnChange;
            }
        }

        protected override void BeforeUndefine()
        {
            base.BeforeUndefine();

            if (this.BoolProperty != null)
            {
                this.BoolProperty.OnChange -= this.OnChange;
            }
        }

        private void OnChange(bool oldValue, bool newValue)
        {
            if (this.graphReference == null)
            {
                return;
            }

            if (oldValue == false && newValue == true)
            {
                // Trigger OnFalseToTrue
                var flow = Flow.New(this.graphReference);
                flow.Invoke(OnFalseToTrue);
            }
            else if (oldValue == true && newValue == false)
            {
                // Trigger OnTrueToFalse
                var flow = Flow.New(this.graphReference);
                flow.Invoke(this.OnTrueToFalse);
            }
        }

        public void OnStart()
        {
            if (this.BoolProperty.Value)
            {
                // Trigger OnStartTrue
                var flow = Flow.New(this.graphReference);
                flow.Invoke(this.OnStartTrue);
            }
            else
            {
                // Trigger OnStartFalse
                var flow = Flow.New(this.graphReference);
                flow.Invoke(this.OnStartFalse);
            }
        }
    }
}
