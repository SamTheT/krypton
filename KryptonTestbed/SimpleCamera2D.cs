using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KryptonTestbed
{
    public delegate void OnCameraSizeChangedEventHandler(Vector2 newSize);

    /// <summary>
    /// A simple camera
    /// Can zoom and rotate
    /// </summary>
    public class SimpleCamera2D
    {
        /// <summary>
        /// Determines if the camera moves at all
        /// Marking the camera will make it always stay at the last specified position
        /// </summary>
        public bool IsStatic = false;

        public Matrix World => Matrix.Identity;//Matrix.CreateTranslation(new Vector3(Position, 0.0f));

        // Having the matrix translated to the center and then back allows the camera to rotate around the center
        // instead of the top-left corner
        public Matrix Transformation =>
                        Matrix.CreateTranslation(new Vector3(Position, 0) * -1f) *
                        //Matrix.CreateTranslation(new Vector3(-Center, 0.0f)) *
                        Matrix.CreateRotationZ(Rotation) *
                        //Matrix.CreateTranslation(new Vector3(Center, 0.0f)) *
                        Matrix.CreateScale(Zoom, Zoom, 1.0f);

        public Matrix Projection => Matrix.CreateOrthographic(Size.X, Size.Y, 0, 1);

        public Vector2 Position
        {
            get { return new Vector2(Viewport.X, Viewport.Y); }
            set { Viewport.X = (int)value.X; Viewport.Y = (int)value.Y; }
        }

        public Vector2 Size
        {
            get { return new Vector2(Viewport.Width, Viewport.Height); }
            set { Viewport.Width = (int)value.X; Viewport.Height = (int)value.Y; OnCameraSizeChanged(value); }
        }

        public Vector2 Center => (Size / 2);
        public Vector2 GlobalCenter => (Position + Center);

        public Viewport Viewport;

        /// <summary>
        /// No zoom is 1.0f
        /// </summary>
        public float Zoom = 1.0f;

        /// <summary>
        /// Radians
        /// </summary>
        public float Rotation = 0.0f;
        
        /// <summary>
        /// Defines bounds of space beyond which camera will not move
        /// Rectangle.Empty for no bounds
        /// </summary>
        public Rectangle Bounds = Rectangle.Empty;
        
        /// <summary>
        /// Triggers when the Size property is changed
        /// </summary>
        public event OnCameraSizeChangedEventHandler OnCameraSizeChanged;



        public SimpleCamera2D(Viewport viewport)
        {
            Viewport = viewport;
        }
        
        public void Update(GameTime gameTime)
        {
            if (Bounds != Rectangle.Empty)
            {
                if (Position.X < Bounds.X)
                    Position = new Vector2(Bounds.X, Position.Y);
                else if (Position.X + Size.X > Bounds.Width)
                    Position = new Vector2(Bounds.Width - Size.X, Position.Y);

                if (Position.Y < Bounds.Y)
                    Position = new Vector2(Position.X, Bounds.Y);
                else if (Position.Y + Size.Y > Bounds.Height)
                    Position = new Vector2(Position.X, Bounds.Height - Size.Y);
            }

            Zoom = MathHelper.Clamp(Zoom, 0.25f, 20.0f);
        }

        public void CenterOn(Vector2 pos)
        {
            Position = pos;// - Center;
        }
    }
}
