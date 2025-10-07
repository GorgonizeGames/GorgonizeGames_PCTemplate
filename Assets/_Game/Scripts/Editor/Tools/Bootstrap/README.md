Optional: Toolbar Buttons

For visual toolbar buttons, install **Unity Toolbar Extender**:

**Installation:**
1. Open Package Manager (Window → Package Manager)
2. Click `+` (top-left)
3. Select "Add package from git URL"
4. Paste: `https://github.com/marijnz/unity-toolbar-extender.git`
5. Click "Add"

**Without Toolbar Extender:**
- F5/F6 shortcuts still work
- Menu items still available
- No compile errors

---

## How It Works

1. **Play Mode**: Saves current scene, opens Bootstrap, starts play
2. **Stop Play**: Returns to your previous scene automatically
3. **Scene Switch**: Opens Bootstrap without playing

## Configuration

Edit `PlayFromBootstrapToolbar.cs` to change Bootstrap scene path:
```csharp
private const string BOOTSTRAP_SCENE_PATH = "Assets/_Game/Scenes/Bootstrap.unity";