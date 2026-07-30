# Homestead Separation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Separate the Homestead from the Building prefab into its own lightweight component with merged sprite assets.

**Architecture:** New `Homestead` component replaces the Homestead's use of `Building`. An Editor script merges the 21 child sprites from `BuildingPreBuildingStage.prefab` and the town sprites from `Building.prefab` into two single PNGs. BuildSign references `Homestead` instead of `Building`.

**Tech Stack:** Unity 6, C#, URP 2D, UnityEditor for sprite merge tool

## Global Constraints

- No comments in code unless explicitly requested
- No ScriptableObjects for game data
- No dependency injection
- Rules/ must be pure C#
- UI is IMGUI (except RecipeBook)
- Follow project singleton pattern for managers
- Use GameEvents for cross-system communication

---

### Task 1: Create Homestead Component

**Files:**
- Create: `Assets/Scripts/Homestead.cs`
- Create: `Assets/Tests/EditMode/HomesteadTests.cs`

**Interfaces:**
- Consumes: `IInteractable` interface, `InteractType` enum
- Produces: `Homestead` class with `bool IsBuilt`, `void SetBuilt()`, `InteractType InteractType`, `void Interact()`

- [ ] **Step 1: Write the failing tests**

```csharp
using NUnit.Framework;
using Lamplight.TestSupport;
using UnityEngine;

public class HomesteadTests
{
    private Homestead _homestead;
    private GameObject _go;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _go = TestBootstrap.CreateGameObject("TestHomestead");
        var sr = _go.AddComponent<SpriteRenderer>();
        _homestead = _go.AddComponent<Homestead>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void IsBuilt_DefaultsToFalse()
    {
        Assert.IsFalse(_homestead.IsBuilt);
    }

    [Test]
    public void SetBuilt_SetsIsBuiltToTrue()
    {
        _homestead.SetBuilt();
        Assert.IsTrue(_homestead.IsBuilt);
    }

    [Test]
    public void SetBuilt_SwapsSpriteToBuiltSprite()
    {
        var builtTex = new Texture2D(16, 16);
        builtTex.SetPixel(0, 0, Color.red);
        builtTex.Apply();
        var builtSprite = Sprite.Create(builtTex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);

        _homestead.SetBuiltSpriteForTest(builtSprite);
        _homestead.SetBuilt();

        Assert.AreEqual(builtSprite, _go.GetComponent<SpriteRenderer>().sprite);
    }

    [Test]
    public void InteractType_IsBuilding()
    {
        Assert.AreEqual(InteractType.Building, _homestead.InteractType);
    }

    [Test]
    public void Interact_WhenBuilt_DoesNotThrow()
    {
        _homestead.SetBuilt();
        Assert.DoesNotThrow(() => _homestead.Interact());
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run via Unity MCP or command line. Expected: compile error — `Homestead` class does not exist.

- [ ] **Step 3: Write the Homestead implementation**

```csharp
using UnityEngine;

public class Homestead : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite builtSprite;

    public bool IsBuilt { get; private set; }

    public InteractType InteractType => InteractType.Building;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetBuilt()
    {
        IsBuilt = true;
        if (_spriteRenderer != null && builtSprite != null)
            _spriteRenderer.sprite = builtSprite;
    }

    public void Interact() { }

    public void SetBuiltSpriteForTest(Sprite sprite)
    {
        builtSprite = sprite;
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Expected: All 5 Homestead tests PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Homestead.cs Assets/Tests/EditMode/HomesteadTests.cs
git commit -m "feat: add Homestead component with tests"
```

---

### Task 2: Update BuildSign to Use Homestead

**Files:**
- Modify: `Assets/Scripts/BuildSign.cs:13` (change field type)
- Modify: `Assets/Scripts/BuildSign.cs:107-118` (change CompleteBuild)
- Modify: `Assets/Scripts/BuildSign.cs:120-154` (change Create factory)
- Modify: `Assets/Tests/EditMode/BuildSignTests.cs:10,22-24,81-93` (use Homestead)

**Interfaces:**
- Consumes: `Homestead` from Task 1
- Produces: `BuildSign.homestead` field of type `Homestead`

- [ ] **Step 1: Write the failing test changes**

Update `BuildSignTests.cs`:

```csharp
using NUnit.Framework;
using Lamplight.TestSupport;
using UnityEngine;

public class BuildSignTests
{
    private InventoryManager _inventory;
    private BuildSign _sign;
    private GameObject _homesteadGo;
    private Homestead _homestead;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        var signGo = TestBootstrap.CreateGameObject("TestSign");
        _sign = signGo.AddComponent<BuildSign>();

        _homesteadGo = TestBootstrap.CreateGameObject("TestHomestead");
        _homestead = _homesteadGo.AddComponent<Homestead>();
        _homesteadGo.SetActive(false);
        _sign.homestead = _homestead;
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void Interact_NoStone_StaysAtSite()
    {
        _sign.Interact();

        Assert.AreEqual(BuildStage.Site, _sign.Stage);
    }

    [Test]
    public void Interact_WithStone_AdvancesToFoundation()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();

        Assert.AreEqual(BuildStage.Foundation, _sign.Stage);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Stone));
    }

    [Test]
    public void Interact_WithWood_AdvancesToFrame()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _sign.Interact();

        Assert.AreEqual(BuildStage.Frame, _sign.Stage);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Wood));
    }

    [Test]
    public void Interact_WithWoodAndNails_AdvancesToWalls()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 2);
        _inventory.TryAdd(ContentDb.Nails, 3);
        _sign.Interact();

        Assert.AreEqual(BuildStage.Walls, _sign.Stage);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Wood));
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Nails));
    }

    [Test]
    public void Interact_CompleteBuild_SetsHomesteadBuilt()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 2);
        _inventory.TryAdd(ContentDb.Nails, 3);
        _sign.Interact();

        Assert.IsTrue(_homestead.IsBuilt);
        Assert.IsTrue(_homesteadGo.activeSelf);
        Assert.IsFalse(_sign.gameObject.activeSelf);
    }

    [Test]
    public void Interact_FrameWithoutNails_StaysAtFrame()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 2);
        _sign.Interact();

        Assert.AreEqual(BuildStage.Frame, _sign.Stage);
    }

    [Test]
    public void HomesteadBuildStageChanged_FiresOnAdvance()
    {
        int firedStage = -1;
        GameEvents.HomesteadBuildStageChanged += s => firedStage = s;

        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();

        Assert.AreEqual(1, firedStage);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Expected: compile error — `BuildSign` still has `Building homesteadBuilding` field.

- [ ] **Step 3: Update BuildSign.cs**

Full replacement of `BuildSign.cs`:

```csharp
using UnityEngine;

public enum BuildStage
{
    Site = 0,
    Foundation = 1,
    Frame = 2,
    Walls = 3
}

public class BuildSign : MonoBehaviour, IInteractable
{
    [SerializeField] internal Homestead homestead;
    [SerializeField] private GameObject siteVisual;

    private SpriteRenderer _spriteRenderer;
    private BuildStage _stage;
    private static readonly Color[] _stageColors = {
        new Color(0.7f, 0.6f, 0.4f),
        new Color(0.6f, 0.6f, 0.6f),
        new Color(0.55f, 0.35f, 0.15f),
        new Color(0.8f, 0.7f, 0.5f),
    };
    private static readonly float[] _stageScales = { 1f, 2f, 4f, 6f };

    public BuildStage Stage => _stage;
    public InteractType InteractType => InteractType.Building;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _spriteRenderer.color = _stageColors[0];
        RefreshSiteVisual();
    }

    public void Interact()
    {
        if (InventoryManager.Instance == null) return;

        switch (_stage)
        {
            case BuildStage.Site:
                if (!InventoryManager.Instance.Has(ContentDb.Stone, 3))
                {
                    GameEvents.OnToastRequested("Need 3 Stone to build the foundation");
                    return;
                }
                InventoryManager.Instance.TryRemove(ContentDb.Stone, 3);
                AdvanceStage(BuildStage.Foundation, "Foundation built!");
                break;

            case BuildStage.Foundation:
                if (!InventoryManager.Instance.Has(ContentDb.Wood, 3))
                {
                    GameEvents.OnToastRequested("Need 3 Wood to build the frame");
                    return;
                }
                InventoryManager.Instance.TryRemove(ContentDb.Wood, 3);
                AdvanceStage(BuildStage.Frame, "Frame built!");
                break;

            case BuildStage.Frame:
                if (!InventoryManager.Instance.Has(ContentDb.Wood, 2) ||
                    !InventoryManager.Instance.Has(ContentDb.Nails, 3))
                {
                    GameEvents.OnToastRequested("Need 2 Wood and 3 Nails to build the walls");
                    return;
                }
                InventoryManager.Instance.TryRemove(ContentDb.Wood, 2);
                InventoryManager.Instance.TryRemove(ContentDb.Nails, 3);
                AdvanceStage(BuildStage.Walls, "Homestead built!");
                CompleteBuild();
                break;

            case BuildStage.Walls:
                break;
        }
    }

    private void AdvanceStage(BuildStage newStage, string toast)
    {
        _stage = newStage;
        if (_spriteRenderer != null && (int)_stage < _stageColors.Length)
            _spriteRenderer.color = _stageColors[(int)_stage];
        if ((int)_stage < _stageScales.Length)
        {
            float s = _stageScales[(int)_stage];
            transform.localScale = new Vector3(s, s, 1f);
            foreach (var c in GetComponents<BoxCollider2D>())
                c.size = new Vector2(0.8f, 1.0f);
        }
        RefreshSiteVisual();
        GameEvents.OnHomesteadBuildStageChanged((int)_stage);
        GameEvents.OnToastRequested(toast);
    }

    private void RefreshSiteVisual()
    {
        bool showSiteVisual = _stage == BuildStage.Site;
        if (siteVisual != null)
            siteVisual.SetActive(showSiteVisual);
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = !showSiteVisual;
    }

    private void CompleteBuild()
    {
        if (homestead != null)
        {
            homestead.gameObject.SetActive(true);
            homestead.SetBuilt();
        }
        gameObject.SetActive(false);
    }

    public static BuildSign Create(Vector3 position, Homestead homestead = null)
    {
        var go = new GameObject("BuildSign");
        go.transform.position = position;

        var tex = new Texture2D(16, 16);
        var pixels = new Color32[256];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels32(pixels);
        tex.Apply();

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, 16, 16),
            new Vector2(0.5f, 0.5f),
            16f);
        sr.color = new Color(0.7f, 0.6f, 0.4f);
        sr.sortingOrder = 5;

        var solid = go.AddComponent<BoxCollider2D>();
        solid.size = new Vector2(0.8f, 1.0f);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 1.0f);

        go.layer = LayerMask.NameToLayer("Interactable");

        var sign = go.AddComponent<BuildSign>();
        sign._spriteRenderer = sr;
        sign.homestead = homestead;

        return sign;
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Expected: All BuildSign and Homestead tests PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/BuildSign.cs Assets/Tests/EditMode/BuildSignTests.cs
git commit -m "refactor: BuildSign uses Homestead instead of Building"
```

---

### Task 3: Create Editor Sprite Merge Tool

**Files:**
- Create: `Assets/Scripts/Editor/SpriteMerger.cs`

**Interfaces:**
- Consumes: `BuildingPreBuildingStage.prefab`, `Building.prefab` via AssetDatabase
- Produces: `Assets/Sprite/HomesteadPreBuild.png`, `Assets/Sprite/HomesteadBuilt.png` as saved assets

- [ ] **Step 1: Create the Editor folder if it doesn't exist**

The `Assets/Scripts/Editor/` directory doesn't exist yet. Create `SpriteMerger.cs` which will also create the folder via Unity.

- [ ] **Step 2: Write the SpriteMerger editor script**

```csharp
using UnityEditor;
using UnityEngine;

public static class SpriteMerger
{
    [MenuItem("Tools/Merge Homestead Sprites")]
    public static void MergeHomesteadSprites()
    {
        MergePreBuildSprite();
        MergeBuiltSprite();
        AssetDatabase.Refresh();
        Debug.Log("Homestead sprites merged successfully.");
    }

    private static void MergePreBuildSprite()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BuildingPreBuildingStage.prefab");
        if (prefab == null)
        {
            Debug.LogError("BuildingPreBuildingStage.prefab not found");
            return;
        }

        var renderers = prefab.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0)
        {
            Debug.LogError("No SpriteRenderers found in BuildingPreBuildingStage");
            return;
        }

        MergeSpriteRenderers(renderers, "Assets/Sprite/HomesteadPreBuild.png");
    }

    private static void MergeBuiltSprite()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Building.prefab");
        if (prefab == null)
        {
            Debug.LogError("Building.prefab not found");
            return;
        }

        var spritesChild = prefab.transform.Find("Building_Sprites");
        if (spritesChild == null)
        {
            Debug.LogError("Building_Sprites child not found in Building.prefab");
            return;
        }

        var renderers = spritesChild.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0)
        {
            Debug.LogError("No SpriteRenderers found in Building_Sprites");
            return;
        }

        MergeSpriteRenderers(renderers, "Assets/Sprite/HomesteadBuilt.png");
    }

    private static void MergeSpriteRenderers(SpriteRenderer[] renderers, string outputPath)
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var sr in renderers)
        {
            if (sr.sprite == null) continue;
            var pos = sr.transform.localPosition;
            float scale = sr.transform.localScale.x;
            float size = sr.sprite.rect.width / sr.sprite.pixelsPerUnit * scale;
            minX = Mathf.Min(minX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxX = Mathf.Max(maxX, pos.x + size);
            maxY = Mathf.Max(maxY, pos.y + size);
        }

        int ppu = 128;
        int width = Mathf.RoundToInt((maxX - minX) * ppu);
        int height = Mathf.RoundToInt((maxY - minY) * ppu);

        if (width <= 0 || height <= 0)
        {
            Debug.LogError($"Invalid merged dimensions: {width}x{height}");
            return;
        }

        var result = new Texture2D(width, height, TextureFormat.RGBA32, false);

        foreach (var sr in renderers)
        {
            if (sr.sprite == null) continue;
            var pos = sr.transform.localPosition;
            float scale = sr.transform.localScale.x;

            var spriteTex = sr.sprite.texture;
            if (spriteTex == null) continue;

            if (!spriteTex.isReadable)
            {
                var texPath = AssetDatabase.GetAssetPath(spriteTex);
                var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
                if (importer != null && !importer.isReadable)
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                    spriteTex = sr.sprite.texture;
                }
            }

            int spriteW = Mathf.RoundToInt(sr.sprite.rect.width);
            int spriteH = Mathf.RoundToInt(sr.sprite.rect.height);
            var pixels = spriteTex.GetPixels(
                (int)sr.sprite.rect.x,
                (int)sr.sprite.rect.y,
                spriteW,
                spriteH);

            int targetW = Mathf.RoundToInt(spriteW * scale);
            int targetH = Mathf.RoundToInt(spriteH * scale);

            var scaledTex = new Texture2D(spriteW, spriteH, TextureFormat.RGBA32, false);
            scaledTex.SetPixels(pixels);
            scaledTex.Apply();

            var resized = new Texture2D(targetW, targetH, TextureFormat.RGBA32, false);
            for (int y = 0; y < targetH; y++)
            {
                for (int x = 0; x < targetW; x++)
                {
                    int srcX = Mathf.FloorToInt((float)x / targetW * spriteW);
                    int srcY = Mathf.FloorToInt((float)y / targetH * spriteH);
                    resized.SetPixel(x, y, scaledTex.GetPixel(srcX, srcY));
                }
            }
            resized.Apply();

            int destX = Mathf.RoundToInt((pos.x - minX) * ppu);
            int destY = Mathf.RoundToInt((pos.y - minY) * ppu);

            for (int y = 0; y < targetH; y++)
            {
                for (int x = 0; x < targetW; x++)
                {
                    int rx = destX + x;
                    int ry = destY + y;
                    if (rx >= 0 && rx < width && ry >= 0 && ry < height)
                    {
                        var existing = result.GetPixel(rx, ry);
                        var incoming = resized.GetPixel(x, y);
                        if (incoming.a > 0)
                            result.SetPixel(rx, ry, incoming);
                    }
                }
            }

            Object.DestroyImmediate(scaledTex);
            Object.DestroyImmediate(resized);
        }

        result.Apply();

        var pngData = result.EncodeToPNG();
        if (!System.IO.Directory.Exists("Assets/Sprite"))
            System.IO.Directory.CreateDirectory("Assets/Sprite");
        System.IO.File.WriteAllBytes(outputPath, pngData);

        Object.DestroyImmediate(result);

        Debug.Log($"Merged sprite saved to {outputPath} ({width}x{height})");
    }
}
```

- [ ] **Step 3: Verify the tool compiles**

Open Unity and confirm no compile errors in the Editor script. Run `Tools > Merge Homestead Sprites` from the Unity menu.

- [ ] **Step 4: Verify the output files exist**

Check that `Assets/Sprite/HomesteadPreBuild.png` and `Assets/Sprite/HomesteadBuilt.png` were created and imported as Sprite assets.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Editor/SpriteMerger.cs Assets/Sprite/HomesteadPreBuild.png Assets/Sprite/HomesteadPreBuild.png.meta Assets/Sprite/HomesteadBuilt.png Assets/Sprite/HomesteadBuilt.png.meta
git commit -m "feat: add sprite merge editor tool for Homestead assets"
```

---

### Task 4: Create Homestead Prefab

**Files:**
- Create: `Assets/Prefabs/Homestead.prefab` (created in Unity Editor)
- Modify: Scene (replace BuildingPreBuildingStage instance with Homestead)

**Interfaces:**
- Consumes: `Homestead` component from Task 1, merged sprites from Task 3
- Produces: `Homestead.prefab` with configured SpriteRenderer, Homestead component, colliders

This task must be done manually in the Unity Editor since programmatic prefab creation in plans is fragile.

- [ ] **Step 1: Create the Homestead prefab in Unity**

1. In the Hierarchy, create a new empty GameObject named "Homestead"
2. Add `SpriteRenderer` component, set sprite to `HomesteadPreBuild`
3. Add `Homestead` component, set `builtSprite` to `HomesteadBuilt`
4. Add `BoxCollider2D` (solid) sized to cover the full sprite
5. Add `BoxCollider2D` (trigger) positioned and sized for the sign area in the bottom-left corner
6. Set the layer to "Interactable"
7. Drag to `Assets/Prefabs/Homestead.prefab`

- [ ] **Step 2: Update the scene**

1. Find the `BuildingPreBuildingStage` instance in the scene
2. Note its position (should be around `(-23, 12, 0)` based on prefab root)
3. Delete the `BuildingPreBuildingStage` instance
4. Drag `Homestead.prefab` into the scene at the same position
5. Find the `BuildSign` in the scene and wire its `homestead` field to the new Homestead instance
6. Remove the old Building (Bakery) object that BuildSign was previously referencing

- [ ] **Step 3: Verify in Play Mode**

1. Enter Play Mode
2. Walk to the Homestead site and interact with the BuildSign
3. Confirm the construction stages still work (3 Stone, 3 Wood, 2 Wood + 3 Nails)
4. Confirm the Homestead sprite swaps to the built version on completion
5. Confirm BuildSign hides itself after completion

- [ ] **Step 4: Commit**

```bash
git add Assets/Prefabs/Homestead.prefab Assets/Prefabs/Homestead.prefab.meta Assets/Scenes/SampleScene.unity
git commit -m "feat: create Homestead prefab and update scene"
```

---

### Task 5: Clean Up Old References

**Files:**
- Modify: `Assets/Scripts/DevLabel.cs` — no changes needed (it only references Building, not Homestead)
- Verify: `Assets/Scripts/GameEvents.cs` — no changes needed
- Verify: `Assets/Scripts/UI/GameHUD.cs` — no changes needed (BuildSign interaction text is correct)
- Optional: Delete `Assets/Prefabs/BuildingPreBuildingStage.prefab` if no longer referenced in scene

**Interfaces:**
- Consumes: All changes from Tasks 1-4
- Produces: Clean codebase with no stale Homestead-as-Building references

- [ ] **Step 1: Search for stale references**

Search codebase for any remaining references to `homesteadBuilding` or `BuildingPreBuildingStage`:

```bash
rg "homesteadBuilding" Assets/
rg "BuildingPreBuildingStage" Assets/Scenes/
```

- [ ] **Step 2: Remove BuildingPreBuildingStage prefab if safe**

If no scene or code references `BuildingPreBuildingStage.prefab` anymore, delete it:

```bash
rm Assets/Prefabs/BuildingPreBuildingStage.prefab
rm Assets/Prefabs/BuildingPreBuildingStage.prefab.meta
```

- [ ] **Step 3: Run all tests**

Run the full EditMode test suite via Unity MCP or command line. Expected: All tests PASS.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "chore: remove BuildingPreBuildingStage prefab, clean up stale references"
```
