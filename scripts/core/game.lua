GameTable = {}

---@class Game
---@field minigame integer|nil the object id of the minigame in torque
---@field clients Client[] the Clients of all of the current members of the minigame
---@field new fun(self:Game,name:string,color:integer):Game
---@field stop fun(self:Game) stops and cleans up Game
---@field set fun(self:Game, k:minigamefields,v:string|number|integer|boolean):Game sets the specified minigame field
local game = {minigame = nil}; game.__index = game

---@type Client
local cl = require('Client')

function game:new(name,color)
	print(name,color)
	local o = {clients = {}}
	setmetatable(o,self)
	local mg = tonumber(ts.call("Lua_Game_MakeGame",name,color))
	assert(mg,'Game:new[Failed to make minigame]')

	mg = math.floor(mg)
	
	o.minigame = mg

	GameTable[mg] = o;

	return o
end

---@alias minigamefields string
---| '"title"'
---| '"InviteOnly"'
---| '"UseAllPlayersBricks"'
---| '"PlayersUseOwnBricks"'
---| '"UseSpawnBricks"'
---| '"Points_BreakBrick"'
---| '"Points_PlantBrick"'
---| '"Points_KillPlayer"'
---| '"Points_KillBot"'
---| '"Points_KillSelf"'
---| '"Points_Die"'
---| '"RespawnTime"'
---| '"VehicleRespawnTime"'
---| '"BrickRespawnTime"'
---| '"BotRespawnTime"'
---| '"FallingDamage"'
---| '"WeaponDamage"'
---| '"SelfDamage"'
---| '"VehicleDamage"'
---| '"BrickDamage"'
---| '"BotDamage"'
---| '"EnableWand"'
---| '"EnableBuilding"'
---| '"PlayerDataBlock"'
---| '"StartEquip0"'
---| '"TimeLimit"'

function game:set(k,v)
	ts.setobj(self.minigame,k,v)
	return self
end

function game:stop()
	-- clean up the game
	ts.callobj(self.minigame,"endgame")
	self.minigame = nil
end

function game:addmember(client)
	client = cl:new(client)
	table.insert(self.clients,client)
end

function game:removemember(client)
	for i, v in ipairs(self.clients) do
		if v == client then
			table.remove(self.clients,i)
		end
	end
end

function TorqueMinigameCallback(mg,callback,...)
	---@type Game
	local gm = GameTable[mg]
	if not gm then
		return
	end

	local func = gm[callback]
	if not func then
		return
	end
	func(gm,...)
end

ts.eval([[
	function Lua_Game_MakeGame(%title,%colorIdx)
	{
		$MiniGameColorTaken[%colorIdx] = 1;
		%mg = new ScriptObject ()
		{
			class = MiniGameSO;
			owner = -1;
			title = %title;
			colorIdx = %colorIdx;
			numMembers = 0;
			InviteOnly = 0;
			UseAllPlayersBricks = 0;
			PlayersUseOwnBricks = 0;
			UseSpawnBricks = true;
			Points_BreakBrick = 0;
			Points_PlantBrick = 0;
			Points_KillPlayer = 0;
			Points_KillBot = 1;
			Points_KillSelf = 0;
			Points_Die = 0;
			RespawnTime = 1;
			VehicleRespawnTime = 1;
			BrickRespawnTime = 1;
			BotRespawnTime = 5000;
			FallingDamage = 1;
			WeaponDamage = 0;
			SelfDamage = 0;
			VehicleDamage = 0;
			BrickDamage = 0;
			BotDamage = 1;
			EnableWand = 0;
			EnableBuilding = 0;
			PlayerDataBlock = PlayerStandardArmor.getId ();
			TimeLimit = 0;
			IsLuaGame = true;
		};
		MiniGameGroup.add (%mg);
		commandToAll ('AddMiniGameLine', %mg.getLine (), %mg, %colorIdx);
		return %mg;
	}

	function MiniGameSO::removeMember (%obj, %client)
	{
		if (%obj.owner == %client && $DefaultMiniGame != %obj)
		{
			%obj.endGame ();
			return;
		}
		%i = 0;
		while (%i < %obj.numMembers)
		{
			if (%obj.member[%i] == %client)
			{
				%j = %i + 1;
				while (%j < %obj.numMembers)
				{
					%obj.member[%j - 1] = %obj.member[%j];
					%j += 1;
				}
				%obj.member[%obj.numMembers - 1] = "";
				%obj.numMembers -= 1;
			}
			%i += 1;
		}
		commandToClient (%client, 'SetPlayingMiniGame', 0);
		commandToClient (%client, 'SetRunningMiniGame', 0);
		%i = 0;
		while (%i < %obj.numMembers)
		{
			%cl = %obj.member[%i];
			messageClient (%cl, 'MsgClientInYourMiniGame', "\c1" @ %client.getPlayerName () @ " left the mini-game.", %client, 0);
			%i += 1;
		}
		%client.setScore (0);
		if (!$Server::LAN)
		{
			if ($Pref::Server::ClearEventsOnMinigameChange)
			{
				%client.ClearEventSchedules ();
			}
			%client.resetVehicles ();
			%mask = $TypeMasks::PlayerObjectType | $TypeMasks::ProjectileObjectType | $TypeMasks::VehicleObjectType | $TypeMasks::CorpseObjectType;
			%client.ClearEventObjects (%mask);
		}
		%client.miniGame = -1;
		if (isObject (%client.Player))
		{
			%client.InstantRespawn ();
		}
		if (%obj.numMembers <= 0 && $DefaultMiniGame != %obj && !%obj.isLuaGame)
		{
			%obj.endGame ();
			%obj.schedule (10, delete);
		}
		%brickGroup = %client.brickGroup;
		%count = %brickGroup.getCount ();
		%i = 0;
		while (%i < %count)
		{
			%checkObj = %brickGroup.getObject (%i);
			if (%checkObj.getDataBlock ().getId () == brickVehicleSpawnData.getId ())
			{
				%checkObj.vehicleMinigameEject ();
			}
			%i += 1;
		}
		%obj.checkLastManStanding ();
	}

	package luaminigamehooks
	{
		function MiniGameSO::addMember (%obj, %client)
		{
			luacall("TorqueMinigameCallback",%obj,"AddMember",%client);
			return parent::addMember (%obj, %client);
		}

		function MiniGameSO::removeMember (%obj, %client)
		{
			luacall("TorqueMinigameCallback",%obj,"RemoveMember",%client);
			return parent::removeMember (%obj, %client);
		}
	};
	activatepackage(luaminigamehooks);
]])

return game