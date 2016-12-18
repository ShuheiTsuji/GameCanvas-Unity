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
        const string PackingTag = "GCAtlas";
        const string SpriteDir  = "Assets/Plugins/UnityGC/Sprites";
        const string RectPath   = SpriteDir + "/Rect.png";
        const string CirclePath = SpriteDir + "/Circle.png";
        const string FontPath   = SpriteDir + "/PixelMplus10.png";
        static bool willRebuildAssetDB = false;

        void OnPreprocessTexture()
        {
            var path = assetImporter.assetPath;

            if (path.IndexOf(ResourceImagePrefix) == 0)
            {
                // インポートした画像をパッキングタグ付きスプライトにします
                var importer = (TextureImporter)assetImporter;
                importer.textureType = TextureImporterType.Sprite;
                importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
                importer.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC2_RGBA8, 80, true);
                importer.SetPlatformTextureSettings("iPhone", 2048, TextureImporterFormat.PVRTC_RGBA4, 80, true);
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 1f;
                importer.spritePackingTag = PackingTag;
                
                var so = new SerializedObject(importer);
                var sp = so.FindProperty("m_Alignment");
                if (sp.intValue != (int)SpriteAlignment.TopLeft)
                {
                    sp.intValue = (int)SpriteAlignment.TopLeft;
                    so.ApplyModifiedProperties();
                }

                if (!willRebuildAssetDB)
                {
                    EditorApplication.update += RebuildAssetDatabase;
                    willRebuildAssetDB = true;
                }
            }
            else if (path.IndexOf(SpriteDir + "/") == 0)
            {
                // インポートした画像をパッキングタグ付きスプライトにします
                var importer = (TextureImporter)assetImporter;
                importer.textureType = TextureImporterType.Sprite;
                importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
                importer.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC2_RGBA8, 80, true);
                importer.SetPlatformTextureSettings("iPhone", 2048, TextureImporterFormat.PVRTC_RGBA4, 80, true);
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.spritePackingTag = PackingTag;
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

                if (!willRebuildAssetDB)
                {
                    EditorApplication.update += RebuildAssetDatabase;
                    willRebuildAssetDB = true;
                }
            }
        }

        public static void CheckDatabase()
        {
            if (UnityEditor.AssetDatabase.LoadAssetAtPath<AssetDatabase>(AssetDatabase.Path) == null)
            {
                RebuildAssetDatabase();
            }
        }

        public static void RebuildAssetDatabase()
        {
            if (willRebuildAssetDB)
            {
                EditorApplication.update -= RebuildAssetDatabase;
                willRebuildAssetDB = false;
            }
            var projectDir = Path.GetDirectoryName(Application.dataPath);

            // 画像データの取得
            var sprites = new Dictionary<int, Sprite>();
            foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:Texture2d", new string[1] { ResourceDir }))
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                var index = int.Parse(sprite.name.Substring(3));
                sprites.Add(index, sprite);
            }

            // 音声データの取得
            var clips = new Dictionary<int, AudioClip>();
            foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:AudioClip", new string[1] { ResourceDir }))
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                var index = int.Parse(clip.name.Substring(3));
                clips.Add(index, clip);
            }

            // 図形データの取得
            var rect = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(RectPath);
            var circle = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);

            // 文字データの取得
            var characters = new List<Sprite>();
            foreach (var obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(FontPath))
            {
                if (obj is Sprite)
                {
                    characters.Add(obj as Sprite);
                }
            }

            // データベースの作成
            var absoluteDBPath = Path.Combine(projectDir, AssetDatabase.Path);
            if (!File.Exists(absoluteDBPath))
            {
                var newDB = ScriptableObject.CreateInstance<AssetDatabase>();
                UnityEditor.AssetDatabase.CreateAsset(newDB, AssetDatabase.Path);
            }
            
            // データベースの保存
            var db = UnityEditor.AssetDatabase.LoadAssetAtPath<AssetDatabase>(AssetDatabase.Path);
            db.images = sprites.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
            db.sounds = clips.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
            db.rect   = rect;
            db.circle = circle;
            db.characters = characters.ToArray();
            UnityEditor.AssetDatabase.SaveAssets();
        }
    }
}
