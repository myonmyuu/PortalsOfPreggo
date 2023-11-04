do
	-- return
end

---@param name string
---@param tt string
---@param time integer
---@param text string
---@param onpress fun()
local function act(name, tt, time, text, onpress)
	addAct(name, tt, time, text, onpress)
end
local print = require "sub/print"
local town = require "Town"
local preggo = require "Preggo"
local plugin = require "PreggoPlugin"
local charas = require "CharaInfo"

---@type Stats, PreggoData
local current, pregData = current, pregData

-- print(charas.Player.charName, charas.Other.charName)


-- can add actions to characters here