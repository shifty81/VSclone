using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TimelessTales.Core
{
    /// <summary>
    /// Manages input state tracking for keyboard and mouse
    /// </summary>
    public class InputManager
    {
        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private Point _screenCenter;
        private bool _mouseCaptured;
        private int _mouseDeltaX;
        private int _mouseDeltaY;

        public InputManager()
        {
            _currentKeyState = Keyboard.GetState();
            _previousKeyState = _currentKeyState;
            _currentMouseState = Mouse.GetState();
            _previousMouseState = _currentMouseState;
            _mouseCaptured = true;
            _mouseDeltaX = 0;
            _mouseDeltaY = 0;
        }

        public void Update()
        {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
            
            // Calculate mouse delta relative to screen center for FPS camera
            // This ensures consistent delta calculation even when mouse is re-centered
            if (_mouseCaptured && _screenCenter != Point.Zero)
            {
                _mouseDeltaX = _currentMouseState.X - _screenCenter.X;
                _mouseDeltaY = _currentMouseState.Y - _screenCenter.Y;
                Mouse.SetPosition(_screenCenter.X, _screenCenter.Y);
            }
            else
            {
                _mouseDeltaX = _currentMouseState.X - _previousMouseState.X;
                _mouseDeltaY = _currentMouseState.Y - _previousMouseState.Y;
            }
        }
        
        public void SetScreenCenter(int x, int y)
        {
            _screenCenter = new Point(x, y);
        }
        
        public void SetMouseCaptured(bool captured)
        {
            _mouseCaptured = captured;
        }

        public bool IsKeyDown(Keys key)
        {
            return _currentKeyState.IsKeyDown(key);
        }

        public bool IsKeyPressed(Keys key)
        {
            return _currentKeyState.IsKeyDown(key) && !_previousKeyState.IsKeyDown(key);
        }

        public bool IsLeftMousePressed()
        {
            return _currentMouseState.LeftButton == ButtonState.Pressed && 
                   _previousMouseState.LeftButton == ButtonState.Released;
        }

        public bool IsRightMousePressed()
        {
            return _currentMouseState.RightButton == ButtonState.Pressed && 
                   _previousMouseState.RightButton == ButtonState.Released;
        }

        public bool IsLeftMouseDown()
        {
            return _currentMouseState.LeftButton == ButtonState.Pressed;
        }

        public bool IsRightMouseDown()
        {
            return _currentMouseState.RightButton == ButtonState.Pressed;
        }

        public int GetMouseX() => _currentMouseState.X;
        public int GetMouseY() => _currentMouseState.Y;
        public int GetMouseDeltaX() => _mouseDeltaX;
        public int GetMouseDeltaY() => _mouseDeltaY;
    }
}
