using PcarsUDP;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices;
using System.Data.SqlClient;

namespace pc2udp
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
        private static string TranslatedFullTrackLocation = "";
        private static string TranslatedTrackLocation = "";
        private static string TranslatedTrackVariation = "";
        private static double LastLapTimeSec = 0;
        private static double LastSector1TimeSec = 0;
        private static double LastSector2TimeSec = 0;
        private static double LastSector3TimeSec = 0;
        private static double FastestSector1TimeSec = 0;
        private static double FastestSector2TimeSec = 0;
        private static double FastestSector3TimeSec = 0;
        private static Int16 TrackTemp;
        private static Int16 AmbTemp;
        private static double PreviousSector = 3;
        private static string SessionLapRecord = "N";
        private static double OldFastestLapTimeSec = 99999999;
        private static double FastestLapTimeSec = 0;
        private static double dbLapRecord = 0;
        private static string AllTimeRecord = "N";
        public static string VehicleName;
        public static string VehicleClass;
        public static string strSessionMode;
        public static char CurrGameState;
        public static char CurrSessionState;
        public static double RaceState;
        public static string strRaceMode;
        public static char CurrRaceState;
        public static string CurrentLapValid;
        public static string LastLapValid;
        public static string FullTrackLocation;
        public static double RainDensity;
        public static string OldFullTrackLocation = "";
        public static string OldVehicleName = "";
        public static double OldLapTimeSec = 0;
        public static int a = 0;
        public static int ValuesReset = 0;


        public static void resetValues()
        {
            LastLapTimeSec = 0;
            LastSector1TimeSec = 0;
            LastSector2TimeSec = 0;
            LastSector3TimeSec = 0;
            FastestSector1TimeSec = 0;
            FastestSector2TimeSec = 0;
            FastestSector3TimeSec = 0;
            PreviousSector = 3;
            OldFastestLapTimeSec = 99999999;
            dbLapRecord = 0;
            SessionLapRecord = "N";
            AllTimeRecord = "N";
            LastLapValid = "Y";
            CurrentLapValid = "Y";
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
                command.Parameters.Add("?laptime", MySqlDbType.Double).Value = LastLapTimeSec;
                command.Parameters.Add("?sector1", MySqlDbType.Double).Value = LastSector1TimeSec;
                command.Parameters.Add("?sector2", MySqlDbType.Double).Value = LastSector2TimeSec;
                command.Parameters.Add("?sector3", MySqlDbType.Double).Value = LastSector3TimeSec;
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
                Console.WriteLine("----Session lap record? " + SessionLapRecord);
                Console.WriteLine(Name + FullTrackLocation + VehicleName + LastLapTimeSec + LastLapValid);
                Console.WriteLine("***********NEW LAPTIME ADDED TO DATABASE***************");
                Console.WriteLine("S1 = " + LastSector1TimeSec);
                Console.WriteLine("S2 = " + LastSector2TimeSec);
                Console.WriteLine("S3 = " + LastSector3TimeSec);
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

            PCars2_UDP uDP = new PCars2_UDP(listener, groupEP);             //Create an UDP object that will retrieve telemetry values from in game.

            Console.WriteLine("wait for udp");

            // Define variables
            int gameState = uDP.GameState3 & 7;
            int sessionState = uDP.GameState3 >> 4;

            //************
            // MAIN LOOP *
            //************
            while (true)
            {
            //Thread.Sleep(20);
            uDP.readPackets(); //Read Packets ever loop iteration


            /* Detect session restarts. trigger a function to reset values */
            if (strRaceMode == "Not Started" & ValuesReset == 0) {
                resetValues();
                Console.WriteLine("Session restart/new session? Values reset.");
                ValuesReset = 1;
            } 

            //****************************
            //Track Location (if not null)
            //****************************
            if (uDP._TrackLocation != null)
            {
                TrackLocation = uDP.TrackLocation;
                //Console.WriteLine("Track location is " + TrackLocation);
                TrackLocation = TrackLocation.Replace("_", " ");

                //Track Variation
                TrackVariation = uDP.TrackVariation;
                //Console.WriteLine("Track variation is " + TrackVariation);
                TrackVariation = TrackVariation.Replace("_", " ");

                // Concatenate tracklocation2 and trackvariation 
                FullTrackLocation = TrackLocation + " " + TrackVariation;
                //Console.WriteLine("FullTrackLocation is " + FullTrackLocation);
            }

                if (uDP._TranslatedTrackLocation != null)
                {
                    TranslatedTrackLocation = uDP.TranslatedTrackLocation;
                    //Console.WriteLine("Track location is " + TrackLocation);
                    TranslatedTrackLocation = TranslatedTrackLocation.Replace("_", " ");

                    //Track Variation
                    TranslatedTrackVariation = uDP.TranslatedTrackVariation;
                    //Console.WriteLine("Track variation is " + TrackVariation);
                    TranslatedTrackVariation = TranslatedTrackVariation.Replace("_", " ");

                    // Concatenate tracklocation2 and trackvariation 
                    TranslatedFullTrackLocation = TranslatedTrackLocation + " " + TranslatedTrackVariation;
                    //Console.WriteLine("FullTrackLocation is " + FullTrackLocation);
                }

                //************************************************************
                // Vehicle name and class id
                //************************************************************
                //Certain classes only have one character ahead of name, the other classes have 2
                // Do a if else
                // If (ClassName = bunch of classes) {SubIndex = 1}
                //else { SubIndex = 2}
                // then use SubIndex in SubString (instead of '2')

                int SubIndex = 2;

                if (uDP.VehicleClass != 0 && uDP.VehicleName != null)
               {
                    var list = new List<uint> { 3357278208, 151060480, 2156134400, 1158676480, 2585198592, 1859780608, 2841116672, 349979920}; //These classes only have 1 character ahead of name
                    var exists = list.Contains(uDP.VehicleClass);
            
                    if (exists == true) // if uDP.Vehicle class index is found in list above...
                    {
                        SubIndex = 1; // ...then set SubIndex to = 1
                    }
                }

                // Get name for Ginetta GT5, which seams to be blank?
                if (uDP.VehicleIndex == 132) { VehicleName = "Ginetta G40 GT5"; } // For some reason, uDP.VehicleName is blank for this car

                if (uDP.VehicleName != null && VehicleName != "Ginetta G40 GT5")
                {
                    //Console.WriteLine("VehicleName is " + VehicleName);     // VehicleName before SubString
                    VehicleName = ((uDP.VehicleName).Substring(SubIndex));
                    //Console.WriteLine("VehicleName is " + VehicleName);     // VehicleName after SubString
                    //Console.WriteLine("ClassINdex is " + uDP.VehicleClass);     // ClassIndex
                    //Console.WriteLine("VehicleIndex is " + uDP.VehicleIndex);
                }
                // Vehicle class
                if (uDP.VehicleClass == 151060480) { VehicleClass = "GTO"; }
                if (uDP.VehicleClass == 1039138816) { VehicleClass = "GT1"; }
                if (uDP.VehicleClass == 2041511936) { VehicleClass = "GT3"; }
                if (uDP.VehicleClass == 3878354944) { VehicleClass = "GT4"; }
                if (uDP.VehicleClass == 3007315968) { VehicleClass = "GT5";}
                if (uDP.VehicleClass == 496107520) { VehicleClass = "G40 Junior"; }
                if (uDP.VehicleClass == 4259840) { VehicleClass = "GTE"; }
                if (uDP.VehicleClass == 4252434432) { VehicleClass = "Group A"; }
                if (uDP.VehicleClass == 572784640) { VehicleClass = "Group B"; }
                if (uDP.VehicleClass == 2176057344) { VehicleClass = "Group C"; }
                if (uDP.VehicleClass == 2980708352) { VehicleClass = "Group 4"; }
                if (uDP.VehicleClass == 2203385856) { VehicleClass = "Group 5"; }
                if (uDP.VehicleClass == 794427392) { VehicleClass = "Group 6"; }
                if (uDP.VehicleClass == 1158676480) { VehicleClass = "CanAm"; }
                if (uDP.VehicleClass == 3614507008) { VehicleClass = "RXLites"; }
                if (uDP.VehicleClass == 2841116672) { VehicleClass = "TC1"; }
                if (uDP.VehicleClass == 409534464) { VehicleClass = "TC"; }
                if (uDP.VehicleClass == 2381971456) { VehicleClass = "VP A"; }
                if (uDP.VehicleClass == 1155465216) { VehicleClass = "VP B"; }
                if (uDP.VehicleClass == 1636630528) { VehicleClass = "V RX"; }
                if (uDP.VehicleClass == 1476722688) { VehicleClass = "VGT A"; }
                if (uDP.VehicleClass == 2908356608) { VehicleClass = "VGT B"; }
                if (uDP.VehicleClass == 1973682176) { VehicleClass = "VGT C"; }
                if (uDP.VehicleClass == 1233584128) { VehicleClass = "VGT D"; }
                if (uDP.VehicleClass == 3239051264) { VehicleClass = "Trackday A"; }
                if (uDP.VehicleClass == 1859780608) { VehicleClass = "Trackday B"; }
                if (uDP.VehicleClass == 3673882624) { VehicleClass = "Ferrari"; }
                if (uDP.VehicleClass == 559480832) { VehicleClass = "F355 Series"; }
                if (uDP.VehicleClass == 3747282944) { VehicleClass = "Indycar"; }
                if (uDP.VehicleClass == 3361865728) { VehicleClass = "F5"; }
                if (uDP.VehicleClass == 788135936) { VehicleClass = "FC"; }
                if (uDP.VehicleClass == 2156134400) { VehicleClass = "FR35"; }
                if (uDP.VehicleClass == 4294443008) { VehicleClass = "FA"; }
                if (uDP.VehicleClass == 836239360) { VehicleClass = "FX"; }
                if (uDP.VehicleClass == 308084736) { VehicleClass = "V8"; }
                if (uDP.VehicleClass == 2669346816) { VehicleClass = "V F1A"; }
                if (uDP.VehicleClass == 2635988992) { VehicleClass = "V F1B"; }
                if (uDP.VehicleClass == 1596850176) { VehicleClass = "V F1C"; }
                if (uDP.VehicleClass == 1984233472) { VehicleClass = "V F1D"; }
                if (uDP.VehicleClass == 309395456) { VehicleClass = "V F3 A"; }
                if (uDP.VehicleClass == 2864316416) { VehicleClass = "V Indy"; }
                if (uDP.VehicleClass == 3645046784) { VehicleClass = "Drift"; }
                if (uDP.VehicleClass == 2183659520) { VehicleClass = "Kart"; }
                if (uDP.VehicleClass == 1424818176) { VehicleClass = "LMP1 2016"; }
                if (uDP.VehicleClass == 3294560256) { VehicleClass = "LMP 900"; }
                if (uDP.VehicleClass == 2251096064) { VehicleClass = "LMP1"; }
                if (uDP.VehicleClass == 3502637056) { VehicleClass = "LMP2"; }
                if (uDP.VehicleClass == 437256192) { VehicleClass = "LMP3"; }
                if (uDP.VehicleClass == 1250557952) { VehicleClass = "Mégane Trophy"; }
                if (uDP.VehicleClass == 843186176) { VehicleClass = "Stockcar"; }
                if (uDP.VehicleClass == 3707043840) { VehicleClass = "Road A"; }
                if (uDP.VehicleClass == 2258239488) { VehicleClass = "Road B"; }
                if (uDP.VehicleClass == 2110521344) { VehicleClass = "Road C"; }
                if (uDP.VehicleClass == 2740781056) { VehicleClass = "Road D"; }
                if (uDP.VehicleClass == 3872456704) { VehicleClass = "Road E"; }
                if (uDP.VehicleClass == 3357278208) { VehicleClass = "Road F"; }
                if (uDP.VehicleClass == 2585198592) { VehicleClass = "Road G"; }
                if (uDP.VehicleClass == 3735355392) { VehicleClass = "RS01 Trophy"; }
                if (uDP.VehicleClass == 692912128) { VehicleClass = "Super Trofeo"; }
                if (uDP.VehicleClass == 2367946752) { VehicleClass = "Trophy Truck"; }
                if (uDP.VehicleClass == 335872000) { VehicleClass = "WRX"; }

                //Correct naming errors in vehicles and tracks
                if (VehicleName == "RenultMeganeRSSMSRTouring") { VehicleName = "Renault Megane R.S. SMS-R Touring"; }
                if (VehicleName == "RenaultMeganeTrophyV6") { VehicleName = "Renault Mégane Trophy V6"; }
                if (VehicleName == "LamborghiniHuracanLP6202SuperTrofeo") { VehicleName = "Lamborghini Huracan LP620-2 Super Trofeo"; }
                if (VehicleName == "BMW1SeriesMCoupeStanceWorksEdition") { VehicleName = "BMW 1 Series M Coupe StanceWorks Edition"; }
                if (VehicleName == "Honda24Concept") { VehicleName = "Honda 2&4 Concept"; }
                if (VehicleName == "Mercedes-AMG A 45 SMS-R Touring") { VehicleName = "Mercedes-AMG A 45 SMS-R Touring"; }
                if (VehicleName == "Opel Astra TCR") { VehicleName = "Opel Astra TCR"; }
                if (FullTrackLocation == "Algarve ") { FullTrackLocation = "Autodromo Internacional do Algarve"; } //Whitespace
                if (FullTrackLocation == "Barcelona Catalunya GP") { FullTrackLocation = "Circuit de Barcelona-Catalunya GP"; }
                if (FullTrackLocation == "Barcelona Catalunya National") { FullTrackLocation = "Circuit de Barcelona-Catalunya National"; }
                if (FullTrackLocation == "Barcelona Catalunya Club") { FullTrackLocation = "Circuit de Barcelona-Catalunya Club"; }
                if (FullTrackLocation == "Brands Hatch Rallycross") { FullTrackLocation = "Brands Hatch Classic Rallycross"; }
                if (FullTrackLocation == "DirtFish Stage1") { FullTrackLocation = "DirtFish Pro Rallycross Course"; } 
                if (FullTrackLocation == "DirtFish Stage2") { FullTrackLocation = "DirtFish Boneyard Course"; }
                if (FullTrackLocation == "DirtFish Stage3") { FullTrackLocation = "DirtFish Pro Rallycross Course"; } //Game bug: 'DirtFish Stage3' sent for both Pro Rallycross Course and Mill Run :(
                if (FullTrackLocation == "SUGO GP") { FullTrackLocation = "Sportsland SUGO"; }
                if (FullTrackLocation == "Spa Francorchamps GP") { FullTrackLocation = "Circuit de Spa-Francorchamps GP"; }
                if (FullTrackLocation == "Spa Francorchamps Historic GP") { FullTrackLocation = "Circuit de Spa-Francorchamps Historic"; }
                if (FullTrackLocation == "Laguna Seca ") { FullTrackLocation = "Mazda Raceway Laguna Seca"; } //Whitespace
                if (FullTrackLocation == "Snetterton 100 Circuit") { FullTrackLocation = "Snetterton 100"; }
                if (FullTrackLocation == "Snetterton 200 Circuit") { FullTrackLocation = "Snetterton 200"; }
                if (FullTrackLocation == "Snetterton 300 Circuit") { FullTrackLocation = "Snetterton 300"; }
                if (FullTrackLocation == "Rouen ") { FullTrackLocation = "Rouen Les Essarts"; } //Whitespace
                if (FullTrackLocation == "Rouen Short") { FullTrackLocation = "Rouen Les Essarts Short"; }
                if (FullTrackLocation == "Monza GP") { FullTrackLocation = "Autodromo Nazionale Monza GP"; }
                if (FullTrackLocation == "Monza Short") { FullTrackLocation = "Autodromo Nazionale Monza Short"; }
                if (FullTrackLocation == "Monza Classic GP") { FullTrackLocation = "Autodromo Nazionale Monza GP Historic"; }
                if (FullTrackLocation == "Monza Classic Historic Oval") { FullTrackLocation = "Autodromo Nazionale Monza Oval Historic"; }
                if (FullTrackLocation == "Monza Classic Historic Mix") { FullTrackLocation = "Autodromo Nazionale Monza Oval + GP Historic"; }
                if (FullTrackLocation == "Hell Rallycross") { FullTrackLocation = "Lankebanen Rallycross"; }
                if (FullTrackLocation == "Imola GP") { FullTrackLocation = "Autodromo Internazionale Enzo E Dino Ferrari Imola"; }
                if (FullTrackLocation == "Le Mans Le Mans Bugatti Circuit") { FullTrackLocation = "Le Mans Bugatti Circuit"; }
                if (FullTrackLocation == "Le Mans Kart Int Le Mans International Karting Circuit") { FullTrackLocation = "Le Mans International Karting Circuit"; }
                if (FullTrackLocation == "Lydden Hill Circuit") { FullTrackLocation = "Lydden Hill GP"; }


                // Bug: Hockenheim Rallycross is sent as Hockenheim Short :( (Translated name: Hockenheimring Short)
                // Bug: 'DirtFish Stage3' sent for both Pro Rallycross Course and Mill Run :(


                // Get names for certain cars
                if (uDP.VehicleIndex == 104) { VehicleName = "Renault Megane R.S. SMS-R Touring"; } // For some reason, this is not fixed by 'if' above

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
                    Console.WriteLine("Current class is " + VehicleClass);
                    Console.WriteLine("VehicleClass is " + uDP.VehicleClass);
                    Console.WriteLine("Translated track full name is " + TranslatedFullTrackLocation);
                    dbCurrentCarTrack();
                }

                // Current Lap 
                double CurrentLap = (uDP.ParticipantInfo[uDP.ViewedParticipantIndex, 13]);

                //Current sector 
                double CurrentSector = (uDP.ParticipantInfo[uDP.ViewedParticipantIndex, 8]);

                //***************************************
                // Try to see if current lap is invalid *
                //***************************************
                if (strRaceMode == "Invalid" && CurrentLapValid == "Y")
                {
                    CurrentLapValid = "N";
                    Console.WriteLine("Lap invalidated, strRaceMode");
                }

                          
                // RaceState
                RaceState = (uDP.ParticipantInfo[uDP.ViewedParticipantIndex, 12]);
                // Console.WriteLine("RaceState is " + RaceState);

                // Gamestate
                //Console.WriteLine("Game state is: " + gameState);
                //Console.WriteLine("Session state is: " + sessionState);

                //***************************************
                // Get player name, update db if change *
                //***************************************
                //TODO load OldName from database first?
                Name = uDP.Name;
                if (Name != OldName && Name != "" && Name != null)
                {
                    Console.WriteLine("Old name is " + OldName);
                    Console.WriteLine("New name is " + Name);
                    dbUsername();
                }

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
                    { strRaceMode = "Racing"; ValuesReset = 0; }
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

                //******************************
                // Check if sector has changed *
                //******************************
                if (PreviousSector != CurrentSector && strRaceMode != "Not Started")
                {
                    // Fastest sector times (not used)
                    FastestSector1TimeSec = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 5]);
                    FastestSector2TimeSec = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 6]);
                    FastestSector3TimeSec = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 7]);

                    // Write current and previous sector to console
                   // Console.WriteLine("Current Sector is " + CurrentSector + " and Previous Sector was " + PreviousSector + "==========================");

                    //***************************************************************************
                    // Loop everything a few times to allow for correct sector times to be sent *
                    //***************************************************************************
                    a++;
                    //Console.WriteLine("a = " + a);
                    if (a > 5)
                    {
                        // Set last sectortime to the previous sector time 
                        if (CurrentSector == 2)
                        {
                            LastSector1TimeSec = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 4]);
                            Console.WriteLine("Sector 1 time is " + LastSector1TimeSec);
                            a = 0; // reset 'a'
                            PreviousSector = CurrentSector; 
                        }

                        if (CurrentSector == 3)
                        {    
                            LastSector2TimeSec = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 4]); // 
                            Console.WriteLine("Sector 2 time is " + LastSector2TimeSec);
                            a = 0;
                            PreviousSector = CurrentSector;
                        }
                        if (CurrentSector == 1)
                        {
                            LastSector3TimeSec = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 4]);
                            Console.WriteLine("Sector 3 time is " + LastSector3TimeSec);
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
                        LastLapTimeSec = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 3]); // lap time in seconds
                        OldFastestLapTimeSec = FastestLapTimeSec; // Store the previous fastest lap in session
                        FastestLapTimeSec = (uDP.ParticipantStatsInfo[uDP.ViewedParticipantIndex, 2]);    // Retrieve fastest lap in session                 

                        // Is it a new session lap record? 
                        if (OldFastestLapTimeSec <= 0) { OldFastestLapTimeSec = 9999999999; } // if no record exists, set to 9999999999 to avoid null issues
                        if (LastLapTimeSec < OldFastestLapTimeSec)
                        {
                            SessionLapRecord = "Y";
                            //Console.WriteLine("NEW SESSION RECORD");
                        }
                        else
                        { SessionLapRecord = "N"; }

                        // Check if it's a new lap that should be sent to db
                        if (LastLapTimeSec > 0 && OldLapTimeSec != LastLapTimeSec && CurrSessionState != 4)  //TT only. excluding formation laps (untested). Should check for in/out laps too.
                        {
                            dbFetchRecord(); //get lap records from db
                            if (dbLapRecord <= 0) { dbLapRecord = 99999999999; }   // if no record exists in db, set to 9999999999 to avoid null issues
                            // Is it an All Time Record?
                            if (LastLapTimeSec < dbLapRecord)
                            {
                                if (LastLapValid == "Y")
                                {
                                    AllTimeRecord = "Y"; Console.WriteLine("* * * * * * * * * * N E W  L A P  R E C O R D * * * * * * * * *");
                                }
                                else { Console.WriteLine("New lap record, but unfortunately invalid lap"); }
                            }
                            else { AllTimeRecord = "N"; }
                            if (LastLapValid == "Y") // Only send valid laps to db
                            {
                                dbSendLapToDb(); // Send the lap to MariaDB
                            }
                            CurrentLapValid = "Y"; // Reset CurrentLapValid
                            OldLapTimeSec = LastLapTimeSec; // Store the last lap time in variable for later comparisons
                        }

                    } // end of 'a++'-loop

                } // end of 'if sector has changed'
            }
        }
    }
}

