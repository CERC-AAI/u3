using UnityEngine;

public class TargettingCursor : MonoBehaviour
{
    public Texture2D cursorTexture;  // The texture to use for the cursor
    public Vector2 cursorSize = new Vector2(32, 32);  // The size of the cursor texture

    void Start()
    {
        // Hide the default cursor
        Cursor.visible = false;
    }

    void OnGUI()
    {
        // Draw the custom cursor texture at the current mouse position
        GUI.DrawTexture(new Rect((Screen.width - cursorSize.x) / 2, (Screen.height - cursorSize.y) / 2, cursorSize.x, cursorSize.y), cursorTexture);
    }
}