using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using ShooterTutorial.GameObjects;
using ShooterTutorial.ShooterContentTypes;


namespace ShooterTutorial
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        // A movement speed for the player.
        private const float PlayerMoveSpeed = 8;

        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        Player _player;
        Weapon _weapon;

        List<Weapon> _weaponList;

        Texture2D _mainBackground;
        ParallaxingBackground _bgLayer1;
        ParallaxingBackground _bgLayer2;
        Rectangle _rectBackground;
        const float Scale = 1f;


        // Keyboard states
        KeyboardState _currentKeyboardState;
        KeyboardState _prevKeyboardState;

        // Gamepad states
        GamePadState _currentGamePadState;
        GamePadState _prevGamePadState;

        // Mouse states
        MouseState _currentMouseState;
        MouseState _prevMouseState;

        // texture to hold the laser.
        Texture2D laserTexture;
        List<Laser> laserBeams;


        // The rate at which enemies appear.
        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;

        TimeSpan powerupSpawnTime;
        TimeSpan previousPowerupSpawnTime;
        float powerupMinSpawnTime = 2.0f;
        float powerupMaxSpawnTime = 8.0f;

        //Enemies
        Texture2D enemyTexture;
        List<Enemy> enemies;

        // powerup
        Texture2D powerupTexture;
        Powerup powerup;

        // a random number gen
        Random random;

        // Collections of explosions
        List<Explosion> explosions;

        //Texture to hold explosion animation.
        Texture2D explosionTexture;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _player = new Player();
            _weapon = new Weapon(this, _player);

            _weaponList = new List<Weapon>();

           // _weaponList.Add(new Bazooka(this, _player));
            //_weaponList.Add(new WaveWeapon(this, _player));
            //_weaponList.Add(new SinusShot(this, _player));
            _weaponList.Add(new TripleWeaponRotateGHA(this, _player));
            //_weaponList.Add(new GravityBombing(this, _player));
           // _weaponList.Add(new CircularShoot(this, _player));

            _bgLayer1 = new ParallaxingBackground();
            _bgLayer2 = new ParallaxingBackground();
            _rectBackground = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            TouchPanel.EnabledGestures = GestureType.FreeDrag;

            // init our laser
            laserBeams = new List<Laser>();

            // Initialize the enemies list
            enemies = new List<Enemy>();

            //used to determine how fast the enemies will respawn.
            enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            // init our random number generator
            random = new Random();

            explosions = new List<Explosion>();
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the player resources
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;    
            var playerPosition = new Vector2(titleSafeArea.X, titleSafeArea.Y + titleSafeArea.Height/2);
            
            Texture2D playerTexture = Content.Load<Texture2D>("Graphics\\shipAnimation");
            Animation playerAnimation = new Animation();
            playerAnimation.Initialize(playerTexture, playerPosition, 115, 69, 8, 30, Color.White, 1, true);
            
            _player.Initialize(playerAnimation, playerPosition);

            // Load the background.
            _bgLayer1.Initialize(Content, "Graphics/bgLayer1", GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, -1);
            _bgLayer2.Initialize(Content, "Graphics/bgLayer2", GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, -2);
            _mainBackground = Content.Load<Texture2D>("Graphics/mainbackground");

            // load the enemy texture.
            enemyTexture = Content.Load<Texture2D>("Graphics\\mineAnimation");

            // load th texture to serve as the laser.
            laserTexture = Content.Load<Texture2D>("Graphics\\laser");

            // load the texture bonus
            powerupTexture = Content.Load<Texture2D>("Graphics\\powerupGHA.png");

            // Load the exploision sprite strip
            explosionTexture = Content.Load<Texture2D>("Graphics\\explosion");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // User inputs.
            // Save the previous state of the keyboard and game pad so we can determine single key/button presses
            _prevGamePadState = _currentGamePadState;
            _prevKeyboardState = _currentKeyboardState;
            _prevMouseState = _currentMouseState;
            // Read the current state of the keyboard and gamepad and store it.
            _currentKeyboardState = Keyboard.GetState();
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);
            _currentMouseState = Mouse.GetState();
            
            UpdatePlayer(gameTime);
            _bgLayer1.Update(gameTime);
            _bgLayer2.Update(gameTime);

            // update lasers
            UpdateLasers(gameTime);

            // update the enemies
            UpdateEnemies(gameTime);

            UpdatePowerup(gameTime);

            // update collisons
            UpdateCollision();

            UpdateExplosions(gameTime);

            base.Update(gameTime);
        }

        private void UpdateExplosions(GameTime gameTime)
        {
            for (var e = 0; e < explosions.Count; e++ )
            {
                explosions[e].Update(gameTime);

                if (!explosions[e].Active)
                    explosions.Remove(explosions[e]);
            }
        }

        void UpdatePlayer(GameTime gameTime)
        {
            _player.Update(gameTime);

            // Touch inputs
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();
                
                if (gesture.GestureType == GestureType.FreeDrag)
                    _player.Position += gesture.Delta;
            }

            // Get Mouse State
            Vector2 mousePosition = new Vector2(_currentMouseState.X, _currentMouseState.Y);
            if (_currentMouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 posDelta = mousePosition - _player.Position;
                posDelta.Normalize();
                posDelta = posDelta * PlayerMoveSpeed;
                _player.Position = _player.Position + posDelta;
            }


            // Thumbstick controls
            _player.Position.X += _currentGamePadState.ThumbSticks.Left.X * PlayerMoveSpeed;
            _player.Position.Y += _currentGamePadState.ThumbSticks.Left.Y * PlayerMoveSpeed;

            // Keyboard/DPad
            if (_currentKeyboardState.IsKeyDown(Keys.Left) || _currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                _player.Position.X -= +PlayerMoveSpeed;
            }
            if (_currentKeyboardState.IsKeyDown(Keys.Right) || _currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                _player.Position.X += PlayerMoveSpeed;
            }

            if (_currentKeyboardState.IsKeyDown(Keys.Up) || _currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                _player.Position.Y -= PlayerMoveSpeed;
            }
            if (_currentKeyboardState.IsKeyDown(Keys.Down) || _currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                _player.Position.Y += PlayerMoveSpeed;
            }

            if (_currentKeyboardState.IsKeyDown(Keys.Space) || _currentGamePadState.Buttons.X == ButtonState.Pressed)
            {
                _weapon.Fire(gameTime);
            }

            if (_currentKeyboardState.IsKeyDown(Keys.NumPad1))
            {
                _weapon = new Weapon(this, _player);
            }

            if (_currentKeyboardState.IsKeyDown(Keys.NumPad2))
            {
                _weapon = new TripleWeapon(this, _player);
            }

            if (_currentKeyboardState.IsKeyDown(Keys.NumPad3))
            {
                _weapon = new AdnWeapon(this, _player);
            }

            if (_currentKeyboardState.IsKeyDown(Keys.NumPad4))
            {
                _weapon = new WaveWeapon(this, _player);
            }

            // Make sure that the player does not go out of bounds
            _player.Position.X = MathHelper.Clamp(_player.Position.X, 0, GraphicsDevice.Viewport.Width - _player.Width);
            _player.Position.Y = MathHelper.Clamp(_player.Position.Y, 0, GraphicsDevice.Viewport.Height - _player.Height);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            // Start drawing
            _spriteBatch.Begin();

            // Draw background.
            _spriteBatch.Draw(_mainBackground, _rectBackground, Color.White);
            _bgLayer1.Draw(_spriteBatch);
            _bgLayer2.Draw(_spriteBatch);


            // Draw the Player
            _player.Draw(_spriteBatch);

            // Draw the lasers.
            foreach (var l in laserBeams)
            {
                l.Draw(_spriteBatch);
            }


            // draw the enemies
            foreach(var e in enemies)
            {
                e.Draw(_spriteBatch);
            };

            if(powerup!=null)
            {
                powerup.Draw(_spriteBatch);
            }

            // draw explosions
            foreach(var e in explosions)
            {
                e.Draw(_spriteBatch);
            };

            // Stop drawing
            _spriteBatch.End(); 


            base.Draw(gameTime);
        }
        
        protected void UpdateLasers(GameTime gameTime)
        {
           
            // update laserbeams
            for (var i = 0; i < laserBeams.Count;i++ )
                {

                    laserBeams[i].Update(gameTime);

                    // Remove the beam when its deactivated or is at the end of the screen.
                    if (!laserBeams[i].Active || laserBeams[i].Position.X > GraphicsDevice.Viewport.Width)
                    {
                        laserBeams.Remove(laserBeams[i]);
                    }
                }
        }

        public void AddLaser(IMovement movement)
        {
            Animation laserAnimation = new Animation();

            // initlize the laser animation
            laserAnimation.Initialize(laserTexture,
                _player.Position,
                46,
                16,
                1,
                30,
                Color.White,
                1f,
                true);

            Laser laser = new Laser();

            // init the laser
            laser.Initialize(laserAnimation, movement);

            laserBeams.Add(laser);

            /* todo: add code to create a laser. */
            //laserSoundInstance.Play();
        }

        protected void UpdateEnemies(GameTime gameTime)
        {
            // spawn a new enemy every 1.5 seconds.
            if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
            {
                previousSpawnTime = gameTime.TotalGameTime;

                // add an enemy
                AddEnemy();
            }

            // update the enemies
            for (var i = 0; i < enemies.Count; i++)
            {
                enemies[i].Update(gameTime);
                if (!enemies[i].Active)
                {
                    enemies.Remove(enemies[i]);
                }
            }
        }

        protected void UpdatePowerup(GameTime gameTime)
        {
            if (powerup != null)
            {
                powerup.Update(gameTime);

                if (!powerup.Active)
                {
                    powerup = null;
                    previousPowerupSpawnTime = gameTime.TotalGameTime;


                    powerupSpawnTime =TimeSpan.FromSeconds(powerupMinSpawnTime + ( powerupMaxSpawnTime - powerupMinSpawnTime ) * random.NextDouble());
                }
            }
            else
            {
                if (gameTime.TotalGameTime - previousPowerupSpawnTime > powerupSpawnTime)
                {
                    previousPowerupSpawnTime = gameTime.TotalGameTime;

                    // randomly generate the postion of the enemy
                    Vector2 position = new Vector2(
                        random.Next(100, GraphicsDevice.Viewport.Width - 100),
                        random.Next(100, GraphicsDevice.Viewport.Height - 100)
                        );


                    int value = random.Next(_weaponList.Count);

                    var weapon = _weaponList[value];

                    powerup = new Powerup(weapon.GetPowerupAnimation(), new LinearMovement(position,new Vector2(-0.01f,0f),5f), weapon);
                }
            }

        }

        protected void AddEnemy()
        {
            // create the animation object
            Animation enemyAnimation = new Animation();

            // Init the animation with the correct 
            // animation information
            enemyAnimation.Initialize(enemyTexture,
                Vector2.Zero,
                47,
                61,
                8,
                30,
                Color.White,
                1f,
                true);

            // randomly generate the postion of the enemy
            Vector2 position = new Vector2(
                GraphicsDevice.Viewport.Width + enemyTexture.Width / 2,
                random.Next(100, GraphicsDevice.Viewport.Height - 100));

            // create an enemy
            Enemy enemy = new Enemy();

            var m = new LinearMovement(position, new Vector2(-1f, 0f));
            m.MoveSpeed = 10f;
            // init the enemy
            enemy.Initialize(enemyAnimation, m);

            // Add the enemy to the active enemies list
            enemies.Add(enemy);

        }


        protected void UpdateCollision()
        {

            // we are going to use the rectangle's built in intersection
            // methods.

            Rectangle playerRectangle;
            Rectangle enemyRectangle;
            Rectangle laserRectangle;

            // create the rectangle for the player
            playerRectangle = new Rectangle(
                (int)_player.Position.X,
                (int)_player.Position.Y,
                _player.Width,
                _player.Height);

            // detect collisions between the player and all enemies.
            for (var i = 0; i < enemies.Count; i++)
            {
                enemyRectangle = new Rectangle(
                   (int)enemies[i].Position.X,
                   (int)enemies[i].Position.Y,
                   enemies[i].Width,
                   enemies[i].Height);

                // determine if the player and the enemy intersect.
                if (playerRectangle.Intersects(enemyRectangle))
                {
                    // kill off the enemy
                    enemies[i].Health = 0;

                    // Show the explosion where the enemy was...
                    AddExplosion(enemies[i].Position);

                    // deal damge to the player
                    _player.Health -= enemies[i].Damage;

                    // if the player has no health destroy it.
                    if (_player.Health <= 0)
                    {
                        _player.Active = false;
                    }
                }

                for (var l = 0; l < laserBeams.Count; l++)
                {
                    // create a rectangle for this laserbeam
                    laserRectangle = new Rectangle(
                        (int)laserBeams[l].Position.X,
                        (int)laserBeams[l].Position.Y,
                        laserBeams[l].Width,
                        laserBeams[l].Height);

                    // test the bounds of the laser and enemy
                    if (laserRectangle.Intersects(enemyRectangle))
                    {

                        // Show the explosion where the enemy was...
                        AddExplosion(enemies[i].Position);

                        // kill off the enemy
                        enemies[i].Health = 0;

                        // kill off the laserbeam
                        laserBeams[l].Active = false;
                    }
                }
            }

            if (powerup != null)
            {
                var powerupRectangle = new Rectangle(
                    (int)powerup.Position.X,
                    (int)powerup.Position.Y,
                    powerup.Width,
                    powerup.Height);

                // determine if the player and the enemy intersect.
                if (playerRectangle.Intersects(powerupRectangle))
                {
                    powerup.Active = false;

                    _weapon = powerup.Weapon;
                }
                for (var l = 0; l < laserBeams.Count; l++)
                {
                    laserRectangle = new Rectangle(
                 (int)laserBeams[l].Position.X,
                 (int)laserBeams[l].Position.Y,
                 laserBeams[l].Width,
                 laserBeams[l].Height);

                    if (laserRectangle.Intersects(powerupRectangle))
                    {
                        powerup.Active = false;

                        _weapon = powerup.Weapon;
                    }
                }

            }
        }

        protected void AddExplosion(Vector2 enemyPosition)
        {
            Animation explosionAnimation = new Animation();

            explosionAnimation.Initialize(explosionTexture,
                enemyPosition,
                134,
                134,
                12,
                30,
                Color.White,
                1.0f,
                true);

            Explosion explosion = new Explosion();
            explosion.Initialize(explosionAnimation, enemyPosition);

            explosions.Add(explosion);
        }
    }
}
    