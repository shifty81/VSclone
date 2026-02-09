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
        private UnderwaterEffectRenderer? _underwaterEffectRenderer;
        private PlayerRenderer? _playerRenderer;
        private UIManager? _uiManager;
        private InputManager? _inputManager;
        private TimeManager? _timeManager;
        private SkyboxRenderer? _skyboxRenderer;
        private TitleScreen? _titleScreen;
        private SettingsMenu? _settingsMenu;
        private ControlsScreen? _controlsScreen;
        private CharacterStatusDisplay? _characterStatusDisplay;
        private PauseMenu? _pauseMenu;
        private DebugOverlay? _debugOverlay;
        private TabMenu? _tabMenu;
        
        // Particle and audio systems
        private Particles.ParticleRenderer? _particleRenderer;
        private Particles.ParticleEmitter? _bubbleEmitter;
        private Particles.ParticleEmitter? _splashEmitter;
        private List<Particles.ParticleEmitter>? _allEmitters;
        private Audio.AudioManager? _audioManager;
        
        // Camera
        private Camera? _camera;
        
        // Game state
        private GameState _currentState = GameState.MainMenu;
        private bool _isPaused = false;
        private bool _inventoryOpen = false;
        private bool _worldMapOpen = false;
        
        // Water state tracking
        private bool _wasUnderwaterLastFrame = false;
        private bool _wasInWaterLastFrame = false;
        private float _splashEmitterTimer = 0f;
        
        // Water effect constants
        private const float SPLASH_EMISSION_DURATION = 0.1f; // 100ms burst
        private const float WATER_ENTRY_SPLASH_HEIGHT = 1.0f;
        private const float WATER_EXIT_SPLASH_HEIGHT = 0.5f;
        private const float PLAYER_HEAD_HEIGHT = 1.6f; // Should match PLAYER_EYE_HEIGHT in Player class
        
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
                _titleScreen.OnSettings += ShowSettings;
                Logger.Info("Title screen initialized");
                
                // Initialize settings menu
                _settingsMenu = new SettingsMenu(GraphicsDevice);
                _settingsMenu.OnBack += HideSettings;
                _settingsMenu.OnControls += ShowControls;
                Logger.Info("Settings menu initialized");
                
                // Initialize controls screen
                _controlsScreen = new ControlsScreen(GraphicsDevice);
                _controlsScreen.OnBack += HideControls;
                Logger.Info("Controls screen initialized");
                
                // Initialize pause menu
                _pauseMenu = new PauseMenu(GraphicsDevice);
                _pauseMenu.OnResume += ResumGame;
                _pauseMenu.OnMainMenu += ReturnToMainMenuFromPause;
                Logger.Info("Pause menu initialized");
                
                // Initialize tab menu
                _tabMenu = new TabMenu(GraphicsDevice);
                Logger.Info("Tab menu initialized");
                
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
                _underwaterEffectRenderer = new UnderwaterEffectRenderer(GraphicsDevice);
                _playerRenderer = new PlayerRenderer(GraphicsDevice);
                _skyboxRenderer = new SkyboxRenderer(GraphicsDevice);
                Logger.Info("Renderers initialized");
                
                // Initialize UI
                Logger.Info("Initializing UI manager...");
                _uiManager = new UIManager(_spriteBatch!, Content);
                
                // Initialize character status display
                _characterStatusDisplay = new CharacterStatusDisplay(GraphicsDevice);
                
                // Initialize debug overlay
                _debugOverlay = new DebugOverlay(GraphicsDevice);
                Logger.Info("UI manager, character status display, and debug overlay initialized");
                
                // Initialize particle system
                Logger.Info("Initializing particle system...");
                _particleRenderer = new Particles.ParticleRenderer(GraphicsDevice);
                
                // Bubble emitter - for when player is underwater
                // Enhanced with wobble motion, size variation, and periodic burst emission
                _bubbleEmitter = new Particles.ParticleEmitter(Vector3.Zero)
                {
                    EmissionRate = 3.0f,
                    ParticleLifetime = 2.5f,
                    ParticleSize = 0.08f,
                    ParticleColor = new Color(200, 220, 255, 180),
                    VelocityBase = new Vector3(0, 0.5f, 0),
                    VelocityVariation = new Vector3(0.1f, 0.2f, 0.1f),
                    IsActive = false, // Start disabled
                    EnableWobble = true,
                    WobbleAmplitude = 0.3f,
                    WobbleFrequency = 4.0f,
                    SizeVariation = 0.4f, // 40% size variation for natural look
                    SurfacePopY = 64.0f, // Pop at sea level
                    UseBurstMode = true, // Periodic bursts from mouth
                    BurstInterval = 3.0f,
                    BurstCount = 5
                };
                
                // Splash emitter - for water entry/exit
                _splashEmitter = new Particles.ParticleEmitter(Vector3.Zero)
                {
                    EmissionRate = 50.0f, // Burst emission
                    ParticleLifetime = 0.8f,
                    ParticleSize = 0.12f,
                    ParticleColor = new Color(150, 190, 230, 200),
                    VelocityBase = new Vector3(0, 2.0f, 0),
                    VelocityVariation = new Vector3(1.5f, 1.0f, 1.5f),
                    IsActive = false // Start disabled
                };
                
                // List of all particle emitters
                _allEmitters = new List<Particles.ParticleEmitter> { _bubbleEmitter, _splashEmitter };
                
                Logger.Info("Particle system initialized");
                
                // Initialize audio system
                Logger.Info("Initializing audio system...");
                _audioManager = new Audio.AudioManager();
                Logger.Info("Audio system initialized (sound files not loaded yet)");
                
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
        
        private void ShowSettings()
        {
            Logger.Info("Showing settings menu");
            _currentState = GameState.Settings;
        }
        
        private void HideSettings()
        {
            Logger.Info("Hiding settings menu");
            _currentState = GameState.MainMenu;
        }
        
        private void ShowControls()
        {
            Logger.Info("Showing controls screen");
            _currentState = GameState.Controls;
        }
        
        private void HideControls()
        {
            Logger.Info("Hiding controls screen");
            _currentState = GameState.Settings;
        }
        
        private void ResumGame()
        {
            Logger.Info("Resuming game");
            _isPaused = false;
            IsMouseVisible = false;
            _inputManager!.SetMouseCaptured(true);
        }
        
        private void ReturnToMainMenuFromPause()
        {
            Logger.Info("Returning to main menu from pause");
            _currentState = GameState.MainMenu;
            _isPaused = false;
            IsMouseVisible = true;
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
                    else if (_currentState == GameState.Settings)
                    {
                        HideSettings();
                    }
                    else if (_currentState == GameState.Controls)
                    {
                        HideControls();
                    }
                    else if (_currentState == GameState.TabMenu)
                    {
                        // Close tab menu and return to playing
                        Logger.Info("Closing tab menu");
                        _currentState = GameState.Playing;
                        IsMouseVisible = false;
                        _inputManager!.SetMouseCaptured(true);
                    }
                }

                if (_currentState == GameState.MainMenu)
                {
                    _titleScreen!.Update(gameTime);
                }
                else if (_currentState == GameState.Settings)
                {
                    _settingsMenu!.Update(gameTime);
                }
                else if (_currentState == GameState.Controls)
                {
                    _controlsScreen!.Update(gameTime);
                }
                else if (_currentState == GameState.TabMenu)
                {
                    _inputManager!.Update();
                    _tabMenu!.Update(_inputManager);
                }
                else if (_currentState == GameState.Playing)
                {
                    // Update input
                    _inputManager!.Update();
                    
                    // Toggle pause
                    if (_inputManager.IsKeyPressed(Keys.P))
                    {
                        _isPaused = !_isPaused;
                        IsMouseVisible = _isPaused;
                        _inputManager.SetMouseCaptured(!_isPaused);
                    }
                    
                    // Toggle debug overlay (F3)
                    if (_inputManager.IsKeyPressed(Keys.F3))
                    {
                        _debugOverlay!.IsVisible = !_debugOverlay.IsVisible;
                    }
                    
                    // Update pause menu if paused
                    if (_isPaused)
                    {
                        _pauseMenu!.Update(gameTime);
                    }
                    
                    // Toggle inventory (legacy simple inventory)
                    if (_inputManager.IsKeyPressed(Keys.I))
                    {
                        _inventoryOpen = !_inventoryOpen;
                        IsMouseVisible = _inventoryOpen;
                        _inputManager.SetMouseCaptured(!_inventoryOpen);
                    }
                    
                    // Toggle tab menu (C for Character sheet / comprehensive menu)
                    if (_inputManager.IsKeyPressed(Keys.C))
                    {
                        _currentState = GameState.TabMenu;
                        IsMouseVisible = true;
                        _inputManager.SetMouseCaptured(false);
                        _inventoryOpen = false; // Close simple inventory
                        _worldMapOpen = false; // Close world map
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
                        
                        // Update vegetation growth
                        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                        _worldManager.VegetationManager.Update(deltaTime);
                        
                        // Update water renderer (for wave animation)
                        _waterRenderer!.Update(gameTime);
                        
                        // Update debug overlay
                        _debugOverlay!.Update(gameTime);
                        
                        // Update water-related audio and particles
                        UpdateWaterEffects(deltaTime);
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
        
        private void UpdateWaterEffects(float deltaTime)
        {
            if (_player == null || _audioManager == null || _bubbleEmitter == null || _splashEmitter == null || _particleRenderer == null)
                return;
            
            bool isUnderwater = _player.IsUnderwater;
            float submersionDepth = _player.SubmersionDepth;
            bool isInWater = submersionDepth > 0;
            
            // Update audio underwater state with depth information
            _audioManager.IsUnderwater = isUnderwater;
            _audioManager.SubmersionDepth = submersionDepth;
            
            // Handle entering water
            if (isInWater && !_wasInWaterLastFrame)
            {
                Logger.Info("Player entered water - creating splash particles");
                // Create splash particles at water entry point
                Vector3 splashPosition = _player.Position + new Vector3(0, WATER_ENTRY_SPLASH_HEIGHT, 0);
                _splashEmitter.Position = splashPosition;
                _splashEmitter.IsActive = true;
                _splashEmitterTimer = SPLASH_EMISSION_DURATION; // Start burst timer
                // TODO: Play splash sound when sound files are available
                // _audioManager.PlaySound("water_splash", volume: 1.0f);
            }
            
            // Handle exiting water
            if (!isInWater && _wasInWaterLastFrame)
            {
                Logger.Info("Player exited water - creating splash particles");
                // Create splash particles at water exit point
                Vector3 splashPosition = _player.Position + new Vector3(0, WATER_EXIT_SPLASH_HEIGHT, 0);
                _splashEmitter.Position = splashPosition;
                _splashEmitter.IsActive = true;
                _splashEmitterTimer = SPLASH_EMISSION_DURATION; // Start burst timer
                // TODO: Play splash sound when sound files are available
                // _audioManager.PlaySound("water_splash", volume: 0.8f);
            }
            
            // Update splash emitter timer for burst effect
            if (_splashEmitterTimer > 0)
            {
                _splashEmitterTimer -= deltaTime;
                if (_splashEmitterTimer <= 0)
                {
                    _splashEmitter.IsActive = false;
                }
            }
            
            // Handle going underwater
            if (isUnderwater && !_wasUnderwaterLastFrame)
            {
                Logger.Info("Player went underwater");
                // Start bubble particles
                _bubbleEmitter.IsActive = true;
                // TODO: Start underwater ambient sound when sound files are available
                // _audioManager.PlayLoopingSound("underwater_ambience", volume: 0.3f);
            }
            
            // Handle surfacing
            if (!isUnderwater && _wasUnderwaterLastFrame)
            {
                Logger.Info("Player surfaced");
                // Stop bubble particles
                _bubbleEmitter.IsActive = false;
                _bubbleEmitter.Clear();
                // TODO: Stop underwater ambient sound when sound files are available
                // _audioManager.StopLoopingSound("underwater_ambience");
            }
            
            // Update bubble emitter position to player's head
            if (isUnderwater)
            {
                Vector3 headPosition = _player.Position + new Vector3(0, PLAYER_HEAD_HEIGHT, 0);
                _bubbleEmitter.Position = headPosition;
            }
            
            // Update all particle emitters
            _bubbleEmitter.Update(deltaTime);
            _splashEmitter.Update(deltaTime);
            
            // Update audio manager with smooth transitions
            _audioManager.Update(deltaTime);
            
            // Store state for next frame
            _wasUnderwaterLastFrame = isUnderwater;
            _wasInWaterLastFrame = isInWater;
        }

        protected override void Draw(GameTime gameTime)
        {
            try
            {
                if (_currentState == GameState.MainMenu)
                {
                    _titleScreen!.Draw(_spriteBatch!);
                }
                else if (_currentState == GameState.Settings)
                {
                    // Draw dark background
                    GraphicsDevice.Clear(new Color(20, 30, 40));
                    
                    _spriteBatch!.Begin();
                    try
                    {
                        _settingsMenu!.Draw(_spriteBatch);
                    }
                    finally
                    {
                        _spriteBatch.End();
                    }
                }
                else if (_currentState == GameState.Controls)
                {
                    // Draw dark background
                    GraphicsDevice.Clear(new Color(20, 30, 40));
                    
                    _spriteBatch!.Begin();
                    try
                    {
                        _controlsScreen!.Draw(_spriteBatch);
                    }
                    finally
                    {
                        _spriteBatch.End();
                    }
                }
                else if (_currentState == GameState.TabMenu)
                {
                    // Draw the game world in the background (slightly darkened by tab menu overlay)
                    GraphicsDevice.Clear(_timeManager!.GetSkyColor());
                    _skyboxRenderer!.Draw(_camera!, _timeManager);
                    _worldRenderer!.Draw(_camera!, gameTime);
                    _waterRenderer!.Draw(_camera!);
                    _playerRenderer!.Draw(_camera!, _player!);
                    
                    // Draw tab menu overlay
                    _spriteBatch!.Begin();
                    try
                    {
                        _tabMenu!.Draw(_spriteBatch, _player!);
                    }
                    finally
                    {
                        _spriteBatch.End();
                    }
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
                    
                    // Draw particles (bubbles, splashes, etc.) after 3D world but before underwater effects
                    if (_particleRenderer != null && _allEmitters != null && _camera != null)
                    {
                        _particleRenderer.Draw(_camera, _allEmitters);
                    }
                    
                    // Draw underwater effects overlay (after 3D rendering and particles, before UI)
                    _underwaterEffectRenderer!.Draw(_player!);
                    
                    // Draw UI (2D overlay)
                    _spriteBatch!.Begin();
                    try
                    {
                        _uiManager!.Draw(_spriteBatch);
                        
                        // Draw character status display (health, hunger, thirst)
                        if (!_inventoryOpen && !_worldMapOpen && !_isPaused)
                        {
                            _characterStatusDisplay!.Draw(_spriteBatch, _player!);
                        }
                        
                        // Draw pause menu on top of everything if paused
                        if (_isPaused)
                        {
                            _pauseMenu!.Draw(_spriteBatch);
                        }
                        
                        // Draw debug overlay (F3) on top of everything
                        if (_debugOverlay!.IsVisible)
                        {
                            _debugOverlay.Draw(_spriteBatch, _player!, _worldManager!);
                        }
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
