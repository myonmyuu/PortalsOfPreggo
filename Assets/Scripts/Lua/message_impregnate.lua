--new FText($" - {cA.CharName} has been impregnated! She is carrying {father.CharName}'s child.").Clr("ff7dcd")

local mother	= args[1] ---@type Stats
local father	= args[2] ---@type Stats
local color		= args[3] ---@type string

local charas = require "CharaInfo"
local grammargen = require "grammar"

local sentence = grammargen:parse[[
root <- {start}{motherhas} been impregnated! {Sheis} carrying {father} {child}.
start <-  - 

child <- child
child <- baby
]]

if mother == charas.Player then
	sentence:addRuleString("motherhas", "You have")
	sentence:addRuleString("Sheis", "You are")
else
	sentence:addRuleString("motherhas", ("%s has"):format(mother.charName))
	sentence:addRuleString("Sheis", "She is")
end

if father.genetics.id ~= mother.genetics.id then
	if father.genetics.id == charas.Player.genetics.id then
		sentence:addRuleString("father", "your")
	else
		sentence:addRuleString("father", ("%s's"):format(father.charName))
	end
else
	if mother.genetics.id == charas.Player.genetics.id then
		sentence:addRuleString("own", "your own ")
	else
		sentence:addRuleString("own", "her own ")
	end
end

local f = ftext()
f.text = sentence:resolve{}
return f:clr(color)