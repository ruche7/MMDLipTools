using System;
using System.ComponentModel;

namespace ruche.wpf.viewModel
{
    /// <summary>
    /// ViewModel の基底クラス。
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティ値が変更された時に発生するイベント。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ値の変更を通知する。
        /// </summary>
        /// <param name="propertyName">プロパティ名。</param>
        protected void NotifyPropertyChanged(string propertyName) =>
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
    }
}
