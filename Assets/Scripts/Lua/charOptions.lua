do
	-- return
end

local preggo = require "Preggo"
local plugin = require "PreggoPlugin"
local charas = require "CharaInfo"

---@type Stats, PreggoData
local current, pregData = current, pregData
local player = charas.Player

local cman		= instance "PoP.CharacterManager"
local ttman		= instance "PoP.ToolTipManager"
local townman	= instance "PoP.TownInterfaceController"
local logcont	= instance "PoP.LogController"

local UnityObject		= static "UnityEngine.Object"
local Vector3			= static "UnityEngine.Vector3"
local TooltipManager	= static "PoP.ToolTipManager"

local staminaneeded
local success

local function pre()
	staminaneeded = {}
	success = true
end

local function post()
	if success then
		for stats, stam in pairs(staminaneeded) do
			stats.changeStamina(-stam)
		end
		townman.updatePlayerExhaustion();
	end
end

function addButton(text, tt, onpress)
	local button = UnityObject.Instantiate(cman.buttonPrefab, cman.buttonRoster.transform)
	--button.transform.localScale = Vector3.one
	local tmptext = button.GetComponentInChildren(type("UnityEngine.TMP_Text"))
	tmptext.text = text
	if tt then
		ttman.addTooltipTo(button, tt, TooltipManager.Position.TopLeft)
	end
	local ubutton = button.GetComponentInChildren(type("UnityEngine.Button"))
	ubutton.onClick.AddListener(function()
		pre()
		onpress()
		post()
	end)
end

function setText(text)
	cman.descriptionText.text = text
end

function check(c)
	success = success and c
	return success
end

function chackStamina(stats, amt)
	staminaneeded[stats] = amt
	if not check(stats.Stamina >= amt) then
		logcont.addMessage("Too exhausted", true)
	end
	return success
end

-- can add actions to characters here
require "qol_actions"