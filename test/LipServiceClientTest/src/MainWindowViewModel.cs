using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using ruche.mmd.service.lip;

namespace LipServiceClientTest
{
    /// <summary>
    /// MainWindow の ViewModel 。
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// コマンドをシリアライズする。
        /// </summary>
        /// <param name="command">コマンド。</param>
        /// <returns>シリアライズ結果。</returns>
        private static string SerializeCommand(LipServiceCommand command)
        {
            var serializer = new DataContractSerializer(command.GetType());

            var output = new StringBuilder();
            var xmlSettings = new XmlWriterSettings { Indent = true };
            using (var writer = XmlWriter.Create(output, xmlSettings))
            {
                serializer.WriteObject(writer, command);
            }

            return output.ToString();
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MainWindowViewModel()
        {
            this.Client = LipServiceClient.OpenNetNamedPipe("MikuMikuLipMaker");
            this.Client.RaiseCommand += OnClientRaiseCommand;

            this.Text = SerializeCommand(this.Client.LastCommand);
        }

        /// <summary>
        /// コマンドから作成したテキストを取得する。
        /// </summary>
        public string Text
        {
            get { return _text; }
            private set
            {
                var v = value ?? "";
                if (v != _text)
                {
                    _text = v;
                    this.PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs("Text"));
                }
            }
        }
        private string _text = "";

        /// <summary>
        /// プロパティ値の変更時に呼び出されるイベント。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 口パクサービスクライアントを取得または設定する。
        /// </summary>
        private LipServiceClient Client { get; set; }

        /// <summary>
        /// 新規コマンド発行時に呼び出される。
        /// </summary>
        private void OnClientRaiseCommand(
            object sender,
            LipServiceCommandEventArgs e)
        {
            this.Text = SerializeCommand(e.Command);
        }
    }
}
