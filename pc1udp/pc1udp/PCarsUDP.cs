using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PcarsUDP
{
    class PCars1_UDP
    {
        const int UDP_STREAMER_CAR_PHYSICS_HANDLER_VERSION = 1;
        const int PARTICIPANT_NAME_LENGTH_MAX = 64;
        const int PARTICIPANTS_PER_PACKET = 16;
        const int UDP_STREAMER_PARTICIPANTS_SUPPORTED = 32;
        const int TRACKNAME_LENGTH_MAX = 64;
        const int VEHICLE_NAME_LENGTH_MAX = 64;
        const int CLASS_NAME_LENGTH_MAX = 20;
        const int VEHICLES_PER_PACKET = 16;
        const int CLASSES_SUPPORTED_PER_PACKET = 60;
        const int UDP_STREAMER_TIMINGS_HANDLER_VERSION = 1;
        const int UDP_STREAMER_TIME_STATS_HANDLER_VERSION = 1;
        const int UDP_STREAMER_PARTICIPANT_VEHICLE_NAMES_HANDLER_VERSION = 1;
        const int UDP_STREAMER_GAME_STATE_HANDLER_VERSION = 1;
        const int UDP_STREAMER_WEATHER_HANDLER_VERSION = 1;
        const int UDP_STREAMER_RACE_STATE_HANDLER_VERSION = 1;
        const int UDP_STREAMER_PARTICIPANTS_HANDLER_VERSION = 1;

        private UdpClient _listener;
        private IPEndPoint _groupEP;

        public string VehicleName2;
        public string VehicleClass2;
        public string strName;

        // ParticipantInfo packet, size 16
    
        private Int16[] _WorldPosition = new short[3];      // 0
        private UInt16 _CurrentLapDistance;                 // 6
        private byte _RacePosition;                         // 8
        private byte _LapsCompleted;                        // 9 
        private byte _CurrentLap;                           // 10
        private byte _SectorNr;                             // 11
        private float _LastSectorTime;                      // 12
                                                            // 16


        // ParticipantInfoStrings, size 1347
        private UInt16 _BuildVersionNumber;
        private byte _PacketType;
        private byte[] _VehicleName = new byte[64];
        private byte[] _VehicleClass = new byte[64];
        private string _TrackLocation;
        private string _TrackVariation;
        private byte[] _Name = new byte[64];
        private float[] _FastestLapTime = new float[16];


        // ParticipantInfoStringsAdditional, size 1028
        // private UInt16 _BuildVersionNumber;
        // private byte _PacketType;
        private byte _Offset;
        // private byte[] _Name = new byte[64];



        // Telemetry packet, size 1367

        //private UInt16 _BuildVersionNumber;
        //private byte _PacketType;
        private byte _GameSessionState;
        private sbyte _ViewedParticipantIndex;
        private sbyte _NumParticipants;

        private byte _UnfilteredThrottle;
        private byte _UnfilteredBrake;
        private sbyte _UnfilteredSteering;
        private byte _UnfilteredClutch;
        private byte _RaceStateFlags;
        private byte _LapsInEvent;
        private float _BestLapTime;
        private float _LastLapTime;
        private float _CurrentTime;
        private float _SplitTimeAhead;
        private float _SplitTimeBehind;
        private float _SplitTime;
        private float _EventTimeRemaining;
        private float _PersonalFastestLapTime;
        private float _WorldFastestLapTime;
        private float _CurrentSector1Time;
        private float _CurrentSector2Time;
        private float _CurrentSector3Time;
        private float _FastestSector1Time;
        private float _FastestSector2Time;
        private float _FastestSector3Time;
        private float _PersonalFastestSector1Time;
        private float _PersonalFastestSector2Time;
        private float _PersonalFastestSector3Time;
        private float _WorldFastestSector1Time;
        private float _WorldFastestSector2Time;
        private float _WorldFastestSector3Time;
        private UInt16 _Joypad;
        private byte _HighestFlag;
        private byte _PitModeSchedule;
        private Int16 _OilTempCelsius;
        private UInt16 _OilPressureKPa;
        private Int16 _WaterTempCelsius;
        private UInt16 _WaterPressureKpa;
        private UInt16 _FuelPressureKpa;
        private byte _CarFlags;
        private byte _FuelCapacity;
        private byte _Brake;
        private byte _Throttle;
        private byte _Clutch;
        private sbyte _Steering;
        private float _FuelLevel;
        private float _Speed;
        private UInt16 _Rpm;
        private UInt16 _MaxRpm;
        private byte _GearNumGears;
        private byte _BoostAmount;
        private sbyte _EnforcedPitStopLap;
        private byte _CrashState;
        private float _OdometerKM;
        private float[] _Orientation = new float[3];
        private float[] _LocalVelocity = new float[3];
        private float[] _WorldVelocity = new float[3];
        private float[] _AngularVelocity = new float[3];
        private float[] _LocalAcceleration = new float[3];
        private float[] _WorldAcceleration = new float[3];
        private float[] _ExtentsCentre = new float[3];
        private byte[] _TyreFlags = new byte[4];
        private byte[] _Terrain = new byte[4];
        private float[] _TyreY = new float[4];
        private float[] _TyreRPS = new float[4];
        private float[] _TyreSlipSpeed = new float[4];
        private byte[] _TyreTemp = new byte[4];
        private byte[] _TyreGrip = new byte[4];
        private float[] _TyreHeightAboveGround = new float[4];
        private float[] _TyreLateralStiffness = new float[4];
        private byte[] _TyreWear = new byte[4];
        private byte[] _BrakeDamage = new byte[4];
        private byte[] _SuspensionDamage = new byte[4];
        private Int16[] _BrakeTempCelsius = new Int16[4];
        private UInt16[] _TyreTreadTemp = new UInt16[4];
        private UInt16[] _TyreLayerTemp = new UInt16[4];
        private UInt16[] _TyreCarcassTemp = new UInt16[4];
        private UInt16[] _TyreRimTemp = new UInt16[4];
        private UInt16[] _TyreInternalAirTemp = new UInt16[4];
        private float[] _WheelLocalPositionY = new float[4];
        private float[] _RideHeight = new float[4];
        private float[] _SuspensionTravel = new float[4];
        private float[] _SuspensionVelocity = new float[4];
        private UInt16[] _AirPressure = new UInt16[4];
        private float _EngineSpeed;
        private float _EngineTorque;
        private byte _AeroDamage;
        private byte _EngineDamage;

        private sbyte _AmbientTemperature;
        private sbyte _TrackTemperature;
        private byte _RainDensity;
        private sbyte _WindSpeed;
        private sbyte _WindDirectionX;
        private sbyte _WindDirectionY;

        private double[,] _ParticipantInfo = new double[56, 16]; //896    
        
        private float _TrackLength;
        private byte[] _Wings = new byte[2];
        private byte _DPad;

        public PCars1_UDP(UdpClient listen, IPEndPoint group)
        {
            _listener = listen;
            _groupEP = group;
        }         

        public void readPackets()
        {
        byte[] UDPpacket = listener.Receive(ref _groupEP);
        Stream stream = new MemoryStream(UDPpacket);
        var binaryReader = new BinaryReader(stream);

        int packetSize = UDPpacket.Length;
                       

        if (packetSize == 1347)
            {
                ReadParticipantInfoStrings(stream, binaryReader);
                //Console.WriteLine("*********.........*************ParticipantsInfoStrings received");
            }
            else if (packetSize == 1028)
            {
                ReadParticipantInfoStringsAdditional(stream, binaryReader);
                Console.WriteLine("---------**************---------ParticipantsInfoStringsAdditional received");
            }
            else if (packetSize == 1367)
            {
                ReadTelemetry(stream, binaryReader);
                //Console.WriteLine("...........................Telemetry received");
            }

        }
       

        public void ReadBase(Stream stream, BinaryReader binaryReader)
        {
            BuildVersionNumber = binaryReader.ReadUInt16();
            PacketType = binaryReader.ReadByte();
        }



        public void ReadParticipantInfoStrings(Stream stream, BinaryReader binaryReader)
        {
        BuildVersionNumber = binaryReader.ReadUInt16();
        PacketType = binaryReader.ReadByte();

        byte[] str = binaryReader.ReadBytes(64);
        int lengthOfStr = Array.IndexOf(str, (byte)0); // e.g. 4 for "clip\0"
        VehicleName2 = System.Text.UTF8Encoding.Default.GetString(str, 0, lengthOfStr);
        byte[] str2 = binaryReader.ReadBytes(64);
        int lengthOfStr2 = Array.IndexOf(str2, (byte)0); // e.g. 4 for "clip\0"
        VehicleClass2 = System.Text.UTF8Encoding.Default.GetString(str2, 0, lengthOfStr2);
        byte[] str3 = binaryReader.ReadBytes(64);
        int lengthOfStr3 = Array.IndexOf(str3, (byte)0); // e.g. 4 for "clip\0"
        TrackLocation = System.Text.UTF8Encoding.Default.GetString(str3, 0, lengthOfStr3);
        byte[] str4 = binaryReader.ReadBytes(64);
        int lengthOfStr4 = Array.IndexOf(str4, (byte)0); // e.g. 4 for "clip\0"
        TrackVariation = System.Text.UTF8Encoding.Default.GetString(str4, 0, lengthOfStr4);
        byte[] bName = binaryReader.ReadBytes(64);
        int lengthOfbName = Array.IndexOf(bName, (byte)0); // e.g. 4 for "clip\0"
        strName = System.Text.UTF8Encoding.Default.GetString(bName, 0, lengthOfbName);
                      

        for (int i = 0; i < 16; i++)
            {
                FastestLapTime[i] = (binaryReader.ReadSingle());
            }
        }
          
     
        public void ReadParticipantInfoStringsAdditional(Stream stream, BinaryReader binaryReader)
        {
            BuildVersionNumber = binaryReader.ReadUInt16();
            PacketType = binaryReader.ReadByte();
            Console.WriteLine("จจจจจจจจจจจPacket type " + PacketType);
            Offset = binaryReader.ReadByte();

            for (int i = 0; i < 16; i++)
            {
                byte[] tmpByte = binaryReader.ReadBytes(64);
                int lengthOftmpByte = Array.IndexOf(tmpByte, (byte)0); // e.g. 4 for "clip\0"
                strName = System.Text.UTF8Encoding.Default.GetString(tmpByte, 0, lengthOftmpByte);
                Console.WriteLine("'จ'จ'จ'จ'จ'จ'จ'จ'จ'จ'จ'จ'จ'จ'Additional Name " + strName);
            }                       
        }

  

        public void ReadTelemetry(Stream stream, BinaryReader binaryReader)
        {
            BuildVersionNumber = binaryReader.ReadUInt16();
            PacketType = binaryReader.ReadByte();
            GameSessionState = binaryReader.ReadByte();
            ViewedParticipantIndex = binaryReader.ReadSByte();
            NumParticipants = binaryReader.ReadSByte();
            UnfilteredThrottle = binaryReader.ReadByte();
            UnfilteredBrake = binaryReader.ReadByte();
            UnfilteredSteering = binaryReader.ReadSByte();
            UnfilteredClutch = binaryReader.ReadByte();
            RaceStateFlags = binaryReader.ReadByte();
            LapsInEvent = binaryReader.ReadByte();
            BestLapTime = binaryReader.ReadSingle();
            LastLapTime = binaryReader.ReadSingle();
            CurrentTime = binaryReader.ReadSingle();
            SplitTimeAhead = binaryReader.ReadSingle();
            SplitTimeBehind = binaryReader.ReadSingle();
            SplitTime = binaryReader.ReadSingle();
            EventTimeRemaining = binaryReader.ReadSingle();
            PersonalFastestLapTime = binaryReader.ReadSingle();
            WorldFastestLapTime = binaryReader.ReadSingle();
            CurrentSector1Time = binaryReader.ReadSingle();
            CurrentSector2Time = binaryReader.ReadSingle();
            CurrentSector3Time = binaryReader.ReadSingle();
            FastestSector1Time = binaryReader.ReadSingle();
            FastestSector2Time = binaryReader.ReadSingle();
            FastestSector3Time = binaryReader.ReadSingle();
            PersonalFastestSector1Time = binaryReader.ReadSingle();
            PersonalFastestSector2Time = binaryReader.ReadSingle();
            PersonalFastestSector3Time = binaryReader.ReadSingle();
            WorldFastestSector1Time = binaryReader.ReadSingle();
            WorldFastestSector2Time = binaryReader.ReadSingle();
            WorldFastestSector3Time = binaryReader.ReadSingle();
         
            Joypad = binaryReader.ReadUInt16();

            HighestFlag = binaryReader.ReadByte();
            PitModeSchedule = binaryReader.ReadByte();

            OilTempCelsius = binaryReader.ReadInt16();
            OilPressureKPa = binaryReader.ReadUInt16();
            WaterTempCelsius = binaryReader.ReadInt16();
            WaterPressureKpa = binaryReader.ReadUInt16();
            FuelPressureKpa = binaryReader.ReadUInt16();
            CarFlags = binaryReader.ReadByte();
            FuelCapacity = binaryReader.ReadByte();
            Brake = binaryReader.ReadByte();
            Throttle = binaryReader.ReadByte();
            Clutch = binaryReader.ReadByte();
            Steering = binaryReader.ReadSByte();
            FuelLevel = binaryReader.ReadSingle();
            Speed = binaryReader.ReadSingle();
            Rpm = binaryReader.ReadUInt16();
            MaxRpm = binaryReader.ReadUInt16();
            GearNumGears = binaryReader.ReadByte();
            BoostAmount = binaryReader.ReadByte();
            EnforcedPitStopLap = binaryReader.ReadSByte();
            CrashState = binaryReader.ReadByte();
            OdometerKM = binaryReader.ReadSingle();

            Orientation[0] = binaryReader.ReadSingle();
            Orientation[1] = binaryReader.ReadSingle();
            Orientation[2] = binaryReader.ReadSingle();

            LocalVelocity[0] = binaryReader.ReadSingle();
            LocalVelocity[1] = binaryReader.ReadSingle();
            LocalVelocity[2] = binaryReader.ReadSingle();

            WorldVelocity[0] = binaryReader.ReadSingle();
            WorldVelocity[1] = binaryReader.ReadSingle();
            WorldVelocity[2] = binaryReader.ReadSingle();

            AngularVelocity[0] = binaryReader.ReadSingle();
            AngularVelocity[1] = binaryReader.ReadSingle();
            AngularVelocity[2] = binaryReader.ReadSingle();

            LocalAcceleration[0] = binaryReader.ReadSingle();
            LocalAcceleration[1] = binaryReader.ReadSingle();
            LocalAcceleration[2] = binaryReader.ReadSingle();

            WorldAcceleration[0] = binaryReader.ReadSingle();
            WorldAcceleration[1] = binaryReader.ReadSingle();
            WorldAcceleration[2] = binaryReader.ReadSingle();

            ExtentsCentre[0] = binaryReader.ReadSingle();
            ExtentsCentre[1] = binaryReader.ReadSingle();
            ExtentsCentre[2] = binaryReader.ReadSingle();

            TyreFlags[0] = binaryReader.ReadByte();
            TyreFlags[1] = binaryReader.ReadByte();
            TyreFlags[2] = binaryReader.ReadByte();
            TyreFlags[3] = binaryReader.ReadByte();

            Terrain[0] = binaryReader.ReadByte();
            Terrain[1] = binaryReader.ReadByte();
            Terrain[2] = binaryReader.ReadByte();
            Terrain[3] = binaryReader.ReadByte();

            TyreY[0] = binaryReader.ReadSingle();
            TyreY[1] = binaryReader.ReadSingle();
            TyreY[2] = binaryReader.ReadSingle();
            TyreY[3] = binaryReader.ReadSingle();

            TyreRPS[0] = binaryReader.ReadSingle();
            TyreRPS[1] = binaryReader.ReadSingle();
            TyreRPS[2] = binaryReader.ReadSingle();
            TyreRPS[3] = binaryReader.ReadSingle();

            TyreSlipSpeed[0] = binaryReader.ReadSingle();
            TyreSlipSpeed[1] = binaryReader.ReadSingle();
            TyreSlipSpeed[2] = binaryReader.ReadSingle();
            TyreSlipSpeed[3] = binaryReader.ReadSingle();

            TyreTemp[0] = binaryReader.ReadByte();
            TyreTemp[1] = binaryReader.ReadByte();
            TyreTemp[2] = binaryReader.ReadByte();
            TyreTemp[3] = binaryReader.ReadByte();

            TyreGrip[0] = binaryReader.ReadByte();
            TyreGrip[1] = binaryReader.ReadByte();
            TyreGrip[2] = binaryReader.ReadByte();
            TyreGrip[3] = binaryReader.ReadByte();

            TyreHeightAboveGround[0] = binaryReader.ReadSingle();
            TyreHeightAboveGround[1] = binaryReader.ReadSingle();
            TyreHeightAboveGround[2] = binaryReader.ReadSingle();
            TyreHeightAboveGround[3] = binaryReader.ReadSingle();

            TyreLateralStiffness[0] = binaryReader.ReadSingle();
            TyreLateralStiffness[1] = binaryReader.ReadSingle();
            TyreLateralStiffness[2] = binaryReader.ReadSingle();
            TyreLateralStiffness[3] = binaryReader.ReadSingle();

            TyreWear[0] = binaryReader.ReadByte();
            TyreWear[1] = binaryReader.ReadByte();
            TyreWear[2] = binaryReader.ReadByte();
            TyreWear[3] = binaryReader.ReadByte();

            BrakeDamage[0] = binaryReader.ReadByte();
            BrakeDamage[1] = binaryReader.ReadByte();
            BrakeDamage[2] = binaryReader.ReadByte();
            BrakeDamage[3] = binaryReader.ReadByte();

            SuspensionDamage[0] = binaryReader.ReadByte();
            SuspensionDamage[1] = binaryReader.ReadByte();
            SuspensionDamage[2] = binaryReader.ReadByte();
            SuspensionDamage[3] = binaryReader.ReadByte();

            BrakeTempCelsius[0] = binaryReader.ReadInt16();
            BrakeTempCelsius[1] = binaryReader.ReadInt16();
            BrakeTempCelsius[2] = binaryReader.ReadInt16();
            BrakeTempCelsius[3] = binaryReader.ReadInt16();

            TyreTreadTemp[0] = binaryReader.ReadUInt16();
            TyreTreadTemp[1] = binaryReader.ReadUInt16();
            TyreTreadTemp[2] = binaryReader.ReadUInt16();
            TyreTreadTemp[3] = binaryReader.ReadUInt16();

            TyreLayerTemp[0] = binaryReader.ReadUInt16();
            TyreLayerTemp[1] = binaryReader.ReadUInt16();
            TyreLayerTemp[2] = binaryReader.ReadUInt16();
            TyreLayerTemp[3] = binaryReader.ReadUInt16();

            TyreCarcassTemp[0] = binaryReader.ReadUInt16();
            TyreCarcassTemp[1] = binaryReader.ReadUInt16();
            TyreCarcassTemp[2] = binaryReader.ReadUInt16();
            TyreCarcassTemp[3] = binaryReader.ReadUInt16();

            TyreRimTemp[0] = binaryReader.ReadUInt16();
            TyreRimTemp[1] = binaryReader.ReadUInt16();
            TyreRimTemp[2] = binaryReader.ReadUInt16();
            TyreRimTemp[3] = binaryReader.ReadUInt16();

            TyreInternalAirTemp[0] = binaryReader.ReadUInt16();
            TyreInternalAirTemp[1] = binaryReader.ReadUInt16();
            TyreInternalAirTemp[2] = binaryReader.ReadUInt16();
            TyreInternalAirTemp[3] = binaryReader.ReadUInt16();

            WheelLocalPositionY[0] = binaryReader.ReadSingle();
            WheelLocalPositionY[1] = binaryReader.ReadSingle();
            WheelLocalPositionY[2] = binaryReader.ReadSingle();
            WheelLocalPositionY[3] = binaryReader.ReadSingle();

            RideHeight[0] = binaryReader.ReadSingle();
            RideHeight[1] = binaryReader.ReadSingle();
            RideHeight[2] = binaryReader.ReadSingle();
            RideHeight[3] = binaryReader.ReadSingle();

            SuspensionTravel[0] = binaryReader.ReadSingle();
            SuspensionTravel[1] = binaryReader.ReadSingle();
            SuspensionTravel[2] = binaryReader.ReadSingle();
            SuspensionTravel[3] = binaryReader.ReadSingle();

            SuspensionVelocity[0] = binaryReader.ReadSingle();
            SuspensionVelocity[1] = binaryReader.ReadSingle();
            SuspensionVelocity[2] = binaryReader.ReadSingle();
            SuspensionVelocity[3] = binaryReader.ReadSingle();

            AirPressure[0] = binaryReader.ReadUInt16();
            AirPressure[1] = binaryReader.ReadUInt16();
            AirPressure[2] = binaryReader.ReadUInt16();
            AirPressure[3] = binaryReader.ReadUInt16();

            EngineSpeed = binaryReader.ReadSingle();
            EngineTorque = binaryReader.ReadSingle();
            AeroDamage = binaryReader.ReadByte();
            EngineDamage = binaryReader.ReadByte();

            AmbientTemperature = binaryReader.ReadSByte();
            TrackTemperature = binaryReader.ReadSByte();
            RainDensity = binaryReader.ReadByte();
            WindSpeed = binaryReader.ReadSByte();
            WindDirectionX = binaryReader.ReadSByte();
            WindDirectionY = binaryReader.ReadSByte();

            for (int i = 0; i < 56; i++)
            {
                ParticipantInfo[i, 0] = Convert.ToDouble(binaryReader.ReadInt16());  //WorldPosition 
                ParticipantInfo[i, 1] = Convert.ToDouble(binaryReader.ReadInt16());  //WorldPosition
                ParticipantInfo[i, 2] = Convert.ToDouble(binaryReader.ReadInt16());  //WorldPosition
                ParticipantInfo[i, 3] = Convert.ToDouble(binaryReader.ReadUInt16());  //sCurrentLapDistance
                ParticipantInfo[i, 4] = Convert.ToDouble(binaryReader.ReadByte()) - 128;  //sRacePosition
                ParticipantInfo[i, 5] = Convert.ToDouble(binaryReader.ReadByte());  //sLapsCompleted
                ParticipantInfo[i, 6] = Convert.ToDouble(binaryReader.ReadByte());  //sCurrentLap
                byte Sector_ALL = binaryReader.ReadByte();
                var Sector_Extracted = Sector_ALL & 7;
				ParticipantInfo[i, 7] = Convert.ToDouble(Sector_Extracted);   //sSector
                ParticipantInfo[i, 8] = Convert.ToDouble(binaryReader.ReadSingle());  //sLastSectorTime
            }


            TrackLength = binaryReader.ReadSingle();
            //Console.WriteLine("Tracklength from PCarsUDP: " + TrackLength);
            Wings[0] = binaryReader.ReadByte();
            Wings[1] = binaryReader.ReadByte();
            DPad = binaryReader.ReadByte();

        }

            


        public void close_UDP_Connection()
        {
            listener.Close();
        }

        public UdpClient listener
        {
            get
            {
                return _listener;
            }
            set
            {
                _listener = value;
            }
        }

        public IPEndPoint groupEP
        {
            get
            {
                return _groupEP;
            }
            set
            {
                _groupEP = value;
            }
        }

        public Int16[] WorldPosition
        {
            get
            {
                return _WorldPosition;
            }
            set
            {
                _WorldPosition = value;
            }
        }

       
      

        public byte PacketType
        {
            get
            {
                return _PacketType;
            }
            set
            {
                _PacketType = value;
            }
        }

        public sbyte ViewedParticipantIndex
        {
            get
            {
                return _ViewedParticipantIndex;
            }
            set
            {
                _ViewedParticipantIndex = value;
            }
        }

        public byte UnfilteredThrottle
        {
            get
            {
                return _UnfilteredThrottle;
            }
            set
            {
                _UnfilteredThrottle = value;
            }
        }

        public byte UnfilteredBrake
        {
            get
            {
                return _UnfilteredBrake;
            }
            set
            {
                _UnfilteredBrake = value;
            }
        }

        public sbyte UnfilteredSteering
        {
            get
            {
                return _UnfilteredSteering;
            }
            set
            {
                _UnfilteredSteering = value;
            }
        }

        public byte UnfilteredClutch
        {
            get
            {
                return _UnfilteredClutch;
            }
            set
            {
                _UnfilteredClutch = value;
            }
        }

        public byte CarFlags
        {
            get
            {
                return _CarFlags;
            }
            set
            {
                _CarFlags = value;
            }
        }

        public Int16 OilTempCelsius
        {
            get
            {
                return _OilTempCelsius;
            }
            set
            {
                _OilTempCelsius = value;
            }
        }

        public UInt16 OilPressureKPa
        {
            get
            {
                return _OilPressureKPa;
            }
            set
            {
                _OilPressureKPa = value;
            }
        }

        public Int16 WaterTempCelsius
        {
            get
            {
                return _WaterTempCelsius;
            }
            set
            {
                _WaterTempCelsius = value;
            }
        }

        public UInt16 WaterPressureKpa
        {
            get
            {
                return _WaterPressureKpa;
            }
            set
            {
                _WaterPressureKpa = value;
            }
        }

        public UInt16 FuelPressureKpa
        {
            get
            {
                return _FuelPressureKpa;
            }
            set
            {
                _FuelPressureKpa = value;
            }
        }

        public byte FuelCapacity
        {
            get
            {
                return _FuelCapacity;
            }
            set
            {
                _FuelCapacity = value;
            }
        }

        public byte Brake
        {
            get
            {
                return _Brake;
            }
            set
            {
                _Brake = value;
            }
        }

        public byte Throttle
        {
            get
            {
                return _Throttle;
            }
            set
            {
                _Throttle = value;
            }
        }

        public byte Clutch
        {
            get
            {
                return _Clutch;
            }
            set
            {
                _Clutch = value;
            }
        }

        public float FuelLevel
        {
            get
            {
                return _FuelLevel;
            }
            set
            {
                _FuelLevel = value;
            }
        }

        public float Speed
        {
            get
            {
                return _Speed;
            }
            set
            {
                _Speed = value;
            }
        }

        public UInt16 Rpm
        {
            get
            {
                return _Rpm;
            }
            set
            {
                _Rpm = value;
            }
        }

        public UInt16 MaxRpm
        {
            get
            {
                return _MaxRpm;
            }
            set
            {
                _MaxRpm = value;
            }
        }

        public sbyte Steering
        {
            get
            {
                return _Steering;
            }
            set
            {
                _Steering = value;
            }
        }

        public byte GearNumGears
        {
            get
            {
                return _GearNumGears;
            }
            set
            {
                _GearNumGears = value;
            }
        }

        public byte BoostAmount
        {
            get
            {
                return _BoostAmount;
            }
            set
            {
                _BoostAmount = value;
            }
        }

        public byte CrashState
        {
            get
            {
                return _CrashState;
            }
            set
            {
                _CrashState = value;
            }
        }

        public float OdometerKM
        {
            get
            {
                return _OdometerKM;
            }
            set
            {
                _OdometerKM = value;
            }
        }

        public float[] Orientation
        {
            get
            {
                return _Orientation;
            }
            set
            {
                _Orientation = value;
            }
        }

        public float[] LocalVelocity
        {
            get
            {
                return _LocalVelocity;
            }
            set
            {
                _LocalVelocity = value;
            }
        }

        public float[] WorldVelocity
        {
            get
            {
                return _WorldVelocity;
            }
            set
            {
                _WorldVelocity = value;
            }
        }

        public float[] AngularVelocity
        {
            get
            {
                return _AngularVelocity;
            }
            set
            {
                _AngularVelocity = value;
            }
        }

        public float[] LocalAcceleration
        {
            get
            {
                return _LocalAcceleration;
            }
            set
            {
                _LocalAcceleration = value;
            }
        }

        public float[] WorldAcceleration
        {
            get
            {
                return _WorldAcceleration;
            }
            set
            {
                _WorldAcceleration = value;
            }
        }

        public float[] ExtentsCentre
        {
            get
            {
                return _ExtentsCentre;
            }
            set
            {
                _ExtentsCentre = value;
            }
        }

        public byte[] TyreFlags
        {
            get
            {
                return _TyreFlags;
            }
            set
            {
                _TyreFlags = value;
            }
        }

        public byte[] Terrain
        {
            get
            {
                return _Terrain;
            }
            set
            {
                _Terrain = value;
            }
        }

        public float[] TyreY
        {
            get
            {
                return _TyreY;
            }
            set
            {
                _TyreY = value;
            }
        }

        public float[] TyreRPS
        {
            get
            {
                return _TyreRPS;
            }
            set
            {
                _TyreRPS = value;
            }
        }

        public byte[] TyreTemp
        {
            get
            {
                return _TyreTemp;
            }
            set
            {
                _TyreTemp = value;
            }
        }

        public float[] TyreHeightAboveGround
        {
            get
            {
                return _TyreHeightAboveGround;
            }
            set
            {
                _TyreHeightAboveGround = value;
            }
        }

        public byte[] TyreWear
        {
            get
            {
                return _TyreWear;
            }
            set
            {
                _TyreWear = value;
            }
        }

        public byte[] BrakeDamage
        {
            get
            {
                return _BrakeDamage;
            }
            set
            {
                _BrakeDamage = value;
            }
        }

        public byte[] SuspensionDamage
        {
            get
            {
                return _SuspensionDamage;
            }
            set
            {
                _SuspensionDamage = value;
            }
        }

        public Int16[] BrakeTempCelsius
        {
            get
            {
                return _BrakeTempCelsius;
            }
            set
            {
                _BrakeTempCelsius = value;
            }
        }

        public UInt16[] TyreTreadTemp
        {
            get
            {
                return _TyreTreadTemp;
            }
            set
            {
                _TyreTreadTemp = value;
            }
        }

        public UInt16[] TyreLayerTemp
        {
            get
            {
                return _TyreLayerTemp;
            }
            set
            {
                _TyreLayerTemp = value;
            }
        }

        public UInt16[] TyreCarcassTemp
        {
            get
            {
                return _TyreCarcassTemp;
            }
            set
            {
                _TyreCarcassTemp = value;
            }
        }

        public UInt16[] TyreRimTemp
        {
            get
            {
                return _TyreRimTemp;
            }
            set
            {
                _TyreRimTemp = value;
            }
        }

        public UInt16[] TyreInternalAirTemp
        {
            get
            {
                return _TyreInternalAirTemp;
            }
            set
            {
                _TyreInternalAirTemp = value;
            }
        }

        public float[] TyreLateralStiffness
        {
            get
            {
                return _TyreLateralStiffness;
            }
            set
            {
                _TyreLateralStiffness = value;
            }
        }

        public float[] WheelLocalPositionY
        {
            get
            {
                return _WheelLocalPositionY;
            }
            set
            {
                _WheelLocalPositionY = value;
            }
        }

        public float[] RideHeight
        {
            get
            {
                return _RideHeight;
            }
            set
            {
                _RideHeight = value;
            }
        }

        public float[] SuspensionTravel
        {
            get
            {
                return _SuspensionTravel;
            }
            set
            {
                _SuspensionTravel = value;
            }
        }

        public float[] SuspensionVelocity
        {
            get
            {
                return _SuspensionVelocity;
            }
            set
            {
                _SuspensionVelocity = value;
            }
        }

        public UInt16[] AirPressure
        {
            get
            {
                return _AirPressure;
            }
            set
            {
                _AirPressure = value;
            }
        }

        public float EngineSpeed
        {
            get
            {
                return _EngineSpeed;
            }
            set
            {
                _EngineSpeed = value;
            }
        }

        public float EngineTorque
        {
            get
            {
                return _EngineTorque;
            }
            set
            {
                _EngineTorque = value;
            }
        }

        public byte[] Wings
        {
            get
            {
                return _Wings;
            }
            set
            {
                _Wings = value;
            }
        }

        public byte AeroDamage
        {
            get
            {
                return _AeroDamage;
            }
            set
            {
                _AeroDamage = value;
            }
        }

        public byte EngineDamage
        {
            get
            {
                return _EngineDamage;
            }
            set
            {
                _EngineDamage = value;
            }
        }

        public UInt16 Joypad
        {
            get
            {
                return _Joypad;
            }
            set
            {
                _Joypad = value;
            }
        }

        public byte DPad
        {
            get
            {
                return _DPad;
            }
            set
            {
                _DPad = value;
            }
        }


        // RaceData

        public float BestLapTime
        {
            get
            {
                return _BestLapTime;
            }
            set
            {
                _BestLapTime = value;
            }
        }

        public float LastLapTime
        {
            get
            {
                return _LastLapTime;
            }
            set
            {
                _LastLapTime = value;
            }
        }

        public float CurrentTime
        {
            get
            {
                return _CurrentTime;
            }
            set
            {
                _CurrentTime = value;
            }
        }

        public float CurrentSector1Time
        {
            get
            {
                return _CurrentSector1Time;
            }
            set
            {
                _CurrentSector1Time = value;
            }
        }

        public float CurrentSector2Time
        {
            get
            {
                return _CurrentSector2Time;
            }
            set
            {
                _CurrentSector2Time = value;
            }
        }


        public float CurrentSector3Time
        {
            get
            {
                return _CurrentSector3Time;
            }
            set
            {
                _CurrentSector3Time = value;
            }
        }

        public float FastestSector1Time
        {
            get
            {
                return _FastestSector1Time;
            }
            set
            {
                _FastestSector1Time = value;
            }
        }

        public float FastestSector2Time
        {
            get
            {
                return _FastestSector2Time;
            }
            set
            {
                _FastestSector2Time = value;
            }
        }

        public float FastestSector3Time
        {
            get
            {
                return _FastestSector3Time;
            }
            set
            {
                _FastestSector3Time = value;
            }
        }

        public float WorldFastestLapTime
        {
            get
            {
                return _WorldFastestLapTime;
            }
            set
            {
                _WorldFastestLapTime = value;
            }
        }

        public float PersonalFastestLapTime
        {
            get
            {
                return _PersonalFastestLapTime;
            }
            set
            {
                _PersonalFastestLapTime = value;
            }
        }

        public float PersonalFastestSector1Time
        {
            get
            {
                return _PersonalFastestSector1Time;
            }
            set
            {
                _PersonalFastestSector1Time = value;
            }
        }

        public float PersonalFastestSector2Time
        {
            get
            {
                return _PersonalFastestSector2Time;
            }
            set
            {
                _PersonalFastestSector2Time = value;
            }
        }

        public float PersonalFastestSector3Time
        {
            get
            {
                return _PersonalFastestSector3Time;
            }
            set
            {
                _PersonalFastestSector3Time = value;
            }
        }

        public float WorldFastestSector1Time
        {
            get
            {
                return _WorldFastestSector1Time;
            }
            set
            {
                _WorldFastestSector1Time = value;
            }
        }

        public float WorldFastestSector2Time
        {
            get
            {
                return _WorldFastestSector2Time;
            }
            set
            {
                _WorldFastestSector2Time = value;
            }
        }

        public float WorldFastestSector3Time
        {
            get
            {
                return _WorldFastestSector3Time;
            }
            set
            {
                _WorldFastestSector3Time = value;
            }
        }

        public byte HighestFlag
        {
            get
            {
                return _HighestFlag;
            }
            set
            {
                _HighestFlag = value;
            }
        }

        public byte PitModeSchedule
        {
            get
            {
                return _PitModeSchedule;
            }
            set
            {
                _PitModeSchedule = value;
            }
        }

        public float TrackLength
        {
            get
            {
                return _TrackLength;
            }
            set
            {
                _TrackLength = value;
            }
        }

        public string TrackLocation
        {
            get
            {
                return _TrackLocation;
            }
            set
            {
                _TrackLocation = value;
            }
        }

        public string TrackVariation
        {
            get
            {
                return _TrackVariation;
            }
            set
            {
                _TrackVariation = value;
            }
        }

        

        public float[] TyreSlipSpeed
        {
            get
            {
                return _TyreSlipSpeed;
            }
            set
            {
                _TyreSlipSpeed = value;
            }
        }

        public byte[] TyreGrip
        {
            get
            {
                return _TyreGrip;
            }
            set
            {
                _TyreGrip = value;
            }
        }

        public float[] FastestLapTime
        {
            get
            {
                return _FastestLapTime;
            }
            set
            {
                _FastestLapTime = value;
            }
        }

        public byte Offset
        {
            get
            {
                return _Offset;
            }
            set
            {
                _Offset = value;
            }

        }

        public byte RaceStateFlags
        {
            get
            {
                return _RaceStateFlags;
            }
            set
            {
                _RaceStateFlags = value;
            }

        }

        public byte LapsInEvent
        {
            get
            {
                return _LapsInEvent;
            }
            set
            {
                _LapsInEvent = value;
            }

        }

     
        public sbyte EnforcedPitStopLap
        {
            get
            {
                return _EnforcedPitStopLap;
            }
            set
            {
                _EnforcedPitStopLap = value;
            }
        }


        

        public byte[] Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }




        // Timings
        public sbyte NumParticipants
        {
            get
            {
                return _NumParticipants;
            }
            set
            {
                _NumParticipants = value;
            }
        }

     

        public float EventTimeRemaining
        {
            get
            {
                return _EventTimeRemaining;
            }
            set
            {
                _EventTimeRemaining = value;
            }
        }

        public float SplitTimeAhead
        {
            get
            {
                return _SplitTimeAhead;
            }
            set
            {
                _SplitTimeAhead = value;
            }
        }

        public float SplitTimeBehind
        {
            get
            {
                return _SplitTimeBehind;
            }
            set
            {
                _SplitTimeBehind = value;
            }
        }

        public float SplitTime
        {
            get
            {
                return _SplitTime;
            }
            set
            {
                _SplitTime = value;
            }
        }

        public double[,] ParticipantInfo
        {
            get
            {
                return _ParticipantInfo;
            }
            set
            {
                _ParticipantInfo = value;
            }
        }

        // GameState
        public UInt16 BuildVersionNumber
        {
            get
            {
                return _BuildVersionNumber;
            }
            set
            {
                _BuildVersionNumber = value;
            }
        }

        public byte GameSessionState
        {
            get
            {
                return _GameSessionState;
            }
            set
            {
                _GameSessionState = value;
            }
        }

        public byte SectorNr
        {
            get
            {
                return _SectorNr;
            }
            set
            {
                _SectorNr = value;
            }
        }

        public sbyte AmbientTemperature
        {
            get
            {
                return _AmbientTemperature;
            }
            set
            {
                _AmbientTemperature = value;
            }
        }

        public sbyte TrackTemperature
        {
            get
            {
                return _TrackTemperature;
            }
            set
            {
                _TrackTemperature = value;
            }
        }

        public byte RainDensity
        {
            get
            {
                return _RainDensity;
            }
            set
            {
                _RainDensity = value;
            }
        }

      
        public sbyte WindSpeed
        {
            get
            {
                return _WindSpeed;
            }
            set
            {
                _WindSpeed = value;
            }
        }

        public sbyte WindDirectionX
        {
            get
            {
                return _WindDirectionX;
            }
            set
            {
                _WindDirectionX = value;
            }
        }

        public sbyte WindDirectionY
        {
            get
            {
                return _WindDirectionY;
            }
            set
            {
                _WindDirectionY = value;
            }
        }

    

        // VehicleInfo

     
        public byte [] VehicleClass
        {
            get
            {
                return _VehicleClass;
            }
            set
            {
                _VehicleClass = value;
            }
        }

        public byte[] VehicleName
        {
            get
            {
                return _VehicleName;
            }
            set
            {
                _VehicleName = value;
            }
        }
     


    }


}