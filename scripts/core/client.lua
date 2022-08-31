---@class Client
---@field new fun(self:Client,c:integer):Client
---@field set fun(self:Client,k:string,v:any):Client
---@field get fun(self:Client,k:string):any
---@field call fun(self:Client,f:string,...:string|integer|number|boolean):string
local cl = {client  = nil,values = {}};cl.__index = cl;cl.__eq = function (op1,op2) return op1.client == op2.cleint end

function cl:new(c)
	local o = {client = c,values = {}}
	setmetatable(o,self)
	return o
end

function cl:set(k,v)
	cl.values[k] = v
	return cl
end

function cl:get(k)
	return cl.values[k]
end

function cl:call(f,...)
	return ts.callobj(self.client,f,...)
end

return cl