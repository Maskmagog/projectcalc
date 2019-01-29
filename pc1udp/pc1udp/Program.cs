//******************************************
// Project CALC - by Martin Holmström
// UDP library by Zach Etier (Zeratall)
// Expanded by Martin Holmström (Maskmagog)
// maskmagog@gmail.com
// https://github.com/Maskmagog/projectcalc
// 
// Feel free to use the program(s) 
// but don't make money on it.
// Change/adapt/modify the code as you want
// but keep these lines. Thank you.
//******************************************

using PcarsUDP;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices;
using System.Data.SqlClient;
using System.Text;

namespace pc1udp
{
    class Program
    {
        // db variables
        public static string Server = "127.0.0.1";    // don't use 'localhost' for some reason
        public static string User = "pcars";
        public static string Database = "pcarsdb";
        public static int Port = 3306;
        public static string Pass = "PG3Dnq4m2BVFaaLC";

        // Declare variables to send to db
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        private static string Name = "";
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        private static string OldName = "";
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        private static string TrackLocation = "";
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        private static string TrackVariation = "";
        public static float TrackLength;
        public static int TrackLengthInt;
        public static string strTrackLengthInt;
        private static double LastLapTime = 0;
        private static double LastSector1Time = 0;
        private static double LastSector2Time = 0;
        private static double LastSector3Time = 0;
        private static double FastestSector1Time = 0;
        private static double FastestSector2Time = 0;
        private static double FastestSector3Time = 0;
        private static Int16 TrackTemp;
        private static Int16 AmbTemp;
        public static int PitSchedule;
        public static int OldPitSchedule;
        private static string strPitSchedule;
        public static int OldPitMode;
        public static int PitMode;
        private static string strPitMode;
        private static string CurrentLapOutLap = "N";
        private static string CurrentLapInLap = "N";
        private static double PreviousSector = 3;
        private static string SessionLapRecord = "N";
        private static double OldFastestLapTime = 99999999;
        private static double FastestLapTime = 0;
        private static double dbLapRecord = 0;
        private static string AllTimeRecord = "N";
        public static string VehicleName;
        public static string VehicleClass;
        public static string OldGameState;
        public static string OldSessionState;
        public static string OldRaceState;
        public static string CurrentLapValid;
        public static string LastLapValid = "Y";
        public static string FullTrackLocation;
        public static double RainDensity;
        public static string OldFullTrackLocation = "";
        public static string OldVehicleName = "";
        public static double OldLapTimeSec = 0;
        public static int a = 0;
        public static int ValuesReset = 0;
        public static string validlapsonly;
        public static string timetrialonly;
        public static string settings1;
        public static string settings2;
        public static string platform;
        public static string setup;
        public static string controller;
        public static double CurrentSector = 3;
        public static string GameState;
        public static string SessionState;
        public static string RaceState;
        public static int LapInvalidated;
        public static int test;

        public static void ResetValues()
        {
            LastLapTime = 0;
            LastSector1Time = 0;
            LastSector2Time = 0;
            LastSector3Time = 0;
            FastestSector1Time = 0;
            FastestSector2Time = 0;
            FastestSector3Time = 0;
            PreviousSector = 3;
            OldFastestLapTime = 99999999;
            dbLapRecord = 0;
            SessionLapRecord = "N";
            AllTimeRecord = "N";
            LastLapValid = "Y";
            CurrentLapValid = "Y";
            CurrentLapInLap = "N";
            CurrentLapOutLap = "N";
            PitMode = 0;
            PitSchedule = 0;
        }



        //************************************
        // Inserts new laptimes into database
        //************************************
        public static void dbSendLapToDb()
        {
            // DATABASE CONNECTION
            string connStr = String.Format("server={0}; user={1}; database={2}; port={3}; password={4}", Server, User, Database, 3306, Pass);

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
                command.Parameters.Add("?vehicleclass", MySqlDbType.VarChar, 64).Value = VehicleClass;
                command.Parameters.Add("?laptime", MySqlDbType.Double).Value = LastLapTime;
                command.Parameters.Add("?sector1", MySqlDbType.Double).Value = LastSector1Time;
                command.Parameters.Add("?sector2", MySqlDbType.Double).Value = LastSector2Time;
                command.Parameters.Add("?sector3", MySqlDbType.Double).Value = LastSector3Time;
                command.Parameters.Add("?tracktemp", MySqlDbType.Int16).Value = TrackTemp;
                command.Parameters.Add("?ambtemp", MySqlDbType.Int16).Value = AmbTemp;
                command.Parameters.Add("?raindensity", MySqlDbType.Double).Value = RainDensity;
                command.Parameters.Add("?sessionmode", MySqlDbType.VarChar, 32).Value = SessionState;
                command.Parameters.Add("?validlap", MySqlDbType.VarChar, 1).Value = LastLapValid;
                command.Parameters.Add("?lapdate", MySqlDbType.DateTime).Value = DateTime.Now;
                command.Parameters.Add("?platform", MySqlDbType.VarChar, 3).Value = platform;
                command.Parameters.Add("?setup", MySqlDbType.VarChar, 8).Value = setup;
                command.Parameters.Add("?controller", MySqlDbType.VarChar, 8).Value = controller;
                command.Parameters.Add("?camera", MySqlDbType.VarChar, 8).Value = "In-car";
                command.ExecuteNonQuery();
                Console.WriteLine("----Session lap record? " + SessionLapRecord);
                Console.WriteLine(Name + "-" + FullTrackLocation + "-" + VehicleName + "-" + LastLapTime + "-" + LastLapValid);
                Console.WriteLine("***********NEW LAPTIME ADDED TO DATABASE***************");
                Console.WriteLine("S1 = " + LastSector1Time);
                Console.WriteLine("S2 = " + LastSector2Time);
                Console.WriteLine("S3 = " + LastSector3Time);
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
        public static void dbFetchRecord()
        {
            // DATABASE CONNECTION
            string connStr = String.Format("server={0}; user={1}; database={2}; port={3}; password={4}", Server, User, Database, 3306, Pass);
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
        // Getting settings from database
        //************************************
        public static void dbFetchSettings()
        {
            // DATABASE CONNECTION
            string connStr = String.Format("server={0}; user={1}; database={2}; port={3}; password={4}", Server, User, Database, 3306, Pass);
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                // Open connection to db
                conn.Open();

                // Perform database operations, TODO change to parameterized
                string cmd = "SELECT timetrialonly, validlapsonly, platform, controller, setup FROM settings";

                MySqlCommand command = new MySqlCommand(cmd, conn);
                MySqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    // Get settings from settings.php
                    validlapsonly = (dataReader["validlapsonly"].ToString());
                    timetrialonly = (dataReader["timetrialonly"].ToString());
                    platform = (dataReader["platform"].ToString());
                    controller = (dataReader["controller"].ToString());
                    setup = (dataReader["setup"].ToString());
                }
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
            string connStr = String.Format("server={0}; user={1}; database={2}; port={3}; password={4}", Server, User, Database, 3306, Pass);

            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                // Open connection to db
                conn.Open();

                var cmdUser = "UPDATE user SET username = '" + Name + "' WHERE id=1"; // Only one record in this db, to keep track of player name
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
            string connStr = String.Format("server={0}; user={1}; database={2}; port={3}; password={4}", Server, User, Database, 3306, Pass);
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL... adding car and track");
                // Open connection to db
                conn.Open();

                var cmd = "UPDATE cartrackdb SET currenttrack = '" + FullTrackLocation + "', currentvehicle = '" + VehicleName + "' WHERE id=1"; //Only 1 row in this db, that changes when player changes car/track in-game
                Console.WriteLine("cmd " + cmd);
                var command = new MySqlCommand(cmd, conn);

                Console.WriteLine("Track Length " + TrackLength);
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
            //*********************
            //  Start UDP reading *
            //*********************
            UdpClient listener = new UdpClient(5606);                       //Create a UDPClient object
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 5606);       //Start recieving data from any IP listening on port 5606 (port for PCARS2)

            PCars1_UDP uDP = new PCars1_UDP(listener, groupEP);             //Create an UDP object that will retrieve telemetry values from in game.

            Console.WriteLine("Waiting for udp");

            //************
            // MAIN LOOP *
            //************
            while (true)
            {
                //Thread.Sleep(20);
                uDP.readPackets(); //Read Packets ever loop iteration

                /* Detect session restarts. trigger a function to reset values */

                if (RaceState == "RACESTATE_NOT_STARTED" && ValuesReset == 0)
                {
                    ResetValues();
                    Console.WriteLine("Session restart/new session? Values reset.");
                    ValuesReset = 1;
                }


                //****************************
                //Track Location (if not null)
                //****************************
                if (uDP.TrackLocation != null)
                {
                    TrackLocation = uDP.TrackLocation;
                    //Console.WriteLine("Track location is " + TrackLocation);
                    TrackLocation = TrackLocation.Replace("_", " "); //Replace underscores with spaces

                    //Track Variation
                    TrackVariation = uDP.TrackVariation;
                    //Console.WriteLine("Track variation is " + TrackVariation);
                    TrackVariation = TrackVariation.Replace("_", " ");
                    if (TrackVariation == "Grand Prix") { TrackVariation = "GP"; }

                    // Concatenate tracklocation and trackvariation 
                    FullTrackLocation = TrackLocation + " " + TrackVariation;
                    //Console.WriteLine("FullTrackLocation is " + FullTrackLocation);

                    TrackLength = uDP.TrackLength;
                    //Console.WriteLine("TrackLength " + TrackLength);
                    TrackLengthInt = (int)TrackLength; //Convert to integer
                    //Console.WriteLine("TrackLengthInt " + TrackLengthInt);
                }

                //************************************************************
                // Vehicle name and class 
                //************************************************************
                if (uDP.VehicleName2 != null)
                {
                    VehicleName = uDP.VehicleName2;
                    VehicleClass = uDP.VehicleClass2;
                }

                //Console.WriteLine("VehicleName is " + VehicleName);    
                //Console.WriteLine("VehicleClass is " + VehicleClass);

                // Vehicle class
                if (VehicleClass == "G40 Junior") { VehicleClass = "G40 Jr"; }
                if (VehicleClass == "Group C1") { VehicleClass = "Gr. C"; }
                if (VehicleClass == "Group 4") { VehicleClass = "Gr. 4"; }
                if (VehicleClass == "Group 5") { VehicleClass = "Gr. 5"; }
                if (VehicleClass == "Group 6") { VehicleClass = "Gr. 6"; }
                if (VehicleClass == "Group A") { VehicleClass = "Gr. A"; }
                if (VehicleClass == "Group B") { VehicleClass = "Gr. B"; }
                if (VehicleClass == "Touring Car") { VehicleClass = "TC"; }
                if (VehicleClass == "Vintage Prototype A") { VehicleClass = "VP A"; }
                if (VehicleClass == "Vintage Prototype B") { VehicleClass = "VP B"; }
                if (VehicleClass == "Vintage RX") { VehicleClass = "V RX"; }
                if (VehicleClass == "Vintage Touring-GT A") { VehicleClass = "VGT A"; }
                if (VehicleClass == "Vintage Touring-GT B") { VehicleClass = "VGT B"; }
                if (VehicleClass == "Vintage Touring-GT C") { VehicleClass = "VGT C"; }
                if (VehicleClass == "Vintage Touring-GT D") { VehicleClass = "VGT D"; }
                if (VehicleClass == "Track Day A") { VehicleClass = "Track A"; }
                if (VehicleClass == "Track Day B") { VehicleClass = "Track B"; }
                if (VehicleClass == "Ferrari Series") { VehicleClass = "Ferrari"; }
                if (VehicleClass == "Ferrari F355 Series") { VehicleClass = "F355"; }
                if (VehicleClass == "Formula Rookie") { VehicleClass = "F5"; }
                if (VehicleClass == "Formula C") { VehicleClass = "FC"; }
                if (VehicleClass == "Formula Renault") { VehicleClass = "FR35"; }
                if (VehicleClass == "Formula A") { VehicleClass = "FA"; }
                if (VehicleClass == "Formula X") { VehicleClass = "FX"; }
                if (VehicleClass == "V8 Supercars") { VehicleClass = "V8"; }
                if (VehicleClass == "Vintage F1 A") { VehicleClass = "V F1A"; }
                if (VehicleClass == "Vintage F1 B") { VehicleClass = "V F1B"; }
                if (VehicleClass == "Vintage F1 C") { VehicleClass = "V F1C"; }
                if (VehicleClass == "Vintage F1 D") { VehicleClass = "V F1D"; }
                if (VehicleClass == "Vintage F3 A") { VehicleClass = "V F3A"; }
                if (VehicleClass == "Vintage Indycar") { VehicleClass = "V Indy"; }
                if (VehicleClass == "Kart1") { VehicleClass = "Kart"; }
                if (VehicleClass == "Megane Trophy") { VehicleClass = "Mégane Trophy"; }
                if (VehicleClass == "Modern Stockcar") { VehicleClass = "Stockcar"; }

                //Correct naming errors in vehicles and tracks
                if (VehicleName != null)
                {
                    if (VehicleName == "RenaultMeganeRSSMSRTouring") { VehicleName = "Renault Mégane R.S. SMS-R Touring"; }
                    if (VehicleName == "RenaultMeganeTrophyV6") { VehicleName = "Renault Mégane Trophy V6"; }
                    if (VehicleName == "RenaultMeganeRS275TrophyR") { VehicleName = "Renault Mégane R.S. 275 Trophy-R"; }
                    if (VehicleName == "RenaultMeganeRSSMSRRallycross") { VehicleName = "Renault Mégane R.S. SMS-R Rallycross"; }
                    if (VehicleName == "LamborghiniHuracanLP6202SuperTrofeo") { VehicleName = "Lamborghini Huracán LP620-2 Super Trofeo"; }
                    string test = "RTR TransAm";
                    if (VehicleName.Contains(test)) { VehicleName = "Ford Mustang 66 RTR TransAm"; }
                    test = "Camaro Z/28";
                    if (VehicleName.Contains(test)) { VehicleName = "Chevrolet Camaro Z/28 69 TransAm"; }
                    if (VehicleName == "Mercedes-Benz 300 SL (W194)") { VehicleName = "Mercedes-Benz 300 SL"; }
                    if (VehicleName == "LamborghiniHuracanLP6104") { VehicleName = "Lamborghini Huracán LP610-4"; }
                    if (VehicleName == "BMW1SeriesMCoupeStanceWorksEdition") { VehicleName = "BMW 1 Series M Coupe StanceWorks Edition"; }
                    if (VehicleName == "Honda24Concept") { VehicleName = "Honda 2&4 Concept"; }
                    if (VehicleName == "Mercedes-AMG A 45 SMS-R Touring") { VehicleName = "Mercedes-AMG A 45 SMS-R Touring"; }
                    if (VehicleName == "Opel Astra TCR") { VehicleName = "Opel Astra TCR"; }
                    if (VehicleName == "McLarenP1GTR") { VehicleName = "McLaren P1 GTR"; }
                    if (VehicleName == "McLarenP1") { VehicleName = "McLaren P1"; }
                    if (VehicleName == "Ginetta G40 GT5") { VehicleClass = "GT5"; }
                }

                if (FullTrackLocation == "Algarve ") { FullTrackLocation = "Algarve"; } //Whitespace 

                if (FullTrackLocation == "Barcelona Catalunya GP") { FullTrackLocation = "Barcelona-Catalunya GP"; }
                if (FullTrackLocation == "Barcelona Catalunya National") { FullTrackLocation = "Barcelona-Catalunya National"; }
                if (FullTrackLocation == "Barcelona Catalunya Club") { FullTrackLocation = "Barcelona-Catalunya Club"; }
                if (FullTrackLocation == "Brands Hatch Rallycross") { FullTrackLocation = "Brands Hatch Classic Rallycross"; }
                if (FullTrackLocation == "Circuit of the Americas National Circuit") { FullTrackLocation = "Circuit of the Americas National"; }
                if (FullTrackLocation == "Circuit of the Americas Club Circuit") { FullTrackLocation = "Circuit of the Americas Club"; }
                if (FullTrackLocation == "Daytona International Speedway Road Course") { FullTrackLocation = "Daytona Road Course"; }
                if (FullTrackLocation == "Daytona International Speedway Tri Oval") { FullTrackLocation = "Daytona Speedway Tri-Oval"; }
                if (FullTrackLocation == "Daytona International Speedway Rallycross") { FullTrackLocation = "Daytona Rallycross"; }
                if (FullTrackLocation == "DirtFish Stage 3")
                {
                    if (TrackLengthInt == 1353) { FullTrackLocation = "DirtFish Mill Run Course"; }
                    else { FullTrackLocation = "DirtFish Pro Rallycross Course"; }
                }
                if (FullTrackLocation == "DirtFish Stage 2") { FullTrackLocation = "DirtFish Boneyard Course"; }
                if (FullTrackLocation == "Indianapolis Motor Speedway Oval") { FullTrackLocation = "Indianapolis Oval"; }
                if (FullTrackLocation == "Indianapolis Motor Speedway Road Course") { FullTrackLocation = "Indianapolis Road Course"; }
                if (FullTrackLocation == "Sugo GP") { FullTrackLocation = "Sportsland SUGO"; }
                if (FullTrackLocation == "Spa Francorchamps GP") { FullTrackLocation = "Spa-Francorchamps GP"; }
                if (FullTrackLocation == "Spa Francorchamps Historic GP") { FullTrackLocation = "Spa-Francorchamps Historic"; }
                if (FullTrackLocation == "Laguna Seca ") { FullTrackLocation = "Mazda Raceway Laguna Seca"; } //Whitespace
                if (FullTrackLocation == "Snetterton 100 Circuit") { FullTrackLocation = "Snetterton 100"; }
                if (FullTrackLocation == "Snetterton 200 Circuit") { FullTrackLocation = "Snetterton 200"; }
                if (FullTrackLocation == "Snetterton 300 Circuit") { FullTrackLocation = "Snetterton 300"; }
                if (FullTrackLocation == "Rouen Les Essarts ") { FullTrackLocation = "Rouen Les Essarts"; } //Whitespace
                if (FullTrackLocation == "Rouen Short") { FullTrackLocation = "Rouen Les Essarts Short"; }
                if (FullTrackLocation == "Monza Classic GP") { FullTrackLocation = "Monza GP Historic"; }
                if (FullTrackLocation == "Monza Classic Historic Oval") { FullTrackLocation = "Monza Oval Historic"; }
                if (FullTrackLocation == "Monza Classic Historic Mix") { FullTrackLocation = "Monza Oval + GP Historic"; }
                if (FullTrackLocation == "Hell RX Rallycross") { FullTrackLocation = "Lankebanen Rallycross"; }
                if (FullTrackLocation == "Le Mans Le Mans Bugatti Circuit") { FullTrackLocation = "Le Mans Bugatti Circuit"; }
                if (FullTrackLocation == "Le Mans Kart Int Le Mans International Karting Circuit") { FullTrackLocation = "Le Mans International Karting Circuit"; }
                if (FullTrackLocation == "Lydden Hill Circuit") { FullTrackLocation = "Lydden Hill GP"; }
                if (FullTrackLocation == "Le Mans Vintage Le Mans Vintage Track") { FullTrackLocation = "Le Mans Vintage Track"; }
                if (FullTrackLocation == "Hockenheimring Short")
                {
                    if (TrackLengthInt == 2593) { FullTrackLocation = "Hockenheim Short"; }
                    else { FullTrackLocation = "Hockenheim Rallycross"; }
                }
                if (FullTrackLocation == "Le Mans Vintage Le Mans Vintage Track") { FullTrackLocation = "Le Mans Vintage Track"; }
                if (FullTrackLocation == "Le Mans 24 Hours of Le Mans Circuit") { FullTrackLocation = "24 Hours of Le Mans Circuit"; }
                if (FullTrackLocation == "Loheac Rallycross of Loheac") { FullTrackLocation = "Rallycross of Loheac"; }
                if (FullTrackLocation == "Zolder GP") { FullTrackLocation = "Zolder"; }

                // Bug: Both Hockenheim Short and Hockenheim Rallycross is sent as 'Hockenheim Short'  (Translated name: Hockenheimring Short)
                // Bug: 'DirtFish Stage3' sent for both 'DirtFish Pro Rallycross Course' and 'DirtFish Mill Run Course'



                //************************************************************************************************************
                //If car or track has changed - update table cartrackdb with currentrack and currentcar so php page can change
                //************************************************************************************************************
                if ((VehicleName != "" && FullTrackLocation != " ") && (VehicleName != OldVehicleName || FullTrackLocation != OldFullTrackLocation))
                {
                    Console.WriteLine("Trying to send new car-track to db.");
                    Console.WriteLine("Current track is " + FullTrackLocation);
                    Console.WriteLine("Current vehicle is " + VehicleName);
                    Console.WriteLine("Current class is " + VehicleClass);
                    Console.WriteLine("Track length is " + TrackLength);
                    dbCurrentCarTrack();
                }

                //***************************************
                // Get player name, update db if change *
                //***************************************

                Name = uDP.strName;
                //Console.WriteLine("Name is " + Name);

                if (Name != OldName && Name != "" && Name != null)
                {
                    Console.WriteLine("Old name is " + OldName);
                    Console.WriteLine("New name is " + Name);
                    dbUsername();
                }




                // GameState
                OldGameState = GameState;
                switch (uDP.GameSessionState & 7)
                {
                    case 0:
                        //Console.WriteLine("Case 0: GameState = GAME_EXITED");
                        GameState = "GAME_EXITED";
                        break;
                    case 1:
                        //Console.WriteLine("Case 1: GameState = GAME_FRONT_END");
                        GameState = "GAME_FRONT_END";
                        break;
                    case 2:
                        //Console.WriteLine("Case 1: GameState = GAME_INGAME_PLAYING");
                        GameState = "GAME_INGAME_PLAYING";
                        break;
                    case 3:
                        //Console.WriteLine("Case 1: GameState = GAME_INGAME_PAUSED");
                        GameState = "GAME_INGAME_PAUSED";
                        break;
                }

                if (GameState != OldGameState)
                {
                    Console.WriteLine("GameState now " + GameState);
                    OldGameState = GameState;
                }
                // SessionState

                OldSessionState = SessionState;
                switch (uDP.GameSessionState >> 4)
                {
                    case 0:
                        SessionState = "SESSION_INVALID";
                        break;
                    case 1:
                        SessionState = "Practice";
                        break;
                    case 2:
                        SessionState = "Test";
                        break;
                    case 3:
                        SessionState = "Qualify";
                        break;
                    case 4:
                        SessionState = "Formation Lap";
                        break;
                    case 5:
                        SessionState = "Race";
                        break;
                    case 6:
                        SessionState = "Time Trial";
                        break;
                }

                if (SessionState != OldSessionState)
                {
                    Console.WriteLine("SessionState now " + SessionState);
                    OldSessionState = SessionState;
                }

                //RaceState
                OldRaceState = RaceState;

                switch (uDP.RaceStateFlags & 7)
                {
                    case 0:
                        RaceState = "RACESTATE_INVALID";
                        break;
                    case 1:
                        RaceState = "RACESTATE_NOT_STARTED";
                        break;
                    case 2:
                        RaceState = "RACESTATE_RACING";
                        break;
                    case 3:
                        RaceState = "RACESTATE_FINISHED";
                        break;
                    case 4:
                        RaceState = "RACESTATE_DISQUALIFIED";
                        break;
                    case 5:
                        RaceState = "RACESTATE_RETIRED";
                        break;
                    case 6:
                        RaceState = "RACESTATE_DNF";
                        break;
                }

                if (RaceState != OldRaceState)
                {
                    Console.WriteLine("RaceState now " + RaceState);
                    OldRaceState = RaceState;
                }

                //********************
                // Pit Schedule *
                //********************                
                OldPitSchedule = PitSchedule;
                PitSchedule = (uDP.PitModeSchedule >> 3 & 3);

                if (PitSchedule == 0) { strPitSchedule = "NONE"; }
                if (PitSchedule == 1) { strPitSchedule = "STANDARD"; }
                if (PitSchedule == 2) { strPitSchedule = "DRIVE_THROUGH"; }
                if (PitSchedule == 3) { strPitSchedule = "STOP_GO"; }

                if (PitSchedule != OldPitSchedule)
                {
                    Console.Write("PitSchedule now " + PitSchedule);
                    OldPitSchedule = PitSchedule;
                }

                // Pit Mode
                OldPitMode = PitMode;
                PitMode = uDP.PitModeSchedule & 7;

                if (PitMode == 0) { strPitMode = "NONE"; }
                if (PitMode == 1) { strPitMode = "DRIVING_INTO_PITS"; }
                if (PitMode == 2) { strPitMode = "IN_PIT"; }
                if (PitMode == 3) { strPitMode = "DRIVING_OUT_OF_PITS"; }
                if (PitMode == 4) { strPitMode = "IN_GARAGE"; }

                if (PitMode != OldPitMode)
                {
                    Console.Write("PitMode is " + PitMode);
                    OldPitMode = PitMode;
                }

                // Track Temperature
                TrackTemp = uDP.TrackTemperature;
                //Console.WriteLine("TrackTemp " + TrackTemp);
                // Ambient Temperature
                AmbTemp = uDP.AmbientTemperature;
                //Console.WriteLine("AmbTemp " + AmbTemp);
                // Rain Density
                RainDensity = uDP.RainDensity;

                // sector
                if (uDP.ParticipantInfo[0, 7] > 0)
                {
                    CurrentSector = uDP.ParticipantInfo[0, 7];
                }

                //***************************************
                // Try to see if current lap is invalid *
                //***************************************
                LapInvalidated = uDP.RaceStateFlags >> 3 & 1;
                //Console.WriteLine("LapInvalidated = " + LapInvalidated);
                //Console.WriteLine("CurrentLapValid = " + CurrentLapValid);
                if (LapInvalidated == 1 && CurrentLapValid == "Y")
                    
                {
                    CurrentLapValid = "N";
                    Console.WriteLine("Lap invalidated, oops.");
                }
                //******************************
                // Check if sector has changed *
                //******************************
                //if (PreviousSector != CurrentSector && RaceState == "RACESTATE_RACING")     
                if (PreviousSector != CurrentSector)
                {
                    //***************************************************************************
                    // Loop everything a few times to allow for correct sector times to be sent *
                    //***************************************************************************
                    a++;
                    if (a > 5) // Continue when a > 5
                    {
                        // Set last sectortime to the previous sector time 
                        if (CurrentSector == 2)
                        {
                            LastSector1Time = (uDP.ParticipantInfo[0, 8]); // Currently not working with ViewedParticipant
                            Console.WriteLine("Sector 1 time is " + LastSector1Time);
                            a = 0; // reset 'a'
                            PreviousSector = CurrentSector;
                        }

                        if (CurrentSector == 3)
                        {
                            LastSector2Time = (uDP.ParticipantInfo[0, 8]); // 
                            Console.WriteLine("Sector 2 time is " + LastSector2Time);
                            a = 0;
                            PreviousSector = CurrentSector;
                        }
                        if (CurrentSector == 1)
                        {
                            LastSector3Time = (uDP.ParticipantInfo[0, 8]);
                            Console.WriteLine("Sector 3 time is " + LastSector3Time);
                            a = 0;
                            PreviousSector = CurrentSector;

                            // Current sector=1 means previous sector=3 means New lap: check invalid lap variable
                            if (CurrentLapValid == "N")
                            {
                                LastLapValid = "N"; // This value is stored in the db
                                Console.WriteLine("Last lap was invalid");
                            }
                            else { LastLapValid = "Y"; Console.WriteLine("Last lap was valid"); }
                            CurrentLapValid = "Y"; // Reset CurrentLapValid because of new lap
                        }

                        //****************************
                        // SENDING LAPTIME TO DATABASE
                        //****************************
                        // Check if we need to update db
                        LastLapTime = uDP.LastLapTime; // lap time in seconds
                        OldFastestLapTime = FastestLapTime; // Store the previous fastest lap in session
                        FastestLapTime = uDP.BestLapTime;    // Retrieve fastest lap in session                 

                        // Is it a new session lap record? 
                        if (OldFastestLapTime <= 0) { OldFastestLapTime = 9999999999; } // if no record exists, set to 9999999999 to avoid null issues
                        if (LastLapTime < OldFastestLapTime)
                        {
                            SessionLapRecord = "Y";
                            //Console.WriteLine("NEW SESSION RECORD");
                        }
                        else
                        { SessionLapRecord = "N"; }

                        // Check if it's a new lap that should be sent to db
                        if (LastLapTime > 0 && OldLapTimeSec != LastLapTime && SessionState != "Formation Lap")  //Excluding formation laps (untested). Should check for in/out laps too.
                        {
                            dbFetchRecord(); //get lap records from db
                            if (dbLapRecord <= 0) { dbLapRecord = 99999999999; }   // if no record exists in db, set to 9999999999 to avoid null issues
                            // Is it an All Time Record?
                            if (LastLapTime < dbLapRecord)
                            {
                                if (LastLapValid == "Y")
                                {
                                    AllTimeRecord = "Y"; Console.WriteLine("* * * * * * * * * * N E W  L A P  R E C O R D * * * * * * * * *");
                                }
                                else { Console.WriteLine("New lap record, but unfortunately invalid lap"); }
                            }
                            else { AllTimeRecord = "N"; }

                            // Check user settings to see what laps is ok to send
                            dbFetchSettings();
                            Console.WriteLine("Settings: Timetrialonly: " + timetrialonly + " and validlapsonly: " + validlapsonly);

                            if (validlapsonly == "Y" && timetrialonly == "Y") // Check user settings (settings.php, table 'settings')
                            {
                                if (LastLapValid == "Y" && SessionState == "Time Trial") // Only send valid laps in TT to db
                                {
                                    dbSendLapToDb(); // Send the lap to MariaDB
                                    Console.WriteLine("TT and valid");
                                    Console.WriteLine("Last lap valid: " + LastLapValid);
                                }
                                else
                                {
                                    Console.WriteLine("Lap not stored. Settings are: Only valid laps, and only laps in TT");
                                }
                            }
                            if (validlapsonly == "N" && timetrialonly == "Y") // Check user settings (settings.php, table 'settings')
                            {
                                if (SessionState == "Time Trial") // Only send laps in TT to db, valid or invalid
                                {
                                    dbSendLapToDb(); // Send the lap to MariaDB
                                    Console.WriteLine("TT");
                                }
                                else
                                {
                                    Console.WriteLine("Lap not stored. Settings are: Only laps in TT");
                                }
                            }
                            if (validlapsonly == "Y" && timetrialonly == "N") // Check user settings (settings.php, table 'settings')
                            {
                                if (LastLapValid == "Y") // Only send valid laps to db in any game mode
                                {
                                    dbSendLapToDb(); // Send the lap to MariaDB
                                    Console.WriteLine("Valid");
                                }
                                else
                                {
                                    Console.WriteLine("Lap not stored. Settings are: Only valid laps");
                                }
                            }
                            if (validlapsonly == "N" && timetrialonly == "N") // Check user settings (settings.php, table 'settings')
                            {
                                dbSendLapToDb(); Console.WriteLine("All laps");  // if validlapsonly = "N" and timetrialonly = N then send all laps to db
                               
                            }
                            CurrentLapValid = "Y"; // Reset CurrentLapValid
                            CurrentLapInLap = "N"; // Reset
                            CurrentLapOutLap = "N"; // Reset
                            OldLapTimeSec = LastLapTime; // Store the last lap time in variable for later comparisons
                        }

                    } // end of 'a++'-loop

                } // end of 'if sector has changed'

            }

        }
    }
}

