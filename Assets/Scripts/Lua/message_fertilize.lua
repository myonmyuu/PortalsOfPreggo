-- new FText($" - An ovum within {motherStats.CharName} has been fertilized by {father.CharName}'s sperm.").Clr("f0b1fc")
-- motherStats, father, "f0b1fc"

local mother	= args[1] ---@type Stats
local father	= args[2] ---@type Stats
local color		= args[3] ---@type string

local charas = require "CharaInfo"
local grammargen = require "grammar"

local sentence = grammargen:parse[[
root <- {start}An ovum within {mother} has been fertilized by {father} sperm.
start <-  - 
]]

if mother == charas.Player then
	sentence:addRuleString("mother", "you")
else
	sentence:addRuleString("mother", mother.charName)
end

if father.genetics.id == charas.Player.genetics.id then
	sentence:addRuleString("father", "your")
else
	sentence:addRuleString("father", ("%s's"):format(father.charName))
end

local t = ftext()
t.text = sentence:resolve{}
return t:clr(color)