CommandTable = CommandTable or {}

function CommandCallback(client,name,...)
	name = tonumber(name)
	CommandTable[name]:callback(client,...)
end

local function default(command,client,...)
	print(client .. " activated command")
end

local function makecommand(name)
	ts.eval([[
		function servercmd]] .. name .. [[(%client,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15,%a16)
		{
			luacall("CommandCallback",%client,]] .. name .. [[,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15,%a16);
		}
	]])
end

---@class Command
---@field new fun(self:Command,name:string,callback:function): Command returns a table made for torque commands
---@field name string the command's name
---@field callback function the function called whenthe command is used
local command = {name = "command", callback = default}; command.__index = command

function command:new(name, callback)
	assert(not CommandTable[name],'Command:new[' .. name .. ' already is a command]')

	local o = {name = name, callback = callback or default}
	setmetatable(o,self)

	CommandTable[name] = o
	makecommand(o.name)
	return o
end

return command