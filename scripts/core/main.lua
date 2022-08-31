---@type Game
local game = require "game"
---@type Maps
local maps = require "maps"
---@type StateSystem
local states = require "states"
---@type RoleList
local roles = require "roles"
---@type ItemTable
local itemtable = require "itemtable"

---@alias roletagcallback string
---| '"ongiven"'
---| '"onkill"'
---| '"onanydeath"'
---| '"onmydeath"'

---@alias roletag string
---| '"Innocent"'
---| '"Traitor"'
---| '"Detective"'

-- role callback functions
--- adds a tag and callback to the list
---@param tag roletag
---@param callback roletagcallback
---@param func function
local function maketagcallback(tag,callback,func)
	TTManager.tagcallbacks[tag] = TTManager.tagcallbacks[tag] or {}
	TTManager.tagcallbacks[tag][callback] = func
end

---does the callbacks for the role's tags
---@param cl Client
---@param callback string
local function dorolecallback(cl,callback,...)
	---@type Role
	local role = cl:get("role")
	if not role then
		return
	end

	for i, tag in ipairs(role.tags) do
		TTManager.tagcallbacks[tag][callback](cl,...)
	end
end

-- traitor
---@param cl Client
local function traitorongive(cl)
	
end

-- state callback functions
local function picknextphase(ss)
	while true do
		if #TTManager.game.clients > 1 then
			coroutine.yield("preround",15);
			coroutine.yield("round",60 * 5);
			coroutine.yield("postround",15);
		else
			coroutine.yield("pregame",5);
		end
	end
end


local function preroundphase()
	-- spawn guns
	local c = ts.callobj("BrickGroup_888888","getCount")
	for i = 0, c - 1 do
		local b = ts.callobj("BrickGroup_888888","getObject",i)
		local it = TTManager.itemspawntable[string.sub(ts.callobj(b,"getName"),1)]
		if it then
			ts.callobj(b,"setItem",it[math.random(#it)].db)
		end
	end
	
	-- disable damage
	TTManager.game:set("WeaponDamage",false)

	-- activate any time spawn
	TTManager.game:set("RespawnTime",0)
end

local function roundphase()
	-- enable damage
	TTManager.game:set("WeaponDamage",true)

	-- hand out roles
	local clients = TTManager.game.clients
	for i, v in ipairs(TTManager.rolelists[1]:makelist(#clients)) do
		clients[i]:set("role",v)
		-- role giving callback
		dorolecallback(clients[i],"ongiven")
	end
	
	-- deactivate any time spawn
	TTManager.game:set("respawntime",-1)
end

local function postroundphase() 
	-- hand win to winners
end

local function roundtick()
	-- do things
end

---@class TTManager
---@field game Game
---@field maploader Maps
---@field states StateSystem
---@field rolelists RoleList[]
---@field itemspawntable ItemTable[]
---@field tagcallbacks table<string,table<string,function>>
TTManager = TTManager

local function starttt()
	TTManager = {
		game = game:new("Terrorist Town",4),
		maploader = maps:new("Add-Ons/TT_*/save.bls"),
		states = states:newss(picknextphase),
		rolelists = {},
		itemspawntable = {},
		tagcallbacks = {}
	}

	TTManager.game:set("BotDamage",false)
	TTManager.game:set("UseAllPlayersBricks",false)
	TTManager.game:set("PlayersUseOwnBricks",false)
	TTManager.game:set("UseSpawnBricks",true)
	TTManager.game:set("TimeLimit",0)
	TTManager.game:set("SelfDamage",true)
	TTManager.game:set("EnableWand",false)
	TTManager.game:set("RespawnTime",0)
	TTManager.game:set("BrickDamage",false)
	TTManager.game:set("WeaponDamage",true)
	TTManager.game:set("FallingDamage",true)
	TTManager.game:set("Points_KillSelf",0)
	TTManager.game:set("Points_KillPlayer",0)
	--TTManager.game:set("PlayerDataBlock",)

	local currstate
	currstate = TTManager.states:news("pregame")
	TTManager.states:add(currstate)

	currstate = TTManager.states:news("preround")
	currstate.start = preroundphase
	TTManager.states:add(currstate)

	currstate = TTManager.states:news("round")
	currstate.start = roundphase
	TTManager.states:add(currstate)

	currstate = TTManager.states:news("postround")
	currstate.start = postroundphase
	TTManager.states:add(currstate)

	TTManager.states:start()

	TTManager.itemspawntable["Grenade"] = itemtable:new("Add-Ons/System_BBB/weapons_Grenade.txt")
	TTManager.itemspawntable["Other"] = itemtable:new("Add-Ons/System_BBB/weapons_Other.txt")
	TTManager.itemspawntable["Primary"] = itemtable:new("Add-Ons/System_BBB/weapons_Primary.txt")
	TTManager.itemspawntable["Secondary"] = itemtable:new("Add-Ons/System_BBB/weapons_Secondary.txt")

	maketagcallback("Traitor","ongiven",func)

	local currrolelist
	currrolelist = roles:new()
	currrolelist:role("Innocent",5,{"Innocent"})
	currrolelist:role("Traitor",2,{"Traitor"})
	currrolelist:role("Detective",1,{"Detective"})
	table.insert(TTManager.rolelists,currrolelist)

	TTManager.maploader:load(1)
end

local function resettt()
	if TTManager then
		return
	end

	TTManager.states:stop()

	starttt()
end


if not TTManager then
	starttt()
end