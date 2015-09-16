using System;
using System.Windows;
using System.Windows.Controls;
using ruche.wpf;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// LipEditControl の View クラス。
    /// </summary>
    public partial class LipEditControl : UserControl
    {
        /// <summary>
        /// DependencyProperty を作成する。
        /// </summary>
        /// <typeparam name="TValue">プロパティの値型。</typeparam>
        /// <param name="name">プロパティ名。</param>
        /// <param name="defaultValue">既定値。</param>
        /// <returns>DependencyProperty 。</returns>
        private static DependencyProperty RegisterProperty<TValue>(
            string name,
            TValue defaultValue)
        {
            return
                DependencyPropertyFactory<LipEditControl>.Register(
                    name,
                    defaultValue);
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public LipEditControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// IsTextVisible 依存関係プロパティ。
        /// </summary>
        public static readonly DependencyProperty IsTextVisibleProperty =
            RegisterProperty("IsTextVisible", true);

        /// <summary>
        /// 入力文UIを表示するか否かを取得または設定する。
        /// </summary>
        public bool IsTextVisible
        {
            get { return (bool)this.GetValue(IsTextVisibleProperty); }
            set { this.SetValue(IsTextVisibleProperty, value); }
        }

        /// <summary>
        /// IsTextToLipKanaVisible 依存関係プロパティ。
        /// </summary>
        public static readonly DependencyProperty IsTextToLipKanaVisibleProperty =
            RegisterProperty("IsTextToLipKanaVisible", true);

        /// <summary>
        /// 入力文から読み仮名へ変換するUIを表示するか否かを取得または設定する。
        /// </summary>
        public bool IsTextToLipKanaVisible
        {
            get { return (bool)this.GetValue(IsTextToLipKanaVisibleProperty); }
            set { this.SetValue(IsTextToLipKanaVisibleProperty, value); }
        }

        /// <summary>
        /// IsFpsVisible 依存関係プロパティ。
        /// </summary>
        public static readonly DependencyProperty IsFpsVisibleProperty =
            RegisterProperty("IsFpsVisible", true);

        /// <summary>
        /// FPS設定UIを表示するか否かを取得または設定する。
        /// </summary>
        public bool IsFpsVisible
        {
            get { return (bool)this.GetValue(IsFpsVisibleProperty); }
            set { this.SetValue(IsFpsVisibleProperty, value); }
        }

        /// <summary>
        /// IsDetailVisible 依存関係プロパティ。
        /// </summary>
        public static readonly DependencyProperty IsDetailVisibleProperty =
            RegisterProperty("IsDetailVisible", true);

        /// <summary>
        /// 詳細設定UIを表示するか否かを取得または設定する。
        /// </summary>
        public bool IsDetailVisible
        {
            get { return (bool)this.GetValue(IsDetailVisibleProperty); }
            set { this.SetValue(IsDetailVisibleProperty, value); }
        }

        /// <summary>
        /// DataContext の変更時に呼び出される。
        /// </summary>
        private void OnDataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as LipEditControlViewModel;
            if (vm != null && vm.MorphPresetDialogShower == null)
            {
                // 口パクモーフプリセット編集ダイアログ表示デリゲートを設定
                vm.MorphPresetDialogShower =
                    presets =>
                    {
                        var dialog = new MorphPresetDialog();
                        dialog.Owner = Window.GetWindow(this);
                        dialog.MorphPresets = presets;

                        var result = dialog.ShowDialog();

                        return (result == true) ? dialog.MorphPresets : null;
                    };
            }
        }
    }
}
