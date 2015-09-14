using System;
using System.Windows;

namespace ruche.wpf
{
    /// <summary>
    /// DependencyProperty を簡単に作成するためのメソッドを提供する静的クラス。
    /// </summary>
    /// <typeparam name="TOwner">プロパティのオーナー型。</typeparam>
    public static class DependencyPropertyFactory<TOwner>
        where TOwner : DependencyObject
    {
        /// <summary>
        /// DependencyProperty を作成する。
        /// </summary>
        /// <typeparam name="TValue">プロパティの値型。</typeparam>
        /// <param name="name">プロパティ名。</param>
        /// <param name="defaultValue">既定値。</param>
        /// <param name="callback">変更時に呼び出されるコールバック。</param>
        /// <returns>DependencyProperty 。</returns>
        public static DependencyProperty Register<TValue>(
            string name,
            TValue defaultValue,
            Action<TOwner, TValue> callback = null)
        {
            var meta =
                (callback == null) ?
                    new PropertyMetadata(defaultValue) :
                    new PropertyMetadata(
                        defaultValue,
                        (d, p) =>
                        {
                            var owner = d as TOwner;
                            if (owner != null)
                            {
                                callback(owner, (TValue)p.NewValue);
                            }
                        });

            return
                DependencyProperty.Register(
                    name,
                    typeof(TValue),
                    typeof(TOwner),
                    meta);
        }
    }
}
