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
            float scaleX = sr.transform.localScale.x;
            float scaleY = sr.transform.localScale.y;
            float spriteWidthUnits = sr.sprite.rect.width / sr.sprite.pixelsPerUnit * scaleX;
            float spriteHeightUnits = sr.sprite.rect.height / sr.sprite.pixelsPerUnit * scaleY;
            float pivotOffsetX = sr.sprite.pivot.x / sr.sprite.pixelsPerUnit * scaleX;
            float pivotOffsetY = sr.sprite.pivot.y / sr.sprite.pixelsPerUnit * scaleY;
            float left = pos.x - pivotOffsetX;
            float bottom = pos.y - pivotOffsetY;
            minX = Mathf.Min(minX, left);
            minY = Mathf.Min(minY, bottom);
            maxX = Mathf.Max(maxX, left + spriteWidthUnits);
            maxY = Mathf.Max(maxY, bottom + spriteHeightUnits);
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
            float scaleX = sr.transform.localScale.x;
            float scaleY = sr.transform.localScale.y;
            float pivotOffsetX = sr.sprite.pivot.x / sr.sprite.pixelsPerUnit * scaleX;
            float pivotOffsetY = sr.sprite.pivot.y / sr.sprite.pixelsPerUnit * scaleY;
            float left = pos.x - pivotOffsetX;
            float bottom = pos.y - pivotOffsetY;

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

            int targetW = Mathf.RoundToInt(spriteW * scaleX);
            int targetH = Mathf.RoundToInt(spriteH * scaleY);

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

            int destX = Mathf.RoundToInt((left - minX) * ppu);
            int destY = Mathf.RoundToInt((bottom - minY) * ppu);

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
                        float outA = incoming.a + existing.a * (1f - incoming.a);
                        if (outA > 0f)
                        {
                            float r = (incoming.r * incoming.a + existing.r * existing.a * (1f - incoming.a)) / outA;
                            float g = (incoming.g * incoming.a + existing.g * existing.a * (1f - incoming.a)) / outA;
                            float b = (incoming.b * incoming.a + existing.b * existing.a * (1f - incoming.a)) / outA;
                            result.SetPixel(rx, ry, new Color(r, g, b, outA));
                        }
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
