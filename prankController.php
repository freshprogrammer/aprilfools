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
		if(GetInput("Rebuild")==="Y") 		AppendCmd("RebuildSchedule");
		if(GetInput("Clear")==="Y") 		AppendCmd("ClearSchedule");
		if(GetInput("EraticM5")==="Y") 		AppendCmd("RunEraticMouse5s");
		if(GetInput("EraticM10")==="Y") 	AppendCmd("RunEraticMouse10s");
		if(GetInput("EraticM20")==="Y") 	AppendCmd("RunEraticMouse20s");
		if(GetInput("EraticK5")==="Y") 		AppendCmd("RunEraticKeyboard5s");
		if(GetInput("EraticK10")==="Y") 	AppendCmd("RunEraticKeyboard10s");
		if(GetInput("EraticK20")==="Y") 	AppendCmd("RunEraticKeyboard20s");
		if(GetInput("Map1k")==="Y") 		AppendCmd("MapNext1Key");
		if(GetInput("Map5k")==="Y") 		AppendCmd("MapNext5Keys");
		if(GetInput("Map10k")==="Y") 		AppendCmd("MapNext10Keys");
		if(GetInput("RandomPopup")==="Y")	AppendCmd("CreateRandomPopup");
		if(GetInput("Play_A")==="Y")		AppendCmd("PlaySound_Asterisk");
		if(GetInput("Play_B")==="Y")		AppendCmd("PlaySound_Beep");
		if(GetInput("Play_E")==="Y")		AppendCmd("PlaySound_Exclamation");
		if(GetInput("Play_H")==="Y")		AppendCmd("PlaySound_Hand");
		if(GetInput("Play_Q")==="Y")		AppendCmd("PlaySound_Question");
	}
	
	function CreateControlButtons()
	{
		//this must match the handle input function
		CreateAControlButton("Cancel","Cancel");
		CreateAControlButton("Kill","Kill","Y", true);
		CreateAControlButton("Pause","Pause");
		CreateAControlButton("Rebuild Schedule","Rebuild");
		CreateAControlButton("Clear Schedule","Clear");
		CreateAControlButton("Eratic Mouse 5s","EraticM5");
		CreateAControlButton("Eratic Mouse 10s","EraticM10");
		CreateAControlButton("Eratic Mouse 20s","EraticM20");
		CreateAControlButton("Eratic Keys 5s","EraticK5");
		CreateAControlButton("Eratic Keys 10s","EraticK10");
		CreateAControlButton("Eratic Keys 20s","EraticK20");
		CreateAControlButton("Map next 1 Keys"   ,"Map1k");
		CreateAControlButton("Map next 5 Keys"   ,"Map5k");
		CreateAControlButton("Map next 10 Keys"   ,"Map10k");
		//CreateAControlButton("Random Popup","RandomPopup");
		CreateAControlButton("Play Asterisk"   ,"Play_A");
		CreateAControlButton("Play Beep"       ,"Play_B");
		CreateAControlButton("Play Exclamation","Play_E");
		CreateAControlButton("Play Hand"       ,"Play_H");
		CreateAControlButton("Play Question"   ,"Play_Q");
		//CreateAControlButton("Bomb Beep","Bomb");
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
		echo "<div style='overflow: auto; width:*; height: 90vh;'>\n";
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