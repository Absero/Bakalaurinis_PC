using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

/*
 *  delay padaryt galima pries funkcija pridejus async ir naudojant await Task.Delay(5);
 *  https://stackoverflow.com/questions/18372355/how-can-i-perform-a-short-delay-in-c-sharp-without-using-sleep
 *
 */

/*
 * TODO:
 */

namespace adcLogeris_WPFFramework {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {

        #region kintamieji

        private const int KANALU_SKAICIUS = 11;     //pakeitus reik priderint ir visa kita (pvz laukeliu skaiciu)
        private const int PAKETO_DYDIS = KANALU_SKAICIUS * 2;
        private const string sensorIP = "192.168.0.150";
        private const string mNewFileHeader = "Time,1,2,3,4,5,6,7,8,9,10,Temp,Amplitude(mm)\n";

        private int mFailuSkaitliukas = 1;

        private string mFullPath;

        private bool mSensorInitialised = false;
        private bool mConnFlagUSB = false;

        private static System.Timers.Timer mTimer;      //taimeris periodiskai tikrint ar viskas prisijunge

        private SerialPort mSerialPort;
        private SimpleTcpClient sensoriusTCP_conf;      //konfiguravimo serveris
        private SimpleTcpClient sensoriusTCP_meas;      //matavimu serveris

        //krosnies kintamieji
        private SerialPort mPortKrosnis;

        #endregion kintamieji

        #region properties

        private double _mKrosniesPradinisTaskas = 24; public double mKrosniesPradinisTaskas {
            get { return _mKrosniesPradinisTaskas; }
            set {
                if (_mKrosniesPradinisTaskas != value) {
                    _mKrosniesPradinisTaskas = value;
                    NotifyPropertyChanged(nameof(mKrosniesPradinisTaskas));
                }
            }
        }

        private int _mKrosniesStatumas = 99; public int mKrosniesStatumas {
            get { return _mKrosniesStatumas; }
            set {
                if (_mKrosniesStatumas != value) {
                    _mKrosniesStatumas = value;
                    NotifyPropertyChanged(nameof(mKrosniesStatumas));
                }
            }
        }

        public String mStatusImage { get; set; } = "/Images/sauktukas.png";

        public String mStatusImageTCP { get; set; } = "/Images/sauktukas.png";
        public String mSensorStatus { get; set; }
        public String mConfServerStatus { get; set; }
        public String mMeasServerStatus { get; set; }

        private String _mKrosnisStatus = "/Images/empty.png"; public String mKrosnisStatus {
            get { return _mKrosnisStatus; }
            set {
                _mKrosnisStatus = value;
                NotifyPropertyChanged(nameof(mKrosnisStatus));
            }
        }

        public List<string> mKanalaiVertes { get; set; } = new List<string>() { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
        public String mFilePath { get; set; }

        public bool mIrasineti { get; set; } = false;

        private String _failoVardas; public String mFileName {
            get => _failoVardas;
            set {
                if (!value.EndsWith(".csv")) {
                    if (value.Contains(".")) {
                        int index = value.IndexOf(".");
                        value = value.Substring(0, index);
                    }
                    value += ".csv";
                }
                _failoVardas = value;
            }
        }

        public String mLastDistance { get; set; } = "0";
        public String mLastAmplitude { get; set; } = "0";

        private int __mEiluciuSkaitliukas = 1; public int mEiluciuSkaitliukas {
            get { return __mEiluciuSkaitliukas; }
            set {
                if (__mEiluciuSkaitliukas != value) {
                    __mEiluciuSkaitliukas = value;
                    NotifyPropertyChanged(nameof(mEiluciuSkaitliukas));
                }
            }
        }

        private bool _mKrosnisNePaleista = true;

        public bool mKrosnisNePaleista {
            get { return _mKrosnisNePaleista; }
            set {
                _mKrosnisNePaleista = value;
                NotifyPropertyChanged("mKrosnisNePaleista");
            }
        }

        private bool __KrosnisTempWait = false; public bool KrosnisTempWait {
            get { return __KrosnisTempWait; }
            set {
                if (__KrosnisTempWait != value) {
                    __KrosnisTempWait = value;
                    NotifyPropertyChanged(nameof(KrosnisTempWait));
                }
            }
        }

        #endregion properties

        #region grafiko braizymo funkcijos

        //
        public class MeasureModel {
            public DateTime DateTime { get; set; }
            public double Value { get; set; }
        }

        private double _axisMax;
        private double _axisMin;
        private double _axisStep;

        public ChartValues<MeasureModel> ChartValues { get; set; }
        public ChartValues<MeasureModel> EinamasTaskasValue { get; set; }

        public Func<double, string> DateTimeFormatter { get; set; }

        public double AxisStep {
            get { return _axisStep; }
            set {
                _axisStep = value;
                NotifyPropertyChanged("AxisStep");
            }
        }

        public double AxisUnit { get; set; }

        public double AxisMax {
            get { return _axisMax; }
            set {
                _axisMax = value;
                NotifyPropertyChanged("AxisMax");
            }
        }

        public double AxisMin {
            get { return _axisMin; }
            set {
                _axisMin = value;
                NotifyPropertyChanged("AxisMin");
            }
        }

        private void SetAxisLimits(DateTime now) {
            AxisMax = now.Ticks + TimeSpan.FromMinutes(1).Ticks; // lets force the axis to be 1 second ahead
            AxisMin = now.Ticks - TimeSpan.FromMinutes(8).Ticks; // and 8 seconds behind
        }

        #endregion grafiko braizymo funkcijos

        #region funkcionavimo funkcijos

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            sensoriusTCP_conf.Disconnect();
            sensoriusTCP_meas.Disconnect();
            //TCP_On_Off(false, "", "");
            AtjungtiPorta();

            mTimer.Stop();
            mTimer.Dispose();
            mTimerKrosnis.Stop();
            mTimerKrosnis.Dispose();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void NumberValidationTextBox2(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9.-]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void LoseFocusOnEnter(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                string senderName = ((TextBox)sender).Name;
                Keyboard.ClearFocus();

                if (senderName.Equals("periodoLaukas")) {
                    TaskuSkaiciusFaile.Text = String.Format("{0}", (int)(60 * 60 * 24 / ((double)(Int32.Parse(periodoLaukas.Text)) / 1000)));
                    if (mSerialPort != null && mSerialPort.IsOpen)
                        if (UInt16.TryParse(periodoLaukas.Text, out UInt16 temp)) {
                            byte[] siuntimoBuferis = new byte[] { 2, (byte)(temp & 0x00ff), (byte)((temp & 0xff00) >> 8) };
                            if (mSerialPort.IsOpen) {
                                mSerialPort.Write(siuntimoBuferis, 0, siuntimoBuferis.Length);
                            }
                        }
                }

                if (senderName.Equals("failoVardas")) {
                    mFailuSkaitliukas = 1;
                    mEiluciuSkaitliukas = 1;
                }
            }
        }

        #endregion funkcionavimo funkcijos

        public MainWindow() {
            InitializeComponent();

            mFilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            int numeris = 0;
            while (File.Exists(mFilePath + "\\" + "file" + numeris + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv")) { numeris++; }
            mFileName = "file" + numeris + "_" + DateTime.Now.ToString("yyyy-MM-dd");
            NotifyPropertyChanged("mFileName");

            TaskuSkaiciusFaile.Text = String.Format("{0}", 60 * 60 * 24 / (Int32.Parse(periodoLaukas.Text) / 1000));

            //konfiguracijos serveris
            sensoriusTCP_conf = new SimpleTcpClient();
            sensoriusTCP_conf.StringEncoder = Encoding.UTF8;
            sensoriusTCP_conf.DataReceived += sensorTCP_ConfServer_DataReceived;
            //duomenu serveris
            sensoriusTCP_meas = new SimpleTcpClient();
            sensoriusTCP_meas.StringEncoder = Encoding.UTF7;
            sensoriusTCP_meas.DataReceived += sensorTCP_MeasServer_DataReceived;

            //taimeris patikrinantis ar viskas kas reikia prisijunge
            mTimer = new System.Timers.Timer(1000 * 60 * 2);
            mTimer.Elapsed += PeriodinisPatikrinimas;
            mTimer.AutoReset = true;
            mTimer.Start();

            //grafikas
            EinamasTaskasValue = new ChartValues<MeasureModel>();

            var mapper = Mappers.Xy<MeasureModel>()
                .X(model => model.DateTime.Ticks)   //use DateTime.Ticks as X
                .Y(model => model.Value);           //use the value property as Y

            //lets save the mapper globally.
            Charting.For<MeasureModel>(mapper);

            //the values property will store our values array
            ChartValues = new ChartValues<MeasureModel>();

            //lets set how to display the X Labels
            DateTimeFormatter = value => new DateTime((long)value).ToString("HH:mm:ss");

            //AxisStep forces the distance between each separator in the X axis
            AxisStep = TimeSpan.FromMinutes(Double.Parse(TempKeitimoIntervalas.Text)).Ticks;
            AxisUnit = TimeSpan.TicksPerMinute;

            AxisMin = einamasLaikas.Ticks;
            AxisMax = einamasLaikas.Ticks + TimeSpan.FromMinutes(Double.Parse(TempKeitimoIntervalas.Text)).Ticks;

            DataContext = this;
        }

        #region USB dalis

        private void COMPort_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (COMPort_ComboBox.SelectedIndex == -1) return;

            mSerialPort = new SerialPort(COMPort_ComboBox.SelectedItem.ToString());

            // jeigu portas jau atidarytas - uzdaryti
            if (mSerialPort.IsOpen) {
                mSerialPort.Close();
                mSerialPort.DataReceived -= MSerialPort_DataReceived;
            }
            mSerialPort.DataReceived += MSerialPort_DataReceived;
            mSerialPort.Open();
            if (mSerialPort.IsOpen) {
                mSerialPort.Write(new byte[] { 1 }, 0, 1);
                mStatusImage = "/Images/OK.png";
                NotifyPropertyChanged("mStatusImage");
                mConnFlagUSB = true;
            } else {
                mStatusImage = "/Images/NOK.png";
                NotifyPropertyChanged("mStatusImage");
            }
        }

        private void MSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            var serialDevice = sender as SerialPort;
            var duomenuSkaicius = serialDevice.BytesToRead;
            Debug.WriteLine("====================================================================================");
            Debug.WriteLine("Gauta baitu> {0:D}", duomenuSkaicius);

            //apdoroti duomenis tik jei gautas reikiamas baitu skaicius, kitu atveju ismesti gautus duomenis
            if (duomenuSkaicius > 0 & duomenuSkaicius % PAKETO_DYDIS == 0) {
                byte[] buffer = new byte[duomenuSkaicius];          //sukurti masyva duomenims
                serialDevice.Read(buffer, 0, buffer.Length);        //nuskaityti duomenis
                // apdoroti gautus duomenis kitoje gijoje
                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    apdorotiDuomenis(duomenuSkaicius, buffer);
                }));
            } else {
                serialDevice.DiscardInBuffer();
            }
        }

        private void apdorotiDuomenis(int duomenuSkaicius, byte[] buffer) {
            for (int i = 0; i < duomenuSkaicius; i += PAKETO_DYDIS) {

                #region DEBUG - isvesti gautus duomenis

                string gautiDuomenys = "";
                for (int j = 0; j < PAKETO_DYDIS; j++) {
                    gautiDuomenys += String.Format("{0:X2} ", buffer[i + j]);
                }
                Debug.WriteLine("Duomenys: " + gautiDuomenys);

                #endregion DEBUG - isvesti gautus duomenis

                mKanalaiVertes.Clear(); //isvalyti kad kiekviena kart butu vis is naujo

                //apdoroti duomenis
                for (int j = 0; j < PAKETO_DYDIS - 2; j += 2) {         //paskutiniai baitai temperaturai
                    mKanalaiVertes.Add(String.Format("{0:F4} ", (double)(buffer[j] | buffer[j + 1] << 8) / 0xfff * 3.3));
                }
                mKanalaiVertes.Add(String.Format("{0}", ((double)((Int16)(buffer[PAKETO_DYDIS - 2] | buffer[PAKETO_DYDIS - 1] << 8))) / 16));  //temperatura pakeist i realia

                NotifyPropertyChanged("mKanalaiVertes");

                #region DEBUG - paziuret apdorotus duomenis

                gautiDuomenys = "";
                foreach (string skaicius in mKanalaiVertes) {
                    gautiDuomenys += skaicius + " ";
                }
                Debug.WriteLine("Apdoroti duomenys: " + gautiDuomenys + "\n");

                #endregion DEBUG - paziuret apdorotus duomenis

                if (mSensorInitialised) {
                    //    sendConf("TRIGGERSW");
                    //    //laukti kol bus gauti duomenys
                    //    WaitSensorData();

                    sendConf("RESETSTATISTIC");
                }

                if (mIrasineti) {
                    //irasyti duomenis
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DateTime.Now.ToString("MM/dd_HH:mm:ss"));
                    foreach (string skaicius in mKanalaiVertes)
                        sb.Append("," + skaicius);

                    sb.Append("," + mLastAmplitude);

                    sb.Append("\n");

                    lastWrittenLine.Text = sb.ToString();

                    if (mEiluciuSkaitliukas <= Int32.Parse(TaskuSkaiciusFaile.Text)) {
                        mEiluciuSkaitliukas++;

                        File.AppendAllText(mFullPath, sb.ToString());
                    } else {
                        mEiluciuSkaitliukas = 1;

                        string failoNr = "__" + mFailuSkaitliukas.ToString();
                        if (mFailuSkaitliukas == 1) {
                            mFileName = mFileName.Substring(0, mFileName.Length - 4 - 10) + DateTime.Now.ToString("yyyy-MM-dd") + failoNr + ".csv";
                        } else {
                            mFileName = mFileName.Substring(0, mFileName.Length - 4 - 2 - (mFailuSkaitliukas - 1).ToString().Length - 10) + DateTime.Now.ToString("yyyy-MM-dd") + failoNr + ".csv";
                        }
                        mFailuSkaitliukas++;

                        mFullPath = mFilePath + @"\" + mFileName;
                        try {
                            File.AppendAllText(mFullPath, mNewFileHeader + sb.ToString());
                        } catch (Exception) { /*ignored*/ }
                        NotifyPropertyChanged("mFileName");
                    }
                }
            }
        }

        #endregion USB dalis

        #region sensorius

        #region gautu duomenu callbackai

        private void sensorTCP_ConfServer_DataReceived(object sender, SimpleTCP.Message e) {
            //Debug.WriteLine("\r\n==================================================");
            //Debug.WriteLine("CONFIG SERVER");
            //Debug.WriteLine("Gauti duomenys> \r\n" + e.MessageString);
        }

        private void sensorTCP_MeasServer_DataReceived(object sender, SimpleTCP.Message e) {

            #region debug

            Debug.WriteLine("========================== " + "MEASUREMENT SERVER " + DateTime.Now.ToString("hh:mm:ss"));
            Debug.WriteLine("Gauta duomenu> " + e.MessageString.Length);

            StringBuilder sb = new StringBuilder();
            foreach (byte simbolis in e.MessageString) {
                sb.Append(String.Format("{0:X2},", simbolis));
            }
            Debug.WriteLine("Duomenys: " + sb.ToString());

            #endregion debug

            if (mSensorInitialised) {
                //gautu duomenu apdorojima

                List<byte> data = new List<byte>();
                foreach (byte b in e.MessageString)
                    data.Add(b);

                if (e.MessageString.Length == 36) {
                    //gerai, apdoroti duomenis
                    double atstumas = ((double)((data[28] & 0xff) | (data[29] & 0xff) << 8 | (data[30] & 0xff) << 16 | (data[31] & 0xff) << 24)) / 1000000;
                    double amplitude = ((double)(data[32] | data[33] << 8 | data[34] << 16 | data[35] << 24)) / 1000000;

                    mLastDistance = atstumas.ToString();
                    mLastAmplitude = amplitude.ToString();

                    NotifyPropertyChanged("mLastDistance");
                    NotifyPropertyChanged("mLastAmplitude");
                }
            } else {
                Debug.WriteLine("Neinicijuotas sensorius");
            }
        }

        #endregion gautu duomenu callbackai

        private void sensorConnect_Click(object sender, RoutedEventArgs e) {
            Button button = (Button)sender;

            if (button.Content.Equals("Connect")) {
                if (sensorConnect()) {
                    button.Content = "Disconnect";

                    //nusiusti pradinius nustatymus
                    pradiniaiNustatymai();
                }
            } else {
                mSensorInitialised = false;
                mSensorStatus = "/Images/empty.png";

                sensoriusTCP_conf.Disconnect();
                sensoriusTCP_meas.Disconnect();

                if (!isConnected_conf())
                    mConfServerStatus = "/Images/empty.png";
                else
                    mConfServerStatus = "/Images/sauktukas.png";

                if (!isConnected_meas())
                    mMeasServerStatus = "/Images/empty.png";
                else
                    mMeasServerStatus = "/Images/sauktukas.png";

                if (!isConnected_meas() & !isConnected_conf()) {
                    button.Content = "Connect";
                }

                NotifyPropertyChanged("mSensorStatus");
                NotifyPropertyChanged("mConfServerStatus");
                NotifyPropertyChanged("mMeasServerStatus");
            }
        }

        private async void pradiniaiNustatymai() {
            /*
             *  GETVALUE NONE                   //sustabdyt siuntima. NONE|<Number>|ALL
             *  TRIGGERCOUNT 0                  //sustabdyt trigerinima. 16383 - continuous. Tarp 0 ir 16383 - kiek reiksmiu rodyt po trigerio
             *  TRIGGER SOFTWARE TERMOFF        //ijungt software trigerinima
             *  MEASRATE 20                     //matavimo daznis kHz
             *  OUTPUT ETHERNET                 //ijungt kad duomenis siustu per cia (?)
             *  OUTREDUCE 1                     //kas kelinta reiksme atiduot (visas)
             *  OUTHOLD 0                       //Infinite holding of the last measurement value
             *  MEASMODE DIST_DIFFUSE           //pasirenkamas rezimas (standartinis)
             *  MEASPEAK DISTA                  //pasirenkama kuria reiksme atiduoda (standartinis)
             *  STATISTICDEPTH 16384            //kiek reiksmiu naudot statistikom. 2^N arba ALL (ALL ima reiksmes iki kol gauna RESETSTATISTIC)
             *  MASTERMV NONE                   //nulio vieta
             *
             *
             *  //toliau reikalingu duomenu pasirinkimas (ethernetu visada siuncia dist1)
             *  OUTSTATISTIC_ETH                //NONE|([MIN] [MAX] [PEAK2PEAK])
             *  OUTADD_ETH                      //NONE|([SHUTTER][COUNTER] [TIMESTAMP] [INTENSITY] [STATE] [TRIGCNT] [TEMP])
             */

            mSensorInitialised = false;

            sendConf("TRIGGERCOUNT 0");
            sendConf("GETVALUE NONE");
            sendConf("TRIGGER SOFTWARE TERMOFF");
            sendConf("MEASRATE 20");
            sendConf("OUTPUT ETHERNET");
            sendConf("OUTREDUCE 800");
            sendConf("OUTHOLD 0");
            sendConf("MEASMODE DIST_DIFFUSE");
            sendConf("MEASPEAK DIST1");
            sendConf("OUTSTATISTIC_ETH PEAK2PEAK");
            sendConf("OUTADD_ETH NONE");
            sendConf("STATISTICDEPTH ALL");

            await Task.Delay(20);   //per tiek laiko turetu viska sukonfiguruot
            mSensorInitialised = true;

            //paleist siuntima
            sendConf("TRIGGERCOUNT 16383");         //rodyt tiek reiksmiu po trigerio
            sendConf("GETVALUE ALL");             //gaut tiek reiksmiu is viso
            sendConf("TRIGGERSW");              //pradeti gavima (jei periodinis)
        }

        private bool sensorConnect() {
            mSensorStatus = "/Images/empty.png";
            mConfServerStatus = "/Images/empty.png";
            mMeasServerStatus = "/Images/empty.png";

            //papingint ar randa prietaisa
            if (PingHost(sensorIP)) {
                mSensorStatus = "/Images/OK.png";
                NotifyPropertyChanged("mSensorStatus");
            } else {
                mSensorStatus = "/Images/NOK.png";
                NotifyPropertyChanged("mSensorStatus");
                return false;
            }

            //atjungti jei prisijunge
            if (!isConnected_conf())
                sensoriusTCP_conf.Disconnect();
            if (!isConnected_meas())
                sensoriusTCP_meas.Disconnect();

            //prisijungti prie nustatymu serverio
            try {
                sensoriusTCP_conf.Connect(sensorIP, 23);
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                mConfServerStatus = "/Images/NOK.png";
                NotifyPropertyChanged("mConfServerStatus");
                return false;
            }

            mConfServerStatus = "/Images/OK.png";
            NotifyPropertyChanged("mConfServerStatus");

            //prisijungti prie duomenu serverio
            try {
                sensoriusTCP_meas.Connect(sensorIP, 1024);
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                mMeasServerStatus = "/Images/NOK.png";
                NotifyPropertyChanged("mMeasServerStatus");
                return false;
            }
            mMeasServerStatus = "/Images/OK.png";
            NotifyPropertyChanged("mMeasServerStatus");

            return true;
        }

        public static bool PingHost(string nameOrAddress) {
            bool pingable = false;
            Ping pinger = null;

            try {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            } catch (PingException) {
                // Discard PingExceptions and return false;
            } finally {
                if (pinger != null) {
                    pinger.Dispose();
                }
            }
            return pingable;
        }

        private bool isConnected_conf() {
            return sensoriusTCP_conf.TcpClient != null;
        }

        private bool isConnected_meas() {
            return sensoriusTCP_meas.TcpClient != null;
        }

        private void sendConf(string data) {
            if (isConnected_conf()) {
                data += "\n";
                //Debug.WriteLine("Siusta> " + data);
                try {
                    sensoriusTCP_conf.Write(data);
                } catch (Exception) { /*ignored*/ }
            }
        }

        #endregion sensorius

        #region misc

        private void IeskotiPortu_Click(object sender, RoutedEventArgs e) {
            COMPort_ComboBox_Krosnis.ItemsSource = "";
            COMPort_ComboBox_Krosnis.SelectedIndex = -1;
            COMPort_ComboBox_Krosnis.ItemsSource = SerialPort.GetPortNames();
        }

        private void IeskotiPortu_Click_krosnis(object sender, RoutedEventArgs e) {
            COMPort_ComboBox_Krosnis.ItemsSource = "";
            COMPort_ComboBox_Krosnis.SelectedIndex = -1;
            COMPort_ComboBox_Krosnis.ItemsSource = SerialPort.GetPortNames();
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e) {
            AtjungtiPorta();
        }

        private void COMPort_ComboBox_Loaded(object sender, RoutedEventArgs e) {
            COMPort_ComboBox.ItemsSource = SerialPort.GetPortNames();
            COMPort_ComboBox_Krosnis.ItemsSource = SerialPort.GetPortNames();
        }

        private void ChooseFolder_Click(object sender, RoutedEventArgs e) {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = mFilePath;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = mFilePath;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok) {
                mFilePath = dlg.FileName;
                NotifyPropertyChanged("mFilePath");
                // Do something with selected folder string
            }
        }

        private void PradetiStabdytiIrasyma(object sender, RoutedEventArgs e) {
            Button mygtukas = (Button)sender;

            if (mygtukas.Content.ToString().Equals("Start")) {

                #region patikrinimai

                if (mSerialPort == null) {
                    statusLabel.Content = "Port is closed";
                    return;
                }
                if (!mSerialPort.IsOpen) {
                    statusLabel.Content = "Port is closed";
                    return;
                }
                if (!Directory.Exists(mFilePath)) {
                    statusLabel.Content = "Folder not found";
                    return;
                }
                if (mFileName.Length == 4 & mFileName.Contains(".csv")) {
                    statusLabel.Content = "File name invalid";
                    return;
                }

                #endregion patikrinimai

                mFullPath = mFilePath + @"\" + mFileName;
                if (File.Exists(mFullPath)) {
                    statusLabel.Content = "Recording to an OLD file";
                } else {
                    statusLabel.Content = "Recording to a NEW file";
                    File.AppendAllText(mFullPath, mNewFileHeader);
                }

                mIrasineti = true;

                mygtukas.Content = "Stop";
            } else {
                mygtukas.Content = "Start";
                mIrasineti = false;
                statusLabel.Content = "Not recording";
            }
            NotifyPropertyChanged("mIrasineti");
        }

        private void NustatymoMygtukas_click(object sender, RoutedEventArgs e) {
            TaskuSkaiciusFaile.Text = String.Format("{0}", (int)(60 * 60 * 24 / ((double)(Int32.Parse(periodoLaukas.Text)) / 1000)));
            if (UInt16.TryParse(periodoLaukas.Text, out UInt16 temp)) {
                byte[] siuntimoBuferis = new byte[] { 2, (byte)(temp & 0x00ff), (byte)((temp & 0xff00) >> 8) };
                if (mSerialPort.IsOpen) {
                    mSerialPort.Write(siuntimoBuferis, 0, siuntimoBuferis.Length);
                }
            }
        }

        private void AtjungtiPorta() {
            if (mSerialPort != null)
                if (mSerialPort.IsOpen) {
                    mSerialPort.Write(new byte[] { 0 }, 0, 1);
                    mSerialPort.Close();
                    mSerialPort.DataReceived -= MSerialPort_DataReceived;
                    COMPort_ComboBox.SelectedIndex = -1;
                    mStatusImage = "/Images/sauktukas.png";
                    NotifyPropertyChanged("mStatusImage");
                    COMPort_ComboBox.ItemsSource = "";
                    mConnFlagUSB = false;
                }
        }

        private void PeriodinisPatikrinimas(object sender, ElapsedEventArgs e) {
            //patikrinti ar USB portas prisijunges
            if (mConnFlagUSB)           //ar turi but prisijunges
                if (mSerialPort != null)
                    if (!mSerialPort.IsOpen) {          //ar dabar atsijunges
                        try {
                            mSerialPort.Open();
                        } catch (IOException ex) { Debug.WriteLine(ex.Message); }  //negalejo prisijungti, nera porto

                        try {
                            if (mSerialPort.IsOpen) {
                                mSerialPort.Write(new byte[] { 1 }, 0, 1);      //paleisti siuntima
                                mStatusImage = "/Images/OK.png";
                                NotifyPropertyChanged("mStatusImage");
                            } else {
                                mStatusImage = "/Images/NOK.png";
                                NotifyPropertyChanged("mStatusImage");
                            }
                        } catch (Exception) { /*ignored*/}
                    }
        }

        #endregion misc

        #region krosnis

        #region krosnies kintamieji

        private DateTime einamasLaikas = DateTime.Parse("00:00:00");
        private static System.Timers.Timer mTimerKrosnis = new System.Timers.Timer();

        private int _TemperaturosSkaitliukas = -1; public int mTemperaturosSkaitliukas {
            get { return _TemperaturosSkaitliukas; }
            set {
                EinamasTaskasValue.Clear();
                if (_TemperaturosSkaitliukas != value) {
                    _TemperaturosSkaitliukas = value;
                    if (value >= 0 & value < ChartValues.Count & ChartValues.Count > 0) {
                        EinamasTaskasValue.Add(new MeasureModel {
                            DateTime = ChartValues[value].DateTime,
                            Value = ChartValues[value].Value
                        });
                    }
                    NotifyPropertyChanged(nameof(mTemperaturosSkaitliukas));
                }
            }
        }

        private int mIntervaloDaugiklis = 1000 * 60; //intervalo laikas

        private double __mTempRibos = 1; public double mTempRibos {
            get { return __mTempRibos; }
            set {
                if (__mTempRibos != value) {
                    __mTempRibos = value;
                    NotifyPropertyChanged(nameof(mTempRibos));
                }
            }
        }

        #endregion krosnies kintamieji

        private void Disconnect_Krosnis_Click(object sender, RoutedEventArgs e) {
            if (mPortKrosnis != null)
                if (mPortKrosnis.IsOpen) {
                    mPortKrosnis.Close();
                    mPortKrosnis.DataReceived -= MSerialPort_DataReceived_Krosnis;
                    COMPort_ComboBox_Krosnis.SelectedIndex = -1;
                    COMPort_ComboBox_Krosnis.ItemsSource = "";
                    atsijungimoMygtukas_krosnis.IsEnabled = false;
                }
        }

        private async void COMPort_ComboBox_SelectionChanged_Krosnis(object sender, SelectionChangedEventArgs e) {
            try {
                if (COMPort_ComboBox_Krosnis.SelectedIndex == -1) return;

                mPortKrosnis = new SerialPort(COMPort_ComboBox_Krosnis.SelectedItem.ToString());
                mPortKrosnis.BaudRate = 9600;
                mPortKrosnis.ReadTimeout = 500;

                // jeigu portas jau atidarytas - uzdaryti
                if (mPortKrosnis.IsOpen) {
                    mPortKrosnis.Close();
                    mPortKrosnis.DataReceived -= MSerialPort_DataReceived_Krosnis;
                }

                //mPortKrosnis.DataReceived += MSerialPort_DataReceived_Krosnis;
                mPortKrosnis.Open();
                if (mPortKrosnis.IsOpen) {
                    atsijungimoMygtukas_krosnis.IsEnabled = true;
                    mPortKrosnis.DiscardInBuffer();
                    mPortKrosnis.Write("R99A");
                    await Task.Delay(200);
                    if (mPortKrosnis.BytesToRead > 0) {
                        mPortKrosnis.DiscardInBuffer();
                        mKrosnisStatus = "/Images/OK.png";
                    } else {
                        mKrosnisStatus = "/Images/NOK.png";
                    }
                }
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void MSerialPort_DataReceived_Krosnis(object sender, SerialDataReceivedEventArgs e) {
        }

        private void AddNextValueKrosnis(object sender, RoutedEventArgs e) {
            if (!NextTempValue.Text.Equals("")) {
                AddKrosniesGrafikasValue(Double.Parse(NextTempValue.Text));
            }
        }

        private void AddKrosniesGrafikasValue(double value) {
            ChartValues.Add(new MeasureModel {
                DateTime = einamasLaikas,
                Value = value
            });
            AxisMax = einamasLaikas.Ticks;
            einamasLaikas += TimeSpan.FromMinutes(Double.Parse(TempKeitimoIntervalas.Text));
            mTemperaturosSkaitliukas = ChartValues.Count - 1;
        }

        private void GenerateFunction(object sender, RoutedEventArgs e) {
            int taskuSkaicius = Int32.Parse(TaskuSkaiciusFunkcijoj.Text);
            double keitimoIntervalas = Double.Parse(TempKeitimoIntervalas.Text);
            double amplitude = Double.Parse(NextTempValue.Text);

            ComboBoxItem pasirinkimas = (ComboBoxItem)(funkcijosForma.SelectedItem);

            if (pasirinkimas.Content.Equals("Sine")) {
                for (int i = 0; i < taskuSkaicius; i++) {
                    AddKrosniesGrafikasValue(amplitude * Math.Sin(
                        2 * Math.PI * i / taskuSkaicius
                        ) + mKrosniesPradinisTaskas);
                }
                return;
            }

            if (pasirinkimas.Content.Equals("Cosine"))
                for (int i = 0; i < taskuSkaicius; i++) {
                    AddKrosniesGrafikasValue(amplitude * Math.Cos(
                        2 * Math.PI * i / taskuSkaicius
                        ) + mKrosniesPradinisTaskas);
                }

            if (pasirinkimas.Content.Equals("Line"))
                for (int i = 0; i < taskuSkaicius; i++) {
                    AddKrosniesGrafikasValue(mKrosniesPradinisTaskas + ((Double.Parse(NextTempValue.Text) - mKrosniesPradinisTaskas) / taskuSkaicius * (i + 1)));
                }
        }

        private void resetGraph(object sender, RoutedEventArgs e) {
            ChartValues.Clear();

            einamasLaikas = DateTime.Parse("00:00:00");
            AxisStep = TimeSpan.FromMinutes(Double.Parse(TempKeitimoIntervalas.Text)).Ticks;
            AxisMin = einamasLaikas.Ticks;
            AxisMax = einamasLaikas.Ticks + TimeSpan.FromMinutes(3).Ticks;
            mTemperaturosSkaitliukas = 0;
        }

        private async void StartKrosnis(object sender, RoutedEventArgs e) {
            Button mygt = (Button)sender;
            if (ChartValues.Count > 0) {
                mygt.IsEnabled = false;
                if (mygt.Content.Equals("START")) {
                    krosnisTimeStamp.Content = DateTime.Now.ToString("HH:mm:ss");
                    mygt.Content = "STOP";
                    krosnisSendCommand(String.Format("W0800000000{0,2:D02}00000A", mKrosniesStatumas));         //nusiust temperaturos keitimo greiti
                    await Task.Delay(100);

                    //nusiust pradine temperatura
                    mTemperaturosSkaitliukas = -1;
                    krosnisSendNextTemp();

                    await Task.Delay(100);
                    krosnisSendCommand("W9902A");           //paleist krosni
                    await Task.Delay(100);
                    mKrosnisNePaleista = false;

                    mTimerKrosnis = new System.Timers.Timer(Double.Parse(TempKeitimoIntervalas.Text) * mIntervaloDaugiklis);
                    mTimerKrosnis.Elapsed += KrosniesPatikrinimas;
                    mTimerKrosnis.AutoReset = true;
                    mTimerKrosnis.Start();
                } else {
                    mTimerKrosnis.Stop();
                    mTimerKrosnis.Elapsed -= KrosniesPatikrinimas;
                    mygt.Content = "START";
                    krosnisSendCommand("W9901A");           //sustabdyt krosni
                    mKrosnisNePaleista = true;
                }
                mygt.IsEnabled = true;
            }
        }

        private double KrosnisDecodeTemp(byte[] buf) {
            double temperatura;
            if (buf.Length == 21) {
                temperatura = buf[1] * 10 + buf[2] + buf[3] * 0.1 + buf[4] * 0.01;
                if (buf[5] == 1) temperatura *= -1;
                return temperatura;
            }
            return -50.0;
        }

        private async Task<bool> KrosnisCheckTemp() {
            if (mPortKrosnis == null ? false : mPortKrosnis.IsOpen) {
                krosnisSendCommand("R01A"); //paklaust kokia dabar temperatura

                int ilgis = 21;
                byte[] atsakymas = new byte[ilgis];
                bool ataskymasGautas = false;

                //palaukt kol bus reikiamas kiekis duomenu
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                while (mPortKrosnis.BytesToRead < ilgis) {
                    if (stopWatch.ElapsedMilliseconds > 500) return false;
                    await Task.Delay(1);
                }
                stopWatch.Stop();

                try {
                    await mPortKrosnis.BaseStream.ReadAsync(atsakymas, 0, ilgis);
                    ataskymasGautas = true;
                } catch (Exception ex) { Debug.WriteLine(ex.Message); }

                if (ataskymasGautas) {
                    //patikrinti ir laukti jei netinka
                    double temperatura = KrosnisDecodeTemp(atsakymas);

                    return (temperatura <= ChartValues[mTemperaturosSkaitliukas].Value + mTempRibos &&
                        temperatura >= ChartValues[mTemperaturosSkaitliukas].Value - mTempRibos);
                }
            }
            return false;
        }

        private bool SensorTempCheck() {
            Double temperatura;
            bool ok = Double.TryParse(mKanalaiVertes[10], out temperatura);
            if (ok) {
                return (temperatura <= ChartValues[mTemperaturosSkaitliukas].Value + mTempRibos &&
                    temperatura >= ChartValues[mTemperaturosSkaitliukas].Value - mTempRibos);
            } else { return true; /* true kad siustu tiesiog kita temperatura */ }
        }

        private async void KrosniesPatikrinimas(object sender, ElapsedEventArgs e) {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                krosnisTimeStamp.Content = DateTime.Now.ToString("HH:mm:ss");
            }));

            if (KrosnisTempWait ? await KrosnisCheckTemp() : SensorTempCheck()) {
                await Task.Delay(100);
                krosnisSendNextTemp();
                await Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    double intervalas = Double.Parse(TempKeitimoIntervalas.Text) * mIntervaloDaugiklis;
                    if (mTimerKrosnis.Interval != intervalas) {
                        mTimerKrosnis.Interval = intervalas;
                    }
                }));
            } else {
                await Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    if (mTimerKrosnis.Interval != mIntervaloDaugiklis)
                        mTimerKrosnis.Interval = mTimerKrosnis.Interval < mIntervaloDaugiklis ? mTimerKrosnis.Interval : mIntervaloDaugiklis;   //kas minute arba maziau
                }));
            }
        }

        private async void krosnisSendCommand(String command) {
            Debug.WriteLine(command);
            if (mPortKrosnis == null ? false : mPortKrosnis.IsOpen) {
                byte[] siuntimoBuferis = Encoding.ASCII.GetBytes(command);
                if (mPortKrosnis.IsOpen) {
                    try {
                        await mPortKrosnis.BaseStream.WriteAsync(siuntimoBuferis, 0, siuntimoBuferis.Length);
                    } catch (Exception) { /*ignored*/ }
                }
            }
        }

        private void krosnisSendNextTemp() {
            mTemperaturosSkaitliukas = mTemperaturosSkaitliukas >= ChartValues.Count - 1 ? 0 : mTemperaturosSkaitliukas + 1;

            double temperatura = ChartValues[mTemperaturosSkaitliukas].Value;
            if (temperatura > 99) temperatura = 99;
            if (temperatura < -40) temperatura = -40;

            double temp = Math.Abs(temperatura);
            int temperaturaSveika = (int)temp;
            int temperaturaTrupm = (int)((temp - temperaturaSveika) * 100);

            krosnisSendCommand(String.Format("W010{0,2:D02}{1,2:D02}{2}000A", temperaturaSveika, temperaturaTrupm, temperatura >= 0 ? "00" : "11"));
        }

        #endregion krosnis
    }
}