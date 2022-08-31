local tss = {}
function tss.messageclient(client,message,...)
	ts.call("chatMessageClient",client,0,"","",ts.call("addTaggedString",message),...)
end

function tss.messageall(message,...)
	local count = ts.callobj("ClientGroup","getCount") - 1
	for i = 0, count do
		local client = ts.callobj("ClientGroup","getObject",i)
		ts.call("chatMessageClient",client,0,"","",ts.call("addTaggedString",message),...)
	end
end

function tss.bottomprintclient(client,message,seconds,hidebar,...)
	ts.call("commandToClient",client,ts.call("addTaggedString","BottomPrint"),ts.call("addTaggedString",message),seconds,hidebar,...)
end

function tss.bottomprintall(message,seconds,hidebar,...)
	local count = ts.callobj("ClientGroup","getCount") - 1
	for i = 0, count do
		local client = ts.callobj("ClientGroup","getObject",i)
		ts.call("commandToClient",client,ts.call("addTaggedString","BottomPrint"),ts.call("addTaggedString",message).."e",seconds,hidebar,...)
	end
end

function tss.centerprintclient(client,message,seconds,...)
	ts.call("commandToClient",client,ts.call("addTaggedString","CenterPrint"),ts.call("addTaggedString",message),seconds,...)
end

function tss.centerprintall(message,seconds,...)
	local count = ts.callobj("ClientGroup","getCount") - 1
	for i = 0, count do
		local client = ts.callobj("ClientGroup","getObject",i)
		ts.call("commandToClient",client,ts.call("addTaggedString","CenterPrint"),ts.call("addTaggedString",message),seconds,...)
	end
end

return tss;