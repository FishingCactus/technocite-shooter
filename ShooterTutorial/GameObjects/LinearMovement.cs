using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace ShooterTutorial.GameObjects
{
    class LinearMovement: IMovement
    {
        // postion of the laser
        public Vector2 Position;

        // Direction of the laser
        public Vector2 Direction;

        public float MoveSpeed;
        
        const float DEFAULT_SPEED = 30f;

        public LinearMovement(Vector2 position, Vector2 direction)
        {
            Position = position;

            Direction = direction;
            Direction.Normalize();

            MoveSpeed = DEFAULT_SPEED;
        }
        public LinearMovement(Vector2 position, Vector2 direction, float speed)
        {
            Position = position;

            Direction = direction;
            Direction.Normalize();

            MoveSpeed = speed;
        }

        public void update( GameTime gameTime )
        {
            Position += MoveSpeed * Direction;
        }

        public Vector2 getPosition()
        {
            return Position;
        }

        public float getRotation()
        {
            return 0f;
        }
        
        public static LinearMovement create(Vector2 position, float verticalOffset, float angle, float speed = DEFAULT_SPEED)
        {
            // Adjust the position slightly to match the muzzle of the cannon.
            position.Y += 37 + verticalOffset;
            position.X += 70;

            var direction = new Vector2();

            direction.X = (float)Math.Cos((double)angle);
            direction.Y = (float)Math.Sin((double)angle);

            var m = new LinearMovement(position, direction);

            m.MoveSpeed = speed;

            return m;
        }
    }
}
