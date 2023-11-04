local dateCard = dateCard
local date = date
---@type boolean
local preggovalid = preggovalid
---@type boolean
local isplayer = isplayer
---@type Stats
local giver = giver
---@type Stats
local reciever = reciever
---@type string
local giverPart = giverPart
---@type string
local recieverPart = recieverPart
---@type CardKeyword[]
local keywords = keywords
---@type DateManager
local datemanager = datemanager

local cumcardnames = {
	"Breed",
	"Creampie",
	"Fuckhard"
}

local preggo = require "Preggo"
local charas = require "CharaInfo"
local print = require "sub/print"

print(" --- date_clickedcard::start")
print(" --- card:", dateCard.name, " isplayer:", isplayer, " valid:", preggovalid)
print(" --- receiver: ", reciever.charName, recieverPart)
print(" --- giver: ", giver.charName, giverPart)

if not datemanager:canPlayCard(dateCard, isplayer) then
	return
end

local climax = false
for keyword in keywords do
	print("keyword: ", keyword)
	if keyword == "Climax" then
		climax = true
		break
	end
end

local cumcard = false
for _, name in pairs(cumcardnames) do
	if name == dateCard.name then
		cumcard = true
		break
	end
end

if not climax and not cumcard then
	return
end

print(" --- cumming! climax:", climax, " cumcard:", cumcard)
preggo:cumIn(reciever, giver)