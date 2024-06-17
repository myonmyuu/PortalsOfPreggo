print = require "sub/print"
error = require "sub/error"

function static(s)
	local staticval = _static[s]
	return staticval or require(s)
end

function instance(s)
	local staticval = _static[s]
	return staticval and staticval.instance or error(("unable to get instance of '%s'"):format(s))
end

local otype = type
function type(s)
	local ores = otype(s)
	if ores ~= "string" then
		return ores
	end
	
	return _types[s] or ores
end

function makenew(t, ...)
	if otype(t) == "string" then
		t = type(t)
	end
	return _construct(t, ...)
end

new = new or error("new table nonexistant?")
setmetatable(new, {
	__call = function(self, t, ...)
		return makenew(t, ...)
	end
})