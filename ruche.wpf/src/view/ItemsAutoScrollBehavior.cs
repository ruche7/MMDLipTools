using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media;

namespace ruche.wpf.view
{
    /// <summary>
    /// ItemsControl およびその派生先のUI要素に自動スクロールを提供するビヘイビア。
    /// </summary>
    public class ItemsAutoScrollBehavior : FrameworkElementBehavior<ItemsControl>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public ItemsAutoScrollBehavior() { }

        /// <summary>
        /// イベントソース。
        /// </summary>
        private INotifyCollectionChanged Source { get; set; }

        /// <summary>
        /// スクロールビューワ。
        /// </summary>
        private ScrollViewer Viewer { get; set; }

        /// <summary>
        /// UI要素の内容が更新された時に呼び出される。
        /// </summary>
        private void Source_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            Viewer.ScrollToEnd();
        }

        #region FrameworkElementBehavior<ItemsControl> オーバライド

        protected override void OnAssociatedObjectLoaded()
        {
            Source = null;
            Viewer = null;

            var src = this.AssociatedObject.ItemsSource as INotifyCollectionChanged;
            if (src == null)
            {
                return;
            }

            var border =
                VisualTreeHelper.GetChild(this.AssociatedObject, 0) as Decorator;
            if (border == null)
            {
                return;
            }
            Viewer = border.Child as ScrollViewer;
            if (Viewer == null)
            {
                return;
            }

            Source = src;

            Source.CollectionChanged += Source_CollectionChanged;
        }

        protected override void OnDetaching()
        {
            if (Source != null)
            {
                Source.CollectionChanged -= Source_CollectionChanged;
            }

            Source = null;
            Viewer = null;

            base.OnDetaching();
        }

        #endregion
    }
}
