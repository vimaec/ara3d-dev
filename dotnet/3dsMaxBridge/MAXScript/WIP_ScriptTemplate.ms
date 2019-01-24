-- TODO: This is a WIP, and not currently supported.

-- A scripted geometry plug-in that executes arbitrary C# code.
-- Custom Attributes: https://help.autodesk.com/view/3DSMAX/2017/ENU/?guid=__files_GUID_ADFD29E4_9751_4F55_98CC_F7C721C9AEE2_htm
-- Scripted plug-in clauses: https://help.autodesk.com/view/3DSMAX/2017/ENU/?guid=__files_GUID_461915FA_31A2_49CE_84AF_2544B782ACA3_htm

-- There is going to be a bunch of assemblies to load. 
-- asm = dotNet.loadAssembly @"%programdata%\Ara3D\3dsMaxBridge\3dsMaxBridge.dll"

plugin simpleObject AraScriptedObject
	name:"Ara Object"
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

	parameters main rollout:args
	(
		Size ui:Size default:10 type:#float animatable:true
	)
	
	parameters main2 rollout:params
	(
		script ui:Script type:#string default:"Hello world!"
		_dummy type:#boolean invisibleInTV:true -- this exists to enable easy invalidation of the object
		-- pluginGraph type:#filename assettype:#MaxCreationGraph readOnly:true enumAsAsset:true 
		-- pluginGraphDependencies type:#filenametab assettype:#MaxCreationGraph readOnly:true enumAsAsset:true tabSize:0 tabSizeVariable:true	
	)

	fn initialize = 
	(
		print "initializing"
		
		-- TODO: I suspect that initialize gets called too often, and that I will have to optimize the creation of plugin instance to avoid 
		-- over creating the thing.

		-- TODO: this will get removed
		-- pluginDefinitionFile = @"C:\Users\ara3d\Autodesk\3ds Max 2019\Max Creation Graph\Tools\Object_Name.maxtool"

		-- This is the class used to communicate with the Ara 3D geometry system
		bridgeClass = dotNetClass "Ara3D.AraGeometryBridge"

		-- Create a .NET instance of a reference maker class that corresponds to this plug-in
		-- TODO: this used to be an important argument for an MCG plug-in, but it isn't anymore.
		-- local refMakerWrapperClass = dotnetclass "Autodesk.Max.MaxPlus.ReferenceMaker"
		-- local _pluginOwner = refMakerWrapperClass._CreateWrapper this
	
		-- Create a .NET instance of a "GeometryPluginInstanceBridge" which allows 
		pluginInstance = bridgeClass.CreatePluginInstance ()
		local meshWrapperClass = dotnetclass "Autodesk.Max.MaxPlus.Mesh"

		-- Create a .NET instance of a Autodesk.Max.MaxPlus.Mesh so that meshes can be easily passed to the plug-in instance
		_meshWrapper = meshWrapperClass._CreateWrapper undefined
		_meshWrapper_SetValue = _meshWrapper._SetValue

		-- TODO: this will get removed
		pluginGraph = pluginDefinitionFile
		pluginGraphDependencies = #()

		-- value wrapper local initializations
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
		-- Force updating the plug-in when the transform is updated on the owning node
		
		-- TODO: this should only happen if one of the arguments is a scene node, otherwise it should no impact on the mesh. SO.... I am commenting it out for now
		-- owningNode = theNode
		-- if (tmHandler == undefined and owningNode != undefined) then ( tmHandler = when transform owningNode changes do ( _dummy = not _dummy ))
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
			case click of 
			(
				1: nodeTM.translation = gridPoint
				2: #stop
			) 
		) 
	) 

	rollout args "Arguments"
	(
		spinner Size "Size" range:[0, 100, 10] type:#float
	)
	
	rollout params "Parameters"
	(
		edittext Script "Script" height:48 labelOnTop:true
		edittext Errors "Errors" height:48 labelOnTop:true
	)

	-- TODO: remove this
	/*
	rollout MCG_Editor "MCG Editor"(
		button MCG_Editor_btn "Open in MCG Editor"
		on MCG_Editor_btn pressed do
		(
			if (doesFileExist pluginDefinitionFile) then 
			(
				geBridge = dotNetClass "ProceduralContentActions.PCGraphEditorBridge"
				geBridge.LoadGraph pluginDefinitionFile
			)
			else (
				local msgStream = stringstream ""
				format "File '%' is missing.\n" pluginDefinitionFile to:msgStream
				local msgStr = msgStream as string
				if GetQuietMode() then
					format msgStr
				else
					MessageBox msgStr title: "Missing maxtool file" beep:false
			)
		)
	)
	*/

	fn transformGetter = 
	(
		if(isValidNode(owningNode)) then owningNode.Transform else matrix3 1
	)

	fn updateMesh = 
	(
		if (pluginInstance == undefined) then ( return undefined )
		local args = dotnet_ValueToDotNetObject #(Size) argsType
		pluginInstance.UpdateMesh (currentTime as integer) _meshWrapper script args
		ok
	)

	on buildMesh do
	(
		print "building mesh"
		-- if (PreInvokeMethod != undefined ) then PreInvokeMethod()
		_meshWrapper_SetValue mesh
		
		-- TODO: I'm not sure that catching all exceptions is the right thing to do.
		try (
			result = updateMesh ()
			if (result == undefined) then ( print "Undefined result from updateMesh" )
		)
		catch ( 
			print ( getCurrentException() )
		)

		_meshWrapper_SetValue undefined

		-- if (PostInvokeMethod != undefined ) then PostInvokeMethod()
		ok
	)

)
