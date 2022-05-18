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
		dataBlock = HealthStation;
		position = %position;
		minigame = %minigame;
	};
	%bot.setTransform(%player.getTransform());
	HSSimGroup.add(%bot);
	%bot.charge = 200;
	%bot.recharge();
	%bot.setCrouching(true);
	%bot.centerPrintData = "<br><br><br>\c6Charge:<br>\c4" @ %bot.charge;
}

function player::HPStationHeal(%player,%bot,%auto)
{	
	if($sim::time - %bot.lastClicked[%player] < 1 && ! %auto)
		return;
	
	%bot.lastClicked[%player] = $sim::time;
	%bot.lastClicked = $sim::time;
	cancel(%player.HPStationHealSchedule);
	

	%maxHP = %player.getDataBlock().maxDamage;
	%hp = %player.getDatablock().maxDamage - %player.getDamageLevel();
	if(%bot.charge > 0 && %hp != %maxHP)
	{
		%bot.playAudio(0, beep_EKG_Sound);
		%bot.charge -= 1;

		%bot.centerPrintData = "<br><br><br>\c6Charge:<br>\c4" @ %bot.charge;
		%player.addHealth(1);
		
		%client = %player.client;
		%healthText = "\c2" @ %player.getDatablock().maxDamage - %player.getDamageLevel();
		BBB_TimerLoop_ForceUpdate(%client, "bottomPrint", 2, %healthText);
		%player.HPStationHealSchedule = %player.schedule(1000,"HPStationHeal",%bot, true);
	}
	else
	{
		%bot.playAudio(0, beep_TryAgain_Sound);
	}
}

function AIPlayer::Recharge(%bot)
{
	cancel(%bot.rechargeSchedule);
	if(%bot.charge < 200 && $sim::time - %bot.lastClicked > 5)
	{
		%bot.charge ++;
		%bot.centerPrintData = "<br><br><br>\c6Charge:<br>\c4" @ %bot.charge;
	}
	%bot.rechargeSchedule = %bot.schedule(3000,Recharge);
}

datablock playerData(HealthStation : PlayerStandardArmor)
{
	shapeFile = "./Microwave.dts";
	uiName = "Microwave";
	boundingBox = "5 5 10.6";
	crouchBoundingBox = "5 5 4";
};

package HealthStation
{
	function Armor::OnTrigger(%data,%obj,%tNum,%tVal)
	{
		if(%tNum == 0)
		{
			if(%tVal)
			{
				%start = %obj.getEyePoint();
				%end = vectorAdd(%start, vectorScale(%obj.getEyeVector(), 5));
				%ray = containerRayCast(%start, %end, $Typemasks::PlayerObjectType, %obj);
				if(isObject(%hit = getWord(%ray, 0)))
				{
					if(%hit.getClassName() $= "AIPlayer")
					{
						if(%hit.getDataBlock().getID() == nameToId(HealthStation))
							%obj.HPStationHeal(%hit);
					}
				}
			}
			else
			{
				cancel(%obj.HPStationHealSchedule);
			}
		}
		return Parent::OnTrigger(%data,%obj,%tNum,%tVal);
	}

	function player::activateStuff(%this)
	{
		
		return parent::activateStuff(%this);
	}

	function MiniGameSO::reset(%mini, %client)
	{
		HSSimGroup.deleteAll();
		return parent::reset(%mini, %client);
	}	

	function BBB_Minigame::CleanUp(%so)
	{	
		HSSimGroup.deleteAll();
		return parent::CleanUp(%so);
	}
};
deactivatePackage(HealthStation);
activatePackage(HealthStation);