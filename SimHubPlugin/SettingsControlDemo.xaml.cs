//using SimHub.Plugins.OutputPlugins.Dash.GLCDTemplating;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Media.TextFormatting;
using System.Text.Json;
using FMOD;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Web;
using MahApps.Metro.Controls;
using System.Runtime.CompilerServices;
using System.CodeDom.Compiler;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Input;
using System.Windows.Shapes;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using SimHub.Plugins.OutputPlugins.GraphicalDash.PSE;
using SimHub.Plugins.Styles;
using System.Windows.Media;
using System.Runtime.Remoting.Messaging;
using SimHub.Plugins.OutputPlugins.GraphicalDash.Behaviors.DoubleText.Imp;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Threading;
using System.Text.RegularExpressions;
using SimHub.Plugins;
using log4net.Plugin;
//using System.Drawing;

using vJoyInterfaceWrap;
//using vJoy.Wrapper;
using System.Runtime;

// Win 11 install, see https://github.com/jshafer817/vJoy/releases
//using vJoy.Wrapper;



namespace User.PluginSdkDemo
{


    /// <summary>
    /// Logique d'interaction pour SettingsControlDemo.xaml
    /// </summary>
    public partial class SettingsControlDemo : System.Windows.Controls.UserControl
    {


        // payload revisiom
        //public uint pedalConfigPayload_version = 110;
        //public uint pedalConfigPayload_type = 100;
        //public uint pedalActionPayload_type = 110;

        public uint indexOfSelectedPedal_u = 1;

        public DataPluginDemo Plugin { get; }


        public DAP_config_st[] dap_config_st = new DAP_config_st[3];
        private string stringValue;


        public bool[] waiting_for_pedal_config = new bool[3];
        public System.Windows.Forms.Timer[] pedal_serial_read_timer = new System.Windows.Forms.Timer[3];
        //public System.Timers.Timer[] pedal_serial_read_timer = new System.Timers.Timer[3];
        int printCtr = 0;

        public double[] Force_curve_Y = new double[100];
        public bool debug_flag = false;

        //public VirtualJoystick joystick;
        internal vJoyInterfaceWrap.vJoy joystick;

        // read config from JSON on startup
        //ReadStructFromJson();


        // read JSON config from JSON file
        //private void ReadStructFromJson()
        //{



        //    try
        //    {
        //        // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to?pivots=dotnet-8-0
        //        // https://www.educative.io/answers/how-to-read-a-json-file-in-c-sharp

        //        string currentDirectory = Directory.GetCurrentDirectory();
        //        string dirName = currentDirectory + "\\PluginsData\\Common";
        //        //string jsonFileName = ComboBox_JsonFileSelected.Text;
        //        string jsonFileName = ((ComboBoxItem)ComboBox_JsonFileSelected.SelectedItem).Content.ToString();
        //        string fileName = dirName + "\\" + jsonFileName + ".json";

        //        string text = System.IO.File.ReadAllText(fileName);

        //        DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(DAP_config_st));
        //        var ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
        //        dap_config_st[indexOfSelectedPedal_u] = (DAP_config_st)deserializer.ReadObject(ms);
        //        //TextBox_debugOutput.Text = "Config loaded!";
        //        //TextBox_debugOutput.Text += ComboBox_JsonFileSelected.Text;
        //        //TextBox_debugOutput.Text += "    ";
        //        //TextBox_debugOutput.Text += ComboBox_JsonFileSelected.SelectedIndex;

        //        updateTheGuiFromConfig();

        //    }
        //    catch (Exception caughtEx)
        //    {

        //        string errorMessage = caughtEx.Message;
        //        TextBox_debugOutput.Text = errorMessage;
        //    }


        //}
        private void vjoy_axis_initialize()
        {
            //center all axis/hats reader
            joystick.SetAxis(16384, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_X);
            joystick.SetAxis(16384, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_Y);
            joystick.SetAxis(16384, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_Z);
            joystick.SetAxis(16384, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RX);
            joystick.SetAxis(16384, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RY);
            joystick.SetAxis(16384, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RZ);
            //joystick.SetJoystickHat(0, Hats.Hat);
            //joystick.SetJoystickHat(0, Hats.HatExt1);
            //joystick.SetJoystickHat(0, Hats.HatExt2);
            //joystick.SetJoystickHat(0, Hats.HatExt3);

        }
        private void DrawGridLines()
        {
            // Specify the number of rows and columns for the grid
            int rowCount = 5;
            int columnCount = 5;

            // Calculate the width and height of each cell
            double cellWidth = canvas.Width / columnCount;
            double cellHeight = canvas.Height / rowCount;

            // Draw horizontal gridlines
            for (int i = 1; i < rowCount; i++)
            {
                Line line = new Line
                {
                    X1 = 0,
                    Y1 = i * cellHeight,
                    X2 = canvas.Width,
                    Y2 = i * cellHeight,
                    //Stroke = Brush.Black,
                    Stroke = System.Windows.Media.Brushes.LightSteelBlue,
                    StrokeThickness = 1,
                    Opacity = 0.1

                };
                canvas.Children.Add(line);
            }

            // Draw vertical gridlines
            for (int i = 1; i < columnCount; i++)
            {
                Line line = new Line
                {
                    X1 = i * cellWidth,
                    Y1 = 0,
                    X2 = i * cellWidth,
                    Y2 = canvas.Height,
                    //Stroke = Brushes.Black,
                    Stroke = System.Windows.Media.Brushes.LightSteelBlue,
                    StrokeThickness = 1,
                    Opacity = 0.1
                };
                canvas.Children.Add(line);
            }
        }

        private void InitReadStructFromJson()
        {



            try
            {
                // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to?pivots=dotnet-8-0
                // https://www.educative.io/answers/how-to-read-a-json-file-in-c-sharp
                string jsonFileName="NA";

                string currentDirectory = Directory.GetCurrentDirectory();
                string dirName = currentDirectory + "\\PluginsData\\Common";
                //string jsonFileName = ComboBox_JsonFileSelected.Text;
                if (indexOfSelectedPedal_u == 0)
                {
                    jsonFileName = ("DiyPedalConfig_Clutch_Default");
                }
                else if (indexOfSelectedPedal_u == 1)
                {
                    jsonFileName = ("DiyPedalConfig_Brake_Default");
                }
                else if (indexOfSelectedPedal_u == 2)
                {
                     jsonFileName = ("DiyPedalConfig_Accelerator_Default");                    
                }

                string fileName = dirName + "\\" + jsonFileName + ".json";
                string text = System.IO.File.ReadAllText(fileName);



                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(DAP_config_st));
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
                dap_config_st[indexOfSelectedPedal_u] = (DAP_config_st)deserializer.ReadObject(ms);
                TextBox_debugOutput.Text = "Config loaded!"+ jsonFileName;
                //TextBox_debugOutput.Text += ComboBox_JsonFileSelected.Text;
                //TextBox_debugOutput.Text += "    ";
                //TextBox_debugOutput.Text += ComboBox_JsonFileSelected.SelectedIndex;

                updateTheGuiFromConfig();

            }
            catch (Exception caughtEx)
            {

                string errorMessage = caughtEx.Message;
                TextBox_debugOutput.Text = errorMessage;
            }


        }

        private void UpdateSerialPortList_click()
        {

            var SerialPortSelectionArray = new List<SerialPortChoice>();
            string[] comPorts = SerialPort.GetPortNames();
            if (comPorts.Length > 0)
            {

                foreach (string portName in comPorts)
                {
                    SerialPortSelectionArray.Add(new SerialPortChoice(portName, portName));
                }
            }
            else
            {
                SerialPortSelectionArray.Add(new SerialPortChoice("NA", "NA"));
            }

            SerialPortSelection.DataContext = SerialPortSelectionArray;
        }

        private bool isDragging = false;
        private Point offset;
       


        public SettingsControlDemo()
        {

            







            for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
            {
                dap_config_st[pedalIdx].payloadHeader_.payloadType = (byte)Constants.pedalConfigPayload_type;
                dap_config_st[pedalIdx].payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;

                dap_config_st[pedalIdx].payloadPedalConfig_.pedalStartPosition = 35;
                dap_config_st[pedalIdx].payloadPedalConfig_.pedalEndPosition = 80;
                dap_config_st[pedalIdx].payloadPedalConfig_.maxForce = 90;
                dap_config_st[pedalIdx].payloadPedalConfig_.relativeForce_p000 = 0;
                dap_config_st[pedalIdx].payloadPedalConfig_.relativeForce_p020 = 20;
                dap_config_st[pedalIdx].payloadPedalConfig_.relativeForce_p040 = 40;
                dap_config_st[pedalIdx].payloadPedalConfig_.relativeForce_p060 = 60;
                dap_config_st[pedalIdx].payloadPedalConfig_.relativeForce_p080 = 80;
                dap_config_st[pedalIdx].payloadPedalConfig_.relativeForce_p100 = 100;
                dap_config_st[pedalIdx].payloadPedalConfig_.dampingPress = 0;
                dap_config_st[pedalIdx].payloadPedalConfig_.dampingPull = 0;
                dap_config_st[pedalIdx].payloadPedalConfig_.absFrequency = 5;
                dap_config_st[pedalIdx].payloadPedalConfig_.absAmplitude = 20;
                dap_config_st[pedalIdx].payloadPedalConfig_.absPattern = 0;
                dap_config_st[pedalIdx].payloadPedalConfig_.absForceOrTarvelBit = 0;
                dap_config_st[pedalIdx].payloadPedalConfig_.lengthPedal_AC = 150;
                dap_config_st[pedalIdx].payloadPedalConfig_.horPos_AB = 215;
                dap_config_st[pedalIdx].payloadPedalConfig_.verPos_AB = 80;
                dap_config_st[pedalIdx].payloadPedalConfig_.lengthPedal_CB = 200;
                dap_config_st[pedalIdx].payloadPedalConfig_.Simulate_ABS_trigger= 0;
                dap_config_st[pedalIdx].payloadPedalConfig_.Simulate_ABS_value= 80;
                dap_config_st[pedalIdx].payloadPedalConfig_.RPM_max_freq = 40;
                dap_config_st[pedalIdx].payloadPedalConfig_.RPM_min_freq = 10;
                dap_config_st[pedalIdx].payloadPedalConfig_.RPM_AMP = 30;
                dap_config_st[pedalIdx].payloadPedalConfig_.BP_trigger_value = 50;
                dap_config_st[pedalIdx].payloadPedalConfig_.BP_amp = 1;
                dap_config_st[pedalIdx].payloadPedalConfig_.BP_freq = 15;
                dap_config_st[pedalIdx].payloadPedalConfig_.BP_trigger = 0;
                dap_config_st[pedalIdx].payloadPedalConfig_.maxGameOutput = 100;
                dap_config_st[pedalIdx].payloadPedalConfig_.kf_modelNoise = 128;
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_0 = 0;
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_1 = 0;
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_2 = 0;
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_3 = 0;
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_4 = 0;

                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_0 = 0;
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_1 = 0;
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_2 = 0;
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_3 = 0;
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_4 = 0;

                dap_config_st[pedalIdx].payloadPedalConfig_.PID_p_gain = 0.3f;
                dap_config_st[pedalIdx].payloadPedalConfig_.PID_i_gain = 50.0f;
                dap_config_st[pedalIdx].payloadPedalConfig_.PID_d_gain = 0.0f;
                dap_config_st[pedalIdx].payloadPedalConfig_.PID_feedforward_gain = 0.0f;

                dap_config_st[pedalIdx].payloadPedalConfig_.control_strategy_b = 0;

                dap_config_st[pedalIdx].payloadPedalConfig_.loadcell_rating = 150;

                dap_config_st[pedalIdx].payloadPedalConfig_.travelAsJoystickOutput_u8 = 0;

                dap_config_st[pedalIdx].payloadPedalConfig_.invertLoadcellReading_u8 = 0;


                InitializeComponent();

               
            }
            // debug mode invisiable
            text_debug_flag.Opacity = 0;
            text_debug_PID_para.Opacity = 0;
            text_debug_dgain.Opacity = 0;
            text_debug_igain.Opacity = 0;
            text_debug_pgain.Opacity = 0;
            text_debug_feedforward.Opacity = 0;
            text_serial.Opacity = 0;
            TextBox_serialMonitor.Visibility = System.Windows.Visibility.Hidden;
            InvertLoadcellReading_check.Opacity = 0;
            PID_tuning_D_gain_slider.Opacity = 0;
            PID_tuning_I_gain_slider.Opacity = 0;
            PID_tuning_P_gain_slider.Opacity = 0;
            PID_tuning_Feedforward_gain_slider.Opacity= 0;
            textBox_debug_Flag_0.Opacity = 0;
            //btn_serial.Visibility = System.Windows.Visibility.Hidden;
            button_pedal_position_reset.Visibility = System.Windows.Visibility.Hidden;
            button_pedal_restart.Visibility = System.Windows.Visibility.Hidden;
            btn_system_id.Visibility = System.Windows.Visibility.Hidden;
            btn_pedal_disconnect.Visibility = System.Windows.Visibility.Hidden;
            //setting drawing color with Simhub theme workaround
            text_min_force.Foreground = btn_update.Background;
            text_max_force.Foreground = btn_update.Background;
            text_max_pos.Foreground = btn_update.Background;
            text_min_pos.Foreground = btn_update.Background;
            text_position.Foreground = btn_update.Background;
            rect0.Fill = btn_update.Background;
            rect1.Fill = btn_update.Background;
            rect2.Fill = btn_update.Background;
            rect3.Fill = btn_update.Background;
            rect4.Fill = btn_update.Background;
            rect5.Fill = btn_update.Background;
            rect6.Fill = btn_update.Background;
            rect7.Fill = btn_update.Background;
            rect8.Fill = btn_update.Background;
            rect9.Fill = btn_update.Background;
            Line_V_force.Stroke = btn_update.Background;
            Line_H_pos.Stroke = btn_update.Background;
            Polyline_BrakeForceCurve.Stroke = btn_update.Background;

            text_damping_text.Foreground = btn_update.Background;
            Line_H_damping.Stroke = btn_update.Background;
            text_damping.Foreground = btn_update.Background;
            rect_damping.Fill = btn_update.Background;
            Line_H_ABS.Stroke = btn_update.Background;
            text_ABS.Foreground = btn_update.Background;
            rect_ABS.Fill = btn_update.Background;
            text_ABS_text.Foreground = btn_update.Background;
            Line_H_ABS_freq.Stroke = btn_update.Background;
            text_ABS_freq.Foreground = btn_update.Background;
            rect_ABS_freq.Fill = btn_update.Background;
            text_ABS_freq_text.Foreground = btn_update.Background;
            Line_H_max_game.Stroke = btn_update.Background;
            text_max_game.Foreground = btn_update.Background;
            text_max_game_text.Foreground = btn_update.Background;
            rect_max_game.Fill = btn_update.Background;

            Line_H_KF.Stroke = btn_update.Background;
            text_KF.Foreground = btn_update.Background;
            rect_KF.Fill = btn_update.Background;
            text_KF_text.Foreground = btn_update.Background;
            Line_H_LC_rating.Stroke = btn_update.Background;
            text_LC_rating.Foreground = btn_update.Background;
            text_LC_rating_text.Foreground = btn_update.Background;
            rect_LC_rating.Fill = btn_update.Background;

            text_RPM_freq_min.Foreground = btn_update.Background;
            text_RPM_freq_max.Foreground = btn_update.Background;
            text_RPM_AMP.Foreground = btn_update.Background;
            Line_H_RPM_AMP.Stroke = btn_update.Background;
            rect_RPM_AMP.Fill = btn_update.Background;
            text_RPM_AMP_text.Foreground = btn_update.Background;

            Line_H_RPM_freq.Stroke = btn_update.Background;
            rect_RPM_max.Fill = btn_update.Background;
            rect_RPM_min.Fill = btn_update.Background;
            text_RPM_freq_text.Foreground = btn_update.Background;

            text_bite_amp.Foreground = btn_update.Background;
            text_bite_freq.Foreground = btn_update.Background;
            rect_bite_amp.Fill = btn_update.Background;
            rect_bite_freq.Fill = btn_update.Background;
            text_bite_amp_text.Foreground = btn_update.Background;
            text_bite_freq_text.Foreground = btn_update.Background;
            Line_H_bite_amp.Stroke = btn_update.Background;
            Line_H_bite_freq.Stroke = btn_update.Background;
            // Call this method to generate gridlines on the Canvas
            DrawGridLines();








            
        }



        public byte[] getBytesPayload(payloadPedalConfig aux)
        {
            int length = Marshal.SizeOf(aux);
            IntPtr ptr = Marshal.AllocHGlobal(length);
            byte[] myBuffer = new byte[length];

            Marshal.StructureToPtr(aux, ptr, true);
            Marshal.Copy(ptr, myBuffer, 0, length);
            Marshal.FreeHGlobal(ptr);

            return myBuffer;
        }


        public byte[] getBytes(DAP_config_st aux)
        {
            int length = Marshal.SizeOf(aux);
            IntPtr ptr = Marshal.AllocHGlobal(length);
            byte[] myBuffer = new byte[length];

            Marshal.StructureToPtr(aux, ptr, true);
            Marshal.Copy(ptr, myBuffer, 0, length);
            Marshal.FreeHGlobal(ptr);

            return myBuffer;
        }


        //public byte[] getBytes_Action(DAP_action_st aux)
        //{
        //    int length = Marshal.SizeOf(aux);
        //    IntPtr ptr = Marshal.AllocHGlobal(length);
        //    byte[] myBuffer = new byte[length];

        //    Marshal.StructureToPtr(aux, ptr, true);
        //    Marshal.Copy(ptr, myBuffer, 0, length);
        //    Marshal.FreeHGlobal(ptr);

        //    return myBuffer;
        //}


        public DAP_config_st getConfigFromBytes(byte[] myBuffer)
        {
            DAP_config_st aux;

            // see https://stackoverflow.com/questions/31045358/how-do-i-copy-bytes-into-a-struct-variable-in-c
            int size = Marshal.SizeOf(typeof(DAP_config_st));
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(myBuffer, 0, ptr, size);

            aux = (DAP_config_st)Marshal.PtrToStructure(ptr, typeof(DAP_config_st));
            Marshal.FreeHGlobal(ptr);

            return aux;
        }


        public DAP_state_st getStateFromBytes(byte[] myBuffer)
        {
            DAP_state_st aux;

            // see https://stackoverflow.com/questions/31045358/how-do-i-copy-bytes-into-a-struct-variable-in-c
            int size = Marshal.SizeOf(typeof(DAP_state_st));
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(myBuffer, 0, ptr, size);

            aux = (DAP_state_st)Marshal.PtrToStructure(ptr, typeof(DAP_state_st));
            Marshal.FreeHGlobal(ptr);

            return aux;
        }


        //unsafe private UInt16 checksumCalc(byte* data, int length)
        //{

        //    UInt16 curr_crc = 0x0000;
        //    byte sum1 = (byte)curr_crc;
        //    byte sum2 = (byte)(curr_crc >> 8);
        //    int index;
        //    for (index = 0; index < length; index = index + 1)
        //    {
        //        int v = (sum1 + (*data));
        //        sum1 = (byte)v;
        //        sum1 = (byte)(v % 255);

        //        int w = (sum1 + sum2) % 255;
        //        sum2 = (byte)w;

        //        data++;// = data++;
        //    }

        //    int x = (sum2 << 8) | sum1;
        //    return (UInt16)x;
        //}


        public SettingsControlDemo(DataPluginDemo plugin) : this()
        {
            this.Plugin = plugin;
            plugin.testValue = 1;
            plugin.wpfHandle = this;


            UpdateSerialPortList_click();
            //closeSerialAndStopReadCallback(1);



            // check if Json config files are present, otherwise create new ones
            //for (int jsonIndex = 0; jsonIndex < ComboBox_JsonFileSelected.Items.Count; jsonIndex++)
            //{

            //    ComboBox_JsonFileSelected.SelectedIndex = jsonIndex;

            //    // which config file is seleced
            //    string currentDirectory = Directory.GetCurrentDirectory();
            //    string dirName = currentDirectory + "\\PluginsData\\Common";
            //    //string jsonFileName = ComboBox_JsonFileSelected(ComboBox_JsonFileSelected.Items[jsonIndex]).Text;
            //    string jsonFileName = ((ComboBoxItem)ComboBox_JsonFileSelected.SelectedItem).Content.ToString();
            //    string fileName = dirName + "\\" + jsonFileName + ".json";


            //    // Check if file already exists, otherwise create    
            //    if (!File.Exists(fileName))
            //    {
            //        // create default config
            //        // https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
            //        // https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
            //        var stream1 = new MemoryStream();
            //        var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
            //        ser.WriteObject(stream1, Plugin.dap_config_initial_st);

            //        stream1.Position = 0;
            //        StreamReader sr = new StreamReader(stream1);
            //        string jsonString = sr.ReadToEnd();

            //        System.IO.File.WriteAllText(fileName, jsonString);
            //    }
            //}

            string currentDirectory = Directory.GetCurrentDirectory();
            string dirName = currentDirectory + "\\PluginsData\\Common";
            //string jsonFileName = ComboBox_JsonFileSelected(ComboBox_JsonFileSelected.Items[jsonIndex]).Text;
            string jsonFileNameA = "DiyPedalConfig_Accelerator_Default";
            string jsonFileNameB = "DiyPedalConfig_Brake_Default";
            string jsonFileNameC = "DiyPedalConfig_Clutch_Default";
            string fileNameA = dirName + "\\" + jsonFileNameA + ".json";
            string fileNameB = dirName + "\\" + jsonFileNameB + ".json";
            string fileNameC = dirName + "\\" + jsonFileNameC + ".json";

            if (!File.Exists(fileNameA))
            {
                // create default config
                // https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
                // https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
                var stream1 = new MemoryStream();
                var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
                ser.WriteObject(stream1, Plugin.dap_config_initial_st);

                stream1.Position = 0;
                StreamReader sr = new StreamReader(stream1);
                string jsonString = sr.ReadToEnd();

                System.IO.File.WriteAllText(fileNameA, jsonString);
            }

            if (!File.Exists(fileNameB))
            {
                // create default config
                // https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
                // https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
                var stream1 = new MemoryStream();
                var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
                ser.WriteObject(stream1, Plugin.dap_config_initial_st);

                stream1.Position = 0;
                StreamReader sr = new StreamReader(stream1);
                string jsonString = sr.ReadToEnd();

                System.IO.File.WriteAllText(fileNameB, jsonString);
            }
            if (!File.Exists(fileNameC))
            {
                // create default config
                // https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
                // https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
                var stream1 = new MemoryStream();
                var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
                ser.WriteObject(stream1, Plugin.dap_config_initial_st);

                stream1.Position = 0;
                StreamReader sr = new StreamReader(stream1);
                string jsonString = sr.ReadToEnd();

                System.IO.File.WriteAllText(fileNameC, jsonString);
            }
            InitReadStructFromJson();
            for (uint pedalIndex = 0; pedalIndex < 3; pedalIndex++)
            {
                //indexOfSelectedPedal_u = pedalIndex;
                //ComboBox_JsonFileSelected.SelectedIndex = Plugin.Settings.selectedJsonFileNames[indexOfSelectedPedal_u];
                //ComboBox_JsonFileSelected.SelectedIndex = Plugin.Settings.selectedJsonIndexLast[indexOfSelectedPedal_u];
                InitReadStructFromJson();
                /*
                if (plugin.Settings.connect_status[pedalIndex] == 1)
                {
                    if (plugin.Settings.reading_config == 1)
                    {
                        if (plugin._serialPort[pedalIndex].IsOpen)
                        {
                            Reading_config_auto(pedalIndex);
                        }
                        else
                        {
                            plugin.Settings.connect_status[pedalIndex] = 0;
                        }
                        
                    }

                }
                */
                
                
                /*
                if (plugin.PortExists(plugin._serialPort[pedalIndex].PortName))
                {
                    if (plugin.Settings.connect_status[pedalIndex] == 1)
                    {
                        if (plugin.Settings.reading_config == 1)
                        {
                            Reading_config_auto(pedalIndex);
                        }

                    }
                    
                }
                else
                {
                    plugin.Settings.connect_status[pedalIndex] = 0;
                }
                */


                updateTheGuiFromConfig();
            }

            if (plugin.Settings.reading_config == 1)
            {
                checkbox_pedal_read.IsChecked = true;
                
            }
            else
            {
                checkbox_pedal_read.IsChecked = false;
            }
            indexOfSelectedPedal_u = plugin.Settings.table_selected;
            MyTab.SelectedIndex = (int)indexOfSelectedPedal_u;

            //reconnect to com port
            if (plugin.Settings.auto_connect_flag == 1)
            {
                checkbox_auto_connect.IsChecked = true;
            }
            else
            {
                checkbox_auto_connect.IsChecked = false;
            }


            // autoconnect serial
            for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
            {
                if (Plugin.connectSerialPort[pedalIdx] == true)
                {
                    if (Plugin.PortExists(Plugin._serialPort[pedalIdx].PortName))
                    {
                        if (Plugin._serialPort[pedalIdx].IsOpen == false)
                        {
                            if (Plugin.Settings.connect_status[pedalIdx] == 1)
                            {
                                openSerialAndAddReadCallback(pedalIdx);
                                Reading_config_auto(pedalIdx);
                            }

                        }
                    }
                    else
                    {
                        Plugin.connectSerialPort[pedalIdx] = false;
                        Plugin.Settings.connect_status[pedalIdx] = 0;
                    }

                }
            }
            //vjoy initialized
            if (Plugin.Settings.vjoy_output_flag == 1)
            {
                Vjoy_out_check.IsChecked = true;
                uint vJoystickId = Plugin.Settings.vjoy_order;
                //joystick = new VirtualJoystick(Plugin.Settings.vjoy_order);
                joystick = new vJoyInterfaceWrap.vJoy();

                joystick.AcquireVJD(vJoystickId);
                //joystick.Aquire();
                vjoy_axis_initialize();
            }
            else
            {
                Vjoy_out_check.IsChecked = false;
            }


        }




        public void updateTheGuiFromConfig()
        {
            // update the sliders

            PID_tuning_P_gain_slider.Value = (double)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_p_gain;
            PID_tuning_I_gain_slider.Value = (double)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_i_gain;
            PID_tuning_D_gain_slider.Value = (double)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_d_gain;
            PID_tuning_Feedforward_gain_slider.Value = (double)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_feedforward_gain;

            info_label.Content = "Connection State:\nDAP Version:";
            string info_text;
            if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
            {
                info_text = "Connected";
            }
            else
            {
                info_text = "Waiting";
            }
            info_text += "\n" + Constants.pedalConfigPayload_version;
            if ((bool)TestAbs_check.IsChecked)
            {
                info_text += "\nABS/TC Testing";
            }
            info_label_2.Content = info_text;


            int debugFlagValue_0 = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.debug_flags_0;
            textBox_debug_Flag_0.Text = debugFlagValue_0.ToString();



            Update_BrakeForceCurve();
            //Simulated ABS trigger
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_trigger == 1)
            {
                Simulate_ABS_check.IsChecked = true;
            }
            else
            {
                Simulate_ABS_check.IsChecked = false;
            }
            // dynamic PID checkbox
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.control_strategy_b == 1)
            {
                dynamic_PID_checkbox.IsChecked = true;
            }
            else
            {
                dynamic_PID_checkbox.IsChecked = false;
            }
            //set control point position
            text_point_pos.Opacity = 0;
            double control_rect_value_max = 100;
            double dyy = canvas.Height / control_rect_value_max;
            Canvas.SetTop(rect0, canvas.Height-dyy*dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p000 - rect0.Height / 2);
            Canvas.SetLeft(rect0, 0*canvas.Width/5-rect0.Width/2);
            Canvas.SetTop(rect1, canvas.Height - dyy * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p020 - rect0.Height / 2);
            Canvas.SetLeft(rect1, 1 * canvas.Width / 5 - rect1.Width / 2);
            Canvas.SetTop(rect2, canvas.Height - dyy * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p040 - rect0.Height / 2);
            Canvas.SetLeft(rect2, 2 * canvas.Width / 5 - rect2.Width / 2);
            Canvas.SetTop(rect3, canvas.Height - dyy * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p060 - rect0.Height / 2);
            Canvas.SetLeft(rect3, 3 * canvas.Width / 5 - rect3.Width / 2);
            Canvas.SetTop(rect4, canvas.Height - dyy * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p080 - rect0.Height / 2);
            Canvas.SetLeft(rect4, 4 * canvas.Width / 5 - rect4.Width / 2);
            Canvas.SetTop(rect5, canvas.Height - dyy * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p100 - rect0.Height / 2);
            Canvas.SetLeft(rect5, 5 * canvas.Width / 5 - rect5.Width / 2);
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.debug_flags_0 != 32)
            {
                rect_State.Visibility = Visibility.Visible;
                text_state.Visibility = Visibility.Visible;
            }
            else
            {
                rect_State.Visibility = Visibility.Hidden;
                text_state.Visibility = Visibility.Hidden;
            }
            Canvas.SetTop(rect_State, canvas.Height - rect_State.Height / 2);
            Canvas.SetLeft(rect_State, -rect_State.Width / 2);
            Canvas.SetLeft(text_state, Canvas.GetLeft(rect_State) + rect_State.Width);
            Canvas.SetTop(text_state, Canvas.GetTop(rect_State) - rect_State.Height);
            text_state.Text = "0%";
            //set for ABS slider
            Canvas.SetTop(rect_SABS_Control, (control_rect_value_max- dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_value ) *dyy-rect_SABS_Control.Height/2);
            Canvas.SetLeft(rect_SABS_Control , 0);
            Canvas.SetTop(rect_SABS, 0);
            Canvas.SetLeft(rect_SABS, 0);
            rect_SABS.Height = canvas.Height - dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_value * dyy;
            Canvas.SetTop(text_SABS, Canvas.GetTop(rect_SABS_Control) -text_SABS.Height -rect_SABS_Control.Height);
            Canvas.SetLeft(text_SABS, canvas.Width-text_SABS.Width);
            text_SABS.Text = "ABS trigger value: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_value + "%";
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_trigger == 1)
            {
                rect_SABS.Opacity = 1;
                rect_SABS_Control.Opacity = 1;
                text_SABS.Opacity = 1;
            }
            else {
                rect_SABS.Opacity = 0;
                rect_SABS_Control.Opacity = 0;
                text_SABS.Opacity = 0;
            }
            //set for travel slider;
            double dx = (canvas_horz_slider.Width-10) / 100;
            Canvas.SetTop(rect6, 15);
            //TextBox_debugOutput.Text= Convert.ToString(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition);
            Canvas.SetLeft(rect6, rect6.Width / 2+dx* dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition);
            Canvas.SetLeft(text_min_pos,  Canvas.GetLeft(rect6) - text_min_pos.Width / 2+rect6.Width/2);
            Canvas.SetTop(text_min_pos, 5);
            text_min_pos.Text = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition + "%";
            text_max_pos.Text = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition + "%";
            Canvas.SetTop(rect7, 15);
            Canvas.SetLeft(rect7, rect7.Width / 2 + dx * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition);
            Canvas.SetLeft(text_max_pos,  Canvas.GetLeft(rect7) - text_max_pos.Width / 2+rect7.Width/2);
            Canvas.SetTop(text_max_pos, 5);

            //set for RPM freq slider;
            dx = (canvas_horz_RPM_freq.Width - 10) / 50;
            Canvas.SetTop(rect_RPM_min, 15);
            
            Canvas.SetLeft(rect_RPM_min, rect_RPM_min.Width / 2 + dx * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_min_freq);
            Canvas.SetLeft(text_RPM_freq_min, Canvas.GetLeft(rect_RPM_min) - text_RPM_freq_min.Width / 2 + rect_RPM_min.Width / 2);
            Canvas.SetTop(text_RPM_freq_min, 5);
            text_RPM_freq_min.Text =dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_min_freq + "Hz";
            text_RPM_freq_max.Text =dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_max_freq + "Hz";
            Canvas.SetTop(rect_RPM_max, 15);
            Canvas.SetLeft(rect_RPM_max, rect_RPM_max.Width / 2 + dx * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_max_freq);
            Canvas.SetLeft(text_RPM_freq_max, Canvas.GetLeft(rect_RPM_max) - text_RPM_freq_max.Width / 2 + rect_RPM_max.Width / 2);
            Canvas.SetTop(text_RPM_freq_max, 5);
            //set for force vertical slider
            double dy = (canvas_vert_slider.Height/250);
            Canvas.SetTop(rect8,canvas_vert_slider.Height-dy* dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.preloadForce);
            Canvas.SetLeft(rect8, canvas_vert_slider.Width / 2 - rect8.Width / 2- Line_V_force.StrokeThickness / 2);
            Canvas.SetLeft(text_min_force,  rect8.Width+3);
            Canvas.SetTop(text_min_force, Canvas.GetTop(rect8));
            Canvas.SetTop(rect9, canvas_vert_slider.Height - dy * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxForce);
            Canvas.SetLeft(rect9, canvas_vert_slider.Width / 2 - rect9.Width / 2-Line_V_force.StrokeThickness / 2);
            Canvas.SetLeft(text_max_force,  rect9.Width+3);
            Canvas.SetTop(text_max_force, Canvas.GetTop(rect9)-6-text_max_force.Height/2);
            
            text_min_force.Text = "Preload:\n" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.preloadForce + "kg";
            text_max_force.Text = "Max Force:\n" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxForce + "kg";
            //damping slider
            double damping_max = 255;
            dx = canvas_horz_damping.Width / damping_max;
            Canvas.SetLeft(rect_damping, dx * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.dampingPress);
            text_damping.Text =  ""+dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.dampingPress;
            Canvas.SetLeft(text_damping, Canvas.GetLeft(rect_damping) + rect_damping.Width / 2 - text_damping.Width / 2);
            Canvas.SetTop(text_damping, 5);
            //ABS amplitude slider
            double abs_max = 255;
            dx = canvas_horz_ABS.Width / abs_max;
            Canvas.SetLeft(rect_ABS, dx * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude);
            switch (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absForceOrTarvelBit)
            {
                case 0:
                    text_ABS.Text = (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude / 20 + "kg";
                    break;
                case 1:
                    text_ABS.Text = (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude / 20 + "%";
                    break;
                default:
                    break;
            }
            
            Canvas.SetLeft(text_ABS, Canvas.GetLeft(rect_ABS) - text_ABS.Width / 2 + rect_ABS.Width / 2);
            Canvas.SetTop(text_ABS, 5);
            //ABS freq slider
            double abs_freq_max = 30;
            dx = canvas_horz_ABS_freq.Width / abs_freq_max;
            Canvas.SetLeft(rect_ABS_freq, dx * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absFrequency);
            text_ABS_freq.Text =dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absFrequency+"Hz";
            Canvas.SetLeft(text_ABS_freq, Canvas.GetLeft(rect_ABS_freq) + rect_ABS_freq.Width / 2-text_ABS_freq.Width/2);
            Canvas.SetTop(text_ABS_freq, 5);

            // ABS pattern
            try
            {
                AbsPattern.SelectedIndex = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absPattern;
            }
            catch (Exception caughtEx)
            {
            }

            // ABS force or travel dependent
            try
            {
                EffectAppliedOnForceOrTravel_combobox.SelectedIndex = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absForceOrTarvelBit;
            }
            catch (Exception caughtEx)
            {
            }


            //max game output slider
            double max_game_max = 100;
            dx = canvas_horz_max_game.Width / max_game_max;
            Canvas.SetLeft(rect_max_game, dx * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxGameOutput);
            text_max_game.Text = "" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxGameOutput+"%";
            Canvas.SetLeft(text_max_game, Canvas.GetLeft(rect_max_game) - text_max_game.Width / 2+rect_max_game.Width/2);
            Canvas.SetTop(text_max_game, 5);
            //KF SLider
            double KF_max = 255;
            dx = canvas_horz_KF.Width / KF_max;
            Canvas.SetLeft(rect_KF, dx * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.kf_modelNoise);
            text_KF.Text = "" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.kf_modelNoise;
            Canvas.SetLeft(text_KF, Canvas.GetLeft(rect_KF) + rect_KF.Width / 2 - text_KF.Width / 2);
            Canvas.SetTop(text_KF, 5);
            //LC rating slider

            double LC_max = 510;
            dx = canvas_horz_LC_rating.Width / LC_max;
            Canvas.SetLeft(rect_LC_rating, dx * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.loadcell_rating*2);
            text_LC_rating.Text = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.loadcell_rating * 2 + "kg";
            Canvas.SetLeft(text_LC_rating, Canvas.GetLeft(rect_LC_rating) + rect_LC_rating.Width / 2 - text_LC_rating.Width / 2);
            Canvas.SetTop(text_LC_rating, 5);
            //RPM AMP slider

            double RPM_AMP_max = 200;
            dx = canvas_horz_RPM_AMP.Width / RPM_AMP_max;
            Canvas.SetLeft(rect_RPM_AMP, dx * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_AMP);
            text_RPM_AMP.Text = ((float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_AMP) / 100 + "kg";
            Canvas.SetLeft(text_RPM_AMP, Canvas.GetLeft(rect_RPM_AMP) - text_RPM_AMP.Width / 2 + rect_RPM_AMP.Width / 2);
            Canvas.SetTop(text_RPM_AMP, 5);

            //Bite point control
            double BP_max = 100;
            dx = (double)canvas.Width / BP_max;
            text_BP.Text = "Bite Point:\n" + ((float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger_value) + "%";
            Canvas.SetLeft(rect_BP_Control, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger_value * dx - rect_BP_Control.Width / 2);
            Canvas.SetLeft(text_BP, Canvas.GetLeft(rect_BP_Control) + rect_BP_Control.Width + 3);
            Canvas.SetTop(text_BP, canvas.Height - text_BP.Height);
            //Bite point freq slider
            double BP_freq_max = 30;
            dx = canvas_horz_bite_freq.Width / BP_freq_max;
            Canvas.SetLeft(rect_bite_freq, dx * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_freq);
            text_bite_freq.Text = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_freq + "Hz";
            Canvas.SetLeft(text_bite_freq, Canvas.GetLeft(rect_bite_freq) + rect_bite_freq.Width / 2 - text_bite_freq.Width / 2);
            Canvas.SetTop(text_bite_freq, 5);

            //Bite point AMP slider
            double BP_amp_max = 200;
            dx = canvas_horz_bite_amp.Width / BP_amp_max;
            Canvas.SetLeft(rect_bite_amp, dx * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_amp);
            text_bite_amp.Text = ((float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_amp) / 100.0f + "kg";
            Canvas.SetLeft(text_bite_amp, Canvas.GetLeft(rect_bite_amp) + rect_bite_amp.Width / 2 - text_bite_amp.Width / 2);
            Canvas.SetTop(text_bite_amp, 5);

            //// Select serial port accordingly
            string tmp = (string)Plugin._serialPort[indexOfSelectedPedal_u].PortName;
            try
            {
                SerialPortSelection.SelectedValue = tmp;
                TextBox_debugOutput.Text = "Serial port selected: " + SerialPortSelection.SelectedValue;

            }
            catch (Exception caughtEx)
            {
            }


            if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen == true)
            {
                ConnectToPedal.IsChecked = true;
                btn_pedal_connect.Content = "Disconnect From Pedal";
            }
            else
            {
                ConnectToPedal.IsChecked = false;
                btn_pedal_connect.Content = "Connect To Pedal";
            }

            if (Plugin.Settings.RPM_enable_flag[indexOfSelectedPedal_u] == 1)
            {
                checkbox_enable_RPM.IsChecked = true;
                checkbox_enable_RPM.Content = "Engine RPM Effect Enabled";
            }
            else
            {
                checkbox_enable_RPM.IsChecked = false;
                checkbox_enable_RPM.Content = "Engine RPM Effect Disabled";
            }

            if (Plugin.Settings.ABS_enable_flag[indexOfSelectedPedal_u] == 1)
            {
                checkbox_enable_ABS.IsChecked = true;
                checkbox_enable_ABS.Content = "ABS/TC Effect Enabled";
            }
            else
            {
                checkbox_enable_ABS.IsChecked = false;
                checkbox_enable_ABS.Content = "ABS/TC Effect Disabled";
            }
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger == 1)
            {
                checkbox_enable_bite_point.IsChecked = true;
                text_BP.Visibility = Visibility.Visible;
                rect_BP_Control.Visibility = Visibility.Visible;
                checkbox_enable_bite_point.Content = "Bite Point Vibration Enabled";

            }
            else
            {
                checkbox_enable_bite_point.IsChecked = false;
                text_BP.Visibility = Visibility.Hidden;
                rect_BP_Control.Visibility = Visibility.Hidden;
                checkbox_enable_bite_point.Content = "Bite Point Vibration Disabled";
            }


            JoystickOutput_check.IsChecked = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.travelAsJoystickOutput_u8 == 1;
            InvertLoadcellReading_check.IsChecked = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.invertLoadcellReading_u8 == 1;
            Label_vjoy_order.Content = Plugin.Settings.vjoy_order;
            //try
            //{
            //    //ComboBox_JsonFileSelected.SelectedItem = Plugin.Settings.selectedJsonFileNames[indexOfSelectedPedal_u];
            //    //ComboBox_JsonFileSelected.SelectedValue = (string)Plugin.Settings.selectedJsonFileNames[indexOfSelectedPedal_u];

            //    ComboBox_JsonFileSelected.SelectedIndex = Plugin.Settings.selectedJsonIndexLast[indexOfSelectedPedal_u];

            //    //ReadStructFromJson();


            //    //SerialPortSelection.SelectedValue
            //    //TextBox_debugOutput.Text = "Error 2: ";
            //    //TextBox_debugOutput.Text += Plugin.Settings.selectedJsonFileNames[indexOfSelectedPedal_u];
            //    //TextBox_debugOutput.Text += "     ";
            //    //TextBox_debugOutput.Text += ComboBox_JsonFileSelected.SelectedValue;
            //}
            //catch (Exception caughtEx)
            //{
            //    string errorMessage = caughtEx.Message;
            //    TextBox_debugOutput.Text = "Error 1: ";
            //    TextBox_debugOutput.Text += errorMessage;
            //}

            //= ComboBox_JsonFileSelected.SelectedItem.ToString();

            //ConnectToPedal.IsChecked = true;

            //TextBox_debugOutput.Text = "Pedal selected: " + indexOfSelectedPedal_u;
            //TextBox_debugOutput.Text += ",    connected: " + ConnectToPedal.IsChecked;
            //TextBox_debugOutput.Text += ",    serial port name: " + tmp;

        }




        private void Update_BrakeForceCurve()
        {

            double[] x = new double[6];
            double[] y = new double[6];
            double x_quantity = 100;
            double y_max = 100;
            double dx = canvas.Width / x_quantity;
            double dy = canvas.Height / y_max;

            x[0] = 0;
            x[1] = 20;
            x[2] = 40;
            x[3] = 60;
            x[4] = 80;
            x[5] = 100;

            y[0] = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p000;
            y[1] = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p020;
            y[2] = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p040;
            y[3] = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p060;
            y[4] = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p080;
            y[5] = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p100;

            // Use cubic interpolation to smooth the original data
            (double[] xs2, double[] ys2, double[] a, double[] b) = Cubic.Interpolate1D(x, y, 100);


            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_0 = (float)a[0];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_1 = (float)a[1];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_2 = (float)a[2];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_3 = (float)a[3];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_4 = (float)a[4];

            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_0 = (float)b[0];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_1 = (float)b[1];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_2 = (float)b[2];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_3 = (float)b[3];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_4 = (float)b[4];


            //TextBox_debugOutput.Text = "";
            //for (uint i = 0; i < a.Length; i++)
            //{
            //    TextBox_debugOutput.Text += "\na[" + i + "]: " + a[i] + "      b[" + i + "]: " + b[i];
            //}


            System.Windows.Media.PointCollection myPointCollection2 = new System.Windows.Media.PointCollection();


            for (int pointIdx = 0; pointIdx < 100; pointIdx++)
            {
                System.Windows.Point Pointlcl = new System.Windows.Point(dx * xs2[pointIdx], dy*ys2[pointIdx]);
                myPointCollection2.Add(Pointlcl);
                Force_curve_Y[pointIdx] =  dy * ys2[pointIdx];
            }

            this.Polyline_BrakeForceCurve.Points = myPointCollection2;
            

        }



        public class SerialPortChoice
        {
            public SerialPortChoice(string display, string value)
            {
                Display = display;
                Value = value;
            }

            public string Value { get; set; }
            public string Display { get; set; }
        }



        // Select which pedal to config
        // see https://stackoverflow.com/questions/772841/is-there-selected-tab-changed-event-in-the-standard-wpf-tab-control
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            indexOfSelectedPedal_u = (uint)MyTab.SelectedIndex;
            Plugin.Settings.table_selected = (uint)MyTab.SelectedIndex;
            // update the sliders & serial port selection accordingly
            updateTheGuiFromConfig();
        }





        /********************************************************************************************************************/
        /*							Slider callbacks																		*/
        /********************************************************************************************************************/

        public void TestAbs_click(object sender, RoutedEventArgs e)
        {
            //if (indexOfSelectedPedal_u == 1)
                if (TestAbs_check.IsChecked==false)
                { 
                    TestAbs_check.IsChecked= true;
                    Plugin.sendAbsSignal = (bool)TestAbs_check.IsChecked;
                    TextBox_debugOutput.Text = "ABS-Test begin";
                    updateTheGuiFromConfig();
                }
                else
                {
                    TestAbs_check.IsChecked = false;
                    //Plugin.sendAbsSignal = !Plugin.sendAbsSignal;
                    Plugin.sendAbsSignal = (bool)TestAbs_check.IsChecked;
                    TextBox_debugOutput.Text = "ABS-Test stopped";
                    updateTheGuiFromConfig();
                }
            
        }









        /********************************************************************************************************************/
        /*							PID tuning                      														*/
        /********************************************************************************************************************/
        public void PID_tuning_P_gain_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_p_gain = (float)e.NewValue;
        }

        public void PID_tuning_I_gain_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_i_gain = (float)e.NewValue;
        }

        public void PID_tuning_D_gain_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_d_gain = (float)e.NewValue;
        }

        public void PID_tuning_Feedforward_gain_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_feedforward_gain = (float)e.NewValue;
        }







        private void NumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //labelEingabe.Content = "Sie haben '" + textBox_debug_Flag_0.Text + "' eingegeben!";
            //TextBox_debugOutput.Text = textBox_debug_Flag_0.Text;

            if (int.TryParse(textBox_debug_Flag_0.Text, out int result))
            {
                if ((result >= 0) && (result <= 255))
                {
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.debug_flags_0 = (byte)result;
                }
            }
        }
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            //if ((e.NewValue >= 0) && (e.NewValue <= 255))
            //{
            //    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.debug_flags_0 = (byte)e.NewValue;
            //}

            // Use a regular expression to allow only numeric input
            Regex regex = new Regex("[^0-9]+");

            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;

            e.Handled = regex.IsMatch(textBox.Text + e.Text);

            ////if (!e.Handled)
            ////{
            ////    if (int.TryParse(textBox.Text + e.Text, out int result))
            ////    {
            ////        if ((result >= 0) && (result <= 255))
            ////        {
            ////            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.debug_flags_0 = (byte)result;
            ////        }
            ////    }
            ////}
        }


        /********************************************************************************************************************/
        /*							Write/read config to/from Json file														*/
        /********************************************************************************************************************/

        //private void ComboBox_SelectionChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        // https://stackoverflow.com/questions/3721430/what-is-the-simplest-way-to-get-the-selected-text-of-a-combo-box-containing-only

        //        string stringValue = ((ComboBoxItem)ComboBox_JsonFileSelected.SelectedItem).Content.ToString();


        //        // string stringValue = ComboBox_JsonFileSelected.SelectedValue.ToString();

        //        //TextBox_debugOutput.Text = stringValue;
        //        Plugin.Settings.selectedJsonFileNames[indexOfSelectedPedal_u] = stringValue;

        //        Plugin.Settings.selectedJsonIndexLast[indexOfSelectedPedal_u] = ComboBox_JsonFileSelected.SelectedIndex;



        //        //ReadStructFromJson();
        //    }
        //    catch (Exception caughtEx)
        //    {

        //        string errorMessage = caughtEx.Message;
        //        TextBox_debugOutput.Text = errorMessage;
        //    }
        //}




        //public void SaveStructToJson_click(object sender, RoutedEventArgs e)
        //{
        //    // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to?pivots=dotnet-8-0

        //    try
        //    {
        //        // which config file is seleced
        //        string currentDirectory = Directory.GetCurrentDirectory();
        //        string dirName = currentDirectory + "\\PluginsData\\Common";
        //        string jsonFileName = ComboBox_JsonFileSelected.Text;
        //        string fileName = dirName + "\\" + jsonFileName + ".json";

        //        this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.version = (byte)pedalConfigPayload_version;

        //        // https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
        //        // https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
        //        var stream1 = new MemoryStream();
        //        var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
        //        ser.WriteObject(stream1, dap_config_st[indexOfSelectedPedal_u]);

        //        stream1.Position = 0;
        //        StreamReader sr = new StreamReader(stream1);
        //        string jsonString = sr.ReadToEnd();

        //        // Check if file already exists. If yes, delete it.     
        //        if (File.Exists(fileName))
        //        {
        //            File.Delete(fileName);
        //        }


        //        System.IO.File.WriteAllText(fileName, jsonString);
        //        TextBox_debugOutput.Text = "Config exported!";

        //    }
        //    catch (Exception caughtEx)
        //    {

        //        string errorMessage = caughtEx.Message;
        //        TextBox_debugOutput.Text = errorMessage;
        //    }

        //}



        //public void ReadStructFromJson_click(object sender, RoutedEventArgs e)
        //{
        //    ReadStructFromJson();
        //}


        /********************************************************************************************************************/
        /*							Refind min endstop																		*/
        /********************************************************************************************************************/
        unsafe public void ResetPedalPosition_click(object sender, RoutedEventArgs e)
        {

            if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
            {
                
                try
                {
                    // compute checksum
                    DAP_action_st tmp;
                    tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                    tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;
                    tmp.payloadPedalAction_.resetPedalPos_u8 = 1;


                    DAP_action_st* v = &tmp;
                    byte* p = (byte*)v;
                    tmp.payloadFooter_.checkSum = Plugin.checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));


                    int length = sizeof(DAP_action_st);
                    byte[] newBuffer = new byte[length];
                    newBuffer = Plugin.getBytes_Action(tmp);


                    // clear inbuffer 
                    Plugin._serialPort[indexOfSelectedPedal_u].DiscardInBuffer();

                    // send query command
                    Plugin._serialPort[indexOfSelectedPedal_u].Write(newBuffer, 0, newBuffer.Length);
                }
                catch (Exception caughtEx)
                {
                    string errorMessage = caughtEx.Message;
                    TextBox_debugOutput.Text = errorMessage;
                }
            }
        }



        /********************************************************************************************************************/
        /*							System identification																	*/
        /********************************************************************************************************************/
        public void StartSystemIdentification_click(object sender, RoutedEventArgs e)
        {

            TextBox_debugOutput.Text = "Start system identification";


            try
            {

                string currentDirectory = Directory.GetCurrentDirectory();
                string dirName = currentDirectory + "\\PluginsData\\Common";
                string logFileName = "DiyActivePedalSystemIdentification";
                string fileName = dirName + "\\" + logFileName + ".txt";

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }


                // This text is added only once to the file.
                if (!File.Exists(fileName))
                {
                    using (StreamWriter sw = File.CreateText(fileName))
                    {

                        // trigger system identification
                        Plugin._serialPort[indexOfSelectedPedal_u].Write("3");

                        //System.Threading.Thread.Sleep(100);


                        // read system return log
                        //while (Plugin._serialPort[indexOfSelectedPedal_u].BytesToRead > 0)
                        //{
                        //    string message = Plugin._serialPort[indexOfSelectedPedal_u].ReadLine();
                        //    sw.Write(message);

                        //    System.Threading.Thread.Sleep(20);

                        //}
                    }

                }

                TextBox_debugOutput.Text = "Finished system identification";


                ////// trigger system identification
                ////Plugin._serialPort[indexOfSelectedPedal_u].Write("3");

                ////System.Threading.Thread.Sleep(100);


                ////// read system return log
                ////while (Plugin._serialPort[indexOfSelectedPedal_u].BytesToRead > 0)
                ////{
                ////    string message = Plugin._serialPort[indexOfSelectedPedal_u].ReadLine();
                ////    using (StreamWriter sw = File.AppendText(fileName))
                ////    {
                ////        sw.WriteLine(message);
                ////    }
                ////    System.Threading.Thread.Sleep(100);
                ////}


            }
            catch (Exception caughtEx)
            {
                string errorMessage = caughtEx.Message;
                TextBox_debugOutput.Text = errorMessage;
            }

        }






        /********************************************************************************************************************/
        /*							Send config to pedal																	*/
        /********************************************************************************************************************/
        unsafe public void SendConfigToPedal_click(object sender, RoutedEventArgs e)
        {
            if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
            {

                // compute checksum
                //getBytes(this.dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_)
                this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.payloadType = (byte)Constants.pedalConfigPayload_type;
                this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.storeToEeprom = 1;
                DAP_config_st tmp = this.dap_config_st[indexOfSelectedPedal_u];

                //payloadPedalConfig tmp = this.dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_;
                DAP_config_st* v = &tmp;
                byte* p = (byte*)v;
                this.dap_config_st[indexOfSelectedPedal_u].payloadFooter_.checkSum = Plugin.checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalConfig));


                //TextBox_debugOutput.Text = "CRC simhub calc: " + this.dap_config_st[indexOfSelectedPedal_u].payloadFooter_.checkSum + "    ";

                TextBox_debugOutput.Text = String.Empty;

                try
                {
                    int length = sizeof(DAP_config_st);
                    //int val = this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.checkSum;
                    //string msg = "CRC value: " + val.ToString();
                    byte[] newBuffer = new byte[length];
                    newBuffer = getBytes(this.dap_config_st[indexOfSelectedPedal_u]);

                    //TextBox_debugOutput.Text = "ConfigLength" + length;

                    // clear inbuffer 
                    Plugin._serialPort[indexOfSelectedPedal_u].DiscardInBuffer();
                    Plugin._serialPort[indexOfSelectedPedal_u].DiscardOutBuffer();


                    // send data
                    Plugin._serialPort[indexOfSelectedPedal_u].Write(newBuffer, 0, newBuffer.Length);
                    //Plugin._serialPort[indexOfSelectedPedal_u].Write("\n");
                }
                catch (Exception caughtEx)
                {
                    string errorMessage = caughtEx.Message;
                    TextBox_debugOutput.Text = errorMessage;
                }

            }
        }


        unsafe public void Reading_config_auto(uint i)
        {
            if (Plugin._serialPort[i].IsOpen)
            {
                // compute checksum
                DAP_action_st tmp;
                tmp.payloadPedalAction_.returnPedalConfig_u8 = 1;
                tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;

                DAP_action_st* v = &tmp;
                byte* p = (byte*)v;
                tmp.payloadFooter_.checkSum = Plugin.checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));


                int length = sizeof(DAP_action_st);
                byte[] newBuffer = new byte[length];
                newBuffer = Plugin.getBytes_Action(tmp);


                // tell the plugin that we expect config data
                waiting_for_pedal_config[i] = true;

                
                // try N times and check whether config has been received
                for (int rep = 0; rep < 1; rep++)
                {
                    // send query command
                    Plugin._serialPort[i].Write(newBuffer, 0, newBuffer.Length);

                    // wait some time and check whether data has been received
                    System.Threading.Thread.Sleep(50);

                    if (waiting_for_pedal_config[i] == false)
                    {
                        break;
                    }
                }                
            }
        }

        public string[] STOPCHAR = { "\r\n"};
        public bool EndsWithStop(string incomingData)
        {
            for (int i = 0; i < STOPCHAR.Length; i++)
            {
                if (incomingData.EndsWith(STOPCHAR[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /********************************************************************************************************************/
        /*							Read config from pedal																	*/
        /********************************************************************************************************************/
        unsafe public void ReadConfigFromPedal_click(object sender, RoutedEventArgs e)
        {
            Reading_config_auto(indexOfSelectedPedal_u);
        }


        public string[] _data = {"", "", "" };// = "";

        //unsafe private void sp_DataReceived(object sender, object e)
        unsafe private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            SerialPort sp = (SerialPort)sender;
            //string _type = (string)e;

            //if (Plugin._serialPort[indexOfSelectedPedal_u].PortName = sp.PortName)

            // identify which pedal has send the data
            int pedalSelected = 255;
            for (int pedalIdx_i = 0; pedalIdx_i < 3; pedalIdx_i++)
            {
                if ( (Plugin._serialPort[pedalIdx_i].PortName == sp.PortName) && (Plugin._serialPort[pedalIdx_i].IsOpen))
                {
                    pedalSelected = pedalIdx_i;
                }
            }

            // once the pedal has identified, go ahead
            if(pedalSelected < 3)
            //if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
            {
                // https://stackoverflow.com/questions/9732709/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it
                

                int length = sizeof(DAP_config_st);
                byte[] newBuffer_config = new byte[length];

                int receivedLength = sp.BytesToRead;

                    
                string incomingData = sp.ReadExisting();
                //if the data doesn't end with a stop char this will signal to keep it in _data 
                //for appending to the following read of data
                bool endsWithStop = EndsWithStop(incomingData);

                //each array object will be sent separately to the callback
                string[] dataArray = incomingData.Split(STOPCHAR, StringSplitOptions.None);

                for (int i = 0; i < dataArray.Length; i++)
                {
                    string newData = dataArray[i];

                //if you are at the last object in the array and this hasn't got a stopchar after
                //it will be saved in _data
                if (!endsWithStop && i == dataArray.Length - 1)
                {
                    _data[pedalSelected] += newData;
                }
                else
                {
                    string dataToSend = _data[pedalSelected] + newData;
                    _data[pedalSelected] = "";


                    // decode into config struct
                    if (dataToSend.Length == length)
                    {
                        DAP_config_st tmp;

                        // transform string into byte
                        fixed (byte* p = Encoding.ASCII.GetBytes(dataToSend))
                        {
                            // create a fixed size buffer
                            length = sizeof(DAP_config_st);
                            byte[] newBuffer_config_2 = new byte[length];

                            // copy the received bytes into byte array
                            for (int j = 0; j < length; j++)
                            {
                                newBuffer_config_2[j] = p[j];
                            }

                            // parse byte array as config struct
                            DAP_config_st pedalConfig_read_st = getConfigFromBytes(newBuffer_config_2);

                            // check whether receive struct is plausible
                            DAP_config_st* v_config = &pedalConfig_read_st;
                            byte* p_config = (byte*)v_config;

                            // payload type check
                            bool check_payload_config_b = false;
                            if (pedalConfig_read_st.payloadHeader_.payloadType == Constants.pedalConfigPayload_type)
                            {
                                check_payload_config_b = true;
                            }

                            // CRC check
                            bool check_crc_config_b = false;
                            if (Plugin.checksumCalc(p_config, sizeof(payloadHeader) + sizeof(payloadPedalConfig)) == pedalConfig_read_st.payloadFooter_.checkSum)
                            {
                                check_crc_config_b = true;
                            }


                            // when all checks are passed, accept the config. Otherwise discard and trow error
                            Dispatcher.Invoke(
                            new Action<DAP_config_st>((t) => this.dap_config_st[pedalSelected] = t),
                            pedalConfig_read_st);


                            this.Dispatcher.Invoke(() =>
                            {
                                // update pedal config
                                if (check_payload_config_b)
                                {
                                    //this.dap_config_st[indexOfSelectedPedal_u] = pedalConfig_read_st;
                                    updateTheGuiFromConfig();
                                }

                                TextBox_debugOutput.Text = "Payload config test 1: " + check_payload_config_b;
                                TextBox_debugOutput.Text += "Payload config test 2: " + check_crc_config_b;
                            });

                        }

                    }
                    else 
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            //TextBox_serialMonitor.Text += "DataArrayLength: " + dataArray.Length + "\n";
                            //TextBox_serialMonitor.Text += "DataLength: " + dataToSend.Length + "\n";
                            TextBox_serialMonitor.Text += dataToSend + "\n";

                            TextBox_serialMonitor.ScrollToEnd();
                            //TextBox_serialMonitor.Text += receivedLength + "\n";
                        });
                    }

                            
                }



                                

                                

                            



                //limits the data stored to 1000 to avoid using up all the memory in case of 
                //failure to register callback or include stopchar

                if (_data[pedalSelected].Length > 1000)
                {
                    _data[pedalSelected] = "";
                }


                //////this.Dispatcher.Invoke(() =>
                //////    {
                //////        TextBox_serialMonitor.Text += incomingData;

                //////        TextBox_serialMonitor.ScrollToEnd();
                //////        //TextBox_serialMonitor.Text += receivedLength + "\n";
                //////    });
                }

                // obtain data and check whether it is from known payload type or just debug info
                
            }
        }



        /********************************************************************************************************************/
        /*							read serial stream																		*/
        /********************************************************************************************************************/
        public void openSerialAndAddReadCallback(uint pedalIdx)
        {
            try
            {
                // serial port settings
                Plugin._serialPort[pedalIdx].Handshake = Handshake.None;
                Plugin._serialPort[pedalIdx].Parity = Parity.None;
                //_serialPort[pedalIdx].StopBits = StopBits.None;


                Plugin._serialPort[pedalIdx].ReadTimeout = 2000;
                Plugin._serialPort[pedalIdx].WriteTimeout = 500;

                // https://stackoverflow.com/questions/7178655/serialport-encoding-how-do-i-get-8-bit-ascii
                Plugin._serialPort[pedalIdx].Encoding = System.Text.Encoding.GetEncoding(28591);

                Plugin._serialPort[pedalIdx].DtrEnable = false;

                Plugin._serialPort[pedalIdx].NewLine = "\r\n";
                Plugin._serialPort[pedalIdx].ReadBufferSize = 10000;

                if (Plugin.PortExists(Plugin._serialPort[pedalIdx].PortName))
                {
                    Plugin._serialPort[pedalIdx].Open();
                    // read callback
                    pedal_serial_read_timer[pedalIdx] = new System.Windows.Forms.Timer();
                    pedal_serial_read_timer[pedalIdx].Tick += new EventHandler(timerCallback_serial);
                    pedal_serial_read_timer[pedalIdx].Tag = pedalIdx;
                    pedal_serial_read_timer[pedalIdx].Interval = 16; // in miliseconds
                    pedal_serial_read_timer[pedalIdx].Start();
                    System.Threading.Thread.Sleep(100);
                }
                else
                {
                    Plugin.Settings.connect_status[pedalIdx] = 0;
                    Plugin.connectSerialPort[pedalIdx] = false;

                }
            }
            catch (Exception ex)
            { }
            



        }

        public void closeSerialAndStopReadCallback(uint pedalIdx)
        {
            if (pedal_serial_read_timer[pedalIdx] != null)
            {
                pedal_serial_read_timer[pedalIdx].Stop();
                pedal_serial_read_timer[pedalIdx].Dispose();
            }
            System.Threading.Thread.Sleep(300);
            if (Plugin._serialPort[pedalIdx].IsOpen)
            {
                Plugin._serialPort[pedalIdx].DiscardInBuffer();
                Plugin._serialPort[pedalIdx].DiscardOutBuffer();
                Plugin._serialPort[pedalIdx].Close();
                Plugin.Settings.connect_status[pedalIdx] = 0;
            }
        }

        Int64 writeCntr = 0;
        unsafe public void timerCallback_serial(object sender, EventArgs e)
        {


            int pedalSelected = Int32.Parse((sender as System.Windows.Forms.Timer).Tag.ToString());
            //int pedalSelected = (int)(sender as System.Windows.Forms.Timer).Tag;

            bool pedalStateHasAlreadyBeenUpdated_b = false;

            // once the pedal has identified, go ahead
            if (pedalSelected < 3)
            //if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
            {

                SerialPort sp = Plugin._serialPort[pedalSelected];



                // https://stackoverflow.com/questions/9732709/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it


                //int length = sizeof(DAP_config_st);
                //byte[] newBuffer_config = new byte[length];

                if (sp.IsOpen)
                {
                    int receivedLength = sp.BytesToRead;

                    if (receivedLength > 0)
                    {

                        string incomingData = sp.ReadExisting();

                        //if the data doesn't end with a stop char this will signal to keep it in _data 
                        //for appending to the following read of data
                        bool endsWithStop = EndsWithStop(incomingData);

                        //each array object will be sent separately to the callback
                        string[] dataArray = incomingData.Split(STOPCHAR, StringSplitOptions.None);

                        for (int i = 0; i < dataArray.Length - 1; i++)
                        {
                            string newData = dataArray[i];

                            //if you are at the last object in the array and this hasn't got a stopchar after
                            //it will be saved in _data
                            if (!endsWithStop && (i == dataArray.Length - 2))
                            {
                                _data[pedalSelected] += newData;
                            }
                            else
                            {
                                string dataToSend = _data[pedalSelected] + newData;
                                _data[pedalSelected] = "";



                                // check for pedal state struct
                                if ((dataToSend.Length == sizeof(DAP_state_st)))
                                {

                                    // transform string into byte
                                    fixed (byte* p = System.Text.Encoding.GetEncoding(28591).GetBytes(dataToSend))
                                    {
                                        // create a fixed size buffer
                                        int length = sizeof(DAP_state_st);
                                        byte[] newBuffer_state_2 = new byte[length];

                                        // copy the received bytes into byte array
                                        for (int j = 0; j < length; j++)
                                        {
                                            newBuffer_state_2[j] = p[j];
                                        }

                                        // parse byte array as config struct
                                        DAP_state_st pedalState_read_st = getStateFromBytes(newBuffer_state_2);

                                        // check whether receive struct is plausible
                                        DAP_state_st* v_state = &pedalState_read_st;
                                        byte* p_state = (byte*)v_state;

                                        // payload type check
                                        bool check_payload_state_b = false;
                                        if (pedalState_read_st.payloadHeader_.payloadType == Constants.pedalStatePayload_type)
                                        {
                                            check_payload_state_b = true;
                                        }

                                        // CRC check
                                        bool check_crc_state_b = false;
                                        if (Plugin.checksumCalc(p_state, sizeof(payloadHeader) + sizeof(payloadPedalState)) == pedalState_read_st.payloadFooter_.checkSum)
                                        {
                                            check_crc_state_b = true;
                                        }

                                        if ((check_payload_state_b) && check_crc_state_b)
                                        {

                                            // write vJoy data
                                            if(Plugin.Settings.vjoy_output_flag==1)
                                            {
                                                switch (pedalSelected)
                                                {

                                                    case 0:
                                                        //joystick.SetJoystickAxis(pedalState_read_st.payloadPedalState_.joystickOutput_u16, Axis.HID_USAGE_RX);  // Center X axis
                                                        joystick.SetAxis(pedalState_read_st.payloadPedalState_.joystickOutput_u16, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RX);	// HID_USAGES Enums
                                                        break;
                                                    case 1:
                                                        //joystick.SetJoystickAxis(pedalState_read_st.payloadPedalState_.joystickOutput_u16, Axis.HID_USAGE_RY);  // Center X axis
                                                        joystick.SetAxis(pedalState_read_st.payloadPedalState_.joystickOutput_u16, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RY);	// HID_USAGES Enums
                                                        break;
                                                    case 2:
                                                        //joystick.SetJoystickAxis(pedalState_read_st.payloadPedalState_.joystickOutput_u16, Axis.HID_USAGE_RZ);  // Center X axis
                                                        joystick.SetAxis(pedalState_read_st.payloadPedalState_.joystickOutput_u16, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RZ);	// HID_USAGES Enums
                                                        break;
                                                    default:
                                                        break;
                                                }

                                            }

                                            

                                            // GUI update
                                            if ( (pedalStateHasAlreadyBeenUpdated_b == false) && (indexOfSelectedPedal_u == pedalSelected) )
                                            {
                                                //TextBox_debugOutput.Text = "Pedal pos: " + pedalState_read_st.payloadPedalState_.pedalPosition_u16;
                                                //TextBox_debugOutput.Text += "Pedal force: " + pedalState_read_st.payloadPedalState_.pedalForce_u16;
                                                //TextBox_debugOutput.Text += ",  Servo pos targe: " + pedalState_read_st.payloadPedalState_.servoPosition_i16;
                                                //TextBox_debugOutput.Text += ",  Servo pos: " + pedalState_read_st.payloadPedalState_.servoPosition_i16;

                                                if (false)
                                                {
                                                    // Specify the path to the file
                                                    string filePath = "C:\\Users\\chris\\Downloads\\output.txt";
                                                    // Use StreamWriter to write to the file
                                                    using (StreamWriter writer = new StreamWriter(filePath, true))
                                                    {
                                                        // Write the content to the file
                                                        writeCntr++;
                                                        writer.Write(writeCntr);
                                                        writer.Write(", ");
                                                        writer.Write(pedalState_read_st.payloadPedalState_.pedalForce_u16);
                                                        writer.Write(", ");
                                                        writer.Write(pedalState_read_st.payloadPedalState_.servoPositionTarget_i16);
                                                        writer.Write(", ");
                                                        writer.Write(pedalState_read_st.payloadPedalState_.servoPosition_i16);
                                                        writer.Write("\n");
                                                    }
                                                }
                                                

                                                pedalStateHasAlreadyBeenUpdated_b = true;

                                                text_point_pos.Opacity = 0;
                                                double control_rect_value_max = 65535;
                                                double dyy = canvas.Height / control_rect_value_max;
                                                double dxx = canvas.Width / control_rect_value_max;

                                                if (debug_flag)
                                                {
                                                    Canvas.SetLeft(rect_State, dxx * pedalState_read_st.payloadPedalState_.pedalPosition_u16 - rect_State.Width / 2);
                                                    Canvas.SetTop(rect_State, canvas.Height - dyy * pedalState_read_st.payloadPedalState_.pedalForce_u16 - rect_State.Height / 2);
                                                    
                                                    Canvas.SetLeft(text_state, Canvas.GetLeft(rect_State) + rect_State.Width);
                                                    Canvas.SetTop(text_state, Canvas.GetTop(rect_State) - rect_State.Height);
                                                    text_state.Text = Math.Round( pedalState_read_st.payloadPedalState_.pedalForce_u16 / control_rect_value_max * 100) + "%";

                                                }
                                                else
                                                {
                                                    Canvas.SetLeft(rect_State, dxx * pedalState_read_st.payloadPedalState_.pedalPosition_u16 - rect_State.Width / 2);
                                                    int round_x = (int)(100 * pedalState_read_st.payloadPedalState_.pedalPosition_u16 / control_rect_value_max)-1;
                                                    int x_showed = round_x + 1;
                                                    round_x = Math.Max(0, Math.Min(round_x, 99));
                                                    Canvas.SetTop(rect_State, canvas.Height - Force_curve_Y[round_x] - rect_State.Height/2);
                                                    Canvas.SetLeft(text_state, Canvas.GetLeft(rect_State) + rect_State.Width);
                                                    Canvas.SetTop(text_state, Canvas.GetTop(rect_State) - rect_State.Height);
                                                    text_state.Text = x_showed + "%";
                                                }

                                            }


                                            continue;
                                        }
                                    }
                                }


                                // decode into config struct
                                if ((waiting_for_pedal_config[pedalSelected]) && (dataToSend.Length == sizeof(DAP_config_st)))
                                {
                                    DAP_config_st tmp;


                                    // transform string into byte
                                    fixed (byte* p = System.Text.Encoding.GetEncoding(28591).GetBytes(dataToSend))
                                    {
                                        // create a fixed size buffer
                                        int length = sizeof(DAP_config_st);
                                        byte[] newBuffer_config_2 = new byte[length];

                                        // copy the received bytes into byte array
                                        for (int j = 0; j < length; j++)
                                        {
                                            newBuffer_config_2[j] = p[j];
                                        }

                                        // parse byte array as config struct
                                        DAP_config_st pedalConfig_read_st = getConfigFromBytes(newBuffer_config_2);

                                        // check whether receive struct is plausible
                                        DAP_config_st* v_config = &pedalConfig_read_st;
                                        byte* p_config = (byte*)v_config;

                                        // payload type check
                                        bool check_payload_config_b = false;
                                        if (pedalConfig_read_st.payloadHeader_.payloadType == Constants.pedalConfigPayload_type)
                                        {
                                            check_payload_config_b = true;
                                        }

                                        // CRC check
                                        bool check_crc_config_b = false;
                                        if (Plugin.checksumCalc(p_config, sizeof(payloadHeader) + sizeof(payloadPedalConfig)) == pedalConfig_read_st.payloadFooter_.checkSum)
                                        {
                                            check_crc_config_b = true;
                                        }

                                        if ((check_payload_config_b) && check_crc_config_b)
                                        {
                                            waiting_for_pedal_config[pedalSelected] = false;
                                            dap_config_st[pedalSelected] = pedalConfig_read_st;
                                            updateTheGuiFromConfig();

                                            continue;
                                        }
                                        else
                                        {
                                            TextBox_debugOutput.Text = "Payload config test 1: " + check_payload_config_b;
                                            TextBox_debugOutput.Text += "Payload config test 2: " + check_crc_config_b;
                                        }
                                    }

                                }
                                //else
                                //{


                                // When too many messages are received, only print every Nth message

                                // When only a few messages are received, make the counter greater than N thus every message is printed
                                if (dataArray.Length < 100)
                                {
                                    printCtr = 600;
                                }

                                if (printCtr++ > 200)
                                {
                                    printCtr = 0;
                                    TextBox_serialMonitor.Text += dataToSend + "\n";
                                    TextBox_serialMonitor.ScrollToEnd();
                                }

                                //}


                            }

                            try
                            {
                                while (TextBox_serialMonitor.LineCount > 30)
                                {
                                    TextBox_serialMonitor.Text = TextBox_serialMonitor.Text.Remove(0, TextBox_serialMonitor.GetLineLength(0));
                                }
                            }
                            catch { }







                            //limits the data stored to 1000 to avoid using up all the memory in case of 
                            //failure to register callback or include stopchar

                            if (_data[pedalSelected].Length > 5000)
                            {
                                _data[pedalSelected] = "";
                            }


                        }

                        // obtain data and check whether it is from known payload type or just debug info

                    }

                }
            }
        }



        /********************************************************************************************************************/
        /*							Connect to pedal																		*/
        /********************************************************************************************************************/


        unsafe public void ConnectToPedal_click(object sender, RoutedEventArgs e)
        {


            if (ConnectToPedal.IsChecked == false)
            {
                if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen == false)
                {
                    try
                    {
                        openSerialAndAddReadCallback(indexOfSelectedPedal_u);
                        TextBox_debugOutput.Text = "Serialport open";
                        ConnectToPedal.IsChecked = true;
                        btn_pedal_connect.Content = "Disconnect From Pedal";

                        // register a callback that is triggered when serial data is received
                        // see https://gist.github.com/mini-emmy/9617732
                        //Plugin._serialPort[indexOfSelectedPedal_u].DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);

                        System.Threading.Thread.Sleep(100);

                        Plugin.Settings.connect_status[indexOfSelectedPedal_u] = 1; 

                    }
                    catch (Exception ex)
                    {
                        TextBox_debugOutput.Text = ex.Message;
                        ConnectToPedal.IsChecked = false;
                    }

                }
                else
                {
                    closeSerialAndStopReadCallback(indexOfSelectedPedal_u);

                    //Plugin._serialPort[indexOfSelectedPedal_u].DataReceived -= sp_DataReceived;

                    ConnectToPedal.IsChecked = false;
                    TextBox_debugOutput.Text = "Serialport already open, close it";
                    Plugin.Settings.connect_status[indexOfSelectedPedal_u] = 0;
                    Plugin.connectSerialPort[indexOfSelectedPedal_u] = false;
                    btn_pedal_connect.Content = "Connect To Pedal";
                }
            }
            else
            {
                ConnectToPedal.IsChecked = false;
                closeSerialAndStopReadCallback(indexOfSelectedPedal_u);
                TextBox_debugOutput.Text = "Serialport close";
                Plugin.connectSerialPort[indexOfSelectedPedal_u] = false;
                Plugin.Settings.connect_status[indexOfSelectedPedal_u] = 0;
                btn_pedal_connect.Content = "Connect To Pedal";

            }

            ////reading config from pedal

            if (checkbox_pedal_read.IsChecked == true)
            {
                Reading_config_auto(indexOfSelectedPedal_u);
            }
            updateTheGuiFromConfig();
        }

        /********************************************************************************************************************/
        /*							Serial port selection																	*/
        /********************************************************************************************************************/
        public void UpdateSerialPortList_click(object sender, RoutedEventArgs e)
        {
            UpdateSerialPortList_click();
        }

        public void SerialPortSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tmp = (string)SerialPortSelection.SelectedValue;
            //Plugin._serialPort[indexOfSelectedPedal_u].PortName = tmp;


            //try 
            //{
            //    TextBox_debugOutput.Text = "Debug: " + Plugin.Settings.selectedComPortNames[indexOfSelectedPedal_u];
            //}
            //catch (Exception caughtEx)
            //{
            //    string errorMessage = caughtEx.Message;
            //    TextBox_debugOutput.Text = errorMessage;
            //}

            try
            {
                //if (Plugin.Settings.connect_status[indexOfSelectedPedal_u] == 0)
                if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen == false)
                {
                    Plugin.Settings.selectedComPortNames[indexOfSelectedPedal_u] = tmp;
                    Plugin._serialPort[indexOfSelectedPedal_u].PortName = tmp;
                }
                TextBox_debugOutput.Text = "COM port selected: " + Plugin.Settings.selectedComPortNames[indexOfSelectedPedal_u];

            }
            catch (Exception caughtEx)
            {
                string errorMessage = caughtEx.Message;
                TextBox_debugOutput.Text = errorMessage;
            }



        }





        public void AbsPatternChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absPattern = (byte)AbsPattern.SelectedIndex;
            }
            catch (Exception caughtEx)
            {
                string errorMessage = caughtEx.Message;
                TextBox_debugOutput.Text = errorMessage;
            }
        }



        





        private void RestartPedal_click(object sender, RoutedEventArgs e)
        {
            Plugin._serialPort[indexOfSelectedPedal_u].DtrEnable = true;
            Plugin._serialPort[indexOfSelectedPedal_u].RtsEnable = true;
            System.Threading.Thread.Sleep(100);
            Plugin._serialPort[indexOfSelectedPedal_u].DtrEnable = false;
            Plugin._serialPort[indexOfSelectedPedal_u].RtsEnable = false;
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Title = "Datei auswählen";
                openFileDialog.Filter = "Configdateien (*.json)|*.json";
                string currentDirectory = Directory.GetCurrentDirectory();
                openFileDialog.InitialDirectory = currentDirectory + "\\PluginsData\\Common";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string content = (string)openFileDialog.FileName;
                    TextBox_debugOutput.Text = content;

                    string filePath = openFileDialog.FileName;


                    if (false)
                    {
                        string text1 = System.IO.File.ReadAllText(filePath);
                        DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(DAP_config_st));
                        var ms = new MemoryStream(Encoding.UTF8.GetBytes(text1));
                        dap_config_st[indexOfSelectedPedal_u] = (DAP_config_st)deserializer.ReadObject(ms);
                    }
                    else
                    {
                        // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/deserialization


                        // c# code to iterate over all fields of struct and set values from json file

                        // Read the entire JSON file
                        string jsonString = File.ReadAllText(filePath);

                        // Parse all of the JSON.
                        //JsonNode forecastNode = JsonNode.Parse(jsonString);
                        dynamic data = JsonConvert.DeserializeObject(jsonString);



                        payloadPedalConfig payloadPedalConfig_fromJson_st = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_;
                        //var s = default(payloadPedalConfig);
                        Object obj = payloadPedalConfig_fromJson_st;// s;



                        FieldInfo[] fi = payloadPedalConfig_fromJson_st.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                        // Iterate over each field and print its name and value
                        foreach (var field in fi)
                        {

                            if (data["payloadPedalConfig_"][field.Name] != null)
                            //if (forecastNode["payloadPedalConfig_"][field.Name] != null)
                            {
                                try
                                {
                                    if (field.FieldType == typeof(float))
                                    {
                                        //float value = forecastNode["payloadPedalConfig_"][field.Name].GetValue<float>();
                                        float value = (float)data["payloadPedalConfig_"][field.Name];
                                        field.SetValue(obj, value);
                                    }

                                    if (field.FieldType == typeof(byte))
                                    {
                                        //byte value = forecastNode["payloadPedalConfig_"][field.Name].GetValue<byte>();
                                        byte value = (byte)data["payloadPedalConfig_"][field.Name];
                                        field.SetValue(obj, value);
                                    }


                                }
                                catch (Exception)
                                {

                                }

                            }
                        }

                        // set values in global structure
                        dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_ = (payloadPedalConfig)obj;// payloadPedalConfig_fromJson_st;
                    }

                    updateTheGuiFromConfig();
                    TextBox_debugOutput.Text = "Config new imported!";
                    TextBox2.Text = "Open " + openFileDialog.FileName;
                }
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog())
            {
                saveFileDialog.Title = "Datei speichern";
                saveFileDialog.Filter = "Textdateien (*.json)|*.json";
                string currentDirectory = Directory.GetCurrentDirectory();
                saveFileDialog.InitialDirectory = currentDirectory + "\\PluginsData\\Common";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                     string fileName = saveFileDialog.FileName;


                    this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;

                    // https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
                    // https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
                    var stream1 = new MemoryStream();
                    //var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
                    //ser.WriteObject(stream1, dap_config_st[indexOfSelectedPedal_u]);


                    // formatted JSON see https://stackoverflow.com/a/38538454
                    var writer = JsonReaderWriterFactory.CreateJsonWriter(stream1, Encoding.UTF8, true, true, "  ");
                    var serializer = new DataContractJsonSerializer(typeof(DAP_config_st));
                    serializer.WriteObject(writer, dap_config_st[indexOfSelectedPedal_u]);
                    writer.Flush();

                    stream1.Position = 0;
                    StreamReader sr = new StreamReader(stream1);
                    string jsonString = sr.ReadToEnd();

                    // Check if file already exists. If yes, delete it.     
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }


                    System.IO.File.WriteAllText(fileName, jsonString);
                    TextBox_debugOutput.Text = "Config new exported!";
                    TextBox2.Text = "Save " + saveFileDialog.FileName;
                    }
            }
        }

        
        private void DisconnectToPedal_click(object sender, RoutedEventArgs e)
        {

            closeSerialAndStopReadCallback(indexOfSelectedPedal_u);


            if (ConnectToPedal.IsChecked == true)
            {
                ConnectToPedal.IsChecked = false;
                TextBox_debugOutput.Text = "Serialport close";
                Plugin.Settings.connect_status[indexOfSelectedPedal_u] = 0;
            }           
            else
            {
                ConnectToPedal.IsChecked = false;
                TextBox_debugOutput.Text = "Not Checked Serialport close";
            }
            updateTheGuiFromConfig();

        }
        


        private void Simulate_ABS_check_Checked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_trigger = 1;
            TextBox_debugOutput.Text = "simulateABS: on";
            rect_SABS.Opacity = 1;
            rect_SABS_Control.Opacity = 1;
            text_SABS.Opacity = 1;

        }
        private void Simulate_ABS_check_Unchecked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_trigger = 0;
            TextBox_debugOutput.Text = "simulateABS: off";
            rect_SABS.Opacity = 0;
            rect_SABS_Control.Opacity = 0;
            text_SABS.Opacity = 0;

        }



        //dragable control rect.

        /*private void InitializeRectanglePositions()
        {
            rectanglePositions.Add("rect1", new Point(75, 75));
            rectanglePositions.Add("rect2", new Point(155, 55));
            rectanglePositions.Add("rect3", new Point(235, 35));
            rectanglePositions.Add("rect4", new Point(315, 15));
        }*/

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            var rectangle = sender as Rectangle;
            offset = e.GetPosition(rectangle);
            rectangle.CaptureMouse();
        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                //double x = e.GetPosition(canvas).X - offset.X;
                double y = e.GetPosition(canvas).Y - offset.Y;

                // Ensure the rectangle stays within the canvas
                //x = Math.Max(0, Math.Min(x, canvas.ActualWidth - rectangle.ActualWidth));
                y = Math.Max(-1*rectangle.Height/2, Math.Min(y, canvas.Height - rectangle.Height/2));

                //Canvas.SetLeft(rectangle, x);
                Canvas.SetTop(rectangle, y);
                double y_max = 100;
                double dx = canvas.Height / y_max;
                double y_actual = (canvas.Height - y -rectangle.Height/2)/dx;
                if (rectangle.Name == "rect0")
                {
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p000 = Convert.ToByte(y_actual);
                    text_point_pos.Text = "Travel:0%";
                    text_point_pos.Text += "\nForce: "+(int)y_actual+"%";
                }
                if (rectangle.Name == "rect1")
                {

                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p020 = Convert.ToByte(y_actual);
                    text_point_pos.Text = "Travel:20%";
                    text_point_pos.Text += "\nForce: " + (int)y_actual + "%";
                }
                if (rectangle.Name == "rect2")
                {
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p040 = Convert.ToByte(y_actual);
                    text_point_pos.Text = "Travel:40%";
                    text_point_pos.Text += "\nForce: " + (int)y_actual + "%";
                }
                if (rectangle.Name == "rect3")
                {
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p060 = Convert.ToByte(y_actual);
                    text_point_pos.Text = "Travel:60%";
                    text_point_pos.Text += "\nForce: " + (int)y_actual + "%";
                }
                if (rectangle.Name == "rect4")
                {
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p080 = Convert.ToByte(y_actual);
                    text_point_pos.Text = "Travel:80%";
                    text_point_pos.Text += "\nForce: " + (int)y_actual + "%";
                }
                if (rectangle.Name == "rect5")
                {
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p100 = Convert.ToByte(y_actual);
                    text_point_pos.Text = "Travel:100%";
                    text_point_pos.Text += "\nForce: " + (int)y_actual + "%";
                }
                text_point_pos.Opacity = 1;

                Update_BrakeForceCurve();



                // Update the position in the dictionary
                //rectanglePositions[rectangle.Name] = new Point(x, y);
            }
        }

        private void Rectangle_MouseMove_H(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                double x = e.GetPosition(canvas_horz_slider).X - offset.X;
                //double y = e.GetPosition(canvas).Y - offset.Y;

                // Ensure the rectangle stays within the canvas

                double min_posiiton = Canvas.GetLeft(rect6) + rectangle.Width / 2;
                double max_position = Canvas.GetLeft(rect7) - rectangle.Width / 2;
                double min_pedal_position = (canvas_horz_slider.Width - 10) * 0.05 + rect6.Width;
                double max_pedal_position = (canvas_horz_slider.Width - 10) * 0.95 + rect7.Width;
                double dx = 100 / (canvas_horz_slider.Width - 10);
                if (rectangle.Name == "rect6")
                {
                    x = Math.Max(min_pedal_position - 1 * rectangle.Width / 2, Math.Min(x, max_position));
                    double actual_x = (x - 5) * dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition = Convert.ToByte(actual_x);

                    if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition > dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition)
                    {
                        dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition;
                        //PedalMinPos_Slider.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition;
                    }
                    TextBox_debugOutput.Text = "Pedal min position:" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition;
                    Canvas.SetLeft(text_min_pos, x - text_min_pos.Width / 2 + rect6.Width / 2);
                    Canvas.SetTop(text_min_pos, 5);
                }
                if (rectangle.Name == "rect7")
                {
                    x = Math.Max(min_posiiton, Math.Min(x, max_pedal_position - rectangle.Width / 2));
                    double actual_x = (x - 5) * dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition = Convert.ToByte(actual_x);

                    if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition < dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition)
                    {
                        dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition;
                        //PedalMaxPos_Slider.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition;
                    }
                    TextBox_debugOutput.Text = "Pedal max position:" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition;
                    Canvas.SetLeft(text_max_pos, x - text_max_pos.Width / 2 + rect7.Width / 2);
                    Canvas.SetTop(text_max_pos, 5);
                }
                text_min_pos.Text = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition + "%";
                text_max_pos.Text = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition + "%";

                //y = Math.Max(-1 * rectangle.ActualHeight / 2, Math.Min(y, canvas.ActualHeight - rectangle.ActualHeight / 2));

                Canvas.SetLeft(rectangle, x);

            }
        }
        private void Rectangle_MouseMove_H_RPM(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                double x = e.GetPosition(canvas_horz_RPM_freq).X - offset.X;
                //double y = e.GetPosition(canvas).Y - offset.Y;

                // Ensure the rectangle stays within the canvas

                double min_posiiton = Canvas.GetLeft(rect_RPM_min) + rectangle.ActualWidth / 2;
                double max_position = Canvas.GetLeft(rect_RPM_max) - rectangle.ActualWidth / 2;
                double dx = 50 / (canvas_horz_RPM_freq.Width - 10);
                if (rectangle.Name == "rect_RPM_min")
                {
                    x = Math.Max(-1 * rectangle.ActualWidth / 2, Math.Min(x, max_position));
                    double actual_x = (x - 5) * dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_min_freq = Convert.ToByte(actual_x);
                    //TextBox_debugOutput.Text = "Pedal min position:" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition;
                    Canvas.SetLeft(text_RPM_freq_min, Canvas.GetLeft(rect_RPM_min) - text_RPM_freq_min.Width / 2+rect_RPM_min.Width/2);
                    Canvas.SetTop(text_RPM_freq_min, 5);
                }
                if (rectangle.Name == "rect_RPM_max")
                {
                    x = Math.Max(min_posiiton, Math.Min(x, canvas_horz_slider.ActualWidth - rectangle.ActualWidth));
                    double actual_x = (x - 5) * dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_max_freq = Convert.ToByte(actual_x);

                    //TextBox_debugOutput.Text = "Pedal max position:" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition;
                    Canvas.SetLeft(text_RPM_freq_max, Canvas.GetLeft(rect_RPM_max) - text_RPM_freq_max.Width / 2+rect_RPM_max.Width/2);
                    Canvas.SetTop(text_RPM_freq_max, 5);
                }
                text_RPM_freq_min.Text = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_min_freq + "Hz";
                text_RPM_freq_max.Text = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_max_freq + "Hz";

                //y = Math.Max(-1 * rectangle.ActualHeight / 2, Math.Min(y, canvas.ActualHeight - rectangle.ActualHeight / 2));

                Canvas.SetLeft(rectangle, x);

            }
        }
        private void Rectangle_MouseMove_V(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                double y = e.GetPosition(canvas_vert_slider).Y - offset.Y;
                //double y = e.GetPosition(canvas).Y - offset.Y;

                // Ensure the rectangle stays within the canvas

                double min_position =  Canvas.GetTop(rect8) - rectangle.Height / 2;
                double max_position = Canvas.GetTop(rect9) + rectangle.Height / 2;
                double dy = 250 / (canvas_vert_slider.Height);
                if (rectangle.Name == "rect8")
                {
                    y = Math.Max(max_position, Math.Min(y, canvas_vert_slider.Height + rectangle.Height / 2));
                    
                    double actual_y = (canvas_vert_slider.Height- y-rectangle.Height/2)  * dy;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.preloadForce = Convert.ToByte(actual_y);

                    if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.preloadForce > dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxForce)
                    {
                        dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.preloadForce = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxForce;
                        //PedalMinForce_Slider.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.preloadForce;
                    }
                    
                    //TextBox_debugOutput.Text = "Pedal min position:" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition;
                    Canvas.SetLeft(text_min_force, rect8.Width+3);
                    Canvas.SetTop(text_min_force, Canvas.GetTop(rect8));
                }
                if (rectangle.Name == "rect9")
                {
                    y = Math.Max(-1 * rectangle.Height / 2, Math.Min(y, min_position ));
                    
                    double actual_y = (canvas_vert_slider.Height - y - rectangle.Height / 2) * dy;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxForce = Convert.ToByte(actual_y);
                    if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxForce < dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.preloadForce)
                    {
                        dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxForce = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.preloadForce;
                        //PedalMaxForce_Slider.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxForce;
                    }
                    
                    //TextBox_debugOutput.Text = "Pedal max position:" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition;
                    Canvas.SetLeft(text_max_force,  rect9.Width+3);
                    Canvas.SetTop(text_max_force, Canvas.GetTop(rect9) - 6-text_max_force.Height/2);
                    
                    
                }
                text_min_force.Text = "Preload:\n" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.preloadForce + "kg";
                text_max_force.Text = "Max Force:\n" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxForce + "kg";
                
                //y = Math.Max(-1 * rectangle.ActualHeight / 2, Math.Min(y, canvas.ActualHeight - rectangle.ActualHeight / 2));

                Canvas.SetTop(rectangle, y);

            }
        }

        private void Rectangle_MouseMove_ABS(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                //double x = e.GetPosition(canvas).X - offset.X;
                double y = e.GetPosition(canvas).Y - offset.Y;

                // Ensure the rectangle stays within the canvas
                double dy = canvas.Height / 100;
                double min_posiiton = 5 * dy;
                double max_position = 50 * dy;
                //min position: 50%, max 95%
                //double dx = 100 / (canvas_horz_slider.Width - 10);
                y = Math.Max(min_posiiton, Math.Min(y, max_position));
                //Canvas.SetTop(rect_SABS, y);
                rect_SABS.Height = y;
                double actual_y = (canvas.Height -y)/dy;
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_value = Convert.ToByte(actual_y);
                TextBox_debugOutput.Text = "ABS trigger value: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_value+"%";
                text_SABS.Text = "ABS trigger value: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_value + "%";
                Canvas.SetTop(text_SABS, y - rect_SABS_Control.Height-text_SABS.Height);
                Canvas.SetTop(rectangle, y-rect_SABS_Control.Height/2);

            }
        }
        private void Rectangle_MouseMove_sigle_slider_H(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;


                //damping
                if (rectangle.Name == "rect_damping")
                {
                    // Ensure the rectangle stays within the canvas
                    double damping_max = 255;
                    double x = e.GetPosition(canvas_horz_damping).X - offset.X;
                    double dx = canvas_horz_damping.Width / damping_max;
                    double min_position = 0 * dx;
                    double max_position = damping_max * dx;
                    //double dx = 100 / (canvas_horz_slider.Width - 10);
                    x = Math.Max(min_position, Math.Min(x, max_position));
                    double actual_x = x / dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.dampingPress = Convert.ToByte(actual_x);
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.dampingPull = Convert.ToByte(actual_x);
                    text_damping.Text = "" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.dampingPress;
                    Canvas.SetLeft(text_damping, Canvas.GetLeft(rect_damping) + rect_damping.Width / 2-text_damping.Width/2);
                    Canvas.SetTop(text_damping, 5);
                    Canvas.SetLeft(rectangle, x);
                }
                // ABS Amplitude
                if (rectangle.Name == "rect_ABS")
                {
                    // Ensure the rectangle stays within the canvas
                    double x = e.GetPosition(canvas_horz_ABS).X - offset.X;
                    double ABS_max = 255;
                    double dx = canvas_horz_ABS.Width / ABS_max;
                    double min_position = 0 * dx;
                    double max_position = ABS_max * dx;
                    //double dx = 100 / (canvas_horz_slider.Width - 10);
                    x = Math.Max(min_position, Math.Min(x, max_position));
                    double actual_x = x / dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude = Convert.ToByte(actual_x);


                    switch (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absForceOrTarvelBit)
                    {
                        case 0:
                            text_ABS.Text = (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude / 20 + "kg";
                            break;
                        case 1:
                            text_ABS.Text = (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude / 20 + "%";
                            break;
                        default:
                            break;
                    }

                    
                    Canvas.SetLeft(text_ABS, Canvas.GetLeft(rect_ABS) - text_ABS.Width / 2 + rect_ABS.Width / 2);
                    Canvas.SetTop(text_ABS, 5);
                    Canvas.SetLeft(rectangle, x);
                }
                //ABS freq
                if (rectangle.Name == "rect_ABS_freq")
                {
                    // Ensure the rectangle stays within the canvas
                    double x = e.GetPosition(canvas_horz_ABS_freq).X - offset.X;
                    double ABS_freq_max = 30;
                    double dx = canvas_horz_ABS_freq.Width / ABS_freq_max;
                    double min_position = 0 * dx;
                    double max_position = ABS_freq_max * dx;
                    //double dx = 100 / (canvas_horz_slider.Width - 10);
                    x = Math.Max(min_position, Math.Min(x, max_position));
                    double actual_x = x / dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absFrequency = Convert.ToByte(actual_x);
                    text_ABS_freq.Text = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absFrequency + "Hz";
                    Canvas.SetLeft(text_ABS_freq, Canvas.GetLeft(rect_ABS_freq) + rect_ABS_freq.Width / 2 - text_ABS_freq.Width / 2);
                    Canvas.SetTop(text_ABS_freq, 5);
                    Canvas.SetLeft(rectangle, x);
                }
                //max game output
                if (rectangle.Name == "rect_max_game")
                {
                    // Ensure the rectangle stays within the canvas
                    double x = e.GetPosition(canvas_horz_max_game).X - offset.X;
                    double max_game_max = 100;
                    double dx = canvas_horz_max_game.Width / max_game_max;
                    double min_position = 0 * dx;
                    double max_position = max_game_max * dx;
                    //double dx = 100 / (canvas_horz_slider.Width - 10);
                    x = Math.Max(min_position, Math.Min(x, max_position));
                    double actual_x = x / dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxGameOutput = Convert.ToByte(actual_x);

                    text_max_game.Text = "" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxGameOutput + "%";
                    Canvas.SetLeft(text_max_game, Canvas.GetLeft(rect_max_game) - text_max_game.Width / 2 + rect_max_game.Width / 2);
                    Canvas.SetTop(text_max_game, 5);
                    Canvas.SetLeft(rectangle, x);
                }
                //KF Slider

                if (rectangle.Name == "rect_KF")
                {
                    // Ensure the rectangle stays within the canvas
                    double x = e.GetPosition(canvas_horz_KF).X - offset.X;
                    double KF_max = 255;
                    double dx = canvas_horz_KF.Width / KF_max;
                    double min_position = 0 * dx;
                    double max_position = KF_max * dx;

                    x = Math.Max(min_position, Math.Min(x, max_position));
                    double actual_x = x / dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.kf_modelNoise = Convert.ToByte(actual_x);

                    text_KF.Text = "" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.kf_modelNoise;
                    Canvas.SetLeft(text_KF, Canvas.GetLeft(rect_KF) + rect_KF.Width / 2 - text_KF.Width / 2);
                    Canvas.SetTop(text_KF, 5);
                    Canvas.SetLeft(rectangle, x);
                }
                //LC rating slider
                if (rectangle.Name == "rect_LC_rating")
                {
                    // Ensure the rectangle stays within the canvas
                    double x = e.GetPosition(canvas_horz_LC_rating).X - offset.X;
                    double LC_max = 510;
                    double dx = canvas_horz_LC_rating.Width / LC_max;
                    double min_position = 0 * dx;
                    double max_position = LC_max * dx;

                    x = Math.Max(min_position, Math.Min(x, max_position));
                    double actual_x = x / dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.loadcell_rating = (byte)(actual_x / 2);

                    text_LC_rating.Text = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.loadcell_rating*2 + "kg";
                    Canvas.SetLeft(text_LC_rating, Canvas.GetLeft(rect_LC_rating) + rect_LC_rating.Width / 2 - text_LC_rating.Width / 2);
                    Canvas.SetTop(text_LC_rating, 5);
                    Canvas.SetLeft(rectangle, x);
                }
                // RPM effect AMP
                if (rectangle.Name == "rect_RPM_AMP")
                {
                    // Ensure the rectangle stays within the canvas
                    double x = e.GetPosition(canvas_horz_RPM_AMP).X - offset.X;
                    double RPM_AMP_max = 200;
                    double dx = (canvas_horz_RPM_AMP.Width -10)/ RPM_AMP_max;
                    double min_position = 0 * dx;
                    double max_position = RPM_AMP_max * dx;

                    x = Math.Max(min_position, Math.Min(x, max_position));
                    double actual_x = x / dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_AMP = (byte)(actual_x);

                    text_RPM_AMP.Text = ((float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_AMP) /100 + "kg";
                    Canvas.SetLeft(text_RPM_AMP, x - text_RPM_AMP.Width / 2 + rect_RPM_AMP.Width / 2);
                    Canvas.SetTop(text_RPM_AMP, 5);
                    Canvas.SetLeft(rectangle, x);
                }

                //Bite point control
                if (rectangle.Name == "rect_BP_Control")
                {
                    // Ensure the rectangle stays within the canvas
                    double x = e.GetPosition(canvas).X - offset.X;
                    double BP_max = 100;
                    double dx = (canvas.Width) / BP_max;
                    double min_position = 10 * dx - rect_BP_Control.Width / 2;
                    double max_position = (BP_max - 10) * dx - rect_BP_Control.Width / 2;

                    x = Math.Max(min_position, Math.Min(x, max_position));
                    double actual_x = (x + rect_BP_Control.Width / 2) / dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger_value = (byte)(actual_x);

                    text_BP.Text = "Bite Point:\n" + ((float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger_value) + "%";
                    Canvas.SetLeft(rectangle, x);
                    Canvas.SetLeft(text_BP, Canvas.GetLeft(rect_BP_Control) + rect_BP_Control.Width + 3);
                    Canvas.SetTop(text_BP, canvas.Height - text_BP.Height);

                }

                if (rectangle.Name == "rect_bite_amp")
                {
                    // Ensure the rectangle stays within the canvas
                    double x = e.GetPosition(canvas_horz_bite_amp).X - offset.X;
                    double bite_amp_max = 200;
                    double dx = canvas_horz_bite_amp.Width / bite_amp_max;
                    double min_position = 0 * dx;
                    double max_position = bite_amp_max * dx;
                    //double dx = 100 / (canvas_horz_slider.Width - 10);
                    x = Math.Max(min_position, Math.Min(x, max_position));
                    double actual_x = x / dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_amp = Convert.ToByte(actual_x);
                    text_bite_amp.Text = ((float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_amp) / 100.0f + "kg";
                    Canvas.SetLeft(text_bite_amp, Canvas.GetLeft(rect_bite_amp) + rect_bite_amp.Width / 2 - text_bite_amp.Width / 2);
                    Canvas.SetTop(text_bite_amp, 5);
                    Canvas.SetLeft(rectangle, x);
                }
                if (rectangle.Name == "rect_bite_freq")
                {
                    // Ensure the rectangle stays within the canvas
                    double x = e.GetPosition(canvas_horz_bite_freq).X - offset.X;
                    double bite_freq_max = 30;
                    double dx = canvas_horz_bite_freq.Width / bite_freq_max;
                    double min_position = 0 * dx;
                    double max_position = bite_freq_max * dx;
                    //double dx = 100 / (canvas_horz_slider.Width - 10);
                    x = Math.Max(min_position, Math.Min(x, max_position));
                    double actual_x = x / dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_freq = Convert.ToByte(actual_x);
                    text_bite_freq.Text = (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_freq) + "Hz";
                    Canvas.SetLeft(text_bite_freq, Canvas.GetLeft(rect_bite_freq) + rect_bite_freq.Width / 2 - text_bite_freq.Width / 2);
                    Canvas.SetTop(text_bite_freq, 5);
                    Canvas.SetLeft(rectangle, x);
                }


            }
        }
        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                isDragging = false;
                rectangle.ReleaseMouseCapture();
                text_point_pos.Opacity=0;
            }
        }
        private void PID_type_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.control_strategy_b = (byte)1;
            TextBox_debugOutput.Text = "Dynamic PID on";
        }
        private void PID_type_checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.control_strategy_b = (byte)0;
            TextBox_debugOutput.Text = "Dynamic PID off";
        }
        private void Debug_checkbox_Checked(object sender, RoutedEventArgs e)
        {

            text_debug_flag.Opacity = 1;
            text_debug_PID_para.Opacity = 1;
            text_debug_dgain.Opacity = 1;
            text_debug_igain.Opacity = 1;
            text_debug_pgain.Opacity = 1;
            text_debug_feedforward.Opacity = 1;
            text_serial.Opacity = 1;
            TextBox_serialMonitor.Visibility = System.Windows.Visibility.Visible;
            PID_tuning_D_gain_slider.Opacity = 1;
            PID_tuning_I_gain_slider.Opacity = 1;
            PID_tuning_P_gain_slider.Opacity= 1;
            PID_tuning_Feedforward_gain_slider.Opacity = 1;
            textBox_debug_Flag_0.Opacity = 1;
            //btn_serial.Visibility = System.Windows.Visibility.Visible;
            btn_system_id.Visibility = System.Windows.Visibility.Visible;
            button_pedal_position_reset.Visibility = System.Windows.Visibility.Visible;
            button_pedal_restart.Visibility = System.Windows.Visibility.Visible;
            btn_pedal_disconnect.Visibility = System.Windows.Visibility.Visible;
            InvertLoadcellReading_check.Opacity = 1;
            //text_state.Visibility = Visibility.Hidden;
            debug_flag = true;
            
        }
        private void Debug_checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            text_debug_flag.Opacity = 0;
            text_debug_PID_para.Opacity = 0;
            text_debug_dgain.Opacity = 0;
            text_debug_igain.Opacity = 0;
            text_debug_pgain.Opacity = 0;
            text_debug_feedforward.Opacity = 0;
            text_serial.Opacity = 0;
            TextBox_serialMonitor.Visibility = System.Windows.Visibility.Hidden;
            PID_tuning_D_gain_slider.Opacity = 0;
            PID_tuning_I_gain_slider.Opacity = 0;
            PID_tuning_P_gain_slider.Opacity = 0;
            PID_tuning_Feedforward_gain_slider.Opacity = 0;
            textBox_debug_Flag_0.Opacity = 0;
            //btn_serial.Visibility = System.Windows.Visibility.Hidden;
            btn_system_id.Visibility = System.Windows.Visibility.Hidden;
            button_pedal_position_reset.Visibility = System.Windows.Visibility.Hidden;
            button_pedal_restart.Visibility = System.Windows.Visibility.Hidden;
            btn_pedal_disconnect.Visibility = System.Windows.Visibility.Hidden;
            InvertLoadcellReading_check.Opacity = 0;
            //text_state.Visibility = Visibility.Visible;
            debug_flag = false;
        }


        private void JoystickOutput_checked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.travelAsJoystickOutput_u8 = 1;

        }
        private void JoystickOutput_unchecked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.travelAsJoystickOutput_u8 = 0;
        }


        private void InvertLoadcellReading_checked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.invertLoadcellReading_u8 = 1;
        }
        private void InvertLoadcellReading_unchecked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.invertLoadcellReading_u8 = 0;
        }



        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.reading_config = 1;
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.reading_config = 0;
        }

        private void checkbox_auto_connect_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.auto_connect_flag = 1;
        }

        private void checkbox_auto_connect_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.auto_connect_flag = 0 ;
        }

        private void checkbox_enable_ABS_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.ABS_enable_flag[indexOfSelectedPedal_u] = 1;
            checkbox_enable_ABS.Content = "ABS/TC Effect Enabled";
        }
        private void checkbox_enable_ABS_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.ABS_enable_flag[indexOfSelectedPedal_u] = 0;
            checkbox_enable_ABS.Content = "ABS/TC Effect Disabled";
        }

        private void checkbox_enable_RPM_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.RPM_enable_flag[indexOfSelectedPedal_u] = 1;
            checkbox_enable_RPM.Content = "Engine RPM Effect Enabled";
        }

        private void checkbox_enable_RPM_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.RPM_enable_flag[indexOfSelectedPedal_u] = 0;
            checkbox_enable_RPM.Content = "Engine RPM Effect Disabled";
        }

        private void Vjoy_out_check_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.vjoy_output_flag = 1;
            ////// vJoy c# wrapper, see https://github.com/bobhelander/vJoy.Wrapper
            ////uint vJoystickId = Plugin.Settings.vjoy_order;
            ////joystick = new VirtualJoystick(Plugin.Settings.vjoy_order);
            ////joystick.Aquire();
            ////vjoy_axis_initialize();

            uint vJoystickId = Plugin.Settings.vjoy_order;
            //joystick = new VirtualJoystick(Plugin.Settings.vjoy_order);
            joystick = new vJoyInterfaceWrap.vJoy();

            joystick.AcquireVJD(vJoystickId);
            //joystick.Aquire();
            vjoy_axis_initialize();

        }


        private void Vjoy_out_check_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.vjoy_output_flag = 0;
            //joystick.Release();
            joystick.RelinquishVJD(Plugin.Settings.vjoy_order);
        }


        public void EffectAppliedOnForceOrTravel_combobox_changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absForceOrTarvelBit = (byte)EffectAppliedOnForceOrTravel_combobox.SelectedIndex;

                if (text_ABS != null)
                {
                    switch (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absForceOrTarvelBit)
                    {
                        case 0:
                            text_ABS.Text = (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude / 20 + "kg";
                            break;
                        case 1:
                            text_ABS.Text = (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude / 20 + "%";
                            break;
                        default:
                            break;
                    }
                }
                

            }
            catch (Exception caughtEx)
            {
                string errorMessage = caughtEx.Message;
                TextBox_debugOutput.Text = errorMessage;
            }

        }

        private void vjoy_plus_click(object sender, RoutedEventArgs e)
        {
            // release old joystick
            joystick.RelinquishVJD(Plugin.Settings.vjoy_order);

            Plugin.Settings.vjoy_order += 1;
            uint max = 16;
            uint min = 1;
            Plugin.Settings.vjoy_order = Math.Max(min, Math.Min(Plugin.Settings.vjoy_order, max));
            Label_vjoy_order.Content = Plugin.Settings.vjoy_order;
            if (Plugin.Settings.vjoy_output_flag == 1)
            {
                //joystick.Release();
                
                //VjdStat status;
                VjdStat status = joystick.GetVJDStatus(Plugin.Settings.vjoy_order);
                //status = joystick.Joystick.GetVJDStatus(Plugin.Settings.vjoy_order);
                switch (status)
                {
                    case VjdStat.VJD_STAT_OWN:
                        TextBox_debugOutput.Text = "vjoy already aquaried";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        break;
                    case VjdStat.VJD_STAT_FREE:

                        TextBox_debugOutput.Text = "vjoy aquaried";
                        //joystick = new VirtualJoystick(Plugin.Settings.vjoy_order);
                        //joystick.Aquire();
                        joystick.AcquireVJD(Plugin.Settings.vjoy_order);
                        if (Vjoy_out_check.IsChecked == false)
                        {
                            Vjoy_out_check.IsChecked = true;
                        }
                        //Console.WriteLine("vJoy Device {0} is free\n", id);
                        break;
                    case VjdStat.VJD_STAT_BUSY:
                        TextBox_debugOutput.Text = "vjoy was aquaried by other program";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        //Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
                        return;
                    case VjdStat.VJD_STAT_MISS:
                        TextBox_debugOutput.Text = "the selected vjoy device not enabled";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        //Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                        return;
                    default:
                        TextBox_debugOutput.Text = "vjoy device error";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        //Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                        return;
                };
            }
            

        }

        private void vjoy_minus_click(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.vjoy_order -= 1;
            uint max = 16;
            uint min = 1;
            Plugin.Settings.vjoy_order = Math.Max(min, Math.Min(Plugin.Settings.vjoy_order, max));
            Label_vjoy_order.Content = Plugin.Settings.vjoy_order;
            if (Plugin.Settings.vjoy_output_flag == 1)
            {
                //joystick.Release();
                joystick.RelinquishVJD(Plugin.Settings.vjoy_order);
                VjdStat status;
                status = joystick.GetVJDStatus(Plugin.Settings.vjoy_order);
                switch (status)
                {
                    case VjdStat.VJD_STAT_OWN:
                        TextBox_debugOutput.Text = "vjoy already aquaried";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        break;
                    case VjdStat.VJD_STAT_FREE:

                        TextBox_debugOutput.Text = "vjoy aquaried";
                        //joystick = new VirtualJoystick(Plugin.Settings.vjoy_order);
                        joystick.AcquireVJD(Plugin.Settings.vjoy_order);
                        //joystick.Aquire();
                        if (Vjoy_out_check.IsChecked == false)
                        {
                            Vjoy_out_check.IsChecked = true;
                        }
                        //Console.WriteLine("vJoy Device {0} is free\n", id);
                        break;
                    case VjdStat.VJD_STAT_BUSY:
                        TextBox_debugOutput.Text = "vjoy was aquaried by other program";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        //Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
                        return;
                    case VjdStat.VJD_STAT_MISS:
                        TextBox_debugOutput.Text = "the selected vjoy device not enabled";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        //Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                        return;
                    default:
                        TextBox_debugOutput.Text = "vjoy device error";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        //Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                        return;
                };
            }




        }
        private void checkbox_enable_bite_point_Checked(object sender, RoutedEventArgs e)
        {

            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger = 1;
            text_BP.Visibility = Visibility.Visible;
            rect_BP_Control.Visibility = Visibility.Visible;
            checkbox_enable_bite_point.Content = "Bite Point Vibration Enabled";


        }

        private void checkbox_enable_bite_point_Unchecked(object sender, RoutedEventArgs e)
        {

            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger = 0;
            text_BP.Visibility = Visibility.Hidden;
            rect_BP_Control.Visibility = Visibility.Hidden;
            checkbox_enable_bite_point.Content = "Bite Point Vibration Disabled";


        }


        /*
private void GetRectanglePositions()
{
   foreach (var kvp in rectanglePositions)
   {
       Console.WriteLine($"{kvp.Key}: X={kvp.Value.X}, Y={kvp.Value.Y}");
   }
}
*/

    }
    
}
