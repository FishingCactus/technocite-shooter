using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShooterTutorial.GameObjects
{
    class TripleWeaponRotateGHA : Weapon
    {
        public TripleWeaponRotateGHA(Game1 game, Player player) : base(game, player)
        {
        }

        public override void Fire(GameTime gameTime)
        {
            // govern the rate of fire for our lasers
            if (gameTime.TotalGameTime - _previousLaserSpawnTime > _laserSpawnTime)
            {
                _previousLaserSpawnTime = gameTime.TotalGameTime;

                // Add the laer to our list.
                _game.AddLaser(ghaMovement.create(_player.Position, -10f, -30.0f * (float)Math.PI / 180.0f, 0));
                _game.AddLaser(ghaMovement.create(_player.Position, 0f, 0f, 0));
                _game.AddLaser(ghaMovement.create(_player.Position, 10f, 30.0f * (float)Math.PI / 180.0f, 0));
            }

        }

        public override Animation GetPowerupAnimation()
        {
            var texture = _game.Content.Load<Texture2D>("Graphics\\powerupGHA.png");

            Animation animation = new Animation();

            // Init the animation with the correct 
            // animation information
            animation.Initialize(texture,
                Vector2.Zero,
                148,
                125,
                1,
                30,
                Color.White,
                1f,
                true);

            return animation;
        }
    }
}
