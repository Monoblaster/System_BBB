datablock PlayerData(GunBackPlayer)
{
	shapeFile = $TTT::Asset @ "./gunbackplayer.dts";
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
		%gunMount.setScopeAlways();
		%gunMount.clearScopeToClient(%player.client);
		
		%gunMount.applyDamage(10000);
	}

	%gunMount.clearScopeToClient(%player.client);

	if(%player.getMountedImage(0) != %player.tool[0].image.getid()) 
	{
		%gunMount.mountImage(%player.tool[0].image,0);
		return;
	}

	%gunMount.UnmountImage(0);
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

	function Player::pickup (%this, %obj, %amount)
	{
		%r = parent::pickup (%this, %obj, %amount);

		%this.GunImages_Update();
		return %r;
	}

	function Armor::OnAdd(%db,%obj)
	{
		%r = parent::OnAdd(%db,%obj);
		%obj.schedule(1000,"GunImages_Update");
		return %r;
	}

	function Armor::OnRemove(%db,%obj)
	{
		if(%obj.GetMountNodeObject(7))
		{
			%obj.GetMountNodeObject(7).delete();
		}
		return parent::OnRemove(%db,%obj);
	}

	function Armor::OnDisabled(%db,%obj)
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