
namespace OGT
{
    public struct ActionInput
    {
        public bool IsDown;
        public float DownTime;
        public bool WasPressedDownThisFrame;

        public void Update(bool isDown, float deltaTime)
        {
            if (this.IsDown)
            {
                this.WasPressedDownThisFrame = false;
                this.DownTime += deltaTime;
            }
            else
            {
                this.WasPressedDownThisFrame = true;
                this.DownTime = 0.0f;
            }

            this.IsDown = isDown;
        }
    }
}
