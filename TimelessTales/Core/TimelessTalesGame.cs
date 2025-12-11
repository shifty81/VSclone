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
        private PlayerRenderer? _playerRenderer;
        private UIManager? _uiManager;
        private InputManager? _inputManager;
        private TimeManager? _timeManager;
        private SkyboxRenderer? _skyboxRenderer;
        
        // Camera
        private Camera? _camera;
        
        // Game state
        private bool _isPaused = false;
        private bool _inventoryOpen = false;
        
        public TimelessTalesGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            
            // Set window size
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
            
            Window.Title = "Timeless Tales - Alpha 0.1";
        }

        protected override void Initialize()
        {
            // Initialize camera
            _camera = new Camera(GraphicsDevice.Viewport);
            
            // Initialize input manager
            _inputManager = new InputManager();
            
            // Set screen center for mouse capture
            int centerX = GraphicsDevice.Viewport.Width / 2;
            int centerY = GraphicsDevice.Viewport.Height / 2;
            _inputManager.SetScreenCenter(centerX, centerY);
            
            // Initialize time manager
            _timeManager = new TimeManager();
            
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
            _playerRenderer = new PlayerRenderer(GraphicsDevice);
            _skyboxRenderer = new SkyboxRenderer(GraphicsDevice);
            
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
            
            // Toggle inventory
            if (_inputManager.IsKeyPressed(Keys.I))
                _inventoryOpen = !_inventoryOpen;
            
            // Toggle fullscreen
            if (_inputManager.IsKeyPressed(Keys.F11))
            {
                _graphics.IsFullScreen = !_graphics.IsFullScreen;
                _graphics.ApplyChanges();
                
                // Update screen center for mouse capture
                int centerX = GraphicsDevice.Viewport.Width / 2;
                int centerY = GraphicsDevice.Viewport.Height / 2;
                _inputManager.SetScreenCenter(centerX, centerY);
            }
            
            if (!_isPaused && !_inventoryOpen)
            {
                // Update time
                _timeManager.Update(gameTime);
                
                // Update player
                _player.Update(gameTime, _inputManager, _worldManager);
                
                // Update camera to follow player (with eye height offset)
                _camera.Position = _player.Position + new Vector3(0, 1.62f, 0); // Eye height offset from feet
                _camera.Rotation = _player.Rotation;
                
                // Update world
                _worldManager.Update(_player.Position);
            }
            
            // Always update UI
            _uiManager.Update(gameTime, _player, _isPaused, _inventoryOpen);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Use sky color from time manager instead of static color
            GraphicsDevice.Clear(_timeManager.GetSkyColor());

            // Draw skybox first (before everything else)
            _skyboxRenderer.Draw(_camera, _timeManager);
            
            // Draw 3D world
            _worldRenderer.Draw(_camera, gameTime);
            
            // Draw player arms (first-person view)
            _playerRenderer.Draw(_camera, _player);
            
            // Draw UI (2D overlay)
            _spriteBatch.Begin();
            _uiManager.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
