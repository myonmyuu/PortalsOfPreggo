---@diagnostic disable: undefined-global

local print = require "sub/print"


--giver, stats, volume, UserData.CreateStatic<UnityEngine.Color>()
local giver 	= args[1]	---@type Stats
local receiver 	= args[2]	---@type Stats
local volume 	= args[3]	---@type number
local color 	= args[4]

local charas = require "CharaInfo"
local grammargen = require "grammar"

-- print("CUMMING", "g", giver, "r", receiver, "v", volume, "c", color)

-- new FText($" ~ {giver.CharName} spurts {Mathf.FloorToInt(volume.Value)}ml of hot {giver.species} jizz into {stats.CharName}'s pussy!").Clr(UnityEngine.Color.white);

---@param s Stats
---@param spec Species
---@return boolean
local function _isspecies(s, spec)
	return tostring(s.genetics.speciesA) == spec or tostring(s.genetics.speciesB) == spec
end

local sentence = grammargen:parse[[
root <- {start}{giver} {verb} {volume} of {adjective} {species} {cum} into {receiver} {pussy}!
start <-  ~ 
]]

sentence:parse[[
adjective <- hot
]]

if _isspecies(giver, "LavaGirl") then
	sentence:parse[[
	adjective <- literally hot
	adjective <- extremely hot
	adjective <- almost boiling
	]]
end

sentence:addRuleString("species", tostring(giver.species))

sentence:parse[[
cum <- jizz
cum(0.5) <- cum
cum(0.1) <- spunk
]]

sentence:parse[[
verb <- spurt{verbend}
verb(0.1) <- jizz{verbendl}
]]

sentence:addRuleString("volume", ("%sml"):format(math.floor(volume)))

if giver ~= charas.Player then
	sentence:addRuleString("giver", giver.charName)
	sentence:parse[[
	verbend <- s
	verbendl <- es
	]]
else
	sentence:addRuleString("giver", "You")
	sentence:parse[[
	verbend <- 
	verbendl <- 
	]]
end

if receiver ~= charas.Player then
	sentence:addRuleString("receiver", ("%s's"):format(receiver.charName))
else
	sentence:addRuleString("receiver", "your")
end

sentence:parse[[
pussy <- pussy
pussy(0.4) <- cunt
pussy(0.1) <- snatch
pussy(0.1) <- vagina
]]

local t = ftext()
t.text = sentence:resolve{}
return t:clr(color.white)
