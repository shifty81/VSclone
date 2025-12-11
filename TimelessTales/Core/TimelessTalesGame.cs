using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TimelessTales.World;
using TimelessTales.Entities;
using TimelessTales.Rendering;
using TimelessTales.UI;

namespace TimelessTales.Core
{
    /// <summary>
    /// Main game class for Timeless Tales - A Vintage Story inspired survival game
    /// </summary>
    public class TimelessTalesGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch? _spriteBatch;
        
        // Core game systems
        private WorldManager? _worldManager;
        private Player? _player;
        private WorldRenderer? _worldRenderer;
        private UIManager? _uiManager;
        private InputManager? _inputManager;
        
        // Camera
        private Camera? _camera;
        
        // Game state
        private bool _isPaused = false;
        
        public TimelessTalesGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            
            // Set window size
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            
            Window.Title = "Timeless Tales - Alpha 0.1";
        }

        protected override void Initialize()
        {
            // Initialize camera
            _camera = new Camera(GraphicsDevice.Viewport);
            
            // Initialize input manager
            _inputManager = new InputManager();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Initialize world manager
            _worldManager = new WorldManager(12345); // Seed for world generation
            _worldManager.Initialize();
            
            // Create player at spawn position
            Vector3 spawnPosition = _worldManager.GetSpawnPosition();
            _player = new Player(spawnPosition);
            _camera.Position = spawnPosition;
            
            // Initialize renderer
            _worldRenderer = new WorldRenderer(GraphicsDevice, _worldManager);
            
            // Initialize UI
            _uiManager = new UIManager(_spriteBatch, Content);
        }

        protected override void Update(GameTime gameTime)
        {
            // Handle exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Update input
            _inputManager.Update();
            
            // Toggle pause
            if (_inputManager.IsKeyPressed(Keys.P))
                _isPaused = !_isPaused;
            
            if (!_isPaused)
            {
                // Update player
                _player.Update(gameTime, _inputManager, _worldManager);
                
                // Update camera to follow player
                _camera.Position = _player.Position;
                _camera.Rotation = _player.Rotation;
                
                // Update world
                _worldManager.Update(_player.Position);
            }
            
            // Always update UI
            _uiManager.Update(gameTime, _player, _isPaused);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw 3D world
            _worldRenderer.Draw(_camera, gameTime);
            
            // Draw UI (2D overlay)
            _spriteBatch.Begin();
            _uiManager.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
