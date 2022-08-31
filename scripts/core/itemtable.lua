---@class ItemTable
---@field new fun(self:ItemTable,fp:string):ItemTable makes a new ItemTable using o and returns it
local it = {}; it.__index = it

function it:new(fp)
	local o = {}
	setmetatable(o,self)

	local f = io.open(fp)
	assert(f,"ItemTable:maketable[Invalid filepath " .. fp .. "]")

	local c = 1
	for l in f:lines() do

		local itm = {}
		local first = true
		for w in string.gmatch(l,"[%w%p ]+") do
			if first then
				local itmdb = ts.get("uiNameTable_items .. w")
				assert(itmdb ~= "","ItemTable:maketable[Invalid item " .. w .. "]")

				itm["db"] = itmdb
				first = false
			else
				table.insert(itm,w)
			end
		end

		table.insert(o,itm)
		c = c + 1
	end

	return o
end

return it