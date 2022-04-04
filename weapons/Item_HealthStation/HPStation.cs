if(!isObject(HSSimGroup))
{
	new SimSet(HSSimGroup);
}

datablock ItemData(HealthStationHandItem)
{
  uiName = "Health Station";
  image = HealthStationHandImage;
  category = "Tools";
  className = "Weapon";
  shapeFile = "base/data/shapes/empty.dts";
  mass = 0.5;
  density = 0.2;
  elasticity = 0;
  friction = 0.6;
  emap = true;
  armReady = false;
  doColorShift = true;
  colorShiftColor = "1 1 1 1";
  canDrop = true;
  
  // BBB
  singleBuy = true;
  description = "A *station* that heals players!";
};

//### Item Image

datablock ShapeBaseImageData(HealthStationHandImage)
{
  shapefile = "base/data/shapes/empty.dts";
  emap = true;
  mountPoint = 0;
  offset = "0 0 0";
  eyeOffset = "0 0 0";
  rotation = eulerToMatrix("0 0 0");
  className = "WeaponImage";
  item = XrayItem;
  melee = false;
  doReaction = false;
  armReady = false;
  doColorShift = true;
  colorShiftColor = "1 1 1 1";

  stateName[0] = "Activate";
  stateTimeoutValue[0] = 0.1;
  stateTransitionOnTimeout[0] = "Ready";

  stateName[1] = "Ready";
  stateTransitionOnTriggerDown[1] = "Fire";
  stateAllowImageChange[1] = false;

  stateName[2] = "Fire";
  stateTransitionOnTimeOut[2] = "Ready";
  stateTimeoutValue[2] = "0.2";
  stateFire[2] = true;
  stateAllowImageChange[2] = true;
  stateScript[2] = "onFire";
};
function MiniGameSO::clearHealthStations(%this)
{
	if(!isObject(HSSimGroup))
		return;
	%count = HSSimGroup.getCount();
	for(%i=0;%i<%count;%i++)
	{
		%bot = HSSimGroup.getObject(0);
		HSSimGroup.remove(%bot);
		%bot.delete();
	}
}

function HealthStationHandImage::onFire(%this,%obj,%slot)
{
  %client = %obj.client;
  %currSlot = %obj.currTool;
 	if(isObject(%obj.client))
	{
		%client.createStation();
		messageClient(%obj.client,'MsgItemPickup','',%currSlot,0);
		serverCmdUnUseTool(%obj.client);
	}
	else
		%obj.unMountImage(%slot);
}

function GameConnection::createStation(%client)
{
	%player = %client.player;
	%position = %player.getposition();
	%minigame = getMiniGameFromObject(%client);
	%bot = new AIPlayer()
	{
		datablock = HealthStation;
		position = %position;
		minigame = %minigame;
	};
	HSSimGroup.add(%bot);
	%bot.charge = 200;
	%bot.recharge();
	%bot.setCrouching(true);
	%bot.centerPrintData = "<br><br><br>\c6Charge:<br>\c4" @ %bot.charge;
}
function player::HPStationHeal(%player,%bot)
{	
	if($sim::time - %bot.lastClicked[%player] < 1)
		return;
	
	%bot.lastClicked[%player] = $sim::time;
	%bot.lastClicked = $sim::time;
	

	%maxHP = %player.getDataBlock().maxDamage;
	%hp = %player.getDatablock().maxDamage - %player.getDamageLevel();
	if(%bot.charge > 1 && %hp != %maxHP)
	{
		%bot.playAudio(0, beep_EKG_Sound);
		%bot.charge -= 1;

		%bot.centerPrintData = "<br><br><br>\c6Charge:<br>\c4" @ %bot.charge;
		%player.addHealth(1);
		
		%client = %player.client;
		%healthText = "\c2" @ %player.getDatablock().maxDamage - %player.getDamageLevel();
		BBB_TimerLoop_ForceUpdate(%client, "bottomPrint", 2, %healthText);
	}
}
function AIPlayer::Recharge(%bot)
{
	%bot.schedule(3000,Recharge);
	if(%bot.charge < 200 && $sim::time - %bot.lastClicked > 5)
	{
		%bot.charge ++;
		%bot.centerPrintData = "<br><br><br>\c6Charge:<br>\c4" @ %bot.charge;
	}
}
datablock TSShapeConstructor(Microwave)
{
	baseShape  = "./Microwave.dts";
	sequence0  = "./Root.dsq root";

	sequence1  = "./Root.dsq root";
	sequence2  = "./Root.dsq root";
	sequence3  = "./Root.dsq root";
	sequence4  = "./Root.dsq root";

	sequence5  = "./Root.dsq root";
	sequence6  = "./Root.dsq root";
	sequence7  = "./Root.dsq root";
	sequence8  = "./Root.dsq root";

	sequence12 = "./Root.dsq root";
	sequence13 = "./Root.dsq root";
	sequence14 = "./Root.dsq root";
	sequence15 = "./Root.dsq root";
};    
datablock playerData(HealthStation : PlayerStandardArmor)
{
	shapeFile = "./Microwave.dts";
	uiName = "Microwave";
	boundingBox = "5 5 10.6";
	crouchBoundingBox = "5 5 4";
};

package HealthStation
{
	function player::activateStuff(%this)
	{
		%start = %this.getEyePoint();
		%end = vectorAdd(%start, vectorScale(%this.getEyeVector(), 5));
		%ray = containerRayCast(%start, %end, $Typemasks::PlayerObjectType, %this);
		if(isObject(%hit = getWord(%ray, 0)))
		{
			if(%hit.getClassName() $= "AIPlayer")
			{
				if(%hit.getDataBlock().getID() == nameToId(HealthStation))
					%this.HPStationHeal(%hit);
			}
		}
		parent::activateStuff(%this);
	}

	function MiniGameSO::reset(%mini, %client)
	{
		parent::reset(%mini, %client);
		if(!%mini.isBBB)
			return;
		%mini.clearHealthStations();
	}	

	function BBB_Minigame::CleanUp(%so)
	{	
		parent::CleanUp(%so);
		%so.clearHealthStations();
	}
};
activatePackage(HealthStation);