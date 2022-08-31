---@class RoleList
---@field new fun(self:RoleList):RoleList
---@field makelist fun(self:RoleList,n:integer):table returns a list of roles made with n players
---@field role fun(self:RoleList,name:string,weight:number,tags:string[]?):Role makes a Role made with o and insterts it into the RoleList and returns it
local rls = {}; rls.__index = rls

function rls:new()
	local o = {}
	setmetatable(o,self)
	return o
end

function rls:makelist(n)
	local lr = {}

	-- calculate total weight
	local tw = 0
	for _,v in ipairs(self) do
		tw = tw + v.weight
	end

	-- make role list
	local un = n
	for _,v in ipairs(self) do

		local nr = math.min(un,math.floor((v.weight / tw) * n))
		for _ = 1, nr do
			table.insert(lr, v)
		end

		un = un - nr
	end

	-- shuffle role list
	for i, v in ipairs(lr) do
		local ri = math.random(i,#lr)
		lr[i],lr[ri] = lr[ri],lr[i]
	end

	return lr
end

local function emptyfunc()
	return false
end

---@class Role
---@field name string display name of role
---@field weight number the weight of the role
---@field tags string[] an array of strings to be used with indentifying the role
local rl = {};

function rls:role(name,weight,tags)
	local o = {name = name,weight = weight,tags = tags or {}}
	table.insert(self,o)
	return o
end

return rls