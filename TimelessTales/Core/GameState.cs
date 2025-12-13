namespace TimelessTales.Core
{
    /// <summary>
    /// Represents the current state of the game
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Loading,
        Settings,
        Controls,
        TabMenu  // Tabbed menu for character, inventory, crafting, etc.
    }
}
