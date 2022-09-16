new ScriptObject(BasicRole)
{
	uiName = "";
	tags = "";
};

function BasicRole::onGiven(%player)
{
	if(!isObject(%player))
	{
		warn("[BasicRole::onGiven]" SPC %player SPC "Bad Player.");
		return;
	}
}

function BasicRole::onKill(%player)
{
	if(!isObject(%player))
	{
		warn("[BasicRole::onKill]" SPC %player SPC "Bad Player.");
		return;
	}
}

function BasicRole::onDeath(%player)
{
	%client = %player.client;
	if(!isObject(%player) || !isObject(%client))
	{
		warn("[BasicRole::onDeath]" SPC %player SPC "Bad Player.");
		return;
	}
	%client.camera.setMode("Corpse", %player);
	%client.camera.setControlObject(0);
	%client.setControlObject(%client.camera);
	messageClient(%client, 'MsgYourDeath', '', %clientName, '', %client.miniGame.respawnTime);

	%client.play2D(BBB_Death_Sound);
	%player.displayName = "an Unidentified Body";
	%player.setShapeName("Unidentified Body", 8564862);
	%player.setShapeNameDistance(13);
	%player.setShapeNameColor("1 1 0");
	%client.player = "";
	%client.clientCorpse = %player;
	%player.client = "";
	%player.corpseClient = %client;
	%player.isCorpse = true;
	CorpseGroup.add(%player);

	%deathTime = %player.deathTime = getSimTime();
	%player.lastWords = "";
	if(%deathTime - %player.lastMsgTime < 2000)
	{
		%player.lastWords = %player.lastMsg;
	}
	
	BBB_Minigame.doWinCheck();
}

new ScriptObject(InnocentRole)
{
	superClass = "BasicRole";

	uiName = "Innocent";
	tags = "Innocent";
};

