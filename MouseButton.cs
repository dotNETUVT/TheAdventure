namespace TheAdventure;

/// @enum MouseButton
/// @brief Defines identifiers for mouse buttons recognized in the game.
///
/// This enumeration lists all mouse buttons that can be pressed and detected within the game context,
/// including standard buttons like primary (left click) and secondary (right click), as well as additional
/// buttons for mice that support more inputs.
public enum MouseButton : byte
{
    /// @brief The primary mouse button, usually the left button.
    Primary = 1,
    /// @brief The middle mouse button, often integrated with the scroll wheel.
    Middle,
    /// @brief The secondary mouse button, usually the right button.
    Secondary,
    Button4,
    Button5,
    Button6,
    Button7,
    Button8,
    Button9,
    /// @brief Represents the count of mouse buttons recognized by the game.
    Count,
}
