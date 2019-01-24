-- A scripted geometry plug-in that executes arbitrary C# code.
-- Custom Attributes: https://help.autodesk.com/view/3DSMAX/2017/ENU/?guid=__files_GUID_ADFD29E4_9751_4F55_98CC_F7C721C9AEE2_htm
-- Scripted plug-in clauses: https://help.autodesk.com/view/3DSMAX/2017/ENU/?guid=__files_GUID_461915FA_31A2_49CE_84AF_2544B782ACA3_htm

-- There is going to be a bunch of assemblies to load. 
asm = dotNet.loadAssembly @"C:\dev\repos\AraGeometry\3dsMaxBridge\bin\Debug\3dsMaxBridge.dll"

plugin simpleObject AraScriptedObject
	name:"C# Script"
	classID:#(0x56880e2a, 0x8323dbe)
	category:"Ara 3D"
	usePBValidity:true
(
	local pluginDefinitionFile -- the plugin definition file
	local pluginInstance -- the plugin instance created from the plugin definition
	local _meshWrapper -- the dotnet wrapper around our mesh pointer
	local argsType = dotNetClass "System.Object[]" -- used when creating args array
	local dotnet_ValueToDotNetObject = dotnet.ValueToDotNetObject
	local tmHandler
	local owningNode

	-- function wrappers
	local _meshWrapper_SetValue
	local pluginInstance_UpdateMesh

	-- value wrapper local and function wrapper declarations

	local bridgeClass

	parameters main rollout:params
	(
		script ui:Script type:#string default:"Hello world!"
		_dummy type:#boolean invisibleInTV:true -- this exists to enable easy invalidation of the object
	)

	fn initialize = 
	(
		print "Initializing Ara 3D C# Object"

		-- TODO: Create a .NET instance of a reference maker class that corresponds to this plug-in
		-- RefMakerWrapperClass = dotnetclass "Autodesk.Max.MaxPlus.ReferenceMaker"
		-- _pluginOwner = refMakerWrapperClass._CreateWrapper this
		
		-- Create a .NET instance of a "GeometryPluginInstanceBridge" which allows interfacing with Ara 3D system 
		bridgeClass = dotNetClass "Ara3D.Ara"
		pluginInstance = bridgeClass.CreatePluginInstance ()
		local meshWrapperClass = dotnetclass "Autodesk.Max.MaxPlus.Mesh"

		-- Create a .NET instance of a Autodesk.Max.MaxPlus.Mesh so that meshes can be easily passed to the plug-in instance
		_meshWrapper = meshWrapperClass._CreateWrapper undefined
		_meshWrapper_SetValue = _meshWrapper._SetValue
	)

	on create do 
	(
		initialize()
	)

	on clone fromObj do 
	(
		initialize()
	)

	on load do 
	(
		initialize()
	)

	on update do 
	(
		initialize()
	)

	fn installTMHandler theNode =
	(
		-- Force updating the plug-in when the transform is updated on the owning node and track what node this plug-in is associated with. 
		-- Cloned instances of the plug-in are ignored. 		
		owningNode = theNode
		
		if (tmHandler == undefined and owningNode != undefined) then ( tmHandler = when transform owningNode changes do ( _dummy = not _dummy ))
		-- TODO: Create a .NET instance of an INode class that corresponds to the owner INode (well we really want the Matrix)
		-- INodeWrapperClass = dotnetclass "Autodesk.Max.MaxPlus.INode"
		-- _inode = INodeWrapperClass._CreateWrapper owningNode 				
	)

	on postload do
	(
		installTMHandler(refs.dependentnodes this firstonly:true)
	)

	on attachedToNode theNode do
	(
		installTMHandler(theNode)
	)

	tool create 
	(
		on mousePoint click do 
		(
			#stop
		) 
	) 

	fn updateMesh =
	(
		print "Building mesh"
		_meshWrapper_SetValue mesh
		
		try (
			local args = dotnet_ValueToDotNetObject #() argsType
			pluginInstance.UpdateMesh (currentTime as integer) _meshWrapper script args			
		)
		catch ( 
			print ( getCurrentException() )
		)

		_meshWrapper_SetValue undefined
	)
	
	rollout params "Parameters"
	(
		edittext Script "Script" height:48 labelOnTop:true
		edittext Errors "Errors" height:48 labelOnTop:true
		button Run "Run" width:45 height:20
		on Run pressed do ( updateMesh () )
	)

	on buildMesh do
	(
		-- TODO: if you want to automatically build the mesh whenever the parameters are updated call recreateMesh		
		ok
	)

)
