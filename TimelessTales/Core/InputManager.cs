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

        public InputManager()
        {
            _currentKeyState = Keyboard.GetState();
            _previousKeyState = _currentKeyState;
            _currentMouseState = Mouse.GetState();
            _previousMouseState = _currentMouseState;
        }

        public void Update()
        {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
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
        public int GetMouseDeltaX() => _currentMouseState.X - _previousMouseState.X;
        public int GetMouseDeltaY() => _currentMouseState.Y - _previousMouseState.Y;
    }
}
