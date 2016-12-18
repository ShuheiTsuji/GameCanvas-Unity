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
    /// 画像情報を保持するためのカスタムアセット
    /// </summary>
    public class AssetDatabase : ScriptableObject
    {
        /// <summary>
        /// カスタムアセットの保存先
        /// </summary>
        public const string Path = "Assets/Plugins/UnityGC/Resources/GCAssetDB.asset";

        /// <summary>
        /// 画像のリスト
        /// </summary>
        public Sprite[] images;

        /// <summary>
        /// 音声のリスト
        /// </summary>
        public AudioClip[] sounds;

        /// <summary>
        /// 矩形
        /// </summary>
        public Sprite rect;

        /// <summary>
        /// 円
        /// </summary>
        public Sprite circle;

        /// <summary>
        /// 文字のリスト
        /// </summary>
        public Sprite[] characters;
    }
}
