#include "DiyActivePedal_types.h"
#include "Arduino.h"

#include "PedalGeometry.h"
#include "StepperWithLimits.h"

#include <EEPROM.h>

static const float ABS_SCALING = 50;

void DAP_config_st::initialiseDefaults() {
  payLoadHeader_.payloadType = DAP_PAYLOAD_TYPE_CONFIG;
  payLoadHeader_.version = DAP_VERSION_CONFIG;
  payLoadHeader_.storeToEeprom = false;

  payLoadPedalConfig_.pedalStartPosition = 10;
  payLoadPedalConfig_.pedalEndPosition = 85;

  payLoadPedalConfig_.maxForce = 60;
  payLoadPedalConfig_.preloadForce = 2;

  payLoadPedalConfig_.relativeForce_p000 = 0;
  payLoadPedalConfig_.relativeForce_p020 = 20;
  payLoadPedalConfig_.relativeForce_p040 = 40;
  payLoadPedalConfig_.relativeForce_p060 = 60;
  payLoadPedalConfig_.relativeForce_p080 = 80;
  payLoadPedalConfig_.relativeForce_p100 = 100;

  payLoadPedalConfig_.dampingPress = 0;
  payLoadPedalConfig_.dampingPull = 0;

  payLoadPedalConfig_.absFrequency = 15;
  payLoadPedalConfig_.absAmplitude = 0;
  payLoadPedalConfig_.absPattern = 0;
  payLoadPedalConfig_.absForceOrTarvelBit = 0;

  payLoadPedalConfig_.lengthPedal_AC = 150;
  payLoadPedalConfig_.horPos_AB = 215;
  payLoadPedalConfig_.verPos_AB = 80;
  payLoadPedalConfig_.lengthPedal_CB = 200;
  payLoadPedalConfig_.Simulate_ABS_trigger = 0;// add for abs trigger
  payLoadPedalConfig_.Simulate_ABS_value = 80;// add for abs trigger
  payLoadPedalConfig_.RPM_max_freq = 40;
  payLoadPedalConfig_.RPM_min_freq = 10;
  payLoadPedalConfig_.RPM_AMP = 5;
  payLoadPedalConfig_.BP_trigger_value =50;
  payLoadPedalConfig_.BP_amp=1;
  payLoadPedalConfig_.BP_freq=15;
  payLoadPedalConfig_.BP_trigger=0;
  payLoadPedalConfig_.G_multi = 50;
  payLoadPedalConfig_.G_window=60;
  payLoadPedalConfig_.WS_amp=1;
  payLoadPedalConfig_.WS_freq=15;
  payLoadPedalConfig_.cubic_spline_param_a_array[0] = 0;
  payLoadPedalConfig_.cubic_spline_param_a_array[1] = 0;
  payLoadPedalConfig_.cubic_spline_param_a_array[2] = 0;
  payLoadPedalConfig_.cubic_spline_param_a_array[3] = 0;
  payLoadPedalConfig_.cubic_spline_param_a_array[4] = 0;

  payLoadPedalConfig_.cubic_spline_param_b_array[0] = 0;
  payLoadPedalConfig_.cubic_spline_param_b_array[1] = 0;
  payLoadPedalConfig_.cubic_spline_param_b_array[2] = 0;
  payLoadPedalConfig_.cubic_spline_param_b_array[3] = 0;
  payLoadPedalConfig_.cubic_spline_param_b_array[4] = 0;

  payLoadPedalConfig_.PID_p_gain = 0.3;
  payLoadPedalConfig_.PID_i_gain = 50.0;
  payLoadPedalConfig_.PID_d_gain = 0.0;
  payLoadPedalConfig_.PID_velocity_feedforward_gain = 0.0;


  payLoadPedalConfig_.MPC_0th_order_gain = 1.0;
  payLoadPedalConfig_.MPC_1st_order_gain = 0.0;
  payLoadPedalConfig_.MPC_2nd_order_gain = 0.0;

  payLoadPedalConfig_.control_strategy_b = 0;

  payLoadPedalConfig_.maxGameOutput = 100;

  payLoadPedalConfig_.kf_modelNoise = 128;
  payLoadPedalConfig_.kf_modelOrder = 1;

  payLoadPedalConfig_.debug_flags_0 = 0;

  payLoadPedalConfig_.loadcell_rating = 150;

  payLoadPedalConfig_.travelAsJoystickOutput_u8 = 0;

  payLoadPedalConfig_.invertLoadcellReading_u8 = 0;

  payLoadPedalConfig_.invertMotorDirection_u8 = 0;
}




void DAP_config_st::storeConfigToEprom(DAP_config_st& config_st)
{
  if (true == config_st.payLoadHeader_.storeToEeprom)
  {
    config_st.payLoadHeader_.storeToEeprom = false; // set to false, thus at restart existing EEPROM config isn't restored to EEPROM
    EEPROM.put(0, config_st); 
    EEPROM.commit();
    Serial.println("Successfully stored config in EPROM");
  }
}

void DAP_config_st::loadConfigFromEprom(DAP_config_st& config_st)
{
  DAP_config_st local_config_st;

  EEPROM.get(0, local_config_st);
  EEPROM.commit();

  // check if version matches revision, in case, update the default config
  if (local_config_st.payLoadHeader_.version == DAP_VERSION_CONFIG)
  {
    config_st = local_config_st;
    Serial.println("Successfully loaded config from EPROM");
  }
  else
  { 
    Serial.println("Couldn't load config from EPROM due to version mismatch");
    Serial.print("Target version: ");
    Serial.println(DAP_VERSION_CONFIG);
    Serial.print("Source version: ");
    Serial.println(local_config_st.payLoadHeader_.version);

  }

}





void DAP_calculationVariables_st::updateFromConfig(DAP_config_st& config_st) {
  startPosRel = ((float)config_st.payLoadPedalConfig_.pedalStartPosition) / 100.0f;
  endPosRel = ((float)config_st.payLoadPedalConfig_.pedalEndPosition) / 100.0f;

  absFrequency = ((float)config_st.payLoadPedalConfig_.absFrequency);
  absAmplitude = ((float)config_st.payLoadPedalConfig_.absAmplitude) / 20.0f; // in kg or percent

  dampingPress = ((float)config_st.payLoadPedalConfig_.dampingPress) / 400.0f;
  RPM_max_freq = ((float)config_st.payLoadPedalConfig_.RPM_max_freq);
  RPM_min_freq = ((float)config_st.payLoadPedalConfig_.RPM_min_freq);
  RPM_AMP = ((float)config_st.payLoadPedalConfig_.RPM_AMP) / 100.0f;
  //Bite point effect;
  
  BP_trigger_value=(float)config_st.payLoadPedalConfig_.BP_trigger_value;
  BP_amp=((float)config_st.payLoadPedalConfig_.BP_amp) / 100.0f;
  BP_freq=(float)config_st.payLoadPedalConfig_.BP_freq;
  WS_amp=((float)config_st.payLoadPedalConfig_.WS_amp) / 100.0f;
  WS_freq=(float)config_st.payLoadPedalConfig_.WS_freq;
  // update force variables
  Force_Min = ((float)config_st.payLoadPedalConfig_.preloadForce);
  Force_Max = ((float)config_st.payLoadPedalConfig_.maxForce); 
  Force_Range = Force_Max - Force_Min;
  Force_Max_default=((float)config_st.payLoadPedalConfig_.maxForce); 
}

void DAP_calculationVariables_st::dynamic_update()
{
  Force_Range = Force_Max - Force_Min;
}

void DAP_calculationVariables_st::reset_maxforce()
{
  Force_Max = Force_Max_default;
}

void DAP_calculationVariables_st::updateEndstops(long newMinEndstop, long newMaxEndstop) {
  stepperPosMinEndstop = newMinEndstop;
  stepperPosMaxEndstop = newMaxEndstop;
  stepperPosEndstopRange = stepperPosMaxEndstop - stepperPosMinEndstop;

  stepperPosMin = stepperPosEndstopRange * startPosRel;
  stepperPosMax = stepperPosEndstopRange * endPosRel;
  stepperPosRange = stepperPosMax - stepperPosMin;
}

void DAP_calculationVariables_st::updateStiffness() {
  springStiffnesss = Force_Range / stepperPosRange;
  springStiffnesssInv = 1.0 / springStiffnesss;
}


