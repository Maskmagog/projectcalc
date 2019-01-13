<!-- ******************************************
// Project CALC - by Martin Holmström
// maskmagog@gmail.com
// https://github.com/Maskmagog/projectcalc
//
// Feel free to use the program(s) 
// but don't make money on it.
// Change/adapt/modify the code as you want
// but keep these lines. Thank you.
// test
//***************************************** -->
<html>
<head>
<title>Project CALC - Community Assisted Leaderboards for Consoles - Project CARS 2</title>
<link rel="shortcut icon" href="img/favicon.ico" type="image/x-icon"> 
<style>
<?php include 'main.css'; ?>
</style>
</head>
<body>
<!-- Tablesorter script -->
<script src="https://code.jquery.com/jquery-1.10.2.js"></script>
<script src="tablesorter/jquery.tablesorter.min.js"></script>
<?php
echo "<div id='wrapper'>";

// LOGO
echo "<div class='logo'><img src='img/pcalc_logo.png' alt='<Project CALC logo'></div>";

/* Function to convert time format to 00:00.000 */
function convertTo($init)
{
      $secs = floor($init);
      $milli = (int) (($init - $secs) * 1000);
	  $milli = str_pad($milli, 3, '0', STR_PAD_LEFT);
      $hours = ($secs / 3600);
      $minutes = (($secs / 60) % 60);
	  $minutes = str_pad($minutes, 2, '0', STR_PAD_LEFT);
      $seconds = $secs % 60;
	  $seconds = str_pad($seconds, 2, '0', STR_PAD_LEFT);	  
	  $stageTime = "$minutes:$seconds.$milli";
      return $stageTime;
    }

/* Connect to database */	
$host ="127.0.0.1";
$user = "pcars";
$pass = "PG3Dnq4m2BVFaaLC";
$db = "pcarsdb";
$mysqli = new mysqli($host,$user,$pass,$db);
/* check connection */
if (mysqli_connect_errno()) {
    printf("Connect failed");
    exit();
}

// Return to leaderboard button
echo "<button onclick='window.history.go(-1); return false;'>Return to leaderboard</button>";

/* Start table, with <thead> and <tbody> for sorting script*/
echo "<table width=100% border=0 color=#000000 cellpadding='5' id='sortTable' class='tablesorter'>
<thead>
<tr>
<th>Track</th>
<th>Player</th>
<th>Car</th>
<th>Lap</th>
<th>Date</th>
<th>Mode</th>
</tr>
</thead>
<tbody>";

// Prepare statement and fetch data
$stmt = $mysqli->prepare("SELECT * FROM laptimes WHERE gamertag = ? ORDER BY track, laptime ASC");
$stmt->bind_param("s", $_GET['player']);
$stmt->execute();
$result = $stmt->get_result();
/* Loop through results and make rows */
while ($row = $result->fetch_assoc()) {  /* fetch the results into an array */

	/* Make alternate row with different background (main.css) */
	$x++; 
	$class = ($x%2 == 0)? 'whiteBackground': 'grayBackground';
	echo "<tr class='$class'>";
	echo "<td><a href='index.php?trackselect=" . $row['track'] . "&carselect=%%&lbselect=AllTopTimes'>" . $row['track'] . "</a></td>";
	echo "<td>" . $row['gamertag'] . "</td>";
	echo "<td><a href='index.php?trackselect=" . $row['track'] . "&carselect=" . $row['vehicle'] . "&lbselect=AllTopTimes'>" . $row['vehicle'] . "</a></td>";
	echo "<td><div class='tooltip'>" . convertTo($row['laptime']) . "<span class='tooltiptext'>S1:" . convertTo($row['sector1']) . " S2:" . convertTo($row['sector2']) . " S3:" . convertTo($row['sector3']) . "</td>"; /*convertTo-function formats time 00:00.000 */
	echo "<td>" . substr($row['lapdate'],0,16) . "</td>";
	echo "<td>" . $row['sessionmode'] . "</td>";
	echo "</tr>";
}
echo "</tbody></table>";   

/* close statement */
$stmt->close();

/* close connection */
$mysqli->close();

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