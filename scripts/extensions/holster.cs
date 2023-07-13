datablock PlayerData(GunBackPlayer)
{
	shapeFile = $TTT::Asset @ "/models/gunbackplayer.dts";
    boundingBox = "4 4 4";

    splash = PlayerSplash;
    splashEmitter[0] = PlayerFoamDropletsEmitter;
    splashEmitter[1] = PlayerFoamEmitter;
    splashEmitter[2] = PlayerBubbleEmitter;

    mediumSplashSoundVelocity = 10;
    hardSplashSoundVelocity = 20;
    exitSplashSoundVelocity = 5;

    impactWaterEasy = Splash1Sound;
    impactWaterMedium = Splash1Sound;
    impactWaterHard = Splash1Sound;
    exitingWater = exitWaterSound;

    jetEmitter = playerJetEmitter;
    jetGroundEmitter = playerJetGroundEmitter;
    jetGroundDistance = 4;
    footPuffNumParts = 10;
    footPuffRadius = 0.25;
};

function GunBackPlayer::onUnMount(%obj)
{
	parent::onUnMount(%obj);
	%obj.delete();
}

function Player::GunImages_Update(%player)
{
	if(!%player.client.hasSpawnedOnce)
	{
		return;
	}

	%gunMount = %player.GetMountNodeObject(7);
	if(!%gunMount)
	{
		%gunMount = new AiPlayer()
		{
			dataBlock = GunBackPlayer;
			isGunImages = true;
		};
		%player.mountObject(%gunMount,7);
		%gunMount.setnetflag(6,true);
		%gunMount.clearScopeToClient(%player.client);
		%gunMount.setScopeAlways();
		%gunMount.applyDamage(10000);
		%gunMount.GunImages_CheckTPS(%player);
	}

	%pDB = %player.getDataBlock();
	%count = %pDB.maxTools;
	for(%i = 0; %i < %count; %i++)
	{
		if(%player.CurrTool == %i)
		{
			continue;
		}

		%iDB = %player.tool[%i];
		%itemTags = %iDB.ItemSlots_Tags;
		%itemTagCount = getWordCount(%itemTags);
		for(%j = 0; %j < %itemTagCount; %j++)
		{
		 	%tag = getWord(%itemTags,%j);
			if(%tag $= "Primary")
			{
				%backImage = %iDB.image.getId();
				break;
			}
		}
	}

	if(!%backImage)
	{
		%gunMount.UnmountImage(0);
	}
	else
	{
		%gunMount.mountImage(%backImage,0);
	}	
}

function GunBackPlayer::DoDismount(%db,%obj)
{
	return "";
}

package GunImages
{
	function ServerCmdUseTool (%client, %slot)
	{
		%r = parent::ServerCmdUseTool(%client, %slot);
		%player = %client.player;
		if(isObject(%player))
		{
			%player.GunImages_Update();
		}
		return %r;
	}

	function ServerCmdUnUseTool (%client)
	{
		%r = parent::ServerCmdUnUseTool (%client);
		%player = %client.player;
		if(isObject(%player))
		{
			%player.GunImages_Update();
		}
		return %r;
	}

	function ServerCmdDropTool (%client, %position)
	{
		%r = parent::ServerCmdDropTool (%client, %position);
		%player = %client.player;
		if(isObject(%player))
		{
			%player.GunImages_Update();
		}
		return %r;
	}

	function ShapeBase::pickup (%this, %obj, %amount)
	{
		%r = parent::pickup (%this, %obj, %amount);

		%this.GunImages_Update();
		return %r;
	}

	function PlayerData::OnAdd(%db,%obj)
	{
		%r = parent::OnAdd(%db,%obj);
		%obj.schedule(1000,"GunImages_Update");
		return %r;
	}

	function PlayerData::OnDisabled(%db,%obj)
	{
		if(%obj.GetMountNodeObject(7))
		{
			%obj.GetMountNodeObject(7).delete();
		}
		return parent::OnDisabled(%db,%obj);
	}

	function GameConnection::onClientEnterGame (%client)
	{
		%r = parent::onClientEnterGame (%client);
		%group = ClientGroup;
		%count = %group.getCount();
		for(%i = 0; %i < %count; %i++)
		{
			%currplayer = %group.getObject(%i).player;
			if(isObject(%currplayer))
			{
				%gunMount = %currplayer.getMountNodeObject(7);
				%currPlayer.unMountObject(%gunMount);
				%currPlayer.GunImages_Update();
			}
		}
		return %r;
	}
};
activatePackage("GunImages");

GunImages_GenerateSideImages();