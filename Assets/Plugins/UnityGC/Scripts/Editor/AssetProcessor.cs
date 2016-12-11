/*------------------------------------------------------------*/
/// <summary>GameCanvas for Unity [Asset Processor]</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2016 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;
using UnityEditor;
using UnityEditor.Sprites;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace GameCanvas.Editor
{
    public class AssetProcessor : AssetPostprocessor
    {
        const string ResourceDir = "Assets/Res";
        const string ResourceImagePrefix = ResourceDir + "/img";
        const string ResourceSoundPrefix = ResourceDir + "/snd";
        const string AtlasDir = "Assets/Plugins/UnityGC/Resources";
        const string PackingTag = "GCAtlas";
        const string AtlasImagePrefix = AtlasDir + "/" + PackingTag;
        const string SpritAlphaShader = "Assets/Plugins/UnityGC/Shaders/GCSplitAlpha.shader";
        const string MaterialPathSplitAlpha = "Assets/Plugins/UnityGC/Materials/GCSplitAlpha.mat";
        static bool willRebuildImageDB = false;
        static bool willRebuildSoundDB = false;

        void OnPreprocessTexture()
        {
            var path = assetImporter.assetPath;

            if (path.IndexOf(ResourceImagePrefix) == 0)
            {
                // インポートした画像をパッキングタグ付きスプライトにします
                var importer = (TextureImporter)assetImporter;
                importer.textureType = TextureImporterType.Sprite;
                importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                importer.filterMode = FilterMode.Point;
                importer.spritePixelsPerUnit = 1f;
                importer.mipmapEnabled = false;
                importer.spritePackingTag = PackingTag;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.textureFormat = TextureImporterFormat.ARGB32;

                if (!willRebuildImageDB)
                {
                    EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOn;
                    EditorApplication.update += RebuildImageDatabase;
                    willRebuildImageDB = true;
                }
            }
            else if (path.IndexOf(AtlasImagePrefix) == 0)
            {
                var importer = (TextureImporter)assetImporter;
                importer.isReadable = true;
                importer.mipmapEnabled = false;
                importer.spriteImportMode = SpriteImportMode.None;
                importer.spritePixelsPerUnit = 1;
                importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
                importer.textureType = TextureImporterType.Advanced;
            }
        }

        void OnPreprocessAudio()
        {
            var path = assetImporter.assetPath;

            if (path.IndexOf(ResourceSoundPrefix) == 0)
            {
                // インポートした音声の圧縮を設定します
                var audioImporter = (AudioImporter)assetImporter;
                var setting = audioImporter.defaultSampleSettings;
                setting.compressionFormat = AudioCompressionFormat.ADPCM;
                setting.loadType = AudioClipLoadType.Streaming;
                audioImporter.defaultSampleSettings = setting;

                if (!willRebuildSoundDB)
                {
                    EditorApplication.update += RebuildSoundDatabase;
                    willRebuildSoundDB = true;
                }
            }
        }

        static Vector4 CalcRectFromUVs(Vector2[] uvs)
        {
            var min = new Vector2(1f, 1f);
            var max = new Vector2(0f, 0f);

            foreach (var uv in uvs)
            {
                min = Vector2.Min(min, uv);
                max = Vector2.Max(max, uv);
            }

            return new Vector4(min.x, min.y, max.x, max.y);
        }

        public static void CheckDatabase()
        {
            if (AssetDatabase.LoadAssetAtPath<ImageDatabase>(ImageDatabase.Path) == null)
            {
                RebuildImageDatabase();
            }
            if (AssetDatabase.LoadAssetAtPath<SoundDatabase>(SoundDatabase.Path) == null)
            {
                RebuildSoundDatabase();
            }
        }

        public static void RebuildImageDatabase()
        {
            if (willRebuildImageDB)
            {
                EditorApplication.update -= RebuildImageDatabase;
                willRebuildImageDB = false;
            }
            var projectDir = Path.GetDirectoryName(Application.dataPath);

            #region アトラスの生成

            // アトラスの再生成
            Packer.SelectedPolicy = Packer.kDefaultPolicy;
            Packer.RebuildAtlasCacheIfNeeded(EditorUserBuildSettings.activeBuildTarget, true, Packer.Execution.ForceRegroup);

            // アトラス別スプライトの列挙
            var spriteList = new Dictionary<string, List<Sprite>>();
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new string[1] { ResourceDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is Sprite)
                    {
                        var sprite = asset as Sprite;
                        string name;
                        Texture2D tex;
                        Packer.GetAtlasDataForSprite(sprite, out name, out tex);

                        if (spriteList.ContainsKey(name)) spriteList[name].Add(sprite);
                        else spriteList.Add(name, new List<Sprite>() { sprite });
                    }
                    else
                    {
                        Resources.UnloadAsset(asset);
                    }
                }
            }

            // マテリアルの読み込み
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(SpritAlphaShader);
            var matRGB = new Material(shader);
            matRGB.SetFloat("_Mode", 0);
            var matAlpha = new Material(shader);
            matAlpha.SetFloat("_Mode", 1);

            // アトラスの保存
            var atlases = new List<string>();
            var imageInfo = new Dictionary<int, ImageInfo>();
            AssetDatabase.StartAssetEditing();
            foreach (var atlasName in Packer.atlasNames)
            {
                if (!spriteList.ContainsKey(atlasName)) continue;
                if (atlasName.IndexOf(PackingTag) != 0) continue;

                var textures = Packer.GetTexturesForAtlas(atlasName);
                var numTexture = textures.Length;
                for (var i = 0; i < numTexture; ++i)
                {
                    var texture = textures[i];
                    var width = texture.width;
                    var height = texture.height;
                    var atlasRGB = new Texture2D(width, height, TextureFormat.ARGB32, false);
                    var atlasAlpha = new Texture2D(width, height, TextureFormat.ARGB32, false);

                    var temp = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
                    Graphics.Blit(texture, temp, matRGB);
                    atlasRGB.ReadPixels(new Rect(0, 0, width, height), 0, 0);

                    Graphics.Blit(texture, temp, matAlpha);
                    atlasAlpha.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    RenderTexture.ReleaseTemporary(temp);

                    var pngRGB = atlasRGB.EncodeToPNG();
                    var pngAlpha = atlasAlpha.EncodeToPNG();

                    var filename = numTexture == 1 ? atlasName : string.Format("{0}_{1}", atlasName, i);
                    var pathRGB = Path.Combine(AtlasDir, filename + ".png");
                    var absolutePathRGB = Path.Combine(projectDir, pathRGB);
                    if (File.Exists(absolutePathRGB)) File.Delete(absolutePathRGB);
                    File.WriteAllBytes(absolutePathRGB, pngRGB);
                    var pathAlpha = Path.Combine(AtlasDir, filename + "_alpha.png");
                    var absolutePathAlpha = Path.Combine(projectDir, pathAlpha);
                    if (File.Exists(absolutePathAlpha)) File.Delete(absolutePathAlpha);
                    File.WriteAllBytes(absolutePathAlpha, pngAlpha);

                    foreach (var sprite in spriteList[atlasName])
                    {
                        string name;
                        Texture2D tex;
                        Packer.GetAtlasDataForSprite(sprite, out name, out tex);

                        if (texture == tex)
                        {
                            var index = int.Parse(sprite.name.Substring(3));
                            var beforeUVs = SpriteUtility.GetSpriteUVs(sprite, false);
                            var afterUVs = SpriteUtility.GetSpriteUVs(sprite, true);
                            var beforeRect = CalcRectFromUVs(beforeUVs);
                            var afterRect = CalcRectFromUVs(afterUVs);

                            imageInfo.Add(index, new ImageInfo()
                            {
                                width = (int)sprite.rect.width,
                                height = (int)sprite.rect.height,
                                rect = beforeRect,
                                atlasName = filename,
                                atlasRect = afterRect
                            });
                        }
                    }

                    AssetDatabase.ImportAsset(pathRGB, ImportAssetOptions.ForceUpdate);
                    atlases.Add(filename);
                }
            }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            Resources.UnloadAsset(shader);

            #endregion

            // データベースの作成
            var absoluteDBPath = Path.Combine(projectDir, ImageDatabase.Path);
            if (File.Exists(absoluteDBPath))
            {
                File.Delete(absoluteDBPath);
                if (File.Exists(absoluteDBPath + ".meta"))
                {
                    File.Delete(absoluteDBPath + ".meta");
                }
            }
            var db = ScriptableObject.CreateInstance<ImageDatabase>();
            db.atlases = atlases.Select(x => AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format("{0}/{1}.png", AtlasDir, x))).ToArray();
            db.alphaAtlases = atlases.Select(x => AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format("{0}/{1}_alpha.png", AtlasDir, x))).ToArray();
            db.images = imageInfo.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
            AssetDatabase.CreateAsset(db, ImageDatabase.Path);
        }

        public static void RebuildSoundDatabase()
        {
            if (willRebuildSoundDB)
            {
                EditorApplication.update -= RebuildSoundDatabase;
                willRebuildSoundDB = false;
            }

            // 音声ファイルの列挙
            var clips = new Dictionary<int, AudioClip>();
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new string[1] { ResourceDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                var index = int.Parse(clip.name.Substring(3));
                clips.Add(index, clip);
            }

            // データベースの作成
            var projectDir = Path.GetDirectoryName(Application.dataPath);
            var absoluteDBPath = Path.Combine(projectDir, SoundDatabase.Path);
            if (File.Exists(absoluteDBPath))
            {
                File.Delete(absoluteDBPath);
                if (File.Exists(absoluteDBPath + ".meta"))
                {
                    File.Delete(absoluteDBPath + ".meta");
                }
            }
            var db = ScriptableObject.CreateInstance<SoundDatabase>();
            db.sounds = clips.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
            AssetDatabase.CreateAsset(db, SoundDatabase.Path);
        }
    }
}
