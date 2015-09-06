using System.Windows;
using System.Windows.Interactivity;

namespace ruche.wpf.view
{
    /// <summary>
    /// UI要素のロード完了時呼び出しをサポートするビヘイビアの基底クラス。
    /// </summary>
    /// <typeparam name="T">FrameworkElement 派生型。</typeparam>
    public abstract class FrameworkElementBehavior<T> : Behavior<T>
        where T : FrameworkElement
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        protected FrameworkElementBehavior() { }

        /// <summary>
        /// UI要素のロード完了時に呼び出される。
        /// </summary>
        /// <remarks>
        /// アタッチ時点でロード完了済みの場合は即座に呼び出される。
        /// </remarks>
        protected virtual void OnAssociatedObjectLoaded() { }

        /// <summary>
        /// UI要素のロード完了時に呼び出される。
        /// </summary>
        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;

            OnAssociatedObjectLoaded();
        }

        #region Behavior<ItemsControl> オーバライド

        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject.IsLoaded)
            {
                // ロード済みなら即呼び出し
                OnAssociatedObjectLoaded();
            }
            else
            {
                // 未ロードなら呼び出し予約
                this.AssociatedObject.Loaded += AssociatedObject_Loaded;
            }
        }

        #endregion
    }
}
