using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ruche.util
{
    /// <summary>
    /// 単一のアイテムを選択状態にすることが可能なコレクションクラス。
    /// </summary>
    public class SelectableValueCollection<T>
        : ObservableCollection<SelectableValue<T>>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public SelectableValueCollection() : this(false)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="src">初期アイテム列挙。</param>
        public SelectableValueCollection(IEnumerable<SelectableValue<T>> src)
            : this(src, false)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="alwaysSelected">
        /// 常に単一の項目が選択されている状態にするならば true 。
        /// </param>
        public SelectableValueCollection(bool alwaysSelected)
        {
            this.IsAlwaysSelected = alwaysSelected;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="src">初期アイテム列挙。</param>
        /// <param name="alwaysSelected">
        /// 常に単一の項目が選択されている状態にするならば true 。
        /// </param>
        public SelectableValueCollection(
            IEnumerable<SelectableValue<T>> src,
            bool alwaysSelected)
        {
            if (src == null)
            {
                throw new ArgumentNullException("src");
            }

            this.IsAlwaysSelected = alwaysSelected;
            foreach (var item in src)
            {
                this.Add(item);
            }
        }

        /// <summary>
        /// 常に単一の項目が選択されている状態にするか否かを取得または設定する。
        /// </summary>
        public bool IsAlwaysSelected
        {
            get { return _alwaysSelected; }
            set
            {
                if (value != _alwaysSelected)
                {
                    _alwaysSelected = value;
                    if (value)
                    {
                        this.SelectOne(null);
                    }
                }
            }
        }
        private bool _alwaysSelected = false;

        /// <summary>
        /// 現在選択中のアイテムのインデックスを取得または設定する。
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                for (int i = 0; i < this.Count; ++i)
                {
                    if (this[i].IsSelected)
                    {
                        return i;
                    }
                }
                return -1;
            }
            set
            {
                if (value >= this.Count || (this.IsAlwaysSelected && value < 0))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                for (int i = 0; i < this.Count; ++i)
                {
                    this[i].IsSelected = (i == value);
                }
            }
        }

        /// <summary>
        /// 現在選択中のアイテムを取得または設定する。
        /// </summary>
        public SelectableValue<T> SelectedItem
        {
            get { return this.FirstOrDefault(i => i.IsSelected); }
            set
            {
                if (value == null)
                {
                    if (this.IsAlwaysSelected)
                    {
                        throw new ArgumentNullException("value");
                    }
                    this.SelectedIndex = -1;
                }
                else
                {
                    var index = this.IndexOf(value);
                    if (index < 0)
                    {
                        throw new ArgumentException(
                            "`value` is not contained.",
                            "value");
                    }
                    this.SelectedIndex = index;
                }
            }
        }

        /// <summary>
        /// 指定した値を持つアイテムを選択する。
        /// </summary>
        /// <param name="value">値。</param>
        /// <returns>
        /// 選択されたアイテムのインデックス。
        /// 値が存在せず、非選択状態になった場合は -1 。
        /// </returns>
        public int SelectItemByValue(T value)
        {
            var target =
                this
                    .Select((item, index) => new { item, index })
                    .FirstOrDefault(
                        v => EqualityComparer<T>.Default.Equals(v.item.Value, value));
            if (target == null && this.IsAlwaysSelected)
            {
                throw new ArgumentException(
                    "`value` is not contained in items.",
                    "value");
            }

            this.SelectedIndex = (target == null) ? -1 : target.index;

            return this.SelectedIndex;
        }

        /// <summary>
        /// SelectOne メソッドの処理中であるか否かを取得または設定する。
        /// </summary>
        private bool IsSelectOneRunning { get; set; }

        /// <summary>
        /// 単一のアイテムのみを選択状態にする。
        /// </summary>
        /// <param name="hintItem">
        /// 選択状態が不正である場合のヒントとなるアイテム。
        /// </param>
        private void SelectOne(SelectableValue<T> hintItem)
        {
            if (this.IsSelectOneRunning)
            {
                return;
            }

            try
            {
                this.IsSelectOneRunning = true;

                var selectedItems =
                    (from i in this where i.IsSelected select i).ToList();
                if (selectedItems.Count > 1)
                {
                    var target =
                        selectedItems.Contains(hintItem) ? hintItem : selectedItems[0];

                    // 他の項目を非選択状態にしてから対象項目を選択する
                    foreach (var item in selectedItems)
                    {
                        if (item != target)
                        {
                            item.IsSelected = false;
                        }
                    }
                    target.IsSelected = true;
                }
                else if (selectedItems.Count == 0 && this.Count > 0)
                {
                    var index = Math.Max(0, this.IndexOf(hintItem));
                    this[index].IsSelected = true;
                }
            }
            finally
            {
                this.IsSelectOneRunning = false;
            }
        }

        /// <summary>
        /// アイテムの選択状態が変更された時に呼び出される。
        /// </summary>
        private void OnItemIsSelectedChanged(object sender, EventArgs e)
        {
            if (this.IsSelectOneRunning)
            {
                return;
            }

            var item = sender as SelectableValue<T>;
            if (item != null)
            {
                if (item.IsSelected || this.IsAlwaysSelected)
                {
                    this.SelectOne(item);
                }
            }
        }

        #region ObservableCollection<SelectableValue> オーバライド

        protected override void SetItem(int index, SelectableValue<T> item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            this[index].IsSelectedChanged -= this.OnItemIsSelectedChanged;
            item.IsSelectedChanged += this.OnItemIsSelectedChanged;

            base.SetItem(index, item);

            if (item.IsSelected || this.IsAlwaysSelected)
            {
                this.SelectOne(item);
            }
        }

        protected override void InsertItem(int index, SelectableValue<T> item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            item.IsSelectedChanged += OnItemIsSelectedChanged;

            base.InsertItem(index, item);

            if (item.IsSelected)
            {
                this.SelectOne(item);
            }
            else if (this.Count == 1 && this.IsAlwaysSelected)
            {
                this[0].IsSelected = true;
            }
        }

        protected override void RemoveItem(int index)
        {
            this[index].IsSelectedChanged -= this.OnItemIsSelectedChanged;

            base.RemoveItem(index);

            if (this.IsAlwaysSelected)
            {
                this.SelectOne(null);
            }
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.IsSelectedChanged -= this.OnItemIsSelectedChanged;
            }

            base.ClearItems();
        }

        #endregion
    }
}
