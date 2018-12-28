<?php
session_start();?> 
<html>
<head>
<title>Project CALC - Community Assisted Leaderboards for Consoles - Project CARS 2</title>
<link rel="shortcut icon" href="img/favicon.ico" type="image/x-icon"> 
<style>
<?php include 'main.css';?>
</style>
<meta http-equiv='refresh' content='15'>
</head>
<body>
<?php
//$sessionfile = ini_get('session.save_path') . '/' . 'sess_'.session_id();  
//echo 'session file: ', $sessionfile, ' ';  
//echo 'size: ', filesize($sessionfile), "\n";
// Wrapper class to control page width etc. See main.css
echo "<div id='racemodewrapper'>";

//**********************
// Connect to database *
//**********************	
$host ="127.0.0.1"; //Don't use localhost for some reason
$user = "pcars";
$pass = "PG3Dnq4m2BVFaaLC";
$db = "pcarsdb";
$mysqli = new mysqli($host,$user,$pass,$db);
/* check connection */
if (mysqli_connect_errno()) {
    printf("Connect failed");
    exit();
}

// Set 'rivalfound' to 0
$playeralreadyfound = 0;
for ($i = 0; $i < $_SESSION['rivalnr']; $i++){
$rivalfound[$i] = 0;
}

// Declare rival variables as arrays
$rivalrow = array();
$rivalrecord = array();
$_SESSION['rival'] = array();
$rivalfound = array();
$rivaldiff = array();
 

// Check for car or track change; add new rivals if so
if ($_SESSION['oldtrackselect'] != $_SESSION['trackselect'] OR $_SESSION['oldcarselect'] != $_SESSION['carselect']) {
	for ($i = 0; $i < $_SESSION['rivalnr']; $i++){
	$_SESSION['rival'][$i] = "";
}	
}

//***********************************************
// Function to convert time format to 00:00.000 *
//*********************************************** 
function convertTo($init)
{
	 //echo " init1 is " . $init;
     $init  = number_format($init, 3);
	 //echo " init2 is " . $init;
    $secs  = floor($init);
	// echo " secs is " . $secs;
    $milli = (($init - $secs) * 1000);
	//echo " milli1 is " . $milli;
	$milli=round($milli);
	// echo " milli2 is " . $milli;
    $milli = str_pad($milli, 3, '0', STR_PAD_LEFT);
     //echo " milli3 is " . $milli;
    $hours   = ($secs / 3600);
    $minutes = (($secs / 60) % 60);
    $minutes = str_pad($minutes, 2, '0', STR_PAD_LEFT);
    $seconds = $secs % 60;
    $seconds = str_pad($seconds, 2, '0', STR_PAD_LEFT);
    if ($hours > 1) {
        $hours = str_pad($hours, 2, '0', STR_PAD_LEFT);
    } else {
        $hours = '00';
    }
    $Time = "$minutes:$seconds.$milli";
    return $Time;
}

function convertToGap($init)
{
	//echo " init1 is " . $init;
     $init  = number_format($init, 3);
	// echo " init2 is " . $init;
    $secs  = floor($init);
	// echo " secs is " . $secs;
    $milli = (($init - $secs) * 1000);
	//echo " milli1 is " . $milli;
	$milli=round($milli);
	// echo " milli2 is " . $milli;
    $milli = str_pad($milli, 3, '0', STR_PAD_LEFT);
    // echo " milli3 is " . $milli;
    $hours   = ($secs / 3600);
    $minutes = (($secs / 60) % 60);
	$minutes = str_pad($minutes, 2, '0', STR_PAD_LEFT);
    $seconds = ($secs % 60) + ($minutes * 60); /* Add 60s per minute ('gap' only shown in seconds.milli) */
	$seconds = str_pad($seconds, 2, '0', STR_PAD_LEFT);	  /* If more seconds digits wanted, change the '2' to '3' */
	$stageTime = "$seconds.$milli";
    return $stageTime;
}
//***********************
// Get username from db *
//***********************
$id = 1; // Only 1 record in this table, namely the username
$stmt = $mysqli->prepare("SELECT username FROM user WHERE id = ?"); // Player gamertag is stored in table user with id=1. Set from pc2udp script, which reads the gamertag from UDP
$stmt->bind_param("i", $id); //i is for integer
$stmt->execute();
$stmt->bind_result($username);
$result = $stmt->fetch();
$stmt->close();

//************************************
// Get current car and track from db *
//************************************
$stmt = $mysqli->prepare("SELECT currenttrack, currentvehicle FROM cartrackdb "); // Current car & track is stored in table cartrackdb. Sent from pc2udp script, which reads the data from UDP
$stmt->execute();
$stmt->bind_result($currenttrack, $currentvehicle);
$result = $stmt->fetch();
$stmt->close();


// Check for track-car changes
// Add check to keep it from checking all the time
if ($_SESSION['autoupdate'] == 'on') {
	if ($currenttrack != $_SESSION['trackselect'] || $currentvehicle != $_SESSION['carselect']) {
		echo "Changing to " . $currenttrack . " and " . $currentvehicle;
		$_SESSION['trackselect'] = $currenttrack; //Set track to track from db
		$_SESSION['carselect'] = $currentvehicle; // Set car to car from db
		for ($i = 0; $i < $_SESSION['rivalnr']; $i++){
		$_SESSION['rival'][$i] = "";
		//echo "<meta http-equiv='refresh' content='0'>"; //Force refresh of page
	}
	}
}

// Get new rivals 
for ($i = 0; $i < $_SESSION['rivalnr']; $i++){
	$_SESSION['rival'][$i] = "";
}


//***************************************
// Finding player and worldrecordholder *
//***************************************
// This select gets lowest laptimes from players per car-track combo, accepting multiple player entries if in different cars (if 'Any car' is selected)
$stmt = $mysqli->prepare("SELECT t1.* FROM laptimes t1
JOIN (
SELECT gamertag, track, vehicle, MIN(laptime) AS min_laptime
FROM laptimes 
WHERE track = ? AND vehicle LIKE ?
GROUP BY gamertag, vehicle
) AS t2 ON t1.gamertag = t2.gamertag AND t1.laptime = t2.min_laptime AND t1.track = t2.track AND t1.vehicle = t2.vehicle
ORDER BY laptime ASC"); 
$stmt->bind_param("ss",$_SESSION['trackselect'],$_SESSION['carselect']); //s is for string
$stmt->execute();
$result = $stmt->get_result();	
$oldplayerrecord = $playerrecord; // Store old lap record before fetching possible new record
while ($row = $result->fetch_assoc()) {
	$a++; // variable to keep count of rows
	
	// set $worldrecord to laptime when $a=1
	if ($a == 1) {$worldrecord = $row['laptime']; $worldrecordholder = $row['gamertag'];}
	// Check for player
	if ($row['gamertag'] == $username && $playeralreadyfound == 0) {
		$playerrow = $a; $playerrecord = $row['laptime']; $playeralreadyfound=1;  // set playerrow to $a and set flag to show that we found players best lap
		if ($playerrecord < $oldplayerrecord) {
			$playerrecordclass = 'newrecord';} // change color to green (in main.css) if new record (not working? oldplayerrecord always 0?)
		if ($playerrecord < $worldrecord) {
			$playerrecordclass = 'newworldrecord';} // change color to purple (in main.css) if new wr (not working?)
		}
}

// Reset variables
$a=0;
$playerfound = 0;
for ($i = 0; $i < $_SESSION['rivalnr']; $i++){
$rivalfound[i] = 0;
}

//**************************************************************************************************
// If no playertime is set for current car-track combo, set playerrow to +1 compared to last place *
//**************************************************************************************************
if ($playerrecord == 0 || $playerrecord == null) {
	// Get total number of rows
	$stmt->execute();
	$stmt->store_result();
	$total_rows = $stmt->num_rows;
	$playerrow = ($total_rows + 1);
}

$a=0;
$b=$_SESSION['rivalnr'];

//******************************************
// Get the rivals ahead on the leaderboard *
//******************************************
// Don''t get rivals if they exist (manually set)
if ($_SESSION['rival'][$i] == "" || $_SESSION['rival'][$i] == null) {
	
	//Get rival rows
	for ($i = 0; $i < $_SESSION['rivalnr']; $i++){
	
	$rivalrow[$i] = ($playerrow-$b); // $b places ahead of player
	$b--; //decrease $b so next rival is one step closer to player
	$stmt->execute(); // not sure if this step is necessary here
	$result = $stmt->get_result();	// not sure if this step is necessary here
	while ($row = $result->fetch_assoc()) {
		$a++; // variable to keep count of rows
		if($a == $rivalrow[$i]) {
			$rivalrow[$i] = $a; $rivalrecord[$i] = $row['laptime']; $_SESSION['rival'][$i] = $row['gamertag'];}
}	
$a=0; //Reset for next looping
}	
}	

// Reset	
$a=0;
$b=$_SESSION['rivalnr'];

//*****************	
// Calculate gaps *
//*****************
// WR gaps
if ($playerrecord > 0) {
$wrdiff=$playerrecord-$worldrecord;}
else {$wrdiff = $worldrecord-$playerrecord;}
// Rival gaps
for ($i = 0; $i < $_SESSION['rivalnr']; $i++){	
	if ($playerrecord > 0) {
		$rivaldiff[$i]=$playerrecord-$rivalrecord[$i];}
	else {$rivaldiff[$i] = $rivalrecord[$i]-$playerrecord;}
}

// Replace accented characters not supported by font
$search  = array('Á','À','É','È','Ú','Ó','Ò','Å','Ä','Ö','Ê','Ñ','ö','å','ä','é','è','á','à','ú','ó'); // Characters to look for
$replace = array('A','A','E','E','U','O','O','A','A','O','E','N','O','A','A','E','E','A','A','U','O'); //Replace with these

//Replace in rival names
for ($i = 0; $i < $_SESSION['rivalnr']; $i++){	
	$_SESSION['rival'][$i] = strtoupper($_SESSION['rival'][$i]);
	$_SESSION['rival'][$i] = str_replace($search, $replace, $_SESSION['rival'][$i]);
}
// Replace in worldrecordholders name
$worldrecordholder = strtoupper($worldrecordholder);
$worldrecordholder = str_replace($search, $replace, $worldrecordholder);

// Replace in player name
$playerdisplay = strtoupper($username);
$playerdisplay = str_replace($search, $replace, $playerdisplay);

// Set max lengths for names
$worldrecordholder = substr($worldrecordholder,0,20);
$playerdisplay = substr($playerdisplay,0,20);
for ($i = 0; $i < $_SESSION['rivalnr']; $i++){	
	$_SESSION['rival'][$i] = substr($_SESSION['rival'][$i],0, 20);
}

// Check if player position is less than number of available rivals.
if (($playerrow) <= ($_SESSION['rivalnr'] + 1) AND $playerrow != 1) // If there's more rivals selected than left on the leaderboard (and player is not WR)
{ 
	$_SESSION['rivalnr'] = ($playerrow - 2); // If player is at nr 5, then only display WR plus 3 more rival, so (5-2).
	echo "<meta http-equiv='refresh' content='0'>"; // refresh to update page with new nr of rivals
}

// Check if player has WR
if ($playerrow == 1)
{
	$_SESSION['rivalnr'] = 3;
	$_SESSION['rival'][0] = "W O R L D";
	$_SESSION['rival'][1] = "R E C O R D";
	$_SESSION['rival'][2] = "! ! ! ! ! !";
	$blink = "blink";
}

//*************************************
//* Start Racing Mode Table
//*************************************
echo "<div class='racepage' id='racepageid'>";
echo "<table id='racingtable' class='racingtable'>
<tr>
<td width='8%' align='right'>1. </td>
<td><div class='racepagename'>" . $worldrecordholder . "</div></td>
<td width='20%'>" . convertTo($worldrecord) . "</td>
<td width='20%'align='right'>" . convertToGap($wrdiff) . "</td>
</tr>";
for ($i = 0; $i < $_SESSION['rivalnr']; $i++){	
echo"<tr>
<td align='right'>" . $rivalrow[$i] . ".</td>
<td class=$blink>" . $_SESSION['rival'][$i] . "</td>
<td>" . convertTo($rivalrecord[$i]) . "</td>
<td align='right'>" . convertToGap($rivaldiff[$i]) . "</td>
</tr>";
}
echo "<tr class=" . $playerrecordclass . "> 
<td align='right'>" . $playerrow . ". </td> 
<td>" . $playerdisplay . "</td>
<td>" . convertTo($playerrecord) . "</td>
<td align='right'></td>
</tr>
</table>"; // End of table
echo "</div>";
$blink=""; // reset $blink

//*******
// LOGO *
//*******
echo "<div class='racelogo'><img src='img/pcalc_logo.png' alt='<Project CALC logo'></div>";

// Print current combo
echo "<span style='color:#ffffff'>Current track is " . $_SESSION['trackselect'] . ". Current car is " . $_SESSION['carselect'] . "</span>";

// Return to leaderboard link
echo "<br><br><span class='returnlink'><a href='index.php?trackselect={$_SESSION['trackselect']}&carselect={$_SESSION['carselect']}&lbselect=AllTopTimes'><button>Back to leaderboard</button></a></span><br><br>";

// Buttons to turn on/off autoupdate
echo "<span class='autoupdatebuttons'><form method='POST' action='race.php'>	
<input type='submit' name='autoupdate' value='Autoupdate on' />
<input type='submit' name='autoupdate' value='Autoupdate off' />
</form>";
// If autoupdate on, clear rivals and refresh
if ($_POST['autoupdate'] == 'Autoupdate on') {
	$_SESSION['autoupdate'] = 'on';
	for ($i = 0; $i < $_SESSION['rivalnr']; $i++){
	$_SESSION['rival'][$i] = "";
	}
	echo "<meta http-equiv='refresh' content='0'>";}
// If autoupdate off, refresh
if ($_POST['autoupdate'] == 'Autoupdate off') {
	$_SESSION['autoupdate'] = 'off';
	echo "<meta http-equiv='refresh' content='0'>";}
	
echo "<span style='color:#ffffff'>Autoupdate is " . $_SESSION['autoupdate'] . "</span></span>";

// Set nr of rivals
echo "<span class='setrivalnr'><form name='setrivalnr' METHOD='POST' ACTION='race.php'><input type='number' name='setrivalnr' min='0' max='19'><button type='submit'>Set nr of rivals</button></form>";
if (isset($_POST['setrivalnr'])) {
    $_SESSION['rivalnr'] = $_POST['setrivalnr'];
	echo "<meta http-equiv='refresh' content='0'>";
}

// Set track and vehicle
$_SESSION['oldtrackselect'] = $_SESSION['trackselect'];
$_SESSION['oldcarselect'] = $_SESSION['carselect'];


echo "</span>";
echo "<span class='footer'>This page is not affiliated with Slightly Mad Studios.</span>";
echo "</div>";