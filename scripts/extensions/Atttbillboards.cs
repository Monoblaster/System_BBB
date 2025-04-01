datablock fxLightData(detectiveBillboard : DefaultBillboard)
{
	flarebitmap = "./detective.png";
	ConstantSize = 0.5;
	flareColor = "0 0 1 1";
};

datablock fxLightData(traitorBillboard : DefaultBillboard)
{
	flarebitmap = "./traitor.png";
	ConstantSize = 0.5;
	flareColor = "1 0 0 1";
};

datablock fxLightData(traitorAVBillboard : DefaultAVBillboard)
{
    flarebitmap = "./traitor.png";
	ConstantSize = 0.5;
	flareColor = "1 0 0 1";
};

datablock fxLightData(traitorAVBillboard2 : DefaultAVBillboard)
{
    flarebitmap = "./traitor.png";
	ConstantSize = 0.5;
	flareColor = "0 1 0 1";
};

datablock fxLightData(traitorAVBillboard3 : DefaultAVBillboard)
{
    flarebitmap = "./traitor.png";
	ConstantSize = 0.5;
	flareColor = "0 0 1 1";
};


datablock fxLightData(normalSilhouetteAVBillboard : DefaultAVBillboard)
{
	flarebitmap = "./radar.png";
	ConstantSize = 1.35;
	flareColor = "0 1 0 1";

	AnimOffsets = true;
	startOffset = "0 0 1.25";
	endOffset = "0 0 1.25";
};

datablock fxLightData(traitorSilhouetteAVBillboard : normalSilhouetteAVBillboard)
{
	flareColor = "1 0 0 1";
};

package tttbillboards
{
	function BBB_Minigame::roundSetup(%so)
	{
		//loop through minigame members and clear xray
		%count = %so.numMembers;
		for(%i = 0; %i < %count; %i++)
		{
			%client = %so.member[%i];
			%client.ClearXrayBillboards();
			%client.AVBillboardGroup.clear();
		}

		return Parent::roundSetup(%so);
	}

	function Armor::onRemove(%this, %obj, %state)
    {
		if(isObject(%obj.roleBBM))
		{
			%obj.roleBBM.delete();
		}
		parent::onRemove(%this, %obj, %state);
	}

    function Armor::onDisabled(%this, %obj, %state)
    {
		%client = %obj.origClient || %obj.client;
		if(!isObject(%client))
		{
			return Parent::onDisabled(%this, %obj, %state);
		}

		if(%client.getClassName() !$= "GameConnection")
		{
			return Parent::onDisabled(%this, %obj, %state);
		}

		if(isObject(%obj.roleBBM))
		{
			%obj.roleBBM.delete();
		}

		%group = ClientGroup;
		%count = %group.getCount();
		for(%i = 0; %i < %count; %i++)
		{
			%currclient = %group.getObject(%i);
			if(%currclient.AVBillboardGroup)
			{
				%currclient.AVBillboardGroup.clear(%client.getBLID());
			}
		}
        return Parent::onDisabled(%this, %obj, %state);
    }

	//when the player is first spawned create their always billboards group
	function GameConnection::onClientEnterGame(%client)
	{	
		%r = Parent::onClientEnterGame(%client);
		%client.centerPrint("\c6Welcome To TTT! Yes that's me talking to you!",5);
		%client.avBillboardGroup = %group = AVBillboardGroup_Make();
		%group.load(%client,30);
		return %r;
	}

	function GameConnection::onClientLeaveGame(%client)
	{
		if(isObject(%client.avBillboardGroup))
		{
			%client.ClearXrayBillboards();
			%client.avBillboardGroup.delete();
		}
			
		return Parent::onClientLeaveGame(%client);
	}
};
deactivatePackage("tttbillboards");
activatePackage("tttbillboards");