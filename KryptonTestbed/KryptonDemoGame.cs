using System;
using Krypton;
using Krypton.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace KryptonTestbed
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class KryptonDemoGame : Game
    {
        private const int NumLights = 25;
        private const int NumHorzontalHulls = 20;
        private const int NumVerticalHulls = 20;
        private const float VerticalUnits = 50;

        private readonly KryptonEngine _krypton;
        private Texture2D _lightTexture;
        private Light2D _light2D;

        private SimpleCamera2D _camera;
        private SpriteBatch spriteBatch;
        private SpriteFont _font;

        public static Random Random = new Random();
        private readonly GraphicsDeviceManager _deviceManager;

        #region Key definitions
        private Dictionary<string, Keys> _keys = new Dictionary<string, Keys>
        {
            { "rotate camera left", Keys.Q },
            { "rotate camera right", Keys.E },
            { "randomize", Keys.R },
            { "move light up", Keys.Up },
            { "move light down", Keys.Down },
            { "move light right", Keys.Right },
            { "move light left", Keys.Left },
            { "shadow type: solid", Keys.D1 },
            { "shadow type: illuminated", Keys.D2 },
            { "shadow type: occluded", Keys.D3 },
            { "lower hull opacity", Keys.O },
            { "increase hull opacity", Keys.P },
            { "debug draw hulls", Keys.H },
            { "debug draw lights", Keys.L },
        };
        #endregion

        public KryptonDemoGame()
        {
            // Setup the graphics device manager with some default settings
            _deviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };

            // Allow the window to be resized (to demonstrate render target recreation)
            Window.AllowUserResizing = true;

            // Setup the content manager with some default settings
            Content.RootDirectory = "Content";

            // Create Krypton
            _krypton = new KryptonEngine(this, "KryptonEffect");

            // As a side note, you may want Krypton to be used as a GameComponent.
            // To do this, you would simply add the following line of code and remove the Initialize and Draw function of krypton below:
            // Components.Add(krypton);
        }

        protected override void Initialize()
        {
            // Make sure to initialize krpyton, unless it has been added to the Game's list of Components
            _krypton.Initialize();

            base.Initialize();

            // Create spritebatch
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void LoadContent()
        {
            // Create a new simple point light texture to use for the lights
            _lightTexture = LightTextureBuilder.CreatePointLight(GraphicsDevice, 512);

            // Create some lights and hulls
            CreateLights(_lightTexture, NumLights);
            CreateHulls(NumHorzontalHulls, NumVerticalHulls);

            // Create a light we can control
            _light2D = new Light2D
            {
                Texture = _lightTexture,
                X = 0,
                Y = 0,
                Range = 25,
                Color = Color.Multiply(Color.CornflowerBlue, 2.0f),
                ShadowType = ShadowType.Occluded
            };

            _krypton.Lights.Add(_light2D);

            // Create a camera
            _camera = new SimpleCamera2D(
                new Viewport(
                    x: 0,
                    y: 0,
                    width: (int)(VerticalUnits * GraphicsDevice.Viewport.AspectRatio),
                    height: (int)VerticalUnits));

            // Load font
            _font = Content.Load<SpriteFont>("font");
        }

        private void CreateLights(Texture2D texture, int count)
        {
            // Make some random lights!
            for (var i = 0; i < count; i++)
            {
                var r = (byte)(Random.Next(255 - 64) + 64);
                var g = (byte)(Random.Next(255 - 64) + 64);
                var b = (byte)(Random.Next(255 - 64) + 64);

                Light2D light = new Light2D()
                {
                    Texture = texture,
                    Range = (float)(Random.NextDouble() * 5 + 5),
                    Color = new Color(r,g,b),
                    //Intensity = (float)(_random.NextDouble() * 0.25 + 0.75),
                    Intensity = 1f,
                    Angle = MathHelper.TwoPi * (float)Random.NextDouble(),
                    X = (float)(Random.NextDouble() * 50 - 25),
                    Y = (float)(Random.NextDouble() * 50 - 25),
                };

                // Here we set the light's field of view
                if (i % 2 == 0)
                {
                    light.Fov = MathHelper.PiOver2 * (float)(Random.NextDouble() * 0.75 + 0.25);
                }

                _krypton.Lights.Add(light);
            }
        }

        private void CreateHulls(int x, int y)
        {
            const float w = 50;
            const float h = 50;

            // Make lines of lines of hulls!
            for (var j = 0; j < y; j++)
            {
                // Make lines of hulls!
                for (var i = 0; i < x; i++)
                {
                    var posX = (i + 0.5f) * w / x - w / 2 + (j % 2 == 0 ? w / x / 2 : 0);
                    var posY = (j + 0.5f) * h / y - h / 2; // +(i % 2 == 0 ? h / y / 4 : 0);

                    var hull = ShadowHull.CreateRectangle(Vector2.One*1f);
                    hull.Position.X = posX;
                    hull.Position.Y = posY;
                    hull.Scale.X = (float)(Random.NextDouble() * 0.75f + 0.25f);
                    hull.Scale.Y = (float)(Random.NextDouble() * 0.75f + 0.25f);

                    _krypton.Hulls.Add(hull);
                }
            }
        }

        protected override void UnloadContent()
        {
            // Not sure if anything actually NEEDS to go here, as the game exits immediately upon unloading content. Please advise if you think this is bad :)
        }

        protected override void Update(GameTime gameTime)
        {
            // Update camera
            _camera.Update(gameTime);

            const int speed = 5;

            // Make sure the user doesn't want to quit (but why would they?)
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                Exit();
                return;
            }

            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
            {
                Exit();
                return;
            }

            // make it much simpler to deal with the time :)
            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            // Allow for randomization of lights and hulls, to demonstrait that each hull and light is individually rendered
            if (Keyboard.GetState().IsKeyDown(_keys["randomize"]))
            {
                // randomize lights
                foreach (var light2D in _krypton.Lights)
                {
                    var light = light2D as Light2D;

                    if (light == null)
                    {
                        continue;
                    }

                    light.Position += Vector2.UnitY * (float)(Random.NextDouble() * 2 - 1) * deltaTime * speed;
                    light.Position += Vector2.UnitX * (float)(Random.NextDouble() * 2 - 1) * deltaTime * speed;
                    light.Angle -= MathHelper.TwoPi * (float)(Random.NextDouble() * 2 - 1) * deltaTime * speed;
                }

                // randomize hulls
                foreach (var hull in _krypton.Hulls)
                {
                    hull.Position += Vector2.UnitY * (float)(Random.NextDouble() * 2 - 1) * deltaTime * speed;
                    hull.Position += Vector2.UnitX * (float)(Random.NextDouble() * 2 - 1) * deltaTime * speed;
                    hull.Angle -= MathHelper.TwoPi * (float)(Random.NextDouble() * 2 - 1) * deltaTime * speed;
                }
            }

            var keyboard = Keyboard.GetState();

            // Light Position Controls
            if (keyboard.IsKeyDown(_keys["move light up"]))
            {
                _light2D.Y += deltaTime * speed;
            }

            if (keyboard.IsKeyDown(_keys["move light down"]))
            {
                _light2D.Y -= deltaTime * speed;
            }

            if (keyboard.IsKeyDown(_keys["move light right"]))
            {
                _light2D.X += deltaTime * speed;
            }

            if (keyboard.IsKeyDown(_keys["move light left"]))
            {
                _light2D.X -= deltaTime * speed;
            }

            var mouse = Mouse.GetState();
            // Update camera's position
            _camera.CenterOn(_light2D.Position);
            //_camera.CenterOn(new Vector2(mouse.X, mouse.Y));

            // Rotate camera
            if (keyboard.IsKeyDown(_keys["rotate camera left"]))
                _camera.Rotation -= MathHelper.TwoPi * deltaTime;
            else if (keyboard.IsKeyDown(_keys["rotate camera right"]))
                _camera.Rotation += MathHelper.TwoPi * deltaTime;

            // Shadow Type Controls
            if (keyboard.IsKeyDown(_keys["shadow type: solid"]))
            {
                _light2D.ShadowType = ShadowType.Solid;
            }

            if (keyboard.IsKeyDown(_keys["shadow type: illuminated"]))
            {
                _light2D.ShadowType = ShadowType.Illuminated;
            }

            if (keyboard.IsKeyDown(_keys["shadow type: occluded"]))
            {
                _light2D.ShadowType = ShadowType.Occluded;
            }

            // Shadow Opacity Controls
            if (keyboard.IsKeyDown(_keys["lower hull opacity"]))
            {
                _krypton.Hulls.ForEach(x => x.Opacity = MathHelper.Clamp(x.Opacity - deltaTime, 0, 1));
            }

            if (keyboard.IsKeyDown(_keys["increase hull opacity"]))
            {
                _krypton.Hulls.ForEach(x => x.Opacity = MathHelper.Clamp(x.Opacity + deltaTime, 0, 1));
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Create a world view projection matrix to use with krypton
            var world = _camera.World;
            var view = _camera.Transformation;
            var projection = _camera.Projection;
            var wvp = world * view * projection;

            // Assign the matrix and pre-render the lightmap.
            // Make sure not to change the position of any lights or shadow hulls after this call, as it won't take effect till the next frame!
            _krypton.Matrix = wvp;
            _krypton.LightMapPrepare();

            // Make sure we clear the backbuffer *after* Krypton is done pre-rendering
            GraphicsDevice.Clear(Color.White);

            // ----- DRAW STUFF HERE ----- //
            // By drawing here, you ensure that your scene is properly lit by krypton.
            // Drawing after KryptonEngine.Draw will cause you objects to be drawn on top of the lightmap (can be useful, fyi)
            // ----- DRAW STUFF HERE ----- //

            // Draw hulls
            DebugDrawHulls(true);

            // Draw krypton (This can be omited if krypton is in the Component list. It will simply draw krypton when base.Draw is called
            _krypton.Draw(gameTime);

            if (Keyboard.GetState().IsKeyDown(_keys["debug draw hulls"]))
            {
                // Draw hulls
                DebugDrawHulls(false);
            }

            if (Keyboard.GetState().IsKeyDown(_keys["debug draw lights"]))
            {
                // Draw hulls
                DebugDrawLights();
            }

            base.Draw(gameTime);

            DrawHelpText();
        }

        private void DebugDrawHulls(bool drawSolid)
        {
            _krypton.RenderHelper.Effect.CurrentTechnique = _krypton.RenderHelper.Effect.Techniques["DebugDraw"];

            GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                FillMode = drawSolid
                    ? FillMode.Solid
                    : FillMode.WireFrame,
            };

            // Clear the helpers vertices
            _krypton.RenderHelper.ShadowHullVertices.Clear();
            _krypton.RenderHelper.ShadowHullIndicies.Clear();

            foreach (var hull in _krypton.Hulls)
            {
                _krypton.RenderHelper.BufferAddShadowHull(hull);
            }


            foreach (var effectPass in _krypton.RenderHelper.Effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                _krypton.RenderHelper.BufferDraw();
            }

        }

        private void DebugDrawLights()
        {
            _krypton.RenderHelper.Effect.CurrentTechnique = _krypton.RenderHelper.Effect.Techniques["DebugDraw"];

            GraphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame,
            };

            // Clear the helpers vertices
            _krypton.RenderHelper.ShadowHullVertices.Clear();
            _krypton.RenderHelper.ShadowHullIndicies.Clear();

            foreach (var light in _krypton.Lights)
            {
                _krypton.RenderHelper.BufferAddBoundOutline(light.Bounds);
            }

            foreach (var effectPass in _krypton.RenderHelper.Effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                _krypton.RenderHelper.BufferDraw();
            }
        }

        private void DrawHelpText()
        {
            spriteBatch.Begin();

            var i = 0;
            var textHeight = _font.MeasureString("A").Y;
            foreach (var keyDef in _keys)
            {
                spriteBatch.DrawString(_font, $"{keyDef.Key} : {keyDef.Value}", new Vector2(0, i * textHeight), Color.White);
                i++;
            }

            spriteBatch.End();
        }
    }
}
