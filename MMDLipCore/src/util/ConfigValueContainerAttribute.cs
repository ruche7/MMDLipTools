using System;

namespace ruche.util
{
    /// <summary>
    /// Config クラスの処理対象となるプロパティを内包する
    /// プロパティであることを示す属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigValueContainerAttribute : Attribute
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public ConfigValueContainerAttribute()
        {
        }
    }
}
