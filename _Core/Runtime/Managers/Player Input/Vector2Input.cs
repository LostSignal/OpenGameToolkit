namespace OGT
{
    public struct Vector2Input
    {
        public float X;
        public float Y;

        public void Update(float x, float y, float deltaTime)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
