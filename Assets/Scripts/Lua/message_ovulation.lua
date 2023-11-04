-- stats, amt, "bea2fc"

local stats	= args[1]		---@type Stats
local amt 	= args[2]		---@type integer
local color = args[3]		---@type string

local charas = require "CharaInfo"
local grammargen = require "grammar"

local sentence = grammargen:parse[[
root <- {start}{chara} {verb} ovulated{extra}.
start <-  - 
]]

if stats == charas.Player then
	sentence:addRuleString("chara", "You")
	sentence:parse[[
	verb <- have
	]]
else
	sentence:addRuleString("chara", stats.charName)
	sentence:parse[[
	verb <- has
	]]
end

if amt > 1 then
	sentence:addRuleString("amt", tostring(amt))
	sentence:parse[[
	extra <-  {amt} eggs
	]]
else
	sentence:addRuleString("extra", "")
end

local t = ftext()
t.text = sentence:resolve{}
return t:clr(color)