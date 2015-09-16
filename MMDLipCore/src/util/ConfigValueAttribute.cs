using System;

namespace ruche.util
{
    /// <summary>
    /// Config クラスの処理対象となるプロパティであることを示す属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigValueAttribute : Attribute
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public ConfigValueAttribute()
        {
        }
    }
}
