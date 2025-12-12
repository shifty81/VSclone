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
        private WaterRenderer? _waterRenderer;
        private PlayerRenderer? _playerRenderer;
        private UIManager? _uiManager;
        private InputManager? _inputManager;
        private TimeManager? _timeManager;
        private SkyboxRenderer? _skyboxRenderer;
        private TitleScreen? _titleScreen;
        
        // Camera
        private Camera? _camera;
        
        // Game state
        private GameState _currentState = GameState.MainMenu;
        private bool _isPaused = false;
        private bool _inventoryOpen = false;
        private bool _worldMapOpen = false;
        
        public TimelessTalesGame()
        {
            try
            {
                Logger.Info("Initializing TimelessTalesGame constructor...");
                
                _graphics = new GraphicsDeviceManager(this);
                Content.RootDirectory = "Content";
                IsMouseVisible = true; // Start with mouse visible for title screen
                
                // Set window size
                _graphics.PreferredBackBufferWidth = 1280;
                _graphics.PreferredBackBufferHeight = 720;
                _graphics.IsFullScreen = false;
                _graphics.ApplyChanges();
                
                Window.Title = "Timeless Tales - Alpha 0.1";
                
                Logger.Info("TimelessTalesGame constructor completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Fatal("Error in TimelessTalesGame constructor", ex);
                throw;
            }
        }

        protected override void Initialize()
        {
            try
            {
                Logger.Info("Initializing game systems...");
                
                // Initialize camera
                _camera = new Camera(GraphicsDevice.Viewport);
                Logger.Info("Camera initialized");
                
                // Initialize input manager
                _inputManager = new InputManager();
                Logger.Info("Input manager initialized");
                
                // Set screen center for mouse capture
                int centerX = GraphicsDevice.Viewport.Width / 2;
                int centerY = GraphicsDevice.Viewport.Height / 2;
                _inputManager.SetScreenCenter(centerX, centerY);
                
                // Initialize time manager
                _timeManager = new TimeManager();
                Logger.Info("Time manager initialized");
                
                base.Initialize();
                Logger.Info("Game initialization completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Fatal("Error during game initialization", ex);
                throw;
            }
        }

        protected override void LoadContent()
        {
            try
            {
                Logger.Info("Loading game content...");
                
                _spriteBatch = new SpriteBatch(GraphicsDevice);
                Logger.Info("SpriteBatch created");
                
                // Initialize title screen
                _titleScreen = new TitleScreen(GraphicsDevice);
                _titleScreen.OnNewGame += StartNewGame;
                Logger.Info("Title screen initialized");
                
                Logger.Info("Content loading completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Fatal("Error loading game content", ex);
                throw;
            }
        }
        
        private void StartNewGame()
        {
            try
            {
                Logger.Info("Starting new game...");
                
                _currentState = GameState.Loading;
                IsMouseVisible = false;
                
                // Initialize world manager
                Logger.Info("Initializing world manager with seed 12345...");
                _worldManager = new WorldManager(12345); // Seed for world generation
                _worldManager.Initialize();
                Logger.Info("World manager initialized");
                
                // Create player at spawn position
                Vector3 spawnPosition = _worldManager!.GetSpawnPosition();
                Logger.Info($"Creating player at spawn position: {spawnPosition}");
                _player = new Player(spawnPosition);
                _camera!.Position = spawnPosition;
                Logger.Info("Player created");
                
                // Initialize renderer
                Logger.Info("Initializing renderers...");
                _worldRenderer = new WorldRenderer(GraphicsDevice, _worldManager);
                _waterRenderer = new WaterRenderer(GraphicsDevice, _worldManager);
                _playerRenderer = new PlayerRenderer(GraphicsDevice);
                _skyboxRenderer = new SkyboxRenderer(GraphicsDevice);
                Logger.Info("Renderers initialized");
                
                // Initialize UI
                Logger.Info("Initializing UI manager...");
                _uiManager = new UIManager(_spriteBatch!, Content);
                Logger.Info("UI manager initialized");
                
                _currentState = GameState.Playing;
                Logger.Info("New game started successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error starting new game", ex);
                // Return to main menu on error
                _currentState = GameState.MainMenu;
                IsMouseVisible = true;
                throw;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            try
            {
                // Handle exit
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                    Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    if (_currentState == GameState.Playing)
                    {
                        // Return to main menu from game
                        Logger.Info("Returning to main menu");
                        _currentState = GameState.MainMenu;
                        IsMouseVisible = true;
                    }
                    else if (_currentState == GameState.MainMenu)
                    {
                        Logger.Info("Exiting game");
                        Exit();
                    }
                }

                if (_currentState == GameState.MainMenu)
                {
                    _titleScreen!.Update(gameTime);
                }
                else if (_currentState == GameState.Playing)
                {
                    // Update input
                    _inputManager!.Update();
                    
                    // Toggle pause
                    if (_inputManager.IsKeyPressed(Keys.P))
                        _isPaused = !_isPaused;
                    
                    // Toggle inventory
                    if (_inputManager.IsKeyPressed(Keys.I))
                    {
                        _inventoryOpen = !_inventoryOpen;
                        IsMouseVisible = _inventoryOpen;
                        _inputManager.SetMouseCaptured(!_inventoryOpen);
                    }
                    
                    // Toggle world map
                    if (_inputManager.IsKeyPressed(Keys.M))
                    {
                        _worldMapOpen = !_worldMapOpen;
                        IsMouseVisible = _worldMapOpen;
                        _inputManager.SetMouseCaptured(!_worldMapOpen);
                    }
                    
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
                    
                    if (!_isPaused && !_inventoryOpen && !_worldMapOpen)
                    {
                        // Update time
                        _timeManager!.Update(gameTime);
                        
                        // Update player
                        _player!.Update(gameTime, _inputManager, _worldManager!);
                        
                        // Update camera to follow player (with eye height offset)
                        _camera!.Position = _player.Position + new Vector3(0, 1.62f, 0); // Eye height offset from feet
                        _camera.Rotation = _player.Rotation;
                        
                        // Update world
                        _worldManager!.Update(_player.Position);
                        
                        // Update water renderer (for wave animation)
                        _waterRenderer!.Update(gameTime);
                    }
                    
                    // Always update UI (pass InputManager for mouse interaction)
                    _uiManager!.Update(gameTime, _player!, _timeManager!, _worldManager!, _isPaused, _inventoryOpen, _worldMapOpen, _inputManager);
                }

                base.Update(gameTime);
            }
            catch (Exception ex)
            {
                Logger.Error("Error in game update loop", ex);
                // Don't crash the game, just log the error and continue
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            try
            {
                if (_currentState == GameState.MainMenu)
                {
                    _titleScreen!.Draw(_spriteBatch!);
                }
                else if (_currentState == GameState.Playing)
                {
                    // Use sky color from time manager instead of static color
                    GraphicsDevice.Clear(_timeManager!.GetSkyColor());

                    // Draw skybox first (before everything else)
                    _skyboxRenderer!.Draw(_camera!, _timeManager);
                    
                    // Draw 3D world (opaque blocks)
                    _worldRenderer!.Draw(_camera!, gameTime);
                    
                    // Draw translucent water (after opaque blocks)
                    _waterRenderer!.Draw(_camera!);
                    
                    // Draw player arms (first-person view)
                    _playerRenderer!.Draw(_camera!, _player!);
                    
                    // Draw UI (2D overlay)
                    _spriteBatch!.Begin();
                    try
                    {
                        _uiManager!.Draw(_spriteBatch);
                    }
                    finally
                    {
                        _spriteBatch.End();
                    }
                }

                base.Draw(gameTime);
            }
            catch (Exception ex)
            {
                Logger.Error("Error in game draw loop", ex);
                // Don't crash the game, just log the error and continue
            }
        }
    }
}
