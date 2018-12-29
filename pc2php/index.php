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
<link href="https://fonts.googleapis.com/css?family=VT323" rel="stylesheet">
<style>
<?php include 'main.css';?> 
</style>
</head>
<body>
<!-- Tablesorter script -->
<script src="https://code.jquery.com/jquery-1.10.2.js"></script>
<script src="tablesorter/jquery.tablesorter.min.js"></script> 

<?php
// Wrapper class to control page width etc. See main.css
echo "<div id='wrapper'>";

//**********************
// Connect to database *
//**********************	
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

/* Get variables, set wildcards if nothing choosen */
$trackselect = isset($_GET['trackselect']) ? $_GET['trackselect'] : "%%" ; //Sent through URL
$carselect = isset($_GET['carselect']) ? $_GET['carselect'] : "%%" ;
$classelect = isset($_GET['classelect']) ? $_GET['classelect'] : "%%" ;
$lbselect = isset($_GET['lbselect']) ? $_GET['lbselect'] : "%%" ; //leaderboard select, either AllTopTimes, TopPersonalTimes or AllPersonalTimes
$page = isset($_GET['page']) ? $_GET['page'] : "" ;
$selected = "";
$playeralreadyfound=0;
$_SESSION['trackselect'] = $trackselect;
$_SESSION['carselect'] = $carselect;
$_SESSION['classelect'] = $classelect;

//***********************************************
// Function to convert time format to 00:00.000 *
//*********************************************** 
function convertTo($init)
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
	$Time = "$seconds.$milli";
    return $Time;
}

/* Pagination variables */
$tbl_name="laptimes"; //database table name
$adjacents = 3; // How many adjacent pages should be shown on each side?

echo "<header>";

//*******
// LOGO *
//*******
echo "<div class='logo' id='logo'><img src='img/pcalc_logo.png' alt='<Project CALC logo'></div>";

//***********************
/* Location select box */
//***********************
echo "<div class='select'>";
echo "<form name='trackcarselect' METHOD='GET' ACTION='index.php'><select id='trackselect' name='trackselect' class='dropdown' width='200' style='width: 200px'>";

// Prepare sql statement, fetch data in array
$stmt = $mysqli->prepare("SELECT DISTINCT track FROM laptimes ORDER BY track ASC");  // Only select tracks in database once (DISTINCT)
$stmt->execute();
$result = $stmt->get_result();	
while ($row = $result->fetch_assoc()) {  // Loop through results, create track dropdown

    unset($track);
    $track = $row['track'];
	if ($track == $_GET['trackselect'] OR $track == $trackselect){  // if current row is the selected one...
		$selected = 'selected="selected"'; } //...then set it to 'selected'
	else {$selected="";}
					
    echo '<option value="'.$track.'" ' . $selected . '>'.$track.'</option>';
}						
	echo "</select>";
	if (!empty($_GET['trackselect'])) {
	$trackselect = $_GET['trackselect']; 
}
$stmt->close();
echo "<br></div>";


//**********************
/* vehicle select box */
//*********************
echo "<div class='select'><select id='carselect' name='carselect' class='dropdown' width='200' style='width: 200px'>";
echo "<option value='%%'>Any car</option>"; // Start with wildcards option, that shows laps for all cars at selected track

// Prepare statement and fetch data
$stmt = $mysqli->prepare("SELECT DISTINCT vehicle FROM laptimes ORDER BY vehicle ASC");
$stmt->execute();
$result = $stmt->get_result();
/* Loop through results and make rows */
while ($row = $result->fetch_assoc()) {  /* fetch the results into an array */

    unset($car);
    $car = $row['vehicle'];
	if ($car == $_GET['carselect'] OR $car == $carselect){
	$selected = 'selected="selected"'; }
	else {$selected="";}
					
    echo '<option value="'.$car.'" ' . $selected . '>'.$car.'</option>';
}
$stmt->close();
echo "</select></div>";

if (!empty($_GET['carselect'])) {
	$carselect = $_GET['carselect'];
}	



//**********************
/* class select box */
//*********************
echo "<div class='select'><select id='classelect' name='classelect' class='dropdown' width='200' style='width: 200px'>";
echo "<option value='%%'>Any class</option>"; // Start with wildcards option, that shows laps for all classes at selected track

// Prepare statement and fetch data
$stmt = $mysqli->prepare("SELECT DISTINCT vehicleclass FROM laptimes ORDER BY vehicleclass ASC");
$stmt->execute();
$result = $stmt->get_result();
/* Loop through results and make rows */
while ($row = $result->fetch_assoc()) {  /* fetch the results into an array */

    unset($class);
    $class = $row['vehicleclass'];
	if ($class == $_GET['classelect'] OR $class == $classelect){
	$selected = 'selected="selected"'; }
	else {$selected="";}
					
    echo '<option value="'.$class.'" ' . $selected . '>'.$class.'</option>';
}
$stmt->close();
echo "</select></div>";

if (!empty($_GET['classelect'])) {
	$classelect = $_GET['classelect'];
}	


// Buttons for selecting AllTopTimes, TopPersonalTimes or AllPersonalTimes (in different div tags)						
echo "<div class='button1'><button name='lbselect' type='submit' value='AllTopTimes'>Show leaderboard</button></div>
<div class='button2'><button name='lbselect' type='submit' value='TopPersonalTimes'>Top personal times</button>
<button name='lbselect' type='submit' value='AllPersonalTimes'>All personal times</button></form></div>";

//************************
// Search for playername * 
//************************
echo "<div class='search'><form action='/player.php' method='get'><input type='text' name='player' size='10' placeholder='search for player'><input type='submit' value='Search'>
</form></div>";

//*****************************************************************	
// Get gamertag: Prepare sql statement, bind parameters, fetch data
//***************************************************************** 
$id = 1; // Only 1 record in this table, namely the username
$stmt = $mysqli->prepare("SELECT username FROM user WHERE id = ?"); // Player gamertag is stored in table user with id=1. Set from pc2udp script, which reads the gamertag from UDP
$stmt->bind_param("i", $id); //i is for integer
$stmt->execute();
$stmt->bind_result($username);
$result = $stmt->fetch();
$stmt->close();
 
// Trying switch here as well, to get $personalonly in pagination code..
switch($_REQUEST['lbselect']) {
    case 'AllTopTimes': 
	$personalonly = "%%";  //  personal leaderboards not requested, add wildcards
		break;
	case 'TopPersonalTimes': 
	$personalonly = $username; //  personal leaderboards requested, add player name
		break;
	case 'AllPersonalTimes':
	$personalonly = $username; //  personal leaderboards requested, add player name
		break;
}

//*************	
// PAGINATION *
//*************
// Get total number of rows for selected leaderboard: Prepare sql statement, bind parameters, fetch data
// Player gamertag is stored in table user with id=1. Set from pc2udp script, which reads the gamertag from UDP
$query = "SELECT t1.* FROM laptimes t1
JOIN (
SELECT gamertag, track, vehicle, vehicleclass, MIN(laptime) AS min_laptime
FROM laptimes 
WHERE track = ? AND vehicle LIKE ? AND vehicleclass LIKE ?
GROUP BY gamertag, vehicle
) AS t2 ON t1.gamertag = t2.gamertag AND t1.laptime = t2.min_laptime AND t1.track = t2.track AND t1.vehicle = t2.vehicle AND t1.vehicleclass = t2.vehicleclass
ORDER BY laptime ASC";
$stmt = $mysqli->prepare($query);
$stmt->bind_param("sss",$trackselect,$carselect,$classelect); 
$stmt->execute();
$stmt->store_result();
$total_rows = $stmt->num_rows;
$stmt->close();

// Setup vars for pagination query. 
$targetpage = "index.php"; 	//your file name  (the name of this file)
$limit = 50; 								//how many items to show per page
$page = $_GET['page'];
if($page) 
	$offset = ($page - 1) * $limit; 			//first item to display on this page
else
	$offset = 0;								//if no page var is given, set offset to 0
// END OF PAGINATION 

//*****************
// Finding player *
//*****************
$stmt = $mysqli->prepare("SELECT t1.* FROM laptimes t1
JOIN (
SELECT gamertag, track, vehicle, vehicleclass, MIN(laptime) AS min_laptime
FROM laptimes 
WHERE track = ? AND vehicle LIKE ? AND vehicleclass LIKE ?
GROUP BY gamertag, vehicle
) AS t2 ON t1.gamertag = t2.gamertag AND t1.laptime = t2.min_laptime AND t1.track = t2.track AND t1.vehicle = t2.vehicle AND t1.vehicleclass = t2.vehicleclass
ORDER BY laptime ASC"); 
$stmt->bind_param("sss",$trackselect,$carselect,$classelect); //s is for string
$stmt->execute();
$result = $stmt->get_result();	
$oldplayerrecord = $playerrecord; // Store old lap record before fetching possibly new record
while ($row = $result->fetch_assoc()) {
	$a++; // variable to keep count of rows
	
	if ($row['gamertag'] == $username && $playeralreadyfound == 0) {
		$playerrow = $a; $playerrecord = $row['laptime']; $playeralreadyfound=1;  // set playerrow to $a and set flag to show that we found players best lap
		if ($playerrecord < $oldplayerrecord) {
			$playerrecordclass = 'newrecord';} // change color to purple (in main.css) if new record
		else {$playerrecordclass = 'nonewrecord';}
	}
	// set $worldrecord to laptime when $a=1
	if ($a == 1) {$worldrecord = $row['laptime']; $worldrecordholder = $row['gamertag'];}
}
// Get what page the player is on
$playerpage = (FLOOR($playerrow/$limit))+1;
// Reset player found-flag
$playeralreadyfound=0; 
$stmt->close();


//********************************
// Display player position 
//********************************
$lbselect = $_REQUEST['lbselect'];
if ($carselect == "%%") {$bestifalltoptimes = " best";}
	else {$bestifalltoptimes = "";}
// Only show player position if it exists
if ($playerrow != "" AND $lbselect=='AllTopTimes') {
	$pospercent = CEIL(($playerrow/$total_rows)*100);  // Calculate what top % player is in
	echo "<div class='position'>Your" . $bestifalltoptimes . " time: <strong>" . convertTo($playerrecord) . "</strong>. Your" . $bestifalltoptimes . " position: <strong>{$playerrow}</strong> out of <strong>{$total_rows}</strong>. Top {$pospercent}% ";
	// Only show link to player page if it's not on first page
	if (CEIL($total_rows/$limit) > 1 AND CEIL($playerrow/$limit) > 1 AND $playerrow > 3) {  // If total number of pages > 1 and players page > 1, then display pagelink
		echo "Goto page: <a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=$playerpage\">$playerpage</a>"; // Create link to page player is on
	} 
	elseif ($playerrow == 3){ echo "<strong>Bronze!</strong>";}
	elseif ($playerrow == 2){ echo "<strong>Silver!/<strong>";}
	elseif ($playerrow == 1){ echo "<span class='blink'><strong>WORLD RECORD!</strong></span>";}
	elseif ($playerrow < 6){ echo "<strong>Top five!</strong>";}
	elseif ($playerrow < 11){ echo "<strong>Top ten!</strong>";}
	else { echo "<strong>Front page!</strong>";}
	echo "</div>";
}	
if ($playerrow == "" || $playerrow == null) {
	echo "<div class='position'>Number of records: <strong>{$total_rows}</strong></div>";  // If no player times, show total number
	} 

//**********************************************************************************************************
//* Change $sqlstring based on user input (buttons Show Leaderboard, Top Personal laps, All Personal Laps) *
//**********************************************************************************************************
switch($_REQUEST['lbselect']) {

    case 'AllTopTimes': // Button Show Leaderboard is pressed = show normal leaderboard
		$personalonly = "%%";
		$stmt = $mysqli->prepare("SELECT t1.* FROM laptimes t1
JOIN (
SELECT gamertag, track, vehicle, vehicleclass, MIN(laptime) AS min_laptime
FROM laptimes 
WHERE track = ? AND vehicle LIKE ? AND vehicleclass LIKE ?
GROUP BY gamertag, vehicle, vehicleclass
) AS t2 ON t1.gamertag = t2.gamertag AND t1.laptime = t2.min_laptime AND t1.track = t2.track AND t1.vehicle = t2.vehicle AND t1.vehicleclass = t2.vehicleclass
ORDER BY laptime ASC LIMIT ? OFFSET ?");
		$stmt->bind_param("sssii", $trackselect,$carselect,$classelect,$limit,$offset); //s is for string, i integer
                break;
	
	case 'TopPersonalTimes': // Button Personal Top Laps is pressed = show best personal laps per car for selected track
		$personalonly = $username;
		$stmt = $mysqli->prepare("SELECT t1.* FROM laptimes t1
JOIN (
SELECT gamertag, track, vehicle, vehicleclass, MIN(laptime) AS min_laptime
FROM laptimes 
WHERE track = ? AND gamertag LIKE ?  
GROUP BY gamertag, vehicle, vehicleclass
) AS t2 ON t1.gamertag = t2.gamertag AND t1.laptime = t2.min_laptime AND t1.track = t2.track AND t1.vehicle = t2.vehicle AND t1.vehicleclass = t2.vehicleclass
ORDER BY laptime ASC LIMIT ? OFFSET ?");
		$stmt->bind_param("ssii", $trackselect,$username,$limit,$offset); //s is for string
                break;
	
	case 'AllPersonalTimes': // Button All Personal Laps is pressed = show all personal laps for selected car-track
		$personalonly = $username;		
		$stmt = $mysqli->prepare("SELECT * FROM laptimes WHERE track = ? AND vehicle LIKE ? AND vehicleclass LIKE ? AND gamertag LIKE ? ORDER BY laptime ASC LIMIT ? OFFSET ?");
		$stmt->bind_param("ssssii", $trackselect,$carselect,$classelect,$username,$limit,$offset); //s is for string
				break;
}
//*************************
// Go to Racing Mode Page *
//*************************
// echo "<span class='racingmodebutton'><a href='race.php'>Go Race Mode</a></span>";
echo "<span class='racingmodebutton'><a href='race.php'><button>Race Mode</button></a></span>";

// End of header
echo "</header>";

//************************************************************************
//* Start leaderboard table, with <thead> and <tbody> for sorting script *
//************************************************************************
echo "<table width=100% border=0 color=#000000 cellpadding='5' cellspacing='5' id='sortTable' class='tablesorter'>
<thead>
<tr>
<th>Rank</th>
<th>Player</th>
<th>Car</th>
<th>Class</th>
<th>Lap</th>
<th>Gap</th>
<th>Date</th>
<th><span class='tooltip'>S<span class='tooltiptext'>Setup</span></span></th>
<th><span class='tooltip'>C<span class='tooltiptext'>Controller</span></span></th>
<th><span class='tooltip'>P<span class='tooltiptext'>Platform</span></span></th>
</tr>
</thead>
<tbody>";

//***************
//* PAGINATION  *
//***************
/* Setup page vars for display. */
	if ($page == 0) {$page = 1;}					//if no page var is given, default to 1.
	$prev = $page - 1;							//previous page is page - 1
	$next = $page + 1;							//next page is page + 1
	$lastpage = ceil($total_rows/$limit);		//lastpage is = total pages / items per page, rounded up.
	$lpm1 = $lastpage - 1;						//last page minus 1
	//echo "Lastpage is " . $lastpage;
	
	/* 
		Now we apply our rules and draw the pagination object. 
		We're actually saving the code to a variable in case we want to draw it more than once.
	*/
	$pagination = "";
	if($lastpage > 1)
	{	
		$pagination .= "<div class=\"pagination\">";
		//previous button
		if ($page > 1) 
			$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=$prev\">previous</a>";
		else
			$pagination.= "<span class=\"disabled\">previous</span>";	
		
		//pages	
		if ($lastpage < 7 + ($adjacents * 2))	//not enough pages to bother breaking it up
		{	
			for ($counter = 1; $counter <= $lastpage; $counter++)
			{
				if ($counter == $page)
					$pagination.= "<span class=\"current\">$counter</span>";
				else
					$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=$counter\">$counter</a>";					
			}
		}
		elseif($lastpage > 5 + ($adjacents * 2))	//enough pages to hide some
		{
			//close to beginning; only hide later pages
			if($page < 1 + ($adjacents * 2))		
			{
				for ($counter = 1; $counter < 4 + ($adjacents * 2); $counter++)
				{
					if ($counter == $page)
						$pagination.= "<span class=\"current\">$counter</span>";
					else
						$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=$counter\">$counter</a>";					
				}
				$pagination.= "...";
				$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=$lpm1\">$lpm1</a>";
				$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=$lastpage\">$lastpage</a>";		
			}
			//in middle; hide some front and some back
			elseif($lastpage - ($adjacents * 2) > $page && $page > ($adjacents * 2))
			{
				$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=1\">1</a>";
				$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=2\">2</a>";
				$pagination.= "...";
				for ($counter = $page - $adjacents; $counter <= $page + $adjacents; $counter++)
				{
					if ($counter == $page)
						$pagination.= "<span class=\"current\">$counter</span>";
					else
						$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=$counter\">$counter</a>";					
				}
				$pagination.= "...";
				$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=$lpm1\">$lpm1</a>";
				$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=$lastpage\">$lastpage</a>";		
			}
			//close to end; only hide early pages
			else
			{
				$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=1\">1</a>";
				$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=2\">2</a>";
				$pagination.= "...";
				for ($counter = $lastpage - (2 + ($adjacents * 2)); $counter <= $lastpage; $counter++)
				{
					if ($counter == $page)
						$pagination.= "<span class=\"current\">$counter</span>";
					else
						$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=$counter\">$counter</a>";					
				}
			}
		}
		
		//next button
		if ($page < $counter - 1) 
			$pagination.= "<a href=\"$targetpage?trackselect={$trackselect}&carselect={$carselect}&classelect={$classelect}&lbselect={$lbselect}&page=$next\">next</a>";
		else
			$pagination.= "<span class=\"disabled\">next</span>";
		$pagination.= "</div>\n";		
	}
/* END OF PAGINATION */

//**************************************
/* Loop through results and make rows */
//**************************************
$stmt->execute();
$result = $stmt->get_result();	
while ($row = $result->fetch_assoc()) 
{
	/* Make alternate row with different background (main.css) */
	$x++; 
	$class = ($x%2 == 0)? 'whiteBackground': 'grayBackground'; 
	$rank = $x + $offset; //offset used for pagination
	
// Check if it's the playerrow, or podium row	
if ($row['gamertag'] == $username && $lbselect == 'AllTopTimes') {$class='player';} // Set row with player name to class="player' if it's on normal leaderboard	
if ($rank == 1) {$class='gold'; } // $trophy="<img width='20' height='10' src=gold.png>";
if ($rank == 2) {$class='silver';}
if ($rank == 3) {$class='bronze';}
if ($rank > 3 ) {$trophy="";}

echo "<tr class='$class'>";
echo "<td>" . $rank . ". $trophy</td>";
echo "<td><a href='player.php?player=" . $row['gamertag'] . "'>" . $row['gamertag'] . "</a></td>"; // link to players complete laps
echo "<td><a href='index.php?trackselect={$trackselect}&carselect={$row['vehicle']}&lbselect=AllTopTimes'>" . $row['vehicle'] . "</a></td>"; // link to leaderboard for that car
echo "<td><a href='index.php?trackselect={$trackselect}&carselect=%%&classelect={$row['vehicleclass']}&lbselect=AllTopTimes'>" . $row['vehicleclass'] . "</a></td>"; // link to leaderboard for that class
echo "<td><span class='tooltip'>" . convertTo($row['laptime']) . "<span class='tooltiptext'>S1:" . convertTo($row['sector1']) . " S2:" . convertTo($row['sector2']) . " S3:" . convertTo($row['sector3']) . "</span></span></td>"; /*convertTo-function formats time 00:00.000 */
$lapTime = ($row['laptime']);
if ($rank == 1) {$_SESSION['topTime'] = $lapTime;} // $rank=1 means it's the toptime, save it in Session variable, to calculate GAP on other pages 
$diff = ($lapTime) - ($_SESSION['topTime']); // Calculate GAP to top time
echo "<td>+" . convertToGap($diff) . "</td>";
echo "<td>" . substr($row['lapdate'],0,16) . "</td>";
echo "<td>" . substr($row['setup'],0,1) . "</td>";
echo "<td>" . substr($row['controller'],0,1) . "</td>";
echo "<td>" . $row['platform'] . "</td>";
echo "</tr>";
}
echo "</tbody></table>";  
//*********************
// Display pagination *
//*********************
if ($lbselect == "AllTopTimes") // Only show pagination buttons on normal leaderboard, not on personal
{
echo $pagination;
}


// Close db connection
$stmt->close();
?>

<!-- Tablesorter script -->
<script>
$(document).ready(function() {
$("#sortTable").tablesorter();
}
);
</script>
<span class="footer">This page is not affiliated with Slightly Mad Studios.</span>
</div>
</body>
</html>