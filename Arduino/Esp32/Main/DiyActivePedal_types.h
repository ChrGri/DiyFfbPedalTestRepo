#pragma once

#include <stdint.h>

// define the payload revision
#define DAP_VERSION_CONFIG 130

// define the payload types
#define DAP_PAYLOAD_TYPE_CONFIG 100
#define DAP_PAYLOAD_TYPE_ACTION 110
#define DAP_PAYLOAD_TYPE_STATE_BASIC 120
#define DAP_PAYLOAD_TYPE_STATE_EXTENDED 130

struct payloadHeader {
  
  // structure identification via payload
  uint8_t payloadType;

  // variable to check if structure at receiver matched version from transmitter
  uint8_t version;

  // store to EEPROM flag
  uint8_t storeToEeprom;

};

struct payloadPedalAction {
  uint8_t triggerAbs_u8;
  uint8_t resetPedalPos_u8;
  uint8_t startSystemIdentification_u8;
  uint8_t returnPedalConfig_u8;
  uint8_t RPM_u8;
  uint8_t G_value;
  uint8_t WS_u8;
};


struct payloadPedalState_Basic {
  uint16_t pedalPosition_u16;
  uint16_t pedalForce_u16;
  uint16_t joystickOutput_u16;
};

struct payloadPedalState_Extended {

  unsigned long timeInMs_u32; 
  float pedalForce_raw_fl32;
  float pedalForce_filtered_fl32;
  float forceVel_est_fl32;

  // register values from servo
  int16_t servoPosition_i16;
  int16_t servoPositionTarget_i16;
  int16_t servo_voltage_0p1V;
  int16_t servo_current_percent_i16;
};

struct payloadPedalConfig {
  // configure pedal start and endpoint
  // In percent
  uint8_t pedalStartPosition;
  uint8_t pedalEndPosition;

  // configure pedal forces
  uint8_t maxForce;
  uint8_t preloadForce;
  
  // design force vs travel curve
  // In percent
  uint8_t relativeForce_p000; 
  uint8_t relativeForce_p020;
  uint8_t relativeForce_p040;
  uint8_t relativeForce_p060;
  uint8_t relativeForce_p080;
  uint8_t relativeForce_p100;

  // parameter to configure damping
  uint8_t dampingPress;
  uint8_t dampingPull;

  // configure ABS effect 
  uint8_t absFrequency; // In Hz
  uint8_t absAmplitude; // In kg/20
  uint8_t absPattern; // 0: sinewave, 1: sawtooth
  uint8_t absForceOrTarvelBit; // 0: Force, 1: travel


  // geometric properties of the pedal
  // in mm
  uint8_t lengthPedal_AC;
  uint8_t horPos_AB;
  uint8_t verPos_AB;
  uint8_t lengthPedal_CB;
  //Simulate ABS trigger
  uint8_t Simulate_ABS_trigger;
  uint8_t Simulate_ABS_value;
  // configure for RPM effect
  uint8_t RPM_max_freq; //In HZ
  uint8_t RPM_min_freq; //In HZ
  uint8_t RPM_AMP; //In Kg

  //configure for bite point
  uint8_t BP_trigger_value;
  uint8_t BP_amp;
  uint8_t BP_freq;
  uint8_t BP_trigger;
    //G force effect
  uint8_t G_multi;
  uint8_t G_window;
  //wheel slip
  uint8_t WS_amp;
  uint8_t WS_freq;
  // cubic spline parameters
  float cubic_spline_param_a_array[5];
  float cubic_spline_param_b_array[5];

  // PID parameters
  float PID_p_gain;
  float PID_i_gain;
  float PID_d_gain;
  float PID_velocity_feedforward_gain;

  // MPC settings
  float MPC_0th_order_gain;
  float MPC_1st_order_gain;
  float MPC_2nd_order_gain;

  uint8_t control_strategy_b;

  // controller settings
  uint8_t maxGameOutput;

  // Kalman filter model noise
  uint8_t kf_modelNoise;
  uint8_t kf_modelOrder;

  // debug flags, sued to enable debug output
  uint8_t debug_flags_0;

  // loadcell rating in kg / 2 --> to get value in kg, muiltiply by 2
  uint8_t loadcell_rating;

  // use loadcell or travel as joystick output
  uint8_t travelAsJoystickOutput_u8;

  // invert loadcell sign
  uint8_t invertLoadcellReading_u8;

  // invert motor direction
  uint8_t invertMotorDirection_u8;

  // spindle pitch in mm/rev
  uint8_t spindlePitch_mmPerRev_u8;

};

struct payloadFooter {
  // To check if structure is valid
  uint16_t checkSum;
};


struct DAP_actions_st {
  payloadHeader payLoadHeader_;
  payloadPedalAction payloadPedalAction_;
  payloadFooter payloadFooter_; 
};

struct DAP_state_basic_st {
  payloadHeader payLoadHeader_;
  payloadPedalState_Basic payloadPedalState_Basic_;
  payloadFooter payloadFooter_; 
};

struct DAP_state_extended_st {
  payloadHeader payLoadHeader_;
  payloadPedalState_Extended payloadPedalState_Extended_;
  payloadFooter payloadFooter_; 
};


struct DAP_config_st {

  payloadHeader payLoadHeader_;
  payloadPedalConfig payLoadPedalConfig_;
  payloadFooter payloadFooter_; 
  
  
  void initialiseDefaults();
  void initialiseDefaults_Accelerator();
  void loadConfigFromEprom(DAP_config_st& config_st);
  void storeConfigToEprom(DAP_config_st& config_st);
};


struct DAP_calculationVariables_st
{
  float springStiffnesss;
  float springStiffnesssInv;
  float Force_Min;
  float Force_Max;
  float Force_Range;
  long stepperPosMinEndstop;
  long stepperPosMaxEndstop;
  long stepperPosEndstopRange;
  float RPM_max_freq;
  float RPM_min_freq;
  float RPM_AMP;
  long stepperPosMin;
  long stepperPosMax;
  float stepperPosRange;
  float startPosRel;
  float endPosRel;
  float absFrequency;
  float absAmplitude;
  float rpm_value;
  float BP_trigger_value;
  float BP_amp;
  float BP_freq;
  float dampingPress;
  float Force_Max_default;
  float WS_amp;
  float WS_freq;

  void updateFromConfig(DAP_config_st& config_st);
  void updateEndstops(long newMinEndstop, long newMaxEndstop);
  void updateStiffness();
  void dynamic_update();
  void reset_maxforce();
};
