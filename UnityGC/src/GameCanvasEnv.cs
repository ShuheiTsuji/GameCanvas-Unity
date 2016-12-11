﻿/*------------------------------------------------------------*/
/// <summary>GameCanvas for Unity [Env]</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2016 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas
{
    /// <summary>
    /// 環境変数
    /// </summary>
    public class Env
    {
        /// <summary>
        /// GameCanvas のバージョン情報
        /// </summary>
        public const string Version = "v2.0.0b";

        /// <summary>
        /// GameCanvas API のバージョン情報
        /// </summary>
        public const string APIVersion = "v1.1";

        /// <summary>
        /// GameCanvas の著作権表記
        /// </summary>
        public const string Copyright = "Copyright (c) 2015-2016 Smart Device Programming.";

        /// <summary>
        /// GameCanvas の制作者情報
        /// </summary>
        public readonly static string[] Authors = 
        {
            "kuro",
            "fujieda",
            "seibe"
        };
    }
}
