#pragma once

#include "DiyActivePedal_types.h"


static const long ABS_ACTIVE_TIME_PER_TRIGGER_MILLIS = 100;
static const long RPM_ACTIVE_TIME_PER_TRIGGER_MILLIS = 100;
static const long BP_ACTIVE_TIME_PER_TRIGGER_MILLIS = 100;
static const long WS_ACTIVE_TIME_PER_TRIGGER_MILLIS = 100;
static int RPM_VALUE_LAST = 0;

class ABSOscillation {
private:
  long _timeLastTriggerMillis;
  long _absTimeMillis;
  long _lastCallTimeMillis = 0;

public:
  ABSOscillation()
    : _timeLastTriggerMillis(0)
  {}

public:
  void trigger() {
    _timeLastTriggerMillis = millis();
  }
  
  void forceOffset(DAP_calculationVariables_st* calcVars_st, uint8_t absPattern, uint8_t absForceOrTarvelBit, float * absForceOffset, float * absPosOffset) {


    long timeNowMillis = millis();
    float timeSinceTrigger = (timeNowMillis - _timeLastTriggerMillis);
    float absForceOffset_local = 0;

    if (timeSinceTrigger > ABS_ACTIVE_TIME_PER_TRIGGER_MILLIS)
    {
      _absTimeMillis = 0;
      absForceOffset = 0;
    }
    else
    {
      _absTimeMillis += timeNowMillis - _lastCallTimeMillis;
      float absTimeSeconds = _absTimeMillis / 1000.0f;

      // abs amplitude
      float absAmp_fl32 = 0;
      switch (absForceOrTarvelBit) {
        case 0:
          absAmp_fl32 = calcVars_st->absAmplitude;
          break;
        case 1:
          absAmp_fl32 = calcVars_st->stepperPosRange * calcVars_st->absAmplitude / 100.;
          break;
        default:
          break;
      }


      
      switch (absPattern) {
        case 0:
          // sine wave pattern
          absForceOffset_local =  absAmp_fl32 * sin(2 * PI * calcVars_st->absFrequency * absTimeSeconds);
          break;
        case 1:
          // sawtooth pattern
          if (calcVars_st->absFrequency > 0)
          {
            absForceOffset_local = absAmp_fl32 * fmod(absTimeSeconds, 1.0 / (float)calcVars_st->absFrequency) * (float)calcVars_st->absFrequency;
          }
          break;
        default:
          break;
      }
      
      switch (absForceOrTarvelBit) {
        case 0:
          *absForceOffset = absForceOffset_local;
          *absPosOffset = 0;
          break;
        case 1:
          *absForceOffset = 0;
          *absPosOffset = absForceOffset_local;
          break;
        default:
          break;
      }
      
    }

    _lastCallTimeMillis = timeNowMillis;

    return;
    
  }
};

class RPMOscillation {
private:
  long _timeLastTriggerMillis;
  long _RPMTimeMillis;
  long _lastCallTimeMillis = 0;
  

public:
  RPMOscillation()
    : _timeLastTriggerMillis(0)
  {}
  float RPM_value =0;
  int32_t RPM_position_offset = 0;
public:
  void trigger() {
    _timeLastTriggerMillis = millis();
  }
  
  void forceOffset(DAP_calculationVariables_st* calcVars_st) {


    long timeNowMillis = millis();
    float timeSinceTrigger = (timeNowMillis - _timeLastTriggerMillis);
    float RPMForceOffset = 0;
    float RPM_max_freq = calcVars_st->RPM_max_freq;
    float RPM_min_freq = calcVars_st->RPM_min_freq;
    //float RPM_max =10;
    float RPM_amp = calcVars_st->RPM_AMP;
    if(RPM_value==0)
    {
      RPM_min_freq=0;
    }


    float RPM_freq=constrain(RPM_value*(RPM_max_freq-RPM_min_freq)/100, RPM_min_freq, RPM_max_freq);
    


    if (timeSinceTrigger > RPM_ACTIVE_TIME_PER_TRIGGER_MILLIS)
    {
      _RPMTimeMillis = 0;
      RPMForceOffset = RPM_VALUE_LAST;
    }
    else
    {
      _RPMTimeMillis += timeNowMillis - _lastCallTimeMillis;
      float RPMTimeSeconds = _RPMTimeMillis / 1000.0f;

      //RPMForceOffset = calcVars_st->absAmplitude * sin(calcVars_st->absFrequency * RPMTimeSeconds);
      RPMForceOffset = RPM_amp * sin( 2*PI* RPM_freq* RPMTimeSeconds);
    }

    _lastCallTimeMillis = timeNowMillis;
    RPM_VALUE_LAST=RPMForceOffset;
    RPM_position_offset = calcVars_st->stepperPosRange*(RPMForceOffset/calcVars_st->Force_Range);
    //return RPMForceOffset;
    

  }
};

class BitePointOscillation {
private:
  long _timeLastTriggerMillis;
  long _BiteTimeMillis;
  long _lastCallTimeMillis = 0;
  

public:
  BitePointOscillation()
    : _timeLastTriggerMillis(0)
  {}
  //float RPM_value =0;
  float BitePoint_Force_offset = 0;
public:
  void trigger() {
    _timeLastTriggerMillis = millis();
  }
  
  void forceOffset(DAP_calculationVariables_st* calcVars_st) {


    long timeNowMillis = millis();
    float timeSinceTrigger = (timeNowMillis - _timeLastTriggerMillis);
    float BitePointForceOffset = 0;
    float BP_freq = calcVars_st->BP_freq;
    //float BP_freq = 15;
    float BP_amp = calcVars_st->BP_amp;
    //float BP_amp = 2;

    if (timeSinceTrigger > BP_ACTIVE_TIME_PER_TRIGGER_MILLIS)
    {
      _BiteTimeMillis = 0;
      BitePointForceOffset = 0;
    }
    else
    {
      _BiteTimeMillis += timeNowMillis - _lastCallTimeMillis;
      float BPTimeSeconds = _BiteTimeMillis / 1000.0f;

      //RPMForceOffset = calcVars_st->absAmplitude * sin(calcVars_st->absFrequency * RPMTimeSeconds);
      BitePointForceOffset = BP_amp * sin( 2*PI* BP_freq* BPTimeSeconds);
    }
    BitePoint_Force_offset=BitePointForceOffset;
    _lastCallTimeMillis = timeNowMillis;
    //RPM_VALUE_LAST=RPMForceOffset;
    
    //return RPMForceOffset;
    

  }
};

// moving average filter:https://github.com/sebnil/Moving-Avarage-Filter--Arduino-Library-/tree/master

#define MAX_DATA_POINTS 100
class MovingAverageFilter
{
public:
  //construct without coefs
  MovingAverageFilter(unsigned int newDataPointsCount);
  int dataPointsCount;
  float process(float in);
  

private:
  float values[MAX_DATA_POINTS];
  int k; // k stores the index of the current array read to create a circular memory through the array
  
  float out;
  int i; // just a loop counter
};

MovingAverageFilter::MovingAverageFilter(unsigned int newDataPointsCount)
{
  k = 0; //initialize so that we start to write at index 0
  if (newDataPointsCount < MAX_DATA_POINTS)
    dataPointsCount = newDataPointsCount;
  else
    dataPointsCount = MAX_DATA_POINTS;

  for (i = 0; i < dataPointsCount; i++)
  {
    values[i] = 0; // fill the array with 0's
  }
}

float MovingAverageFilter::process(float in)
{
  out = 0;

  values[k] = in;
  k = (k + 1) % dataPointsCount;

  for (i = 0; i < dataPointsCount; i++)
  {
    out += values[i];
  }

  return out / dataPointsCount;
}

MovingAverageFilter movingAverageFilter(100);
// G force effect
class G_force_effect
{
  public:
  float G_value=0;
  float G_force_raw=0;
  float G_force=0;
  

  void forceOffset(DAP_calculationVariables_st* calcVars_st, uint8_t G_multi)
  {
    uint32_t Force_Range;
    float G_multiplier=((float)G_multi)/100;
    Force_Range=calcVars_st->Force_Range;
    if(G_value==-128)
    {
      G_force_raw=0;

    }
    else
    {
      G_force_raw=10*(G_value)*G_multiplier/9.8;
      //G_force_raw=constrain(G_force_raw,-1*Force_Range*0.25,Force_Range*0.25);
    }

    //apply filter
    G_force=movingAverageFilter.process(G_force_raw);
    //G_force=G_force_raw;
    
  }
};

class WSOscillation {
private:
  long _timeLastTriggerMillis;
  long _WSTimeMillis;
  long _lastCallTimeMillis = 0;
  

public:
  WSOscillation()
    : _timeLastTriggerMillis(0)
  {}
  //float RPM_value =0;
  float WS_Force_offset = 0;
public:
  void trigger() {
    _timeLastTriggerMillis = millis();
  }
  
  void forceOffset(DAP_calculationVariables_st* calcVars_st) {


    long timeNowMillis = millis();
    float timeSinceTrigger = (timeNowMillis - _timeLastTriggerMillis);
    float WSForceOffset = 0;
    float WS_freq = calcVars_st->WS_freq;
    //float BP_freq = 15;
    float WS_amp = calcVars_st->WS_amp;
    //float BP_amp = 2;

    if (timeSinceTrigger > WS_ACTIVE_TIME_PER_TRIGGER_MILLIS)
    {
      _WSTimeMillis = 0;
      WSForceOffset = 0;
    }
    else
    {
      _WSTimeMillis += timeNowMillis - _lastCallTimeMillis;
      float WSTimeSeconds = _WSTimeMillis / 1000.0f;

      //RPMForceOffset = calcVars_st->absAmplitude * sin(calcVars_st->absFrequency * RPMTimeSeconds);
      WSForceOffset = WS_amp * sin( 2*PI* WS_freq* WSTimeSeconds);
      /*if (WS_freq > 0)
      {
        //WSForceOffset = WS_amp * fmod(WSTimeSeconds, 1.0 / (float)WS_freq) * WS_freq;
        //WSForceOffset = WS_amp * (2*fmod(WSTimeSeconds, 1.0 / (float)WS_freq) * WS_freq-1);
      }
      */
            
          
    }
    WS_Force_offset=WSForceOffset;
    _lastCallTimeMillis = timeNowMillis;
    //RPM_VALUE_LAST=RPMForceOffset;
    
    //return RPMForceOffset;
    

  }
};
