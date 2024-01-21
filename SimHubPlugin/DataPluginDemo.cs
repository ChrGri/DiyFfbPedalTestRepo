using GameReaderCommon;
//using log4net.Plugin;
using SimHub.Plugins;
using SimHub.Plugins.OutputPlugins.Dash.GLCDTemplating;
using System;
using System.IO.Ports;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;




// https://stackoverflow.com/questions/14344305/best-way-to-structure-class-struct-in-c-sharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Serializable]

static class Constants
{
    // payload revisiom
    public const uint pedalConfigPayload_version = 120;


    // pyload types
    public const uint pedalConfigPayload_type = 100;
    public const uint pedalActionPayload_type = 110;
    public const uint pedalStatePayload_type = 120;
}



public struct payloadHeader
{
    // structure identification via payload
    public byte payloadType;

    // variable to check if structure at receiver matched version from transmitter
    public byte version;

    public byte storeToEeprom;
}


public struct payloadPedalAction
{
    public byte triggerAbs_u8;
    public byte resetPedalPos_u8;
    public byte startSystemIdentification_u8;
    public byte returnPedalConfig_u8;
    public byte RPM_u8;
};

public struct payloadPedalState
{
    public UInt16 pedalPosition_u16;
    public UInt16 pedalForce_u16;
    public UInt16 joystickOutput_u16;

    // register values from servo
    public Int16 servoPosition_i16;
    public Int16 servoPositionTarget_i16;
};

public struct payloadPedalConfig
{
    // configure pedal start and endpoint
    // In percent
    public byte pedalStartPosition;
    public byte pedalEndPosition;

    // configure pedal forces
    public byte maxForce;
    public byte preloadForce;

    // design force vs travel curve
    // In percent
    public byte relativeForce_p000;
    public byte relativeForce_p020;
    public byte relativeForce_p040;
    public byte relativeForce_p060;
    public byte relativeForce_p080;
    public byte relativeForce_p100;

    // parameter to configure damping
    public byte dampingPress;
    public byte dampingPull;

    // configure ABS effect 
    public byte absFrequency; // In Hz
    public byte absAmplitude; // In kg/20
    public byte absPattern; // 0: sinewave, 1: sawtooth
    public byte absForceOrTarvelBit;


    // geometric properties of the pedal
    // in mm
    public byte lengthPedal_AC;
    public byte horPos_AB;
    public byte verPos_AB;
    public byte lengthPedal_CB;
    public byte Simulate_ABS_trigger; //simulateABS
    public byte Simulate_ABS_value; //simulated ABS value
    public byte RPM_max_freq;
    public byte RPM_min_freq;
    public byte RPM_AMP;
    public byte BP_trigger_value;
    public byte BP_amp;
    public byte BP_freq;
    public byte BP_trigger;
    // cubic spline params
    public float cubic_spline_param_a_0;
    public float cubic_spline_param_a_1;
    public float cubic_spline_param_a_2;
    public float cubic_spline_param_a_3;
    public float cubic_spline_param_a_4;

    public float cubic_spline_param_b_0;
    public float cubic_spline_param_b_1;
    public float cubic_spline_param_b_2;
    public float cubic_spline_param_b_3;
    public float cubic_spline_param_b_4;

    // PID settings
    public float PID_p_gain;
    public float PID_i_gain;
    public float PID_d_gain;
    public float PID_feedforward_gain;

    public byte control_strategy_b;

    public byte maxGameOutput;

    // Kalman filter model noise
    public byte kf_modelNoise;

    // debug flags, sued to enable debug output
    public byte debug_flags_0;

    // loadcell rating in kg / 2 --> to get value in kg, muiltiply by 2
    public byte loadcell_rating;

    // use loadcell or travel as joystick output
    public byte travelAsJoystickOutput_u8;

    // invert loadcell sign
    public byte invertLoadcellReading_u8;


}

public struct payloadFooter
{
    // To check if structure is valid
    public UInt16 checkSum;
}



public struct DAP_action_st
{
    public payloadHeader payloadHeader_;
    public payloadPedalAction payloadPedalAction_;
    public payloadFooter payloadFooter_;
}


public struct DAP_config_st
{
    public payloadHeader payloadHeader_;
    public payloadPedalConfig payloadPedalConfig_;
    public payloadFooter payloadFooter_;
}

public struct DAP_state_st
{
    public payloadHeader payloadHeader_;
    public payloadPedalState payloadPedalState_;
    public payloadFooter payloadFooter_;
}



namespace User.PluginSdkDemo
{
    [PluginDescription("My plugin description")]
    [PluginAuthor("OpenSource")]
    [PluginName("DIY active pedal plugin")]
    public class DataPluginDemo : IPlugin, IDataPlugin, IWPFSettingsV2
    {

        public PluginManager pluginHandle;// = this;

        public bool sendAbsSignal = false;
		public DAP_config_st dap_config_initial_st;
        public byte rpm_last_value = 0 ;
        public byte game_running_index = 0 ;
        public uint testValue = 0;

        public SettingsControlDemo wpfHandle;


        // ABS trigger timer
        DateTime absTrigger_currentTime = DateTime.Now;
        DateTime absTrigger_lastTime = DateTime.Now;
        //// payload revisiom
        //public uint pedalConfigPayload_version = 110;

        //// pyload types
        //public uint pedalConfigPayload_type = 100;
        //public uint pedalActionPayload_type = 110;

        //public SettingsControlDemo settings { get; }

        //SettingsControlDemo wpfHandler;


        //https://www.c-sharpcorner.com/uploadfile/eclipsed4utoo/communicating-with-serial-port-in-C-Sharp/
        public SerialPort[] _serialPort = new SerialPort[3] {new SerialPort("COM7", 921600, Parity.None, 8, StopBits.One),
            new SerialPort("COM7", 921600, Parity.None, 8, StopBits.One),
            new SerialPort("COM7", 921600, Parity.None, 8, StopBits.One)};




        public bool[] connectSerialPort = { false, false, false };


        public DataPluginDemoSettings Settings;



        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Gets the left menu icon. Icon must be 24x24 and compatible with black and white display.
        /// </summary>
        public ImageSource PictureIcon => this.ToIcon(Properties.Resources.sdkmenuicon);

        /// <summary>
        /// Gets a short plugin title to show in left menu. Return null if you want to use the title as defined in PluginName attribute.
        /// </summary>
        public string LeftMenuTitle => "DIY FFB Pedal";

        /// <summary>
        /// Called one time per game data update, contains all normalized game data,
        /// raw data are intentionnally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
        ///
        /// This method is on the critical path, it must execute as fast as possible and avoid throwing any error
        ///
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="data">Current game data, including current and previous data frame.</param>
        /// 
        unsafe public UInt16 checksumCalc(byte* data, int length)
        {

            UInt16 curr_crc = 0x0000;
            byte sum1 = (byte)curr_crc;
            byte sum2 = (byte)(curr_crc >> 8);
            int index;
            for (index = 0; index < length; index = index + 1)
            {
                int v = (sum1 + (*data));
                sum1 = (byte)v;
                sum1 = (byte)(v % 255);

                int w = (sum1 + sum2) % 255;
                sum2 = (byte)w;

                data++;// = data++;
            }

            int x = (sum2 << 8) | sum1;
            return (UInt16)x;
        }

        public byte[] getBytes_Action(DAP_action_st aux)
        {
            int length = Marshal.SizeOf(aux);
            IntPtr ptr = Marshal.AllocHGlobal(length);
            byte[] myBuffer = new byte[length];

            Marshal.StructureToPtr(aux, ptr, true);
            Marshal.Copy(ptr, myBuffer, 0, length);
            Marshal.FreeHGlobal(ptr);

            return myBuffer;
        }

        unsafe public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
			
			bool sendAbsSignal_local_b = false;
            bool sendTcSignal_local_b = false;
            double RPM_value =0;
            double RPM_MAX = 0;

            //for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
            //{
            //    if (_serialPort[pedalIdx].IsOpen)
            //    {
            //        int receivedLength = _serialPort[pedalIdx].BytesToRead;

            //        settings.TextBox_debugOutput.Text = "Test";
            //    }
            //}



            // Send ABS signal when triggered by the game
            if (data.GameRunning)
            {
                if (data.OldData != null && data.NewData != null)
                {
                    if (data.NewData.ABSActive > 0)
                    {
                        sendAbsSignal_local_b = true;
                    }

                    if (data.NewData.TCActive > 0)
                    {
                        sendTcSignal_local_b = true;
                    }
                    if (data.NewData.CarSettings_MaxRPM == 0)
                    {
                        RPM_MAX = 10000;
                    }
                    else
                    {
                        RPM_MAX = data.NewData.CarSettings_MaxRPM;
                    }
                    RPM_value = (data.NewData.Rpms / RPM_MAX*100);
                    game_running_index = 1;

                }
                else
                {
                    RPM_value = 0;                
                }
            }
            else
            {
                RPM_value = 0;
            }
			




            absTrigger_currentTime = DateTime.Now;
            TimeSpan diff = absTrigger_currentTime - absTrigger_lastTime;
            int millisceonds = (int)diff.TotalMilliseconds;
            if (millisceonds <= 5)
            {
                sendAbsSignal_local_b = false;
                sendTcSignal_local_b = false;
            }
            else
            {
                absTrigger_lastTime = DateTime.Now;
            }



            bool update_flag = false;

            if (data.GameRunning)
            {
                // Send ABS trigger signal via serial
                for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
                {
                    if (_serialPort[pedalIdx].IsOpen)
                    {

                        DAP_action_st tmp;
                        tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                        tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;
                        tmp.payloadPedalAction_.triggerAbs_u8 = 0;
                        tmp.payloadPedalAction_.RPM_u8 = (Byte)rpm_last_value;
                        if (Settings.RPM_enable_flag[pedalIdx] == 1)
                        {
                            if (Math.Abs(RPM_value - rpm_last_value) >10)
                            {
                                tmp.payloadPedalAction_.RPM_u8 = (Byte)RPM_value;
                                update_flag = true;
                                rpm_last_value = (Byte)RPM_value;
                            }
                        }

                        if (pedalIdx == 1)
                        {
                            if (sendAbsSignal_local_b & Settings.ABS_enable_flag[pedalIdx] ==1)
                            {
                                //_serialPort[1].Write("2");

                                // compute checksum
                                tmp.payloadPedalAction_.triggerAbs_u8 = 1;
                                update_flag = true;

                            }
                        }
                        if (pedalIdx == 2)
                        {
                            if (sendTcSignal_local_b & Settings.ABS_enable_flag[pedalIdx] == 1)
                            {
                                // compute checksum

                                tmp.payloadPedalAction_.triggerAbs_u8 = 1;
                                update_flag = true;

                            }
                        }

                        if (update_flag)
                        {
                            DAP_action_st* v = &tmp;
                            byte* p = (byte*)v;
                            tmp.payloadFooter_.checkSum = checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));


                            int length = sizeof(DAP_action_st);
                            byte[] newBuffer = new byte[length];
                            newBuffer = getBytes_Action(tmp);


                            // clear inbuffer 
                            _serialPort[pedalIdx].DiscardInBuffer();

                            // send query command
                            _serialPort[pedalIdx].Write(newBuffer, 0, newBuffer.Length);


                        }
                    }
                }
                /*
                // Send TC trigger signal via serial
                if (_serialPort[2].IsOpen)
                {
                    DAP_action_st tmp;

                    tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                    tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;
                    tmp.payloadPedalAction_.triggerAbs_u8 = 0;
                    tmp.payloadPedalAction_.RPM_u8 = rpm_last_value;

                    if ((RPM_value - rpm_last_value > 10) || (RPM_value - rpm_last_value < -10))
                    {
                        tmp.payloadPedalAction_.RPM_u8 = (Byte)RPM_value;
                        update_flag = true;
                        rpm_last_value = (Byte)RPM_value;
                    }
                    if (sendTcSignal_local_b)
                    {
                        // compute checksum

                        tmp.payloadPedalAction_.triggerAbs_u8 = 1;
                        update_flag = true;

                    }
                    if (update_flag)
                    {
                        DAP_action_st* v = &tmp;
                        byte* p = (byte*)v;
                        tmp.payloadFooter_.checkSum = checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));


                        int length = sizeof(DAP_action_st);
                        byte[] newBuffer = new byte[length];
                        newBuffer = getBytes_Action(tmp);


                        // clear inbuffer 
                        _serialPort[2].DiscardInBuffer();

                        // send query command
                        _serialPort[2].Write(newBuffer, 0, newBuffer.Length);
                        rpm_last_value = (Byte)RPM_value;
                    }



                }
                */

            }
            else
            {
                if (game_running_index == 1)
                {
                    game_running_index = 0;
                    DAP_action_st tmp;
                    tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                    tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;
                    tmp.payloadPedalAction_.triggerAbs_u8 = 0;
                    tmp.payloadPedalAction_.RPM_u8 = 0;
                    rpm_last_value = 0;
                    DAP_action_st* v = &tmp;
                    byte* p = (byte*)v;
                    tmp.payloadFooter_.checkSum = checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));
                    int length = sizeof(DAP_action_st);
                    byte[] newBuffer = new byte[length];
                    newBuffer = getBytes_Action(tmp);
                    for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
                    {
                        if (_serialPort[pedalIdx].IsOpen)
                        {
                            // clear inbuffer 
                            _serialPort[pedalIdx].DiscardInBuffer();

                            // send query command
                            _serialPort[pedalIdx].Write(newBuffer, 0, newBuffer.Length);
                        }
                    }
                    
                }
            }
            // Send ABS test signal if requested
            if (sendAbsSignal)
            {
                sendAbsSignal_local_b = true;
                sendTcSignal_local_b = true;
                DAP_action_st tmp;
                tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;
                tmp.payloadPedalAction_.triggerAbs_u8 = 1;
                tmp.payloadPedalAction_.RPM_u8 = 0;
                DAP_action_st* v = &tmp;
                byte* p = (byte*)v;
                tmp.payloadFooter_.checkSum = checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));
                int length = sizeof(DAP_action_st);
                byte[] newBuffer = new byte[length];
                newBuffer = getBytes_Action(tmp);
                if (_serialPort[2].IsOpen)
                {
                    // clear inbuffer 
                    _serialPort[2].DiscardInBuffer();

                    // send query command
                    _serialPort[2].Write(newBuffer, 0, newBuffer.Length);
                }
                if (_serialPort[1].IsOpen)
                {
                    // clear inbuffer 
                    _serialPort[1].DiscardInBuffer();

                    // send query command
                    _serialPort[1].Write(newBuffer, 0, newBuffer.Length);
                }
            }

        }



        /////////********************************************************************************************************************/
        /////////*							read serial stream																		*/
        /////////********************************************************************************************************************/
        ////////public System.Windows.Forms.Timer[] pedal_serial_read_timer = new System.Windows.Forms.Timer[3];
        ////////public void openSerialAndAddReadCallback(uint pedalIdx)
        ////////{

        ////////    // serial port settings
        ////////    _serialPort[pedalIdx].Handshake = Handshake.None;
        ////////    _serialPort[pedalIdx].Parity = Parity.None;
        ////////    //_serialPort[pedalIdx].StopBits = StopBits.None;


        ////////    _serialPort[pedalIdx].ReadTimeout = 2000;
        ////////    _serialPort[pedalIdx].WriteTimeout = 500;

        ////////    // https://stackoverflow.com/questions/7178655/serialport-encoding-how-do-i-get-8-bit-ascii
        ////////    _serialPort[pedalIdx].Encoding = System.Text.Encoding.GetEncoding(28591);

        ////////    _serialPort[pedalIdx].DtrEnable = false;

        ////////    _serialPort[pedalIdx].NewLine = "\r\n";
        ////////    _serialPort[pedalIdx].ReadBufferSize = 10000;


        ////////    _serialPort[pedalIdx].Open();


        ////////    // read callback
        ////////    pedal_serial_read_timer[pedalIdx] = new System.Windows.Forms.Timer();
        ////////    pedal_serial_read_timer[pedalIdx].Tick += new EventHandler(timer1_Tick);
        ////////    pedal_serial_read_timer[pedalIdx].Tag = pedalIdx;
        ////////    pedal_serial_read_timer[pedalIdx].Interval = 100; // in miliseconds
        ////////    pedal_serial_read_timer[pedalIdx].Start();
        ////////    System.Threading.Thread.Sleep(100);
        ////////}

        ////////public void closeSerialAndStopReadCallback(uint pedalIdx)
        ////////{
        ////////    if (pedal_serial_read_timer[pedalIdx] != null)
        ////////    {
        ////////        pedal_serial_read_timer[pedalIdx].Stop();
        ////////        pedal_serial_read_timer[pedalIdx].Dispose();
        ////////    }
        ////////    System.Threading.Thread.Sleep(300);
        ////////    if (_serialPort[pedalIdx].IsOpen)
        ////////    {
        ////////        _serialPort[pedalIdx].DiscardInBuffer();
        ////////        _serialPort[pedalIdx].DiscardOutBuffer();
        ////////        _serialPort[pedalIdx].Close();

        ////////    }
        ////////}


        ////////int printCtr = 0;
        ////////unsafe public void timer1_Tick(object sender, EventArgs e)
        ////////{

        ////////    // if WPF isn't available, update the WPF handler and skip
        ////////    if (wpfHandler == null)
        ////////    {
        ////////        //SettingsControlDemo wpfHandler = (SettingsControlDemo)GetWPFSettingsControl(this);

        ////////        wpfHandler = (SettingsControlDemo)GetWPFSettingsControl(pluginHandle);
        ////////        return;
        ////////    }

        ////////    int pedalSelected = Int32.Parse((sender as System.Windows.Forms.Timer).Tag.ToString());
        ////////    //int pedalSelected = (int)(sender as System.Windows.Forms.Timer).Tag;

        ////////    bool pedalStateHasAlreadyBeenUpdated_b = false;

        ////////    // once the pedal has identified, go ahead
        ////////    if (pedalSelected < 3)
        ////////    //if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
        ////////    {

        ////////        SerialPort sp = _serialPort[pedalSelected];



        ////////        // https://stackoverflow.com/questions/9732709/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it


        ////////        //int length = sizeof(DAP_config_st);
        ////////        //byte[] newBuffer_config = new byte[length];

        ////////        if (sp.IsOpen)
        ////////        {
        ////////            int receivedLength = sp.BytesToRead;

        ////////            if (receivedLength > 0)
        ////////            {

        ////////                string incomingData = sp.ReadExisting();

        ////////                //if the data doesn't end with a stop char this will signal to keep it in _data 
        ////////                //for appending to the following read of data
        ////////                bool endsWithStop = wpfHandler.EndsWithStop(incomingData);

        ////////                //each array object will be sent separately to the callback
        ////////                string[] dataArray = incomingData.Split(wpfHandler.STOPCHAR, StringSplitOptions.None);

        ////////                for (int i = 0; i < dataArray.Length - 1; i++)
        ////////                {
        ////////                    string newData = dataArray[i];

        ////////                    //if you are at the last object in the array and this hasn't got a stopchar after
        ////////                    //it will be saved in _data
        ////////                    if (!endsWithStop && (i == dataArray.Length - 2))
        ////////                    {
        ////////                        wpfHandler._data[pedalSelected] += newData;
        ////////                    }
        ////////                    else
        ////////                    {
        ////////                        string dataToSend = wpfHandler._data[pedalSelected] + newData;
        ////////                        wpfHandler._data[pedalSelected] = "";



        ////////                        // check for pedal state struct
        ////////                        if ((dataToSend.Length == sizeof(DAP_state_st)))
        ////////                        {

        ////////                            // transform string into byte
        ////////                            fixed (byte* p = System.Text.Encoding.GetEncoding(28591).GetBytes(dataToSend))
        ////////                            {
        ////////                                // create a fixed size buffer
        ////////                                int length = sizeof(DAP_state_st);
        ////////                                byte[] newBuffer_state_2 = new byte[length];

        ////////                                // copy the received bytes into byte array
        ////////                                for (int j = 0; j < length; j++)
        ////////                                {
        ////////                                    newBuffer_state_2[j] = p[j];
        ////////                                }

        ////////                                // parse byte array as config struct
        ////////                                DAP_state_st pedalState_read_st = wpfHandler.getStateFromBytes(newBuffer_state_2);

        ////////                                // check whether receive struct is plausible
        ////////                                DAP_state_st* v_state = &pedalState_read_st;
        ////////                                byte* p_state = (byte*)v_state;

        ////////                                // payload type check
        ////////                                bool check_payload_state_b = false;
        ////////                                if (pedalState_read_st.payloadHeader_.payloadType == Constants.pedalStatePayload_type)
        ////////                                {
        ////////                                    check_payload_state_b = true;
        ////////                                }

        ////////                                // CRC check
        ////////                                bool check_crc_state_b = false;
        ////////                                if (checksumCalc(p_state, sizeof(payloadHeader) + sizeof(payloadPedalState)) == pedalState_read_st.payloadFooter_.checkSum)
        ////////                                {
        ////////                                    check_crc_state_b = true;
        ////////                                }

        ////////                                if ((check_payload_state_b) && check_crc_state_b)
        ////////                                {

        ////////                                    if (pedalStateHasAlreadyBeenUpdated_b == false)
        ////////                                    {
        ////////                                        wpfHandler.TextBox_debugOutput.Text = "Pedal pos: " + pedalState_read_st.payloadPedalState_.pedalPosition_u16;
        ////////                                        wpfHandler.TextBox_debugOutput.Text += "Pedal force: " + pedalState_read_st.payloadPedalState_.pedalForce_u16;
        ////////                                        pedalStateHasAlreadyBeenUpdated_b = true;

        ////////                                        wpfHandler.text_point_pos.Opacity = 0;
        ////////                                        double control_rect_value_max = 65535;
        ////////                                        double dyy = wpfHandler.canvas.Height / control_rect_value_max;
        ////////                                        double dxx = wpfHandler.canvas.Width / control_rect_value_max;


        ////////                                        Canvas.SetLeft(wpfHandler.rect_State, dxx * pedalState_read_st.payloadPedalState_.pedalPosition_u16 - wpfHandler.rect_State.Width / 2);
        ////////                                        Canvas.SetTop(wpfHandler.rect_State, wpfHandler.canvas.Height - dyy * pedalState_read_st.payloadPedalState_.pedalForce_u16 - wpfHandler.rect_State.Height / 2);
        ////////                                    }


        ////////                                    continue;
        ////////                                }
        ////////                            }
        ////////                        }


        ////////                        // decode into config struct
        ////////                        if ((wpfHandler.waiting_for_pedal_config[pedalSelected]) && (dataToSend.Length == sizeof(DAP_config_st)))
        ////////                        {
        ////////                            DAP_config_st tmp;


        ////////                            // transform string into byte
        ////////                            fixed (byte* p = System.Text.Encoding.GetEncoding(28591).GetBytes(dataToSend))
        ////////                            {
        ////////                                // create a fixed size buffer
        ////////                                int length = sizeof(DAP_config_st);
        ////////                                byte[] newBuffer_config_2 = new byte[length];

        ////////                                // copy the received bytes into byte array
        ////////                                for (int j = 0; j < length; j++)
        ////////                                {
        ////////                                    newBuffer_config_2[j] = p[j];
        ////////                                }

        ////////                                // parse byte array as config struct
        ////////                                DAP_config_st pedalConfig_read_st = wpfHandler.getConfigFromBytes(newBuffer_config_2);

        ////////                                // check whether receive struct is plausible
        ////////                                DAP_config_st* v_config = &pedalConfig_read_st;
        ////////                                byte* p_config = (byte*)v_config;

        ////////                                // payload type check
        ////////                                bool check_payload_config_b = false;
        ////////                                if (pedalConfig_read_st.payloadHeader_.payloadType == Constants.pedalConfigPayload_type)
        ////////                                {
        ////////                                    check_payload_config_b = true;
        ////////                                }

        ////////                                // CRC check
        ////////                                bool check_crc_config_b = false;
        ////////                                if (checksumCalc(p_config, sizeof(payloadHeader) + sizeof(payloadPedalConfig)) == pedalConfig_read_st.payloadFooter_.checkSum)
        ////////                                {
        ////////                                    check_crc_config_b = true;
        ////////                                }

        ////////                                if ((check_payload_config_b) && check_crc_config_b)
        ////////                                {
        ////////                                    wpfHandler.waiting_for_pedal_config[pedalSelected] = false;
        ////////                                    wpfHandler.dap_config_st[pedalSelected] = pedalConfig_read_st;
        ////////                                    wpfHandler.updateTheGuiFromConfig();

        ////////                                    continue;
        ////////                                }
        ////////                                else
        ////////                                {
        ////////                                    wpfHandler.TextBox_debugOutput.Text = "Payload config test 1: " + check_payload_config_b;
        ////////                                    wpfHandler.TextBox_debugOutput.Text += "Payload config test 2: " + check_crc_config_b;
        ////////                                }
        ////////                            }

        ////////                        }
        ////////                        //else
        ////////                        //{


        ////////                        // When too many messages are received, only print every Nth message

        ////////                        // When only a few messages are received, make the counter greater than N thus every message is printed
        ////////                        if (dataArray.Length < 10)
        ////////                        {
        ////////                            printCtr = 600;
        ////////                        }

        ////////                        if (printCtr++ > 200)
        ////////                        {
        ////////                            printCtr = 0;
        ////////                            wpfHandler.TextBox_serialMonitor.Text += dataToSend + "\n";
        ////////                            wpfHandler.TextBox_serialMonitor.ScrollToEnd();
        ////////                        }

        ////////                        //}


        ////////                    }

        ////////                    try
        ////////                    {
        ////////                        while (wpfHandler.TextBox_serialMonitor.LineCount > 30)
        ////////                        {
        ////////                            wpfHandler.TextBox_serialMonitor.Text = wpfHandler.TextBox_serialMonitor.Text.Remove(0, wpfHandler.TextBox_serialMonitor.GetLineLength(0));
        ////////                        }
        ////////                    }
        ////////                    catch { }







        ////////                    //limits the data stored to 1000 to avoid using up all the memory in case of 
        ////////                    //failure to register callback or include stopchar

        ////////                    if (wpfHandler._data[pedalSelected].Length > 1000)
        ////////                    {
        ////////                        wpfHandler._data[pedalSelected] = "";
        ////////                    }


        ////////                }

        ////////                // obtain data and check whether it is from known payload type or just debug info

        ////////            }

        ////////        }
        ////////    }
        ////////}



        /// <summary>
        /// Returns the settings control, return null if no settings control is required
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
        {

            return new SettingsControlDemo(this);
        }


        /// <summary>
        /// Called at plugin manager stop, close/dispose anything needed here !
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void End(PluginManager pluginManager)
        {           
            // Save settings
            this.SaveCommonSettings("GeneralSettings", Settings);

            // close serial communication
            if (wpfHandle != null)
            {

                try
                {
                    //wpfHandle.joystick.Release();
                    //wpfHandle.joystick.Dispose();
                    wpfHandle.joystick.RelinquishVJD(Settings.vjoy_order);
                    
                }
                catch (Exception caughtEx)
                { 
                }
                

                for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
                {
                    wpfHandle.closeSerialAndStopReadCallback(pedalIdx);
                }
            }
            
        }



        public bool PortExists(string portName)
        {
            string[] portNames = SerialPort.GetPortNames();
            return Array.Exists(portNames, name => name.Equals(portName, StringComparison.OrdinalIgnoreCase));
        }





        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {

            pluginHandle = pluginManager;
        
            SimHub.Logging.Current.Info("Starting DIY active pedal plugin");

            // Load settings
            Settings = this.ReadCommonSettings<DataPluginDemoSettings>("GeneralSettings", () => new DataPluginDemoSettings());

            // Declare a property available in the property list, this gets evaluated "on demand" (when shown or used in formulas)
            this.AttachDelegate("CurrentDateTime", () => DateTime.Now);

            // Declare an event
            this.AddEvent("SpeedWarning");


            // Declare an action which can be called
            this.AddAction("IncrementSpeedWarning",(a, b) =>
            {
                Settings.SpeedWarningLevel++;
                SimHub.Logging.Current.Info("Speed warning changed");
            });

            // Declare an action which can be called
            this.AddAction("DecrementSpeedWarning", (a, b) =>
            {
                Settings.SpeedWarningLevel--;
            });


            //Settings.selectedJsonIndexLast[0]
            SimHub.Logging.Current.Info("Diy active pedas plugin - Test 1");
            SimHub.Logging.Current.Info("Diy active pedas plugin - COM port: " + Settings.selectedComPortNames[0]);




            // get WPF handler
            //wpfHandler = (SettingsControlDemo)GetWPFSettingsControl(pluginManager);

            //if (wpfHandler.)
            {
                // prepare serial port interfaces
                for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
                {
                    if (_serialPort[pedalIdx].IsOpen)
                    {
                        System.Threading.Thread.Sleep(300);
                    }


                    try
                    {
                        _serialPort[pedalIdx].PortName = Settings.selectedComPortNames[pedalIdx];
                    }
                    catch (Exception caughtEx)
                    {
                    }

                    //try connect back to com port
                    if (Settings.auto_connect_flag == 1)
                    {

                        if (Settings.connect_status[pedalIdx] == 1)
                        {
                            //_serialPort[pedalIdx].PortName = Settings.selectedComPortNames[pedalIdx];
                            //SerialPort.GetPortNames
                            if (PortExists(_serialPort[pedalIdx].PortName))
                            {
                                if (_serialPort[pedalIdx].IsOpen == false)
                                {
                                    //if (wpfHandle != null)
                                    //{
                                    //    wpfHandle.openSerialAndAddReadCallback(pedalIdx);
                                    //}

                                    connectSerialPort[pedalIdx] = true;
                                }
                                else
                                {
                                    //if (wpfHandle != null)
                                    //{
                                    //    wpfHandle.closeSerialAndStopReadCallback(pedalIdx);
                                    //}
                                    //ConnectToPedal.IsChecked = false;
                                    //TextBox_debugOutput.Text = "Serialport already open, close it";
                                    Settings.connect_status[pedalIdx] = 0;
                                    connectSerialPort[pedalIdx] = false;
                                }


                            }
                            else
                            {
                                Settings.connect_status[pedalIdx] = 0;
                                connectSerialPort[pedalIdx] = false;
                            }
                        }
                        else
                        {
                            Settings.connect_status[pedalIdx] = 0;
                            connectSerialPort[pedalIdx] = false;
                        }

                    }

                }

            }

            
            
            

         


            //// check if Json config files are present, otherwise create new ones
            //for (uint jsonIndex = 0; jsonIndex < ComboBox_JsonFileSelected.Items.Count; jsonIndex++)
            //{
            //	// which config file is seleced
            //	string currentDirectory = Directory.GetCurrentDirectory();
            //	string dirName = currentDirectory + "\\PluginsData\\Common";
            //	string jsonFileName = ComboBox_JsonFileSelected(ComboBox_JsonFileSelected.Items[jsonIndex]).Text;
            //	string fileName = dirName + "\\" + jsonFileName + ".json";


            //	// Check if file already exists, otherwise create    
            //	if (!File.Exists(fileName))
            //	{
            //		// create default config
            //		// https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
            //		// https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
            //		var stream1 = new MemoryStream();
            //		var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
            //		ser.WriteObject(stream1, dap_config_initial_st);

            //		stream1.Position = 0;
            //		StreamReader sr = new StreamReader(stream1);
            //		string jsonString = sr.ReadToEnd();

            //		System.IO.File.WriteAllText(fileName, jsonString);
            //	}
            //}



            dap_config_initial_st.payloadHeader_.payloadType = (byte)Constants.pedalConfigPayload_type;
            dap_config_initial_st.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
            dap_config_initial_st.payloadHeader_.storeToEeprom = 0;
            dap_config_initial_st.payloadPedalConfig_.pedalStartPosition = 35;
            dap_config_initial_st.payloadPedalConfig_.pedalEndPosition = 80;
            dap_config_initial_st.payloadPedalConfig_.maxForce = 90;
            dap_config_initial_st.payloadPedalConfig_.relativeForce_p000 = 0;
            dap_config_initial_st.payloadPedalConfig_.relativeForce_p020 = 20;
            dap_config_initial_st.payloadPedalConfig_.relativeForce_p040 = 40;
            dap_config_initial_st.payloadPedalConfig_.relativeForce_p060 = 60;
            dap_config_initial_st.payloadPedalConfig_.relativeForce_p080 = 80;
            dap_config_initial_st.payloadPedalConfig_.relativeForce_p100 = 100;
            dap_config_initial_st.payloadPedalConfig_.dampingPress = 0;
            dap_config_initial_st.payloadPedalConfig_.dampingPull = 0;
            dap_config_initial_st.payloadPedalConfig_.absFrequency = 5;
            dap_config_initial_st.payloadPedalConfig_.absAmplitude = 100;
            dap_config_initial_st.payloadPedalConfig_.absPattern = 0;
            dap_config_initial_st.payloadPedalConfig_.absForceOrTarvelBit = 0;
            dap_config_initial_st.payloadPedalConfig_.lengthPedal_AC = 150;
            dap_config_initial_st.payloadPedalConfig_.horPos_AB = 215;
            dap_config_initial_st.payloadPedalConfig_.verPos_AB = 80;
            dap_config_initial_st.payloadPedalConfig_.lengthPedal_CB = 200;
            dap_config_initial_st.payloadPedalConfig_.Simulate_ABS_trigger = 0;
            dap_config_initial_st.payloadPedalConfig_.Simulate_ABS_value = 50;
            dap_config_initial_st.payloadPedalConfig_.RPM_max_freq = 40;
            dap_config_initial_st.payloadPedalConfig_.RPM_min_freq = 10;
            dap_config_initial_st.payloadPedalConfig_.RPM_AMP = 5;
            dap_config_initial_st.payloadPedalConfig_.BP_trigger_value = 50;
            dap_config_initial_st.payloadPedalConfig_.BP_amp = 1;
            dap_config_initial_st.payloadPedalConfig_.BP_freq = 15;
            dap_config_initial_st.payloadPedalConfig_.BP_trigger = 0;

            dap_config_initial_st.payloadPedalConfig_.maxGameOutput = 100;

            dap_config_initial_st.payloadPedalConfig_.kf_modelNoise = 128;
            dap_config_initial_st.payloadPedalConfig_.debug_flags_0 = 0;

            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_a_0 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_a_1 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_a_2 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_a_3 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_a_4 = 0;

            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_b_0 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_b_1 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_b_2 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_b_3 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_b_4 = 0;

            dap_config_initial_st.payloadPedalConfig_.PID_p_gain = 0.3f;
            dap_config_initial_st.payloadPedalConfig_.PID_i_gain = 50.0f;
            dap_config_initial_st.payloadPedalConfig_.PID_d_gain = 0.0f;
            dap_config_initial_st.payloadPedalConfig_.PID_feedforward_gain = 0.0f;
            

            dap_config_initial_st.payloadPedalConfig_.control_strategy_b = 0;

            dap_config_initial_st.payloadPedalConfig_.loadcell_rating = 150;

            dap_config_initial_st.payloadPedalConfig_.travelAsJoystickOutput_u8 = 0;

            dap_config_initial_st.payloadPedalConfig_.invertLoadcellReading_u8 = 0;



            


        }
    }
}