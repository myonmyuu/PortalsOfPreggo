return function (...)
	local res = ""
	for _, value in pairs({...}) do
		res = res.." "..tostring(value)
	end
	log(res)
end