MapsTable = {};MapsTable.__index = table;setmetatable(MapsTable,MapsTable)

---@class Maps
---@field list table[]
---@field index integer|nil
---@field currmap integer
---@field load fun(self: Maps,mapnum: integer) loads indexed map
local maps = {currmap = 0,index = nil}; maps.__index = maps

local function collectmaps(self,path)
	-- clear previously collected maps
	for i,_ in ipairs(self.list) do
		self.list[i] = nil
	end

	local mappath = ts.call("FindFirstFile", path)
	while mappath ~= "" do
		local envpath = string.gsub(mappath,"save.bls","envirorment.txt")
		table.insert(self.list,{mappath = mappath,envpath = envpath})
		mappath = ts.call("FindNextFile", path)
	end
end

function maps:new(path)
	local o = {list = {}}
	setmetatable(o,self)

	MapsTable.insert(o)
	o.index = #MapsTable

	collectmaps(o,path)

	return o
end

function maps:load(mapnum)
	if mapnum == self.currmap then
		return
	end
	-- clear previous map first
	ts.setobj("BrickGroup_888888", "chainDeleteCallback", "luacall(\"InitiateMapLoad\"," .. self.index .. "," .. mapnum ..");")
	ts.callobj("BrickGroup_888888", "chainDeleteAll")
end

function InitiateMapLoad(mapsindex,mapnum)
	local maps = MapsTable[mapsindex]

	mapnum = tonumber(mapnum)
	local map = maps.list[mapnum]
	if not map then
		return
	end

	-- loading enviroment
	ts.call("GameModeGuiServer::ParseGameModeFile",map.envpath,1)

	ts.call("EnvGuiServer::getIdxFromFilenames")
	ts.call("EnvGuiServer::SetSimpleMode")

	if not ts.get("EnvGuiServer::SimpleMode") then
		ts.call("EnvGuiServer::fillAdvancedVarsFromSimple")
		ts.call("EnvGuiServer::SetAdvancedMode")
	end


	ts.call("serverDirectSaveFileLoad",map.mappath,3,"",2,1)
	maps.currmap = mapnum
end

return maps;