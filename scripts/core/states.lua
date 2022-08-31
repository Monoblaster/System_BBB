---@class StateSystem
---@field newss fun(self:StateSystem,next:function):StateSystem
---@field news fun(self:StateSystem,name:string):State
---@field start fun(self:StateSystem):StateSystem
---@field stop fun(self:StateSystem):StateSystem
---@field add fun(self:StateSystem,o:State):StateSystem
---@field next fun(self:StateSystem):(string,integer)|nil
---@field currstate State|nil
---@field currstatetime integer|nil
---@field tickrate integer|nil
---@field schedule integer|nil
---@field nextco function|nil
local ss = {next = nil,currstate = nil,currstatetime = nil,tickrate = 250};ss.__index = ss

---@class State
---@field name string the name of the sate
---@field start fun(self:State)|nil function called when state starts
---@field stop fun(self:State)|nil	function called when state stops
---@field tick fun(self:State)|nil function called while state is active
local s = {name = "Default",start = nil,stop = nil,tick = nil};s.__index = s

function ss:newss(next)
	local o = {next = next}
	setmetatable(o,self)

	assert(next,"StateSystem:newss[undefined next function]")

	return o
end

function ss:news(name)
	o = {name = name}
	setmetatable(o,self)
	return o
end

function ss:start()
	self:dotick(os.clock())
	assert(self.next,"StateSystem:start[undefined next function]")
	self.nextco = coroutine.wrap(self.next)
	return self
end

function ss:stop()
	cancel(self.schedule)
	self.nextco = nil
	return self
end

---does state tick
---@param ltt integer 
function ss:dotick(ltt)
	local deltatime = os.clock() - ltt
	if not self.currstate then
		-- pick a new phase
		local statename
		statename,self.currstatetime = self:nextco()
		self.currstate = self[statename]

		if self.currstate.start then
			self.currstate:start()
		end
	else
		-- call current phase's tick
		if self.currstate.tick then
			self.currstate:tick()
		end
	end

	self.currstatetime = self.currstatetime - deltatime
	if self.currstatetime <= 0 then
		-- stop the current phase
		if self.currstate.stop then
			self.currstate:stop()
		end
		self.currstate = nil
	end

	self.schedule = schedule(self.tickrate,self.dotick,self,os.clock())
end

function ss:add(o)
	o = o or {}
	setmetatable(o,s)

	assert(not self[o.name],"StateSystem:add[state with same name]")

	self[o.name] = o
	return self
end


return ss