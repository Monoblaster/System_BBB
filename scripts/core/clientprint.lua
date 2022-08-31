---@class ClientPrint
---@field prints table<loc,table<integer,table<just,string>>>
---@field new fun(self:ClientPrint):ClientPrint
---@field setprint fun(self:ClientPrint,l:loc,ln:integer,jst:just,s:string):ClientPrint
---@field print fun(self:ClientPrint):(string,string) returns center and bottom print
local cp = {prints = {{},{}}}; cp.__index = cp

---@alias just
---| '"left"' #left justified
---| '"center"' #center justified
---| '"right"' #right justified

---@alias loc
---| '"center"' #center print
---| '"bottom"' #bottom print

function cp:new()
	local o = {}
	setmetatable(o,self)
	return o
end

function cp:setprint(lc,ln,jst,s)
	ln = math.floor(ln)

	local currindex = self.prints[lc]
	if not currindex then
		currindex = {}
		self.prints[lc] = currindex
	end

	currindex = currindex[ln]
	if not currindex then
		local i = 1
		while i < ln do
			if not self.prints[lc][i] then
				self.prints[lc][i] = {}
			end
			i = i + 1
		end
		currindex = {}
		self.prints[lc][ln] = currindex
	end

	currindex[jst] = s

	return self
end

function cp:print()
	local lt = {center = "", bottom = ""}
	for lc, lct in pairs(self.prints) do
		for ln, lnt in ipairs(lct) do
			local jt = {left = "", center = "", right = ""}
			for jst, s in pairs(lnt) do
				jt[jst] = s
			end

			local ojt = {"left","center","right"}
			for _, jst in ipairs(ojt) do
				local s = jt[jst]
				if s ~= "" then
					lt[lc] = lt[lc] .. "<just:" .. jst .. ">" .. s
				end
			end
			lt[lc] = lt[lc] .. "<br>"
		end
	end
	return lt.center, lt.bottom
end

return cp