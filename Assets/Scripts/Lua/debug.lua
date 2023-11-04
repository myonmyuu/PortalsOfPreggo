-- PRESS L FOR DEBUG, P SHOW/HIDE
-- log(eventName or "none")

---@type PreggoManager
local preggo = get "Preggo"
---@type CharaInfo
local charas = get "CharaInfo"
---@type fun(...)
local print = get "sub/print"

local function _setgenid(ch, id)
	local gene = ch.genetics
	gene.id = id
	ch.genetics = gene
end

-- print(eventName or "none", prog or 0, event or 0)

-- preggo:cumIn(charas.Sylvie, charas.Castalia)

-- preggo:cumIn(charas.Sylvie, charas.Player)
-- preggo:cumIn(charas.Player, charas.Castalia)

-- local e, p = preggo:tryGetData(charas.Player)
-- local gene = charas.Other.genetics
-- gene.id = 90000
-- charas.Other.genetics = gene
-- _setgenid(charas.Other, -2147483647)
-- print("yuh", charas.Other.genetics.id)

local save = require "Save"

for value in save.bestiaryEntries do
	print(value)
end


-- if e then
-- 	p:clearSemen()
-- end