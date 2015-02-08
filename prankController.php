<?php
	//globals
	$scheduleFileName = "prankSchedule.txt";
	$activeSchedule = "prankSchedule.txt";
	$newCmdTag = "_NEW_";
	$cmdSeperationTag = "\n";

	Main();
	
	function Main()
	{
		HandleInput();
		/*
		$newSchedule = "TESTING";
		for ($i = 1; ; $i++) {
		    if ($i > 100) {
		        break;
		    }
		    $newSchedule .= "T#".$i."<BR>";
		}
		$newSchedule .= "TESTING_END";
		
		WriteSchedule($newSchedule);*/
		
		BuildPage();
	}
	
	function HandleInput()
	{
		//global $activeSchedule;
		//$activeSchedule = GetInput("activeSchedule");

		//handle schedule upload from app
		if(GetInput("upload")==="Y")
			WriteSchedule(GetInput("uploaddata"));
		
		//process button commands
		//this must match the handle input function as well as the PrankerEvent enum names in the app code
		//these must be else ifs so it is limited to 1 new cmd 
		if(GetInput("Kill")==="Y") AppendCmd("KillApplication");
		if(GetInput("Stop")==="Y") AppendCmd("PauseApplication");
		if(GetInput("Beep")==="Y") AppendCmd("Beep");
		if(GetInput("Rebuild")==="Y") AppendCmd("RebuildSchedule");
	}
	
	function CreateControlButtons()
	{
		//this must match the handle input function
		CreateAControlButton("Refresh","");
		CreateAControlButton("Stop","Stop");
		CreateAControlButton("Beep","Beep");
		CreateAControlButton("Rebuild Schedule","Rebuild");
	}
	
	function BuildPage()
	{
		global $cmdSeperationTag;
		//commented raw schedule
		//control buttons
		//	line
		//current schedule
		$curSchedule = ReadSchedule();
		echo "<!--START_CMDS\n";
		echo $curSchedule;
		echo "\nEND_CMDS-->\n";
		echo "<div>\n";
		CreateControlButtons();
		echo "<hr>\n";
		echo "</div>\n";
		echo "<div style='overflow: auto; width:*; height: 90vh;'>\n";
		echo str_replace($cmdSeperationTag, "<BR>\n", $curSchedule);
		echo "</div>\n";
		
		//WriteSchedule($newSchedule);
		//$schedule = ReadSchedule();
	}
	
	function CreateAControlButton($dispName,$fieldName,$value='Y')
	{
		echo "	<form name='' action='' method=get style='display: inline-block;'>\n";
		echo "	<input type='submit' value='$dispName'>\n";
		echo "	<input type='hidden'  name='$fieldName' value='$value'>\n";
		echo "	</form>\n";
	}
	
	function AppendCmd($newCmd)
	{
		global $newCmdTag;
		global $cmdSeperationTag;
		$newCmd = $newCmdTag . $newCmd . $cmdSeperationTag;
		$curSchedule = ReadSchedule();
		WriteSchedule($newCmd.$curSchedule);
	}
	
	function ReadSchedule()
	{
		global $scheduleFileName;
		$data = 'NONE';
		if (file_exists($scheduleFileName)) {
			$data = file_get_contents($scheduleFileName);
		} else {
			file_put_contents($scheduleFileName, $data);
		}
		return $data;
	}
	
	function WriteSchedule($input)
	{
        global $scheduleFileName;
	    file_put_contents($scheduleFileName, $input);
	}
	
	function GetInput($name)
	{
		if(isset($_POST[$name]))
		{
			$input = $_POST[$name];
		}
		else if(isset($_GET[$name]))
		{
			$input = $_GET[$name];
		}
		else
		{
			$input = "";
		}
		return trim($input);
	}
?>