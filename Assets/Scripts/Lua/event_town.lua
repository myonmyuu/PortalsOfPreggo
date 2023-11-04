local preggo = require "Preggo"
local charas = require "CharaInfo"

local print = require "sub/print"


print(eventName or "none", prog or 0, event or 0)

---@type table<string, fun()>
local events = {
	SylviePortal = function ()
		if (prog < 1) then
			return
		end
		preggo:cumIn(charas.Sylvie, charas.Castalia)
		preggo:cumIn(charas.Sylvie, charas.Castalia)
		preggo:cumIn(charas.Sylvie, charas.Castalia)
		preggo:cumIn(charas.Sylvie, charas.Castalia)
	end
}

if not eventName then
	return
end

local evf = events[eventName]
if evf then
	evf()
end