datablock PlayerData(FloatingBillboardPlayer)
{
    shapeFile = "base/data/shapes/empty.dts";
	boundingBox = vectorScale("20 20 20", 4);
};

datablock PlayerData(MountedBillboardPlayer)
{
    shapeFile = "./billboardMount.dts";
	boundingBox = vectorScale("20 20 20", 4);
};


//custom billboards
datablock fxLightData(detectiveBillboard)
{
	LightOn = false;

	flareOn = true;
	flarebitmap = "./detective.png";
	ConstantSize = 0.5;
    ConstantSizeOn = true;
    FadeTime = 0.1;

	LinkFlare = false;
	blendMode = 1;
	flareColor = "0 0 1 1";
};

datablock fxLightData(traitorBillboard)
{
	LightOn = false;

	flareOn = true;
	flarebitmap = "./traitor.png";
	ConstantSize = 0.5;
    ConstantSizeOn = true;
    FadeTime = 0.1;

	LinkFlare = false;
	blendMode = 1;
	flareColor = "1 0 0 1";
};

datablock fxLightData(ghostRadarBillboard)
{
	LightOn = false;

	flareOn = true;
	flarebitmap = "./radar.png";
	ConstantSize = 1;
    ConstantSizeOn = true;
    FadeTime = 0.000001;

	LinkFlare = false;
	blendMode = 1;
	flareColor = "1 0 0 1";

	AnimOffsets = true;
	startOffset = "0 0 2.2";
	endOffset = "0 0 2.2";
};

datablock fxLightData(normalRadarBillboard)
{
	LightOn = false;

	flareOn = true;
	flarebitmap = "./radar.png";
	ConstantSize = 1.35;
    ConstantSizeOn = true;
    FadeTime = 999999;

	LinkFlare = false;
	blendMode = 1;
	flareColor = "0 1 0 1";

	AnimOffsets = true;
	startOffset = "0 0 1.25";
	endOffset = "0 0 1.25";
};

function Billboard_Create(%lightDB,%mountDB,%dontGhost)
{
	if(!isObject(%lightDB))
	{
		warn("Billboard_Create: " @ %lightDB @ " is not a valid fxLight datablock");
		return;
	}

	if(!isObject(%mountDB))
	{
		warn("Billboard_Create: " @ %mountDB @ " is not a valid player datablock");
		return;
	}

	if(%lightDB.getClassName() !$= "fxLightData")
	{
		warn("Billboard_Create: " @ %lightDB @ " is not a valid fxLight datablock");
		return;
	}

	if(%mountDB.getClassName() !$= "PlayerData")
	{
		warn("Billboard_Create: " @ %mountDB @ " is not a valid player datablock");
		return;
	}

	%billboard = new fxLight()
	{
		dataBlock = %lightDB;
	};

	//move it up so it doesn't crash the game
	%mount = new aiPlayer()
	{
		dataBlock = %mountDB;
		position = "0 0 999999";
	};
	
	%mount.setDamageLevel(10000);
	%mount.setTransform("0 0 0");

	%mount.light = %billboard;
	%billboard.attachToObject(%mount);
	

	%billboard.setNetFlag(6,true);
	%mount.setNetFlag(6,true);
	if(!%dontGhost)
	{
		//make sure it scopes just because it has to manually be done with these flags
		%billboard.setScopeAlways();
		%mount.setScopeAlways();
	}
	else
	{
		//disable ghosting automaticaly
		%billboard.setNetFlag(8,false);
		%mount.setNetFlag(8,false);
	}

	return %mount;
}

function Billboard_Ghost(%billboard,%client)
{
	if(%client $= "ALL")
	{
		%group = ClientGroup;
		%count = %group.getCount();
		for(%i = 0; %i < %count; %i++)
		{	
			%client = %group.getObject(%i);
			Billboard_Ghost(%billboard,%client);
		}
		return %billboard;
	}

	if(!isObject(%billboard) || !isObject(%billboard.light))
	{
		warn("Billboard_Ghost: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(%billboard.getClassName() !$= "aiPlayer" && %billboard.light.getClassName() $= "fxLight")
	{
		warn("Billboard_Ghost: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(!isObject(%client))
	{
		warn("Billboard_Ghost: " @ %client @ " is not a valid client");
		return;
	}

	if(%client.getClassName() !$= "GameConnection")
	{
		warn("Billboard_Ghost: " @ %client @ " is not a valid client");
		return;
	}

	%billboard.ScopeToClient(%client);
	%billboard.light.ScopeToClient(%client);

	return %billboard;
}

function Billboard_ClearGhost(%billboard,%client)
{
	if(%client $= "ALL")
	{
		%group = ClientGroup;
		%count = %group.getCount();
		for(%i = 0; %i < %count; %i++)
		{	
			%client = %group.getObject(%i);
			Billboard_ClearGhost(%billboard,%client);
		}
		return %billboard;
	}

	if(!isObject(%billboard) || !isObject(%billboard.light))
	{
		warn("Billboard_ClearGhost: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(%billboard.getClassName() !$= "aiPlayer" && %billboard.light.getClassName() $= "fxLight")
	{
		warn("Billboard_ClearGhost: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(!isObject(%client))
	{
		warn("Billboard_ClearGhost: " @ %client @ " is not a valid client");
		return;
	}

	if(%client.getClassName() !$= "GameConnection")
	{
		warn("Billboard_ClearGhost: " @ %client @ " is not a valid client");
		return;
	}

	%billboard.ClearScopeToClient(%client);
	%billboard.light.ClearScopeToClient(%client);

	return %billboard;
}

function Billboard_Mount(%billboard,%player)
{	
	if(!isObject(%billboard) || !isObject(%billboard.light))
	{
		warn("Billboard_Mount: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(%billboard.getClassName() !$= "aiPlayer" && %billboard.light.getClassName() $= "fxLight")
	{
		warn("Billboard_Mount: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(isObject(%player.billboard))
	{
		warn("Billboard_Mount: Billboard already mounted to " @ %player);
		return;
	}

	%player.MountObject(%billboard,8);
	%player.billboard = %billboard;

	return %billboard;
}

function Billboard_Ummount(%player)
{	
	if(!isObject(%player.billboard))
	{
		warn("Billboard_Ummount: Billboard is not already mounted to " @ %player);
		return;
	}

	%billboard = %player.billboard;
	if(!isObject(%billboard) || !isObject(%billboard.light))
	{
		warn("Billboard_Ummount: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(%billboard.getClassName() !$= "aiPlayer" && %billboard.light.getClassName() $= "fxLight")
	{
		warn("Billboard_Ummount: " @ %billboard @ " is not a valid billboard");
		return;
	}
    
	%player.unMountObject(8);

	return %billboard;
}

function Billboard_Delete(%billboard)
{	
	if(!isObject(%billboard) || !isObject(%billboard.light))
	{
		warn("Billboard_Delete: " @ %billboard @ " is not a valid billboard");
		return;
	}

	if(%billboard.getClassName() !$= "aiPlayer" && %billboard.light.getClassName() $= "fxLight")
	{
		warn("Billboard_Delete: " @ %billboard @ " is not a valid billboard");
		return;
	}

	%billboard.light.delete();
	%billboard.delete();
}

function VisibleBillboard_Create(%client,%mountDB,%count)
{
	if(!isObject(%mountDB))
	{
		warn("VisibleBillboard_Create: " @ %mountDB @ " is not a valid player datablock");
		return;
	}

	if(%mountDB.getClassName() !$= "PlayerData")
	{
		warn("VisibleBillboard_Create: " @ %mountDB @ " is not a valid player datablock");
		return;
	}

	%client.visibleBillboardGroup = %group = new scriptGroup()
	{
		class = "VisibleBillboard";
		client = %client;
	};

	%mount = new aiPlayer()
	{
		dataBlock = "FloatingBillboardPlayer";
	};
	%mount.setDamageLevel(10000);
	
	%mount.setNetFlag(8,false);
	%mount.setNetFlag(6,true);
	%mount.ScopeToClient(%client);

	%client.player.mountObject(%mount,8);

	for(%i = 0; %i < %count; %i++)
	{
		%billboard = Billboard_Create("GhostRadarBillboard",%mountDB,true);
		Billboard_Ghost(%billboard,%client);
		%billboard.light.attachToObject(%mount);

		schedule(%i * 300 + 300, 0, "FinishvisibleBillboard", %billboard, %group);		
	}

	return %group;
}

function FinishVisibleBillboard(%billboard,%group)
{
	%group.add(%billboard);
	%billboard.light.attachToObject(%billboard);
	Billboard_ClearGhost(%billboard,%group.client);
}

function VisibleBillboard::Billboard(%group,%lightDB,%position,%tag)
{
	if(!isObject(%lightDB))
	{
		warn("VisibleBillboard::Billboard: " @ %lightDB @ " is not a valid fxLight datablock");
		return;
	}

	if(%lightDB.getClassName() !$= "fxLightData")
	{
		warn("VisibleBillboard::Billboard: " @ %lightDB @ " is not a valid fxLight datablock");
		return;
	}

	%active = %group.active;
	if(%active >= %group.getCount())
	{
		warn("VisibleBillboard::Billboard: " @ %group @ " group over count");
		return;
	}
	
	//get an inactive billboard
	%billboard = %group.getObject(%active);
	%billboard.tag = %tag;
	%group.active++;

	//set it's datablock and enable
	%billboard.light.setDatablock(%lightDB);
	Billboard_Ghost(%billboard,%group.client);

	%billboard.setTransform(%position);

	return %billboard;
}

function VisibleBillboard::ClearBillboards(%group,%tag)
{
	%count = %group.getCount();
	for(%i = %count - 1; %i >= 0; %i--)
	{
		%billboard = %group.getObject(%i);
		if(%tag !$= "" && %tag !$= %billboard.tag)
		{
			continue;
		}

		//disable all of the lights and unmount if mounted
		%mount = %billboard.getObjectMount();
		if(isObject(%mount))
		{
			%mount.unmountObject(%billboard);
		}

		Billboard_ClearGhost(%billboard,%group.client);
		//remove old tag
		%billboard.tag = "";

		//push to back so only active billboards are in fornt
		%group.pushToBack(%billboard);
		%group.active--;
	}

	return %group;
}

package billboards
{
    function Armor::onDisabled(%this, %obj, %state)
    {
        if(isObject(%obj.billboard))
        {
            Billboard_Delete(%obj.billboard);
        }
        return Parent::onDisabled(%this, %obj, %state);
    }

    function Player::Delete(%this)
    {
        if(isObject(%this.billboard))
        {
            Billboard_Delete(%this.billboard);
        }
        Parent::Delete(%this);
    }

	//when the player is first spawned create their always billboards group
	// function GameConnection::onClientEnterGame(%client)
	// {	
	// 	%r = Parent::onClientEnterGame(%client);
	// 	schedule(5000,%client,"VisibleBillboard_Create",%client,"FloatingBillboardPlayer",30);	
	// 	return %r;
	// }

	// function GameConnection::onClientLeaveGame(%client)
	// {
	// 	if(isObject(%client.visibleBillboardGroup))
	// 	{
	// 		%client.visibleBillboardGroup.deleteall();
	// 		%client.visibleBillboardGroup.delete();
	// 	}
		
	// 	return Parent::onClientLeaveGame(%client);
	// }
};
activatePackage("billboards");