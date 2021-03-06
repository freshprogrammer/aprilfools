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
		if(GetInput("Cancel")==="Y") 		AppendCmd("CancelAllNewComands");
		if(GetInput("Kill")==="Y") 			AppendCmd("KillApplication");
		if(GetInput("Pause")==="Y") 		AppendCmd("PausePranking");
		if(GetInput("Bomb")==="Y") 			AppendCmd("PlayBombBeeping");
		if(GetInput("HideCursor")==="Y") 	AppendCmd("MoveCursorToRandomCorner");
		if(GetInput("EraticM5")==="Y") 		AppendCmd("RunEraticMouse5s");
		if(GetInput("EraticM10")==="Y") 	AppendCmd("RunEraticMouse10s");
		if(GetInput("EraticM20")==="Y") 	AppendCmd("RunEraticMouse20s");
		if(GetInput("WanderM5")==="Y") 		AppendCmd("RunWanderMouse5s");
		if(GetInput("WanderM10")==="Y") 	AppendCmd("RunWanderMouse10s");
		if(GetInput("WanderM20")==="Y") 	AppendCmd("RunWanderMouse20s");
		if(GetInput("EraticK5")==="Y") 		AppendCmd("RunEraticKeyboard5s");
		if(GetInput("EraticK10")==="Y") 	AppendCmd("RunEraticKeyboard10s");
		if(GetInput("EraticK20")==="Y") 	AppendCmd("RunEraticKeyboard20s");
		if(GetInput("Map1k")==="Y") 		AppendCmd("MapNext1Key");
		if(GetInput("Map5k")==="Y") 		AppendCmd("MapNext5Keys");
		if(GetInput("Map10k")==="Y") 		AppendCmd("MapNext10Keys");
		if(GetInput("StopKeyMap")==="Y") 	AppendCmd("StopMappingAllKeys");
		if(GetInput("RandomPopup")==="Y")	AppendCmd("CreateRandomPopup");
		if(GetInput("Play_A")==="Y")		AppendCmd("PlaySound_Asterisk");
		if(GetInput("Play_B")==="Y")		AppendCmd("PlaySound_Beep");
		if(GetInput("Play_E")==="Y")		AppendCmd("PlaySound_Exclamation");
		if(GetInput("Play_H")==="Y")		AppendCmd("PlaySound_Hand");
		if(GetInput("Play_Q")==="Y")		AppendCmd("PlaySound_Question");
		if(GetInput("Play_A3")==="Y")		AppendCmd("PlaySound_Asterisk3X");
		if(GetInput("Play_H3")==="Y")		AppendCmd("PlaySound_Hand3X");
		if(GetInput("Play_E3")==="Y")		AppendCmd("PlaySound_Exclamation3X");
		if(GetInput("Flicker5")==="Y")		AppendCmd("FlickerScreen0_5_Times");
		//schedules
		if(GetInput("Schedule")==="C") 		AppendCmd("ClearSchedule");
		if(GetInput("Schedule")==="SE") 	AppendCmd("CreateSchedule_SuperEasy");
		if(GetInput("Schedule")==="E") 		AppendCmd("CreateSchedule_Easy");
		if(GetInput("Schedule")==="M") 		AppendCmd("CreateSchedule_Medium");
		if(GetInput("Schedule")==="M1") 	AppendCmd("CreateSchedule_Medium_SingleKeySwaps");
		if(GetInput("Schedule")==="M2") 	AppendCmd("CreateSchedule_Medium_DoubleKeySwaps");
		if(GetInput("Schedule")==="M3") 	AppendCmd("CreateSchedule_Medium_PlusSome");
	}
	
	function CreateControlButtons()
	{
		//this must match the handle input function
		CreateAControlButton("Cancel"			,"Cancel");
		CreateAControlButton("Kill"				,"Kill","Y", true);
		CreateAControlButton("Pause"			,"Pause");
		CreateScheduleSelector();
		CreateAControlButton("Hide Cursor"		,"HideCursor");
		CreateAControlButton("Eratic mouse 5s"	,"EraticM5");
		CreateAControlButton("Eratic mouse 10s"	,"EraticM10");
		//CreateAControlButton("Eratic mouse 20s","EraticM20");
		CreateAControlButton("Wander mouse 5s"	,"WanderM5");
		CreateAControlButton("Wander mouse 10s"	,"WanderM10");
		//CreateAControlButton("Wander mouse 20s","WanderM20");
		CreateAControlButton("Rnd keys 5s"		,"EraticK5");
		CreateAControlButton("Rnd keys 10s"		,"EraticK10");
		//CreateAControlButton("Rnd keys 20s"	,"EraticK20");
		CreateAControlButton("Map next key"		,"Map1k");
		CreateAControlButton("Map next 5 keys"	,"Map5k");
		CreateAControlButton("Map next 10 keys"	,"Map10k");
		CreateAControlButton("Stop mapping keys","StopKeyMap");
		//CreateAControlButton("Random popup"		,"RandomPopup");
		CreateAControlButton("Play asterisk"   	,"Play_A");
		CreateAControlButton("Play beep"       	,"Play_B");
		CreateAControlButton("Play hand"       	,"Play_H");
		CreateAControlButton("Play exclamation"	,"Play_E");
		CreateAControlButton("Play asterisk 3x" ,"Play_A3");
		CreateAControlButton("Play hand 3x"     ,"Play_H3");
		CreateAControlButton("Play exclamation 3x","Play_E3");
		//CreateAControlButton("Play question"   ,"Play_Q");//doesnt work
		//CreateAControlButton("Bomb beep"		,"Bomb");
		CreateAControlButton("Flicker Screen 5x","Flicker5");
	}
	
	function CreateScheduleSelector()
	{
		echo "	<form name='scheduleForm' action='' method=post style='display: inline-block;'>\n";
		echo "		<select name='Schedule' id='scheduleSelect' onchange='this.form.submit()'>\n";
		echo "			<option value=''>Change Schedule...</option>\n";
		echo "			<option value=''>-</option>\n";
		echo "			<option value='C'>Clear</option>\n";
		echo "			<option value='SE'>XEasy schedule</option>\n";
		echo "			<option value='E'>Easy</option>\n";
		echo "			<option value='M'>Medium</option>\n";
		echo "			<option value='M1'>Medium_SingleKeySwaps</option>\n";
		echo "			<option value='M2'>Medium_DoubleKeySwaps</option>\n";
		echo "			<option value='M3'>Medium_PlusSome</option>\n";
		echo "		</select>\n";
		echo "	</form>\n";
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
		echo "<a href=".">Refresh</a>\n";
		CreateControlButtons();
		echo "<hr>\n";
		echo "</div>\n";
		echo "<div style='overflow: auto; width:*;'>\n";
		echo str_replace($cmdSeperationTag, "<BR>\n", $curSchedule);
		echo "</div>\n";
		
		//WriteSchedule($newSchedule);
		//$schedule = ReadSchedule();
	}
	
	function CreateAControlButton($dispName,$fieldName,$value='Y',$confirm=false)
	{
		if($confirm)
			echo "	<form name='' action='' method=post style='display: inline-block;' onsubmit=\"return confirm('Are you sure you want to do this?')\">\n";
		else
			echo "	<form name='' action='' method=post style='display: inline-block;'>\n";
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