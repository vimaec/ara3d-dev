
-- Creare Environment class (cannot create an object of this class)
-- This class does not have a constructor (it's a static class)
-- Environment = dotNetClass "System.Environment"
-- appData = Environment.GetEnvironmentVariable "AppData"
sysEn = dotNetClass "System.Environment"
speFo = dotNetClass "System.Environment+specialFolder"
programDataPath = sysEn.getfolderPath speFo.commonApplicationData 
bridgePath = programDataPath + @"\Ara3D\3dsMaxBridge\Ara3D.3dsMaxBridge.dll"

--  showProperties speFo --There are a tonne of folders to choose from
-- showProperties sysEn --Lots of useful info here too

global Ara3DMaxBridgeAsm = dotNet.loadAssembly bridgePath
global AraAPI = dotNetClass "Ara3D.API"

macroCode = "macroScript Ara3D_CSharpScriptEditor
category: \"Ara 3D\"
buttonText: \"Ara C# Editor\"
tooltip: \"Ara 3D C# Script Editor\"
(
   AraAPI.ShowEditor ()	
)
"
execute macroCode

-- Register an Ara 3D Menu if need be 
if menuMan.registerMenuContext 0x572418f3 then
(
	-- Get the main menu bar
	local mainMenuBar = menuMan.getMainMenuBar()	
	-- Create a new menu
	local subMenu = menuMan.createMenu "Ara 3D"
	-- create a menu item that calls the sample macroScript
	local testItem = menuMan.createActionItem "Ara3D_CSharpScriptEditor" "Ara 3D"
	-- Add the item to the menu
	subMenu.addItem testItem -1
	-- Create a new menu item with the menu as it's sub-menu
	local subMenuItem = menuMan.createSubMenuItem "Ara 3D" subMenu
	-- compute the index of the next-to-last menu item in the main menu bar
	local subMenuIndex = mainMenuBar.numItems()
	-- Add the sub-menu just at the second to last slot
	mainMenuBar.addItem subMenuItem subMenuIndex
	-- redraw the menu bar with the new item
	menuMan.updateMenuBar()
)

araGeometryPath = programDataPath + @"\Ara3D\3dsMaxBridge\Ara3D.Geometry.dll"
araGeometryAsm = dotNet.loadAssembly araGeometryPath

araLinqPath = programDataPath + @"\Ara3D\3dsMaxBridge\Ara3D.LinqArray.dll"
araLinqAsm = dotNet.loadAssembly araLinqPath


AraAPI.ShowEditor ()
-- f = dotNetObject "Ara3D.ScriptEditorForm"
-- f.ShowDialog ()
