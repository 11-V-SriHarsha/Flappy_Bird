using System.Drawing;

namespace FlappyBird
{
    public class Bird
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Velocity { get; set; }
        public int Size { get; } = 45;
        
        private const float Gravity = 0.7f;      // Increased from 0.5f
        private const float JumpForce = -10f;    // Increased from -8f for stronger jumps

        public Bird(int x, int y)
        {
            X = x;
            Y = y;
            Velocity = 0;
        }

        public void Update()
        {
            Velocity += Gravity;
            Y += Velocity;
        }

        public void Jump()
        {
            Velocity = JumpForce;
        }

        public Rectangle Bounds
        {
            get { return new Rectangle((int)X, (int)Y, Size, Size); }
        }
    }
}
