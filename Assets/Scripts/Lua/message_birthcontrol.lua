-- stats, "bea2fc"

local stats	= args[1]		---@type Stats
local color = args[2]		---@type string

local charas = require "CharaInfo"
local grammargen = require "grammar"

local sentence = grammargen:parse[[
root <- {start}{charas} ovulation was cancelled due to birth control.
start <-  - 
]]

if stats == charas.Player then
	sentence:addRuleString("charas", "Your")
else
	sentence:addRuleString("charas", ("%s's"):format(stats.charName))
end

local t = ftext()
t.text = sentence:resolve{}
return t:clr(color)