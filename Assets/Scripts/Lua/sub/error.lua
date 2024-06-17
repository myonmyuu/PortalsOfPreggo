return function (...)
	local res = ""
	for _, value in pairs({...}) do
		res = res.."\t"..tostring(value)
	end
	lerror(res)
end