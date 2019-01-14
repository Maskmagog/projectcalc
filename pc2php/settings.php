<!-- ******************************************
// Project CALC - by Martin Holmström
// maskmagog@gmail.com
// https://github.com/Maskmagog/projectcalc
//
// Feel free to use the program(s) 
// but don't make money on it.
// Change/adapt/modify the code as you want
// but keep these lines. Thank you.
//***************************************** -->
<?php
session_start(); // Start php session to keep session variables
//$sessionfile = ini_get('session.save_path') . '/' . 'sess_'.session_id();  
//echo 'session file: ', $sessionfile, ' ';  
//echo 'size: ', filesize($sessionfile), "\n";
?>
<html>
<head>
<title>Project CALC - Community Assisted Leaderboards for Consoles - Project CARS 2</title>
<link rel="shortcut icon" href="img/favicon.ico" type="image/x-icon"> 
<style>
<?php include 'main.css';?> 
</style>
</head>
<body>
<?php
// Wrapper class to control page width etc. See main.css
echo "<div id='wrapper'>";


// Connect to database
$host ="127.0.0.1"; //Don't use localhost for some reason
$user = "pcars";
$pass = "PG3Dnq4m2BVFaaLC";
$db = "pcarsdb";

$mysqli = new mysqli($host,$user,$pass,$db);
/* check connection */
if (mysqli_connect_errno())
{
echo "Failed to connect to MySQL";
}
//*******
// LOGO *
//*******
echo "<div class='logo' id='logo'><img src='img/pcalc_logo.png' alt='<Project CALC logo'></div>";

// Get current settings
$stmt = $mysqli->prepare("SELECT * FROM settings");
$stmt->execute();
$result = $stmt->get_result();
$row = $result->fetch_assoc();

echo "These settings control what laps will be stored in the database. It will not affect laptimes already stored. It will not affect the laptimes fetched from cars2-stats-steam.wmdportal.com<br>";
// Radio buttons for TT and valid laps
echo "<div class='settings'><form name='ttsettings' action='settings.php' method='POST'>";
echo "<br>Time trial only ?<br><input type='radio' name='timetrialonly' "; if($row['timetrialonly']=="Y") { echo "checked"; } echo " value='Y'>YES<br><input type='radio' name='timetrialonly'"; if($row['timetrialonly']=="N") { echo "checked"; } echo " value='N'>NO";
echo "<br>Valid laps only?<br><input type='radio' name='validlapsonly' "; if($row['validlapsonly']=="Y") { echo "checked"; } echo " value='Y'>YES<br><input type='radio' name='validlapsonly'"; if($row['validlapsonly']=="N") { echo "checked"; } echo " value='N'>NO";
echo "<br><input type='submit' name='ttsubmit' value='Submit'>";
//End form
echo "</form>";
// Send to MariaDB
if (isset($_POST['ttsubmit'])) {
$sql = ("UPDATE settings SET timetrialonly='{$_POST['timetrialonly']}', validlapsonly='{$_POST['validlapsonly']}' WHERE id=1");
if(mysqli_query($mysqli, $sql)){
    echo "<br><br>Records were updated successfully.";
	echo "<meta http-equiv='refresh' content='0'>";
} else {
    echo "<br><br>ERROR: Could not update database";
}
}
echo "<br><br>-----------------------------------------<br>";

// Controller and platform
echo "<div class='settings'><form name='gamesettings' action='settings.php' method='POST'>";
echo "<br>Here you can set your platform and controller type.<br>You can also set your normal setup: Default, Custom or Mixed depending on your habits.<br>";

echo "<select id='platformselect' name='platformselect' class='dropdown' style='width: 90px'>";
if ($row['platform'] == 'XB1') {$selected = " selected='selected' ";} else { $selected ="";}
echo "<option value='XB1' . $selected>XB1</option>";
if ($row['platform'] == 'PS4') {$selected = " selected='selected' ";}  else { $selected ="";}
echo "<option value='PS4' . $selected>PS4</option>";
if ($row['platform'] == 'PC') {$selected = " selected='selected' ";}  else { $selected ="";}
echo "<option value='PC' . $selected>PC</option></select><br>";

echo "<select id='controllerselect' name='controllerselect' class='dropdown' style='width: 90px'>";
if ($row['controller'] == 'W') {$selected = " selected='selected' ";} else { $selected ="";}
echo "<option value='W' . $selected>Wheel</option>";
if ($row['controller'] == 'G') {$selected = " selected='selected' ";} else { $selected ="";}
echo "<option value='G' . $selected>Gamepad</option>";
if ($row['controller'] == 'K') {$selected = " selected='selected' ";} else { $selected ="";}
echo "<option value='K' . $selected>Keyboard</option></select><br>";

echo "<select id='setupselect' name='setupselect' class='dropdown' style='width: 90px'>";
if ($row['setup'] == 'D') {$selected = " selected='selected' ";} else { $selected ="";}
echo "<option value='D' . $selected>Default</option>";
if ($row['setup'] == 'C') {$selected = " selected='selected' ";} else { $selected ="";}
echo "<option value='C' . $selected>Custom</option>";
if ($row['setup'] == '?') {$selected = " selected='selected' ";} else { $selected ="";}
echo "<option value='M' . $selected>Mixed</option></select><br>";
// Submit nutton for game settings
echo "<br><input type='submit' name='gamesubmit' value='Submit'>";
//End form
echo "</form>";
// Send to MariaDB
if (isset($_POST['gamesubmit'])) {
$sql = ("UPDATE settings SET platform='{$_POST['platformselect']}', controller='{$_POST['controllerselect']}', setup='{$_POST['setupselect']}' WHERE id=1");
if(mysqli_query($mysqli, $sql)){
    echo "<br><br>Records were updated successfully.";
	echo "<meta http-equiv='refresh' content='0'>";
} else {
    echo "<br><br>ERROR: Could not update database";
}
}

// Return to leaderboard link
echo "<br><br><a href='index.php?trackselect={$_SESSION['trackselect']}&carselect={$_SESSION['carselect']}&classelect={$_SESSION['classelect']}&lbselect=AllTopTimes'><button>Back to leaderboard</button></a><br><br>";

//Close connection
mysqli_close($conn);

?>
</body>
</html>