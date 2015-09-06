using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Media;

namespace ruche.wpf.viewModel
{
    /// <summary>
    /// Color 値の ViewModel クラス。
    /// </summary>
    public class ColorViewModel : ViewModelBase
    {
        /// <summary>
        /// Color 型のプロパティにバインドした ColorViewModel インスタンスを作成する。
        /// </summary>
        /// <typeparam name="T">
        /// プロパティを保持するインスタンスの型。
        /// INotifyPropertyChanged インタフェースを実装する必要がある。
        /// </typeparam>
        /// <param name="owner">プロパティを保持するインスタンス。</param>
        /// <param name="propertyName">Color 型のプロパティ名。</param>
        /// <returns>ColorViewModel インスタンス。</returns>
        /// <remarks>
        /// 成功した場合、 owner の PropertyChanged イベントに
        /// 作成したインスタンスの OnBindingPropertyChanged メソッドが追加される。
        /// </remarks>
        public static ColorViewModel Bind<T>(T owner, string propertyName)
            where T : class, INotifyPropertyChanged
        {
            if (object.ReferenceEquals(owner, null))
            {
                throw new ArgumentNullException("owner");
            }
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            // プロパティ情報チェック
            CheckColorProperty(typeof(T), propertyName);

            // 式木構築
            var ownerExp = Expression.Parameter(typeof(T), "owner");
            var ownerPropExp = Expression.Property(ownerExp, propertyName);
            var valueExp = Expression.Parameter(typeof(Color), "value");
            var getterExp = Expression.Lambda<Func<T, Color>>(ownerPropExp, ownerExp);
            var setterExp =
                Expression.Lambda<Action<T, Color>>(
                    Expression.Assign(ownerPropExp, valueExp),
                    ownerExp,
                    valueExp);

            // getter 作成
            var getterLambda = getterExp.Compile();
            Func<Color> getter = () => getterLambda(owner);

            // setter 作成
            var setterLambda = setterExp.Compile();
            Action<Color> setter = value => setterLambda(owner, value);

            // インスタンス構築
            var instance = new ColorViewModel(getter());

            // バインド用プロパティ設定
            instance.BindingPropertyName = propertyName;
            instance.BindingPropertyGetter = getter;
            instance.BindingPropertySetter = setter;

            // バインドインスタンス変更時の処理を登録
            owner.PropertyChanged += instance.OnBindingPropertyChanged;

            return instance;
        }

        /// <summary>
        /// Bind メソッドでバインドするプロパティをチェックする。
        /// </summary>
        /// <param name="ownerType">プロパティを保持するインスタンスの型。</param>
        /// <param name="propertyName">Color 型のプロパティ名。</param>
        private static void CheckColorProperty(Type ownerType, string propertyName)
        {
            var info = ownerType.GetProperty(propertyName);
            if (info == null)
            {
                throw new ArgumentException("The property is not found.", "propertyName");
            }
            if (info.PropertyType != typeof(Color))
            {
                throw new ArgumentException(
                    "The property type is not typeof(Color).",
                    "propertyName");
            }
            if (!info.CanRead)
            {
                throw new ArgumentException(
                    "The property cannot be read.",
                    "propertyName");
            }
            if (!info.CanWrite)
            {
                throw new ArgumentException(
                    "The property cannot be written.",
                    "propertyName");
            }
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public ColorViewModel() : this(new Color())
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="color">色の初期値。</param>
        public ColorViewModel(Color color)
        {
            this.Color = color;
        }

        /// <summary>
        /// 色を取得または設定する。
        /// </summary>
        public Color Color
        {
            get { return color; }
            set
            {
                if (value != color)
                {
                    var oldColor = color;

                    color = value;
                    if (BindingPropertySetter != null)
                    {
                        BindingPropertySetter(value);
                    }

                    NotifyColorChanged(oldColor);
                }
            }
        }
        private Color color;

        /// <summary>
        /// 色の R 成分値を取得または設定する。
        /// </summary>
        public byte R
        {
            get { return Color.R; }
            set
            {
                var c = Color;
                if (value != c.R)
                {
                    c.R = value;
                    Color = c;
                }
            }
        }

        /// <summary>
        /// 色の G 成分値を取得または設定する。
        /// </summary>
        public byte G
        {
            get { return Color.G; }
            set
            {
                var c = Color;
                if (value != c.G)
                {
                    c.G = value;
                    Color = c;
                }
            }
        }

        /// <summary>
        /// 色の B 成分値を取得または設定する。
        /// </summary>
        public byte B
        {
            get { return Color.B; }
            set
            {
                var c = Color;
                if (value != c.B)
                {
                    c.B = value;
                    Color = c;
                }
            }
        }

        /// <summary>
        /// 色の A 成分値を取得または設定する。
        /// </summary>
        public byte A
        {
            get { return Color.A; }
            set
            {
                var c = Color;
                if (value != c.A)
                {
                    c.A = value;
                    Color = c;
                }
            }
        }

        /// <summary>
        /// 色の ScR 成分値を取得または設定する。
        /// </summary>
        public float ScR
        {
            get { return Color.ScR; }
            set
            {
                var v = Math.Min(Math.Max(0.0f, value), 1.0f);
                var c = Color;
                if (v != c.ScR)
                {
                    c.ScR = v;
                    Color = c;
                }
            }
        }

        /// <summary>
        /// 色の ScG 成分値を取得または設定する。
        /// </summary>
        public float ScG
        {
            get { return Color.ScG; }
            set
            {
                var v = Math.Min(Math.Max(0.0f, value), 1.0f);
                var c = Color;
                if (v != c.ScG)
                {
                    c.ScG = v;
                    Color = c;
                }
            }
        }

        /// <summary>
        /// 色の ScB 成分値を取得または設定する。
        /// </summary>
        public float ScB
        {
            get { return Color.ScB; }
            set
            {
                var v = Math.Min(Math.Max(0.0f, value), 1.0f);
                var c = Color;
                if (v != c.ScB)
                {
                    c.ScB = v;
                    Color = c;
                }
            }
        }

        /// <summary>
        /// 色の ScA 成分値を取得または設定する。
        /// </summary>
        public float ScA
        {
            get { return Color.ScA; }
            set
            {
                var v = Math.Min(Math.Max(0.0f, value), 1.0f);
                var c = Color;
                if (v != c.ScA)
                {
                    c.ScA = v;
                    Color = c;
                }
            }
        }

        /// <summary>
        /// Color プロパティの変更時に呼び出されるイベント。
        /// </summary>
        public event EventHandler ColorChanged;

        /// <summary>
        /// Bind メソッドによってインスタンスを構築した場合に
        /// バインド元の PropertyChanged イベントに追加されるデリゲートメソッド。
        /// </summary>
        public void OnBindingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BindingPropertyName)
            {
                this.Color = BindingPropertyGetter();
            }
        }

        /// <summary>
        /// バインド元プロパティ名を取得または設定する。
        /// </summary>
        private string BindingPropertyName { get; set; }

        /// <summary>
        /// バインド元プロパティから値を取得するデリゲートを取得または設定する。
        /// </summary>
        private Func<Color> BindingPropertyGetter { get; set; }

        /// <summary>
        /// バインド元プロパティに値を設定するデリゲートを取得または設定する。
        /// </summary>
        private Action<Color> BindingPropertySetter { get; set; }

        /// <summary>
        /// 色が変更された時に呼び出される。
        /// </summary>
        /// <param name="oldColor">変更前の色。</param>
        private void NotifyColorChanged(Color oldColor)
        {
            var newColor = this.Color;

            if (newColor != oldColor)
            {
                if (ColorChanged != null)
                {
                    ColorChanged(this, EventArgs.Empty);
                }
                NotifyPropertyChanged("Color");
            }

            if (newColor.R != oldColor.R)
            {
                NotifyPropertyChanged("R");
            }
            if (newColor.G != oldColor.G)
            {
                NotifyPropertyChanged("G");
            }
            if (newColor.B != oldColor.B)
            {
                NotifyPropertyChanged("B");
            }
            if (newColor.A != oldColor.A)
            {
                NotifyPropertyChanged("A");
            }

            if (newColor.ScR != oldColor.ScR)
            {
                NotifyPropertyChanged("ScR");
            }
            if (newColor.ScG != oldColor.ScG)
            {
                NotifyPropertyChanged("ScG");
            }
            if (newColor.ScB != oldColor.ScB)
            {
                NotifyPropertyChanged("ScB");
            }
            if (newColor.ScA != oldColor.ScA)
            {
                NotifyPropertyChanged("ScA");
            }
        }
    }
}
