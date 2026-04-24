
namespace OGT
{
    public struct TriggerInput
    {
        public float Value;

        public void Update(float currentInput)
        {
            this.Value = currentInput;
        }
    }
}
