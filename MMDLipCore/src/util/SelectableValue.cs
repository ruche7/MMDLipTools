using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ruche.util
{
    /// <summary>
    /// 任意の値とその選択状態を保持するクラス。
    /// </summary>
    public class SelectableValue<T> : INotifyPropertyChanged
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public SelectableValue()
        {
        }

        /// <summary>
        /// 値を取得または設定する。
        /// </summary>
        public T Value
        {
            get { return _value; }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(value, _value))
                {
                    _value = value;

                    this.OnValueChanged(EventArgs.Empty);
                    this.NotifyPropertyChanged(nameof(Value));
                }
            }
        }
        private T _value = default(T);

        /// <summary>
        /// 値が選択されているか否かを取得または設定する。
        /// </summary>
        public bool IsSelected
        {
            get { return _selected; }
            set
            {
                if (value != _selected)
                {
                    _selected = value;

                    this.OnIsSelectedChanged(EventArgs.Empty);
                    this.NotifyPropertyChanged(nameof(IsSelected));
                }
            }
        }
        private bool _selected = false;

        /// <summary>
        /// プロパティ値が変更された時に発生するイベント。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Value プロパティ値が変更された時に発生するイベント。
        /// </summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// IsSelected プロパティ値が変更された時に発生するイベント。
        /// </summary>
        public event EventHandler IsSelectedChanged;

        /// <summary>
        /// 値の文字列表現を作成する。
        /// </summary>
        /// <returns>値の文字列表現。</returns>
        public override string ToString() => $"{this.Value}";

        /// <summary>
        /// プロパティ値の変更を通知する。
        /// </summary>
        /// <param name="propertyName">プロパティ名。</param>
        protected void NotifyPropertyChanged(string propertyName) =>
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Value プロパティ値の変更時に呼び出される。
        /// </summary>
        /// <param name="e">常に EventArgs.Empty 。</param>
        protected virtual void OnValueChanged(EventArgs e) =>
            this.ValueChanged?.Invoke(this, e);

        /// <summary>
        /// IsSelected プロパティ値の変更時に呼び出される。
        /// </summary>
        /// <param name="e">常に EventArgs.Empty 。</param>
        protected virtual void OnIsSelectedChanged(EventArgs e) =>
            this.IsSelectedChanged?.Invoke(this, e);
    }
}
