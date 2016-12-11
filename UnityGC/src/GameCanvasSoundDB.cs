/*------------------------------------------------------------*/
/// <summary>Sound Database</summary>
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
    /// 音声情報を保持するためのカスタムアセット
    /// </summary>
    public class SoundDatabase : ScriptableObject
    {
        /// <summary>
        /// カスタムアセットの保存先
        /// </summary>
        public const string Path = "Assets/Plugins/UnityGC/Resources/GCSoundDB.asset";

        /// <summary>
        /// 音声リスト
        /// </summary>
        public AudioClip[] sounds;
    }
}
