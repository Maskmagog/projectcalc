using PcarsUDP;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices;

namespace pc2udp
{
    class Program
    {
        // Declare variables to send to db
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        private static string Name = "";
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        private static string OldName = "";
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        private static string TrackLocation = "";
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        private static string TrackVariation = "";
        private static double LastLapTimeSec = 0;
        private static double LastSector1TimeSec = 0;
        private static double LastSector2TimeSec = 0;
        private static double LastSector3TimeSec = 0;
        private static double FastestSector1TimeSec = 0;
        private static double FastestSector2TimeSec = 0;
        private static double FastestSector3TimeSec = 0;
        private static Int16 TrackTemp;
        private static Int16 AmbTemp;
        private static double PreviousSector = 1;
        private static string SessionLapRecord = "N";
        private static double OldFastestLapTimeSec = 99999999;
        private static double FastestLapTimeSec = 0;
        private static double dbLapRecord = 0;
        private static string AllTimeRecord = "N";
        public static string VehicleName;
        public static string strSessionMode;
        public static char CurrGameState;
        public static char CurrSessionState;
        public static double RaceState;
        public static string strRaceMode;
        public static char CurrRaceState;
        public static string CurrentValidLap;
        public static string LastLapValid;
        public static string FullTrackLocation;
        public static double RainDensity;
        public static string OldFullTrackLocation = "Unknown";
        public static string OldVehicleName = "Unknown";
        public static double OldLapTimeSec = 0;

        public static void resetValues()
        {
            LastLapTimeSec = 0;
            LastSector1TimeSec = 0;
            LastSector2TimeSec = 0;
            LastSector3TimeSec = 0;
            FastestSector1TimeSec = 0;
            FastestSector2TimeSec = 0;
            FastestSector3TimeSec = 0;
            PreviousSector = 1;
            OldFastestLapTimeSec = 99999999;
            dbLapRecord = 0;
            SessionLapRecord = "N";
            AllTimeRecord = "N";
            TrackLocation = "";
            FullTrackLocation = "";
            VehicleName = "";
        }

        //************************************
        // Inserts new laptimes into database
        //************************************
        public static void dbupdate()
        {
            // DATABASE CONNECTION
            string connStr = "server=127.0.0.1;user=pcars;database=pcarsdb;port=3306;password=PG3Dnq4m2BVFaaLC"; // Change this to your values. 127.0.0.1 = localhost
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                // Open connection to db
                conn.Open();

                // INSERT
                var commandText = "INSERT INTO laptimes(gamertag, track, vehicle, vehicleclass, laptime, sector1, sector2, sector3, tracktemp, ambtemp, raindensity, sessionmode, validlap, lapdate, platform, setup, controller, camera)" + " VALUES(?gamertag, ?track, ?vehicle, ?vehicleclass, ?laptime, ?sector1, ?sector2, ?sector3, ?tracktemp, ?ambtemp, ?raindensity, ?sessionmode, ?validlap, ?lapdate, ?platform, ?setup, ?controller, ?camera)";
                var command = new MySqlCommand(commandText, conn);
                command.Parameters.Add("?gamertag", MySqlDbType.VarChar, 64).Value = Name;
                command.Parameters.Add("?track", MySqlDbType.VarChar, 64).Value = FullTrackLocation;
                command.Parameters.Add("?vehicle", MySqlDbType.VarChar, 64).Value = VehicleName;
                command.Parameters.Add("?vehicleclass", MySqlDbType.VarChar, 64).Value = "Class"; // Not functioning right now
                command.Parameters.Add("?laptime", MySqlDbType.Double).Value = LastLapTimeSec;
                Console.WriteLine("----Session lap record? " + SessionLapRecord);
                Console.WriteLine(Name + FullTrackLocation + VehicleName + LastLapTimeSec + LastLapValid);
                if (SessionLapRecord == "Y" && LastLapValid == "Y") // LastSectorTimes is sometimes wrong (not stored directly by game). If it's a session record, use FastestSectorTimes instead.
                {
                    command.Parameters.Add("?sector1", MySqlDbType.Double).Value = FastestSector1TimeSec;
                    command.Parameters.Add("?sector2", MySqlDbType.Double).Value = FastestSector2TimeSec;
                    command.Parameters.Add("?sector3", MySqlDbType.Double).Value = FastestSector3TimeSec;
                }
                else
                {
                    command.Parameters.Add("?sector1", MySqlDbType.Double).Value = LastSector1TimeSec;
                    command.Parameters.Add("?sector2", MySqlDbType.Double).Value = LastSector2TimeSec;
                    command.Parameters.Add("?sector3", MySqlDbType.Double).Value = LastSector3TimeSec;
                }
                command.Parameters.Add("?tracktemp", MySqlDbType.Int16).Value = TrackTemp;
                command.Parameters.Add("?ambtemp", MySqlDbType.Int16).Value = AmbTemp;
                command.Parameters.Add("?raindensity", MySqlDbType.Double).Value = RainDensity;
                command.Parameters.Add("?sessionmode", MySqlDbType.VarChar, 32).Value = strSessionMode;
                command.Parameters.Add("?validlap", MySqlDbType.VarChar, 1).Value = LastLapValid;
                command.Parameters.Add("?lapdate", MySqlDbType.DateTime).Value = DateTime.Now;
                command.Parameters.Add("?platform", MySqlDbType.VarChar, 3).Value = "XB1";
                command.Parameters.Add("?setup", MySqlDbType.VarChar, 8).Value = "Default";
                command.Parameters.Add("?controller", MySqlDbType.VarChar, 8).Value = "Wheel";
                command.Parameters.Add("?camera", MySqlDbType.VarChar, 8).Value = "In-car";
                command.ExecuteNonQuery();
                Console.WriteLine("***********NEW LAPTIME ADDED TO DATABASE***************");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // Close connection to db
            conn.Close();
            Console.WriteLine("Done.");
        }

        //************************************
        // Getting records from the database
        //************************************
        public static void dbfetchrecord()
        {
            // DATABASE CONNECTION
            string connStr = "server=127.0.0.1;user=pcars;database=pcarsdb;port=3306;password=PG3Dnq4m2BVFaaLC"; // Change this to your values. 127.0.0.1 = localhost
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                // Open connection to db
                conn.Open();

                // Perform database operations, TODO change to parameterized
                string commandText = "SELECT laptime FROM laptimes WHERE gamertag='" + Name + "' AND vehicle='" + VehicleName + "' AND track='" + FullTrackLocation + "' ORDER BY laptime ASC LIMIT 1";

                MySqlCommand command = new MySqlCommand(commandText, conn);
                string strdbLapRecord = command.ExecuteScalar().ToString();
                dbLapRecord = Convert.ToDouble(strdbLapRecord);
                if (dbLapRecord <= 0) { dbLapRecord = 99999999999; }
                //Console.WriteLine("-------strdbLapRecord is " + strdbLapRecord);
                Console.WriteLine("--------dbLapRecord is " + dbLapRecord);
            }
            catch (Exception ex)
            {
                Console.WriteLine("No previous record found.");
            }

            // Close connection to db
            conn.Close();
            Console.WriteLine("Done.");
        }
        //************************************
        // Inserts your username into database
        //************************************
        public static void dbUsername()
        {
            // DATABASE CONNECTION
            string connStr = "server=127.0.0.1;user=pcars;database=pcarsdb;port=3306;password=PG3Dnq4m2BVFaaLC"; // Change this to your values. 127.0.0.1 = localhost
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                // Open connection to db
                conn.Open();

                var cmdUser = "UPDATE user SET username = '" + Name + "' WHERE id=1";
                Console.WriteLine("cmdUser " + cmdUser);
                var command = new MySqlCommand(cmdUser, conn);

                command.ExecuteNonQuery();
                Console.WriteLine("*******NEW USER ADDED TO DATABASE*******");
                OldName = Name;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // Close connection to db
            conn.Close();
            Console.WriteLine("Done.");
        }

        //************************************
        // Inserts new car and track into db
        //************************************
        public static void dbCurrentCarTrack()
        {
            // DATABASE CONNECTION
            string connStr = "server=127.0.0.1;user=pcars;database=pcarsdb;port=3306;password=PG3Dnq4m2BVFaaLC"; // Change this to your values. 127.0.0.1 = localhost
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL... adding car and track");
                // Open connection to db
                conn.Open();

                var cmdUser = "UPDATE cartrackdb SET currenttrack = '" + FullTrackLocation + "', currentvehicle = '" + VehicleName + "' WHERE id=1"; //Only 1 row in this db, that changes when player changes car/track in-game
                Console.WriteLine("cmdUser " + cmdUser);
                var command = new MySqlCommand(cmdUser, conn);

                command.ExecuteNonQuery();
                Console.WriteLine("******NEW CAR - TRACK ADDED TO DATABASE*******");
                OldFullTrackLocation = FullTrackLocation;
                OldVehicleName = VehicleName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // Close connection to db
            conn.Close();
            Console.WriteLine("Done.");
        }

        static void Main(string[] args)
        {
            //  UDP
            UdpClient listener = new UdpClient(5606);                       //Create a UDPClient object
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 5606);       //Start recieving data from any IP listening on port 5606 (port for PCARS2)

            PCars2_UDP uDP = new PCars2_UDP(listener, groupEP);             //Create an UDP object that will retrieve telemetry values from in game.

            Console.WriteLine("wait for udp");

            // Define variables
            int gameState = uDP.GameState3 & 7;
            int sessionState = uDP.GameState3 >> 4;

            // MAIN LOOP
            while (true)
            {
                //Thread.Sleep(20);
                uDP.readPackets(); //Read Packets ever loop iteration


                /* Do some testing to detect session restarts. maybe a function to reset values */
                /*if (strRaceMode == "Not Started") {
                    resetValues();
                    Console.WriteLine("Session restart/new session? Values reset.");
                } */

                //****************************
                //Track Location (if not null)
                //****************************
                if (uDP._TrackLocation != null)
                {
                    TrackLocation = uDP.TrackLocation;
                    //Console.WriteLine("Track location is " + TrackLocation);
                    string TrackLocation2 = TrackLocation.Replace("_", " ");

                    //Track Variation
                    TrackVariation = uDP.TrackVariation;
                    //Console.WriteLine("Track variation is " + TrackVariation);

                    // Concatenate tracklocation2 and trackvariation 
                    FullTrackLocation = TrackLocation2 + " " + TrackVariation;
                    //Console.WriteLine("FullTrackLocation is " + FullTrackLocation);
                }

                //************************************************************
                // Vehicle name and class id
                //************************************************************
                //Certain classes only have one character ahead of name
                // Do a if else
                // If (ClassName = bunch of classes) {SubIndex = 1}
                //else { SubIndex = 2}
                // then use SubIndex in SubString (instead of '2')

                if (uDP.VehicleIndex == 132) { VehicleName = "Ginetta G40 GT5"; } // For some reason, uDP.VehicleName is blank for this car

                if (uDP.VehicleName != null && VehicleName != "Ginetta G40 GT5")
                {
                    //Console.WriteLine("VehicleName is " + VehicleName);
                    VehicleName = ((uDP.VehicleName).Substring(2));
                    //Console.WriteLine("VehicleNameSub is " + VehicleNameSub);
                }

                //Correct naming errors in vehicles and tracks
                if (VehicleName == "orsche 917/10") { VehicleName = "Porsche 917/10"; }
                if (VehicleName == "ormula Renault 3.5") { VehicleName = "Formula Renault 3.5"; }
                if (VehicleName == "allara IR-12 Chevrolet (Speedway)") { VehicleName = "Dallara IR-12 Chevrolet (Speedway)"; }
                if (VehicleName == "allara IR-12 Chevrolet (Road Course)") { VehicleName = "Dallara IR-12 Chevrolet (Road)"; }
                if (VehicleName == "MW 2002 Turbo") { VehicleName = "BMW 2002 Turbo"; }
                if (VehicleName == "ord Escort RS1600") { VehicleName = "Ford Escort RS1600"; }
                if (VehicleName == "ercedes-AMG A 45 SMS-R Touring") { VehicleName = "Mercedes-AMG A 45 SMS-R Touring"; }
                if (VehicleName == "MW 320 TC (E90)") { VehicleName = "BMW 320 TC (E90)"; }
                if (VehicleName == "errari 488 Challenge (EU)") { VehicleName = "Ferrari 488 Challenge (EU)"; }
                if (VehicleName == "adical SR3-RS") { VehicleName = "Radical SR3-RS"; }
                if (VehicleName == "enault Clio Cup") { VehicleName = "Renault Clio Cup"; }
                if (FullTrackLocation == "SUGO GP") { FullTrackLocation = "Sportsland SUGO"; }
                if (FullTrackLocation == "Laguna Seca") { FullTrackLocation = "Mazda Raceway Laguna Seca"; }
                if (FullTrackLocation == "Snetterton 100_Circuit") { FullTrackLocation = "Snetterton 100"; }
                if (FullTrackLocation == "Snetterton 200_Circuit") { FullTrackLocation = "Snetterton 200"; }
                if (FullTrackLocation == "Snetterton 300_Circuit") { FullTrackLocation = "Snetterton 300"; }

                //************************************************************
                // Vehicle class name
                //************************************************************
                if (uDP.ClassName != null)
                {
                    
                    //Console.WriteLine("ClassIndex " + uDP.ClassIndex);
                    //Console.WriteLine("ClassName " + uDP.ClassName);
                }

                //************************************************************************************************************
                //If car or track has changed - update table cartrackdb with currentrack and currentcar so php page can change
                //************************************************************************************************************
                if ((VehicleName != null && FullTrackLocation != null) && (VehicleName != OldVehicleName || FullTrackLocation != OldFullTrackLocation))
                {
                    Console.WriteLine("Trying to send new car-track to db.");
                    Console.WriteLine("Current track is " + FullTrackLocation);
                    Console.WriteLine("Current vehicle is " + VehicleName);
                    dbCurrentCarTrack();
                }

                // Current Lap
                double CurrentLap = (uDP.ParticipantInfo[uDP.ViewedParticipantIndex, 13]);

                //Current sector change
                double CurrentSector = (uDP.ParticipantInfo[uDP.ViewedParticipantIndex, 8]);

                // Try to see if previous lap was invalid
                if (strRaceMode == "Invalid")
                { LastLapValid = "N"; }
                else { LastLapValid = "Y"; }

                if (strSessionMode == "Invalid")
                { LastLapValid = "N"; }

                // Last sector time
                double LastSectorTime = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 4]);
                if (LastSectorTime <= 0)
                { LastLapValid = "N"; }

                //Console.WriteLine("LastLapValid is " + LastLapValid);

                // See if sector has changed 
                if (PreviousSector != CurrentSector && CurrentSector != 0 && strRaceMode == "Racing")
                {
                    // Set last sectortime to one of the sector times. 
                    Thread.Sleep(200);  // Test to let sector times etc get a chance to be received
                    Console.WriteLine("Current Sector is " + CurrentSector + " and Previous Sector was " + PreviousSector + "==========================");
                    
                    if (CurrentSector == 3)
                    {
                        LastSector2TimeSec = LastSectorTime;
                        //Console.WriteLine("Sector 2 time is " + LastSector2TimeSec);
                        PreviousSector = CurrentSector;
                    }
                    if (CurrentSector == 1)
                    {
                        LastSector3TimeSec = LastSectorTime;
                        //Console.WriteLineSector 3 time is " + LastSector3TimeSec);
                        PreviousSector = CurrentSector;
                    }

                    if (CurrentSector == 2)
                    {
                        LastSector1TimeSec = LastSectorTime;
                        //Console.WriteLineSector 1 time is " + LastSector1TimeSec);
                        PreviousSector = CurrentSector;
                    }
                }

                // Fastest sector times
                FastestSector1TimeSec = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 5]);
                FastestSector2TimeSec = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 6]);
                FastestSector3TimeSec = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 7]);

                //****************************
                // SENDING LAPTIME TO DATABASE
                //****************************
                // Check if we need to update db
                LastLapTimeSec = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 3]);
                OldFastestLapTimeSec = FastestLapTimeSec; // Store the previous fastest lap in session
                FastestLapTimeSec = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 2]);    // Retrieve fastest lap in session                 

                // Is it a new session lap record? 
                if (OldFastestLapTimeSec <= 0) { OldFastestLapTimeSec = 9999999999; }
                if (LastLapTimeSec < OldFastestLapTimeSec && OldFastestLapTimeSec != 9999999999)
                {
                    SessionLapRecord = "Y";
                    //Console.WriteLine("NEW SESSION RECORD");
                }
                else
                { SessionLapRecord = "N"; }

                // CHeck if lap is valid
                if (strRaceMode == "Invalid")
                { CurrentValidLap = "N"; }
                else { CurrentValidLap = "Y"; }

                // Check if it's a new lap that should be sent to db
                if (LastLapTimeSec > 0 && OldLapTimeSec != LastLapTimeSec && CurrSessionState != 4) //excluding formation laps (untested). Should check for in/out laps too.
                {
                    dbfetchrecord(); //get lap records from db
                    if (dbLapRecord <= 0) { dbLapRecord = 99999999999; }
                    // Is it an All Time Record?
                    if (LastLapTimeSec < dbLapRecord)
                    {
                        AllTimeRecord = "Y"; Console.WriteLine("* * * * * * * * * * N E W  L A P  R E C O R D * * * * * * * * *");
                    }
                    else { AllTimeRecord = "N"; }
                    dbupdate(); // Send the lap to MariaDB

                    OldLapTimeSec = LastLapTimeSec; // Store the last lap time in variable for later comparisons
                }

                // Car index
                double CarIndex = (uDP.ParticipantInfo[uDP.ViewedParticipantIndex, 11]);
                //Console.WriteLine("CarIndex is " + CarIndex);

                // RaceState
                RaceState = (uDP.ParticipantInfo[uDP.ViewedParticipantIndex, 12]);
                // Console.WriteLine("RaceState is " + RaceState);

                // Gamestate
                //Console.WriteLine("Game state is: " + gameState);
                //Console.WriteLine("Session state is: " + sessionState);

                //Event time left
                float EventTimeRemaining = uDP.EventTimeRemaining;
                //Console.WriteLine("Event time remaining is " + EventTimeRemaining);

                // LapDistance
                double LapDistance = (uDP.ParticipantInfo[uDP.ViewedParticipantIndex, 6]);
                //Console.WriteLine("Lap distance is " + LapDistance);

                //***************************************
                // Get player name, update db if change *
                //***************************************
                //TODO load OldName from database first!
                Name = uDP.Name;
                if (Name != OldName && Name != "" && Name != null)
                {
                    Console.WriteLine("Old name is " + OldName);
                    Console.WriteLine("New name is " + Name);
                    dbUsername();
                }

                //*****************************
                // Flag colours
                //*****************************
                double HighestFlag = (uDP.ParticipantInfo[uDP.ViewedParticipantIndex, 9]);
                //Console.WriteLine(HighestFlag);

                //*********************
                // Race state (Probably wrong way to get this, but some of them are working :))
                //*********************
                RaceState = (uDP.ParticipantInfo[uDP.ViewedParticipantIndex, 12]);
                if (RaceState != 0)
                {
                    int raceint = Convert.ToInt16(RaceState);
                   
                    if (raceint == 0)
                    { strRaceMode = "Invalid"; }
                    if (raceint == 9)
                    { strRaceMode = "Not Started"; }
                    if (raceint == 2)
                    { strRaceMode = "Racing"; }
                    if (raceint == 3)
                    { strRaceMode = "Finished"; }
                    if (raceint == 12)
                    { strRaceMode = "Disqualified"; }
                    if (raceint == 5)
                    { strRaceMode = "Retired"; }
                    if (raceint == 6)
                    { strRaceMode = "DNF"; }
                    if (raceint == 10)
                    { strRaceMode = "Invalid"; }
                    //Console.WriteLine("strRaceMode is" + strRaceMode);
                }

                //*********************
                // Game & Session state
                //*********************
                if (uDP.GameState != 0)
                {
                    string gamehex = uDP.GameState.ToString("X2");
                    CurrGameState = gamehex[1];
                    CurrSessionState = gamehex[0];
                    //Console.WriteLine("CurrSessionState is " + CurrSessionState);
                    //Console.WriteLine("CurrGameState is " + CurrGameState);
                    if (CurrSessionState.Equals('0'))
                    { strSessionMode = "Invalid"; }
                    if (CurrSessionState.Equals('1'))
                    { strSessionMode = "Practice"; }
                    if (CurrSessionState.Equals('2'))
                    { strSessionMode = "Test Session"; }
                    if (CurrSessionState.Equals('3'))
                    { strSessionMode = "Qualify"; }
                    if (CurrSessionState.Equals('4'))
                    { strSessionMode = "Formation Lap"; }
                    if (CurrSessionState.Equals('5'))
                    { strSessionMode = "Race"; }
                    if (CurrSessionState.Equals('6'))
                    { strSessionMode = "Time Trial"; }
                    //Console.WriteLine("strSessionMode = " + strSessionMode);

                // TODO Use GameState to catch restarts and reset values
                }

                // Track Temperature
                TrackTemp = uDP.TrackTemperature;

                // Ambient Temperature
                AmbTemp = uDP.AmbientTemperature;

                // Rain Density
                RainDensity = uDP.RainDensity;
         
            }
        }
    }
}

