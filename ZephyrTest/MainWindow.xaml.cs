using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Web.Script.Serialization;

namespace ZephyrTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string URL = "http://api.zephyrportal.com/test.ashx";
        StartStopButtonUpdater ButtonUpdater;
        private Dictionary<string, Func<double, string>> OutputMethods;
        private Func<double, string> SelectedOutputMethod;
        public MainWindow()
        {
            InitializeComponent();
            OutputMethods = new Dictionary<string, Func<double, string>>
            {
                { "ARITHMETIC", ReturnArithmetic },
                { "HEX", ReturnHex }
            };
            SelectedOperation.ItemsSource = OutputMethods.Keys;
            Select_Operation_Click(this, new RoutedEventArgs());
            ButtonUpdater = new StartStopButtonUpdater();
            this.DataContext = ButtonUpdater;
        }
        /// <summary>
        /// Click handler to set active output formatting method
        /// </summary>
        private void Select_Operation_Click(object sender, RoutedEventArgs e)
        {
            SelectedOutputMethod = OutputMethods[(string)SelectedOperation.SelectedItem];
        }
        /// <summary>
        /// Arithmentic output formatter
        /// <para>Converts double to string for printing</para> 
        /// </summary>
        private string ReturnArithmetic(double value)
        { return value.ToString(); }
        /// <summary>
        /// Hex output formatter
        /// <para>Casts double to int then converts to hex string for printing</para>
        /// </summary>
        private string ReturnHex(double value)
        { return "0x" + ((int)value).ToString("X"); }
        private CancellationTokenSource _CancelSource;
        /// <summary>
        /// Click handler to start or stop the background thread
        /// </summary>
        private void Start_Stop_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonUpdater.Text == "START")
            {
                _CancelSource = new CancellationTokenSource();
                DoBackgroundWork(_CancelSource.Token);
            }
            else
            {
                _CancelSource?.Cancel();
                _CancelSource = null;
            }
            ButtonUpdater.UpdateState();
        }
        /// <summary>
        /// Async background method
        /// <para>Calls Zephyr API for ZephyrData structure to evaluate and add result to OutputData</para>
        /// </summary>
        private async void DoBackgroundWork(CancellationToken cancelToken)
        {
            try
            {
                var JSONSerializer = new JavaScriptSerializer();
                using (var HttpClient = new System.Net.Http.HttpClient())
                    while (true)
                    {
                        cancelToken.ThrowIfCancellationRequested();
                        string JSON = await HttpClient.GetStringAsync(URL);
                        var Data = JSONSerializer.Deserialize<ZephyrData>(JSON);
                        OutputData.Items.Insert(0, SelectedOutputMethod(Data.Evaluate()));
                        await Task.Delay(1000);
                    }
            }
            catch (OperationCanceledException) { }
        }
    }
    /// <summary>
    /// Helper class to cycle through available Start/Stop button states
    /// </summary>
    class StartStopButtonUpdater : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string[] ButtonStates;
        private int SelectedIndex;

        private string _Text;
        public StartStopButtonUpdater()
        {
            ButtonStates = new string[2] { "START", "STOP" };
            _Text = ButtonStates[0];
        }
        public string Text
        {
            get { return _Text; }
            private set
            {
                _Text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Text"));
            }
        }
        /// <summary>
        /// Update button text to next available state
        /// </summary>
        public void UpdateState()
        {
            SelectedIndex = ++SelectedIndex % ButtonStates.Length;
            Text = ButtonStates[SelectedIndex];
        }
    }
}
