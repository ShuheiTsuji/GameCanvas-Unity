/*------------------------------------------------------------*/
/// <summary>Image Database</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2016 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;

namespace GameCanvas
{
    /// <summary>
    /// 描画のための画像情報
    /// </summary>
    [System.Serializable]
    public struct ImageInfo
    {
        /// <summary>画像のオリジナル幅</summary>
        public int width;
        /// <summary>画像のオリジナル高さ</summary>
        public int height;
        /// <summary>アトラスに含まれる領域（余白を除いた領域）</summary>
        public Vector4 rect;
        /// <summary>アトラスのアセット名</summary>
        public string atlasName;
        /// <summary>アトラスの対応する領域</summary>
        public Vector4 atlasRect;
    }

    /// <summary>
    /// 画像情報を保持するためのカスタムアセット
    /// </summary>
    public class ImageDatabase : ScriptableObject
    {
        /// <summary>
        /// カスタムアセットの保存先
        /// </summary>
        public const string Path = "Assets/Plugins/UnityGC/Resources/GCImageDB.asset";

        /// <summary>
        /// イメージ情報
        /// </summary>
        public ImageInfo[] images;

        /// <summary>
        /// アトラス(RGB)リスト
        /// </summary>
        public Texture2D[] atlases;

        /// <summary>
        /// アトラス(Alpha)リスト
        /// </summary>
        public Texture2D[] alphaAtlases;
    }
}
