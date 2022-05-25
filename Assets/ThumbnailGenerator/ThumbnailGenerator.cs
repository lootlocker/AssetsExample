using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class ThumbnailGenerator : MonoBehaviour
{

    public RenderTexture renderTexture;

    public string assetName;

    [ContextMenu("GenerateThumbnail")]
    public void GenerateThumbnail()
    {
        Texture2D newTexture = ToTexture2D(renderTexture);
        Sprite newSprite = Sprite.Create(newTexture, new Rect(0.0f, 0.0f, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f));
        SaveSpriteAsAsset(newSprite);
    }

    public Texture2D ToTexture2D(RenderTexture rTex)
    {
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = rTex;
        GetComponent<Camera>().Render();
        // Create a new Texture2D and read the RenderTexture image into it
        Texture2D tex = new Texture2D(rTex.width, rTex.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = currentActiveRT;
        return tex;
    }

    void SaveSpriteAsAsset(Sprite sprite)
    {
        string proj_path = Application.dataPath+"/"+assetName+"thumbnail.png";
        File.WriteAllBytes(proj_path, ImageConversion.EncodeToPNG(sprite.texture));

        //AssetDatabase.Refresh();
        proj_path = Path.Combine("/" + assetName + "thumbnail.png", proj_path);
        /*
        var ti = AssetImporter.GetAtPath("Assets"+"/"+assetName+"thumbnail.png") as TextureImporter;
        ti.spritePixelsPerUnit = sprite.pixelsPerUnit;
        ti.mipmapEnabled = false;
        ti.textureType = TextureImporterType.Sprite;

        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        */
    }
}
