using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using static JS.Assets;
using Debug = UnityEngine.Debug;

namespace JS
{

	public class Templating
	{
		[MenuItem("Project Setup/Assets/Choose", false, -10)]
		public static void CustomizeProject()
		{
			var dialog = PackageTemplateDialog
				.Show("Install Assets", "");
			if (string.IsNullOrEmpty(dialog)) return;
		}
	}
	
    public class Assets
    {
        public static string AssetStore5 =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unity/Asset Store-5.x");

        public static void OpenAssetFolder() => Process.Start(AssetStore5);

        public static bool Exists(string assetFolder) => Directory.Exists(Path.Combine(AssetStore5, assetFolder));

        public static void Import(string asset, string folder)
        {
            if (!asset.EndsWith(".unitypackage")) asset += ".unitypackage";
            var filePath = Path.Combine(AssetStore5, folder, asset);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Asset '{filePath}' Not Download. Please Download it from Package Manager first.");
                Packages.OpenPackageManager();
                return;
            }

            AssetDatabase.ImportPackage(filePath, false);
        }
    }

    public class Packages
    {
	    static AddRequest _request;

	    readonly static Queue<string> _queue = new Queue<string>();

	    public static void InstallUnityPackage(params string[] packs) => Install(packs.Select(x=>$"com.unity.{x}").ToArray());

	    public static void Install(params string[] packs)
	    {
		    foreach (var item in packs) _queue.Enqueue(item);
		    if (_queue.Count > 0) Begin();
	    }

	    static async void Begin()
	    {
		    _request = Client.Add(_queue.Dequeue());
		    while (!_request.IsCompleted) await Task.Delay(10);
		    if(_request.Status == StatusCode.Failure) Debug.LogError(_request.Error.message);
		    if (_queue.Count <= 0) return;
		    await Task.Delay(1000);
		    Begin();
	    }


	    public static void OpenPackageManager() => EditorApplication.ExecuteMenuItem("Window/Package Manager");
    }

    public class PackageConfigMenu
    {


	    

	    [MenuItem("Project Setup/Packages/Config/Clear All")]
	    public static void ClearPackages()
	    {
		    var dir = Path.Combine(Application.dataPath, "../Packages/manifest.json");
		    if (!File.Exists(dir)) return;
		    var dialog = EditorUtility.DisplayDialog("Clear All Packages",
			    "Are you sure? It will remove everything, including this menu!", 
			    "Clear Packages", "Cancel", DialogOptOutDecisionType.ForThisSession,"clear-packages");
		    if (!dialog) return;
		    File.Delete(dir);
		    File.WriteAllText(dir,"{ \"dependencies\": { } }");
	    }
	    
    }

    public class PackageInstallMenu
    {
	    [MenuItem("Project Setup/Packages/Open Manager")]
	    public static void OpenPackageManager() => Packages.OpenPackageManager();
		 


	    [MenuItem("Project Setup/Packages/Install/Favourites/Newtonsoft JSON")]
	    public static void Newton() => 
		    Packages.Install("com.unity.nuget.newtonsoft-json");

	    [MenuItem("Project Setup/Packages/Install/Unity/Input")]
	    public static void InputSystem() =>
		    Packages.InstallUnityPackage("inputsystem");
	    
	    [MenuItem("Project Setup/Packages/Install/Unity/Cinemachine")]
	    static void AddCinemachine() => Packages.InstallUnityPackage("cinemachine");
	    
	    
	    [MenuItem("Project Setup/Packages/Install/Unity/Rider")]
	    static void AddRider() => 
		    Packages.InstallUnityPackage("ide.rider");
	    
	    [MenuItem("Project Setup/Packages/Install/Unity/TextMesh Pro")]
	    static void AddTMP() => 
		    Packages.InstallUnityPackage("textmeshpro");
	    
	    [MenuItem("Project Setup/Packages/Install/Unity/Timeline")]
	    static void AddTimeline() => 
		    Packages.InstallUnityPackage("timeline");
	    
	    [MenuItem("Project Setup/Packages/Install/Unity/PSD Importer")]
	    static void AddPSDImporter() => 
		    Packages.InstallUnityPackage("2d.psdimporter");
	    
	    [MenuItem("Project Setup/Packages/Install/Unity/Addressables")]
	    static void AddAddressables() => 
		    Packages.InstallUnityPackage("addressables");
	    
	    [MenuItem("Project Setup/Packages/Install/Unity/Polybrush")]
	    static void AddPolybrush() => 
		    Packages.InstallUnityPackage("polybrush");
	    
	    [MenuItem("Project Setup/Packages/Install/Unity/Probuilder")]
	    static void AddProbuilder() => 
		    Packages.InstallUnityPackage("probuilder");
	    
	    [MenuItem("Project Setup/Packages/Install/Unity/Recorder")]
	    static void AddRecorder() => 
		    Packages.InstallUnityPackage("recorder");
	    
	    [MenuItem("Project Setup/Packages/Install/Unity/VFX Graph")]
	    static void AddVFXGraph() => 
		    Packages.InstallUnityPackage("visualeffectsgraph");
    }


    public class AssetInstallMenu
    {
        [MenuItem("Project Setup/Assets/Open Directory", false, -10)]
        public static void OpenAssetFolder() => Assets.OpenAssetFolder();
        
        public class EditorExtensions
	    {
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Misc/Relations Graph")]
		    public static void RelationsInspector() => Import("RelationsInspector Demo", "Seldom Tools/Editor ExtensionsUtilities");
		    
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Favourites/Code Editor",false,-100)]
		    public static void ScriptInspector3() => Import(
		    "Script Inspector 3", "Flipbook Games/Editor ExtensionsVisual Scripting");

		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Favourites/Console Pro",false,-100)]
		    public static void EditorConsolePro() => Import(
		    "Editor Console Pro", "FlyingWorm/Editor ExtensionsSystem");
		    		    
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Favourites/Editor Enhancer",false,-100)]
		    public static void UltimateEditorEnhancer() => Import(
		    "Ultimate Editor Enhancer v3", "Infinity Code/Editor ExtensionsUtilities");
		    
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Favourites/Selection History",false,-100)]
		    public static void SelectionHistory() => Import(
		    "Selection History", "Staggart Creations/Editor ExtensionsUtilities"); 
		    
		    
		   
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Hot Reload")]
		    public static void HotReload() => Import(
		    "Hot Reload Edit Code Without Compiling", "The Naughty Cult/Editor ExtensionsUtilities");

		    
        	
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Organization/Smart Library",false,10)]
		    public static void SmartLibrary() => Import(
		    "Smart Library - Asset Manager", "Bewildered Studios/Editor ExtensionsUtilities");
        	
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Utility/Fullscreen Editor",false,10)]
		    public static void FullscreenEditor() => Import(
		    "Fullscreen Editor", "Muka Schultze/Editor ExtensionsUtilities");
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Odin/Odin Inspector",false,10)]
		    public static void OdinInspectorandSerializer() => Import(
		    "Odin Inspector and Serializer", "Sirenix/Editor ExtensionsSystem");
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Odin/Odin Validator",false,10)]
		    public static void OdinValidator() => Import(
		    "Odin Inspector and Serializer", "Sirenix/Editor ExtensionsUtilities");
		    
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Asset Management/Grab Yer Assets",false,10)]
		    public static void GrabYerAssets() => Import(
		    "Grab Yer Assets", "Xeir/Editor ExtensionsSystem");
		    
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Asset Management/Asset Cleaner Pro")]
		    public static void AssetCleanerPro() => Import(
		    "Asset Cleaner PRO - Clean Find References", "GameDev Tools/Editor ExtensionsUtilities");

		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Asset Management/Asset Usage Finder")]
		    public static void AssetUsageFinder() => Import(
		    "Asset Usage Finder", "GameDev Tools/Editor ExtensionsUtilities");

		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Asset Management/Asset Hunter 2")]
		    public static void AssetHunter2() => Import(
		    "Asset Hunter 2", "HeurekaGames/Editor ExtensionsUtilities");

		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Asset Management/Asset Hunter PRO")]
		    public static void AssetHunterPRO() => Import(
		    "Asset Hunter PRO", "HeurekaGames/Editor ExtensionsUtilities");
		    
		    
		    
		    
		    
		    
		    
		    
		    
		    
		    
		    
		    
        	
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Utility/Component Names",false,10)]
		    public static void ComponentNames() => Import(
		    "Component Names", "Sisus/Editor ExtensionsUtilities");
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Misc/Physics Transform Gizmo",false,10)]
		    public static void GrabbitEditorPhysicsTransforms() => Import(
		    "Grabbit - Editor Physics Transforms", "Jungle/Editor ExtensionsUtilities");
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Utility/Audio Preview",false,10)]
		    public static void AudioPreviewTool() => Import(
		    "Audio Preview Tool", "Warped Imagination/Editor ExtensionsAudio");
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Utility/Scene Bookmark",false,10)]
		    public static void SceneViewBookmarkTool() => Import(
		    "Scene View Bookmark Tool", "Warped Imagination/Editor ExtensionsUtilities");
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Utility/Scene Leap",false,10)]
		    public static void LEAP() => Import(
		    "LEAP", "ShrinkRay Entertainment/Editor ExtensionsDesign");
            
		    [MenuItem(
		    "Project Setup/Assets/Install/Plugins/Editor Extensions/Utility/F Zoom",false,10)]
		    public static void PerfectF() => Import(
		    "Perfect F - Zoom and Pivot around GameObjects Easily", "ShrinkRay Entertainment/Editor ExtensionsDesign");
		    
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Organization/Rainbow Folders",false,10)]
            public static void RainbowFolders() => Import(
                "Rainbow Folders 2", "Borodar/Editor ExtensionsSystem");

		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Hierarchy/Rainbow Hierarchy",false,10)]
            public static void RainbowHierarchy() => Import(
                "Rainbow Hierarchy 2", "Borodar/Editor ExtensionsSystem");

		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Hierarchy/QHierarchy",false,10)]
        public static void QHierarchy() => Import(
            "QHierarchy", "Sergey Smurov/Editor ExtensionsUtilities");

        
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Deprecated/Transform Pro",false,10)]
		    public static void TransformPro() => Import(
		    "TransformPro", "Untitled Games/Editor ExtensionsUtilities");
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Deprecated/Rider Flow",false,10)]
        public static void RiderFlow() => Import(
            "RiderFlow", "JetBrains/Editor ExtensionsDesign");
		    [MenuItem("Project Setup/Assets/Install/Plugins/Editor Extensions/Deprecated/MonKey",false,10)]
        public static void MonKeyProductivityCommands() => Import(
            "MonKey - Productivity Commands", "Jungle/Editor ExtensionsUtilities");        
	    
	    
	    
	    
	    
	    
	    
	    
	    }
	    
	    
	    public class Saving 
	    {
		    [MenuItem("Project Setup/Assets/Install/Saving/Save System")]
		    public static void SavesSystem() => Import(
		    "Save System", "Yu Yang/ScriptingIntegration");
		    
		    [MenuItem("Project Setup/Assets/Install/Saving/Databox")]
		    public static void Databox() => Import(
		    "Databox - Data editor save solution", "Giant Grey/Editor ExtensionsUtilities");

		    [MenuItem("Project Setup/Assets/Install/Saving/Easy Save")]
		    public static void EasySave() => Import(
		    "Easy Save - The Complete Save Data Serializer System", "Moodkie/Editor ExtensionsUtilities");

		    [MenuItem("Project Setup/Assets/Install/Saving/Save for Unity")]
		    public static void SaveForUnity() => Import(
		    "Save for Unity Complete", "SteveSmithSoftware/Editor ExtensionsUtilities");

		    [MenuItem("Project Setup/Assets/Install/Saving/EzCloud")]
		    public static void EzCloud() => Import(
		    "EzCloud - The Complete Cloud Solution for Unity", "Xerus Games/ScriptingIntegration");
	    }
	    
	    public class Audio 
	    {
		    [MenuItem("Project Setup/Assets/Install/Audio/Koreographer")]
		    public static void Koreographer() => Import(
		    "Koreographer Professional Edition", "Sonic Bloom/Editor ExtensionsAudio");
		    [MenuItem("Project Setup/Assets/Install/Audio/Fmod")]
		    public static void Fmod() => Import(
		    "FMOD for Unity", "FMOD/Editor ExtensionsAudio");
		    [MenuItem("Project Setup/Assets/Install/Audio/Music Beat Visualizer")]
		    public static void MusicBeat() => Import(
		    "Music Beat - Audio Visualizer", "Sindri Studios/Editor ExtensionsAudio");
		    [MenuItem("Project Setup/Assets/Install/Audio/Audio Helm")]
		    public static void AudioHelm() => Import(
		    "Audio Helm - Live Music Creator", "Matt Tytel/ScriptingAudio");
	    }
	    
	    
	    
        
	    public class Runtime 
	    { 
		    [MenuItem("Project Setup/Assets/Install/Plugins/Runtime/Consoles/Minibuffer Console",false,10)]
		    public static void MinibufferConsole() => Import(
		    "Minibuffer Console", "Seawisp Hunter LLC/ScriptingInput - Output");

		    [MenuItem("Project Setup/Assets/Install/Plugins/Runtime/Consoles/In-game Debug Console",false,10)]
		    public static void IngameDebugConsole() => Import(
		    "In-game Debug Console", "yasirkula/ScriptingGUI");
		    [MenuItem("Project Setup/Assets/Install/Plugins/Runtime/Consoles/SR Debugger")]
		    public static void SRDebugger() => Import(
		    "SRDebugger - Console Tools On-Device", "Stompy Robot LTD/ScriptingGUI");
		    [MenuItem("Project Setup/Assets/Install/Plugins/Runtime/Consoles/Quantum Console")]
		    public static void QuantumConsole() => Import(
		    "Quantum Console", "QFSW/Editor ExtensionsUtilities");
		    
		    
		    [MenuItem("Project Setup/Assets/Install/Plugins/Runtime/Editor")]
		    public static void RuntimeEditor() => Import(
		    "Runtime Editor", "Vadim Andriyanov/Editor ExtensionsModeling");

		    [MenuItem("Project Setup/Assets/Install/Plugins/Runtime/Transform Handles")]
		    public static void RuntimeTransformHandles() => Import(
		    "Runtime Transform Handles", "Vadim Andriyanov/Editor ExtensionsModeling");   
		    
	    }
	   
	    public class Simulation 
	    {
		    [MenuItem("Project Setup/Assets/Install/Simulation/Obi/Cables")]
		    public static void Cables() => Import(
		    "Filo - The Cable Simulator", "Virtual Method/ScriptingPhysics");

		    [MenuItem("Project Setup/Assets/Install/Simulation/Obi/Cloth")]
		    public static void ObiCloth() => Import(
		    "Obi Cloth", "Virtual Method/ScriptingPhysics");

		    [MenuItem("Project Setup/Assets/Install/Simulation/Obi/Fluid")]
		    public static void ObiFluid() => Import(
		    "Obi Fluid", "Virtual Method/ScriptingPhysics");

		    [MenuItem("Project Setup/Assets/Install/Simulation/Obi/Rope")]
		    public static void ObiRope() => Import(
		    "Obi Rope", "Virtual Method/ScriptingPhysics");

		    [MenuItem("Project Setup/Assets/Install/Simulation/Obi/Softbody")]
		    public static void ObiSoftbody() => Import(
		    "Obi Softbody", "Virtual Method/ScriptingPhysics");
	    
	    
		    [MenuItem("Project Setup/Assets/Install/Simulation/Creature/Animal Herd")]
		    public static void AnimalSimulationHerdSim() => Import(
		    "Animal Simulation HerdSim", "Unluck Software/3D ModelsCharactersAnimals");

		    [MenuItem("Project Setup/Assets/Install/Simulation/Creature/Bird Flock")]
		    public static void BirdFlockBundle() => Import(
		    "Bird Flock Bundle", "Unluck Software/3D ModelsCharactersAnimalsBirds");

		    [MenuItem("Project Setup/Assets/Install/Simulation/Creature/Fish")]
		    public static void FishSchoolBundle() => Import(
		    "Fish School Bundle", "Unluck Software/3D ModelsCharactersAnimalsFish");
	    
	    
	    	
	    }
	    
	    
	    public class Ui 
	    {
	    	
	    	public class Pack 
	    	{
		    	[MenuItem("Project Setup/Assets/Install/Ui/Pack/Gamestrap")]
		    	public static void Gamestrap() => Import(
		    	"UI - Gamestrap", "Gamestrap/Editor ExtensionsGUI");

		    	[MenuItem("Project Setup/Assets/Install/Ui/Pack/Modern Procedural UI Kit")]
		    	public static void ModernProceduralUIKit() => Import(
		    	"Modern Procedural UI Kit", "Scrollbie Studio/ScriptingGUI");
	    	
	    		
	    		
	    	}
	    	
	    	
	    	public class IMGUI 
	    	{
		    	[MenuItem("Project Setup/Assets/Install/Ui/Imgui/Doozy")]
		    	public static void Doozy() => Import(
		    	"Doozy UI Manager", "Doozy Entertainment/Editor ExtensionsVisual Scripting");
		    	
		    	[MenuItem("Project Setup/Assets/Install/Ui/Imgui/Infinite Scroll View")]
		    	public static void InfiniteScroll() => Import(
		    	"Infinite Scroll View 20", "DTT/ScriptingGUI");

		    	[MenuItem("Project Setup/Assets/Install/Ui/Imgui/Paginated Scrollviews")]
		    	public static void PaginatedScrolls() => Import(
		    	"Paginated Scrollviews", "DTT/ScriptingGUI");

		    	[MenuItem("Project Setup/Assets/Install/Ui/Imgui/Procedural UI")]
		    	public static void ProceduralUI() => Import(
		    	"Procedural UI", "DTT/ScriptingGUI");

		    	[MenuItem("Project Setup/Assets/Install/Ui/Imgui/Text Animator")]
		    	public static void TextAnimator() => Import(
		    	"Text Animator for Unity", "ScriptingGUI/Febucci Tools");
		    	[MenuItem("Project Setup/Assets/Install/Ui/Imgui/AnyUI")]
		    	public static void AnyUI() => Import(
		    	"AnyUI - Map Your UI On Any 3D Surface", "StereoArts/ScriptingGUI");
		    	
		    	
		    	[MenuItem("Project Setup/Assets/Install/Ui/Imgui/Procedural UI Image")]
		    	public static void ProceduralUIImage() => Import(
		    	"Procedural UI Image", "Josh H/ScriptingGUI");
		    	
		    	
		    	[MenuItem("Project Setup/Assets/Install/Ui/Pack/Gamestrap")]
		    	public static void UiBuilder() => Import(
		    	"UI - Builder", "beffio/Textures MaterialsGUI Skins");	
		    	
		    	
		    	
		    	
		    	

		    	[MenuItem("Project Setup/Assets/Install/Ui/Imgui/Fast UI Background Blur")]
		    	public static void TranslucentImage() => Import(
		    	"Translucent Image - Fast UI Background Blur", "Tais Assets/ScriptingGUI");

		    	[MenuItem("Project Setup/Assets/Install/Ui/Imgui/True Shadow")]
		    	public static void TrueShadow() => Import(
		    	"True Shadow - UI Soft Shadow and Glow", "Tais Assets/ScriptingGUI");
		    	
		    	[MenuItem("Project Setup/Assets/Install/Ui/Imgui/Ui Extensions")]
		    	public static void UIExtensions() => Import(
		    	"UI Extensions", "UI Extensions/Textures MaterialsGUI Skins");
		    	
		    	



		    	[MenuItem("Project Setup/Assets/Install/Ui/Imgui/Dock Panel")]
		    	public static void DockPanel() => Import(
		    	"Dock Panel", "Vadim Andriyanov/ScriptingGUI");

		    	[MenuItem("Project Setup/Assets/Install/Ui/Imgui/Menu Control")]
		    	public static void MenuControl() => Import(
		    	"Dock Panel", "Vadim Andriyanov/ScriptingGUI");

		    	[MenuItem("Project Setup/Assets/Install/Ui/Imgui/Virtualizing Tree View")]
		    	public static void VirtualizingTreeView() => Import(
		    	"Virtualizing Tree View", "Vadim Andriyanov/ScriptingGUI");
		    	
		    	
		    	
		    	
		    	
	    	}
	    	
	    	
	    	public class Toolkit 
	    	{
	    		
		    	[MenuItem("Project Setup/Assets/Install/Ui/Toolkit/Ui Particles")]
		    	public static void UiParticles() => Import(
		    	"UI Toolkit Particles - Particle Image for UI Elements", "KAMGAM/ScriptingGUI");

		    	[MenuItem("Project Setup/Assets/Install/Ui/Toolkit/Unified Settings")]
		    	public static void UnifiedSettingsGameOptionsUI() => Import(
		    	"Unified Settings Game Options UI", "KAMGAM/ScriptingGUI");

		    	[MenuItem("Project Setup/Assets/Install/Ui/Toolkit/3D Object Image")]
		    	public static void KamMinimap() => Import(
		    	"3D Object Image for UI Toolkit - World objects Mini-Maps Inventory Portraits",
		    	"KAMGAM/Textures MaterialsGUI Skins");

		    	[MenuItem("Project Setup/Assets/Install/Ui/Toolkit/Blurred Background")]
		    	public static void KamBlur() => Import(
		    	"UI Toolkit Blurred Background - Fast translucent UI Blur image", "KAMGAM/Textures MaterialsGUI Skins");

		    	[MenuItem("Project Setup/Assets/Install/Ui/Toolkit/UI Bundle")]
		    	public static void KamUiToolkit() => Import(
		    	"UI Toolkit Bundle", "KAMGAM/Textures MaterialsGUI Skins");

		    	[MenuItem("Project Setup/Assets/Install/Ui/Toolkit/Scroll View")]
		    	public static void InfiniScroll() => Import(
		    	"UI Toolkit Scroll View Pro - Infinite Scrolling Snapping Paging", "KAMGAM/Textures MaterialsGUI Skins");

		    	[MenuItem("Project Setup/Assets/Install/Ui/Toolkit/Shadows Outlines Glow")]
		    	public static void ShadowGlow() => Import(
		    	"UI Toolkit Shadows Outlines Glow", "KAMGAM/Textures MaterialsGUI Skins");


	    		
	    		
	    	}
	    	
		    public class Debugging 
		    {
		    	
			    [MenuItem("Project Setup/Assets/Install/Plugins/Debugging/Monitor Components")]
			    public static void MonitorComponents() => Import(
			    "Monitor Components", "Peter Bruun/Editor ExtensionsUtilities");

			    [MenuItem("Project Setup/Assets/Install/Plugins/Debugging/Draw XXL")]
			    public static void DrawXXL() => Import(
			    "Draw XXL", "Symphony Games/Editor ExtensionsUtilities");
		    	
		    	
		    }	
	    	
	    
	    	public class Icons 
	    	{

		    	public class Controllers 
		    	{
			    	[MenuItem(
			    	"Project Setup/Assets/Install/Ui/Icons/Controller/Standard")]
        public static void ControllerOverlays() => Import(
			    	"Controller Overlays Button Kits 3 styles x8 controllers keyboardmouse",
			    	"nickknacks/Textures MaterialsIcons UI");

        [MenuItem("Project Setup/Assets/Install/Ui/Icons/Controller/VR")]
        public static void VrControllerOverlays() => Import(
			    	"VR Controller Overlay Kits 3 styles x7 controllers", "nickknacks/Textures MaterialsIcons UI");

        [MenuItem("Project Setup/Assets/Install/Ui/Icons/Controller/Icon Pack")]
        public static void ControllerIconPack() => Import(
			    	"Controller Icon Pack", "NullSave/Textures MaterialsIcons UI");
		    		
		    		
		    	}

	    		
	    		
	    	}
	    	
	    	
	    	
	     	
	    }
	    
	    
	    public class Logic 
	    {
	    
		    [MenuItem("Project Setup/Assets/Install/Plugins/Logic/Graphs/Behaviour Designer")]
		    public static void BehaviourDesigner() => Import(
		    "Behavior Designer - Behavior Trees for Everyone", "Opsive/Editor ExtensionsVisual Scripting");

		    [MenuItem("Project Setup/Assets/Install/Plugins/Logic/Graphs/Behaviour Designer (Movement)")]
		    public static void BehaviourDesignerMovement() => Import(
		    "Behavior Designer - Movement Pack", "Opsive/Editor ExtensionsVisual Scripting");

		    [MenuItem("Project Setup/Assets/Install/Plugins/Logic/Graphs/Flow Canvas")]
		    public static void FlowCanvas() => Import(
		    "FlowCanvas", "Paradox Notion/Editor ExtensionsVisual Scripting");

		    [MenuItem("Project Setup/Assets/Install/Plugins/Logic/Graphs/Node Canvas")]
		    public static void NodeCanvas() => Import(
		    "NodeCanvas", "Paradox Notion/Editor ExtensionsVisual Scripting");
	    	
	    	
	    }
	    
	    
	    public class Effects 
	    {
	    	
	    	public class Shaders 
	    	{
		    	[MenuItem("Project Setup/Assets/Install/Effects/Shaders/Highlight/Highlight Plus")]
		    	public static void HighlightPlus() => Import(
		    	"Highlight Plus - All in One Outline Selection Effects", "Kronnect/Shaders");   		
		    	[MenuItem("Project Setup/Assets/Install/Effects/Post Processing/Curved UI")]
		    	public static void CurvedUI() => Import(
		    	"Curved UI - VR Ready Solution To Bend Warp Your Canvas", "Chisely/Editor ExtensionsGUI");
		    	
		    	[MenuItem("Project Setup/Assets/Install/Effects/Shaders/Tilt Shift")]
		    	public static void TiltShift() => Import(
		    	"Artistic Tilt Shift", "Fronkon Games/ShadersFullscreen Camera Effects");
		    	
		    	[MenuItem("Project Setup/Assets/Install/Effects/Shaders/Transitions Plus")]
		    	public static void TransitionsPlus() => Import(
		    	"Transitions Plus", "Kronnect/ScriptingCamera");
		    	
		    	
		    	[MenuItem("Project Setup/Assets/Install/Effects/Damage Numbers Pro")]
		    	public static void DamageNumbersPro() => Import(
		    	"Damage Numbers Pro", "Ekincan Tas/Textures MaterialsGUI Skins");
		    	
		    	
		    	[MenuItem("Project Setup/Assets/Install/Effects/Shader//All in 1/Sprite Shader")]
		    	public static void AllIn1SpriteShader() => Import(
		    	"All In 1 Sprite Shader", "Seaside Studios/Shaders");

		    	[MenuItem("Project Setup/Assets/Install/Effects/Shader/All in 1/Vfx Textures")]
		    	public static void AllIn1VfxTextures() => Import(
		    	"All In 1 Vfx Textures", "Seaside Studios/VFX");

		    	[MenuItem("Project Setup/Assets/Install/Effects/Shader/All in 1/Vfx Toolkit")]
		    	public static void AllIn1VfxToolkit() => Import(
		    	"All In 1 Vfx Toolkit", "Seaside Studios/VFX");
		    	
		    	[MenuItem("Project Setup/Assets/Install/Effects/Shader/Fullscreen Camera FX")]
		    	public static void ColorfulFX() => Import(
		    	"Colorful FX", "Thomas Hourdel/ShadersFullscreen Camera Effects");
	    	
	    	
		    	
	    		   
	    	}
	    	
	    	public class Particles 
	    	{
		    	[MenuItem("Project Setup/Assets/Install/Effects/Particles/Stylized VFX")]
		    	public static void StylizedVFX() => Import(
		    	"Stylized VFX", "Vefects/VFX");

		    	[MenuItem("Project Setup/Assets/Install/Effects/Particles/Stylized VFX (URP)")]
		    	public static void StylizedVFXURP() => Import(
		    	"Stylized VFX URP", "Vefects/VFX");
	    	}
	    	
	 
	    	
	    	
		    [MenuItem("Project Setup/Assets/Install/Effects/Trails FX")]
		    public static void TrailsFX() => Import(
		    "Trails FX", "Kronnect/Shaders");
	    	
	    	
	    } 
	    
	    public class Inputs 
	    {
		    [MenuItem("Project Setup/Assets/Install/Inputs/Rewired")]
		    public static void Rewired() => Import(
		    "Rewired", "Guavaman Enterprises/Editor ExtensionsUtilities");
	    	
	    	
	    	
	    }
	    
	   
	   
	   
	    public class Tweening 
	    {
		    [MenuItem("Project Setup/Assets/Install/Plugins/Tweening/Prime Tween")]
		    public static void PrimeTween() => Import(
		    "PrimeTween High-Performance Animations and Sequences", "Kyrylo Kuzyk/Editor ExtensionsAnimation");
		    [MenuItem("Project Setup/Assets/Install/Plugins/Tweening/DoTween Pro")]
		    public static void DOTweenPro() => Import(
		    "DOTween Pro", "Demigiant/Editor ExtensionsVisual Scripting");
		    [MenuItem("Project Setup/Assets/Install/Plugins/Tweening/DoTween")]
		    public static void DOTween() => Import(
		    "DOTween HOTween v2", "Demigiant/Editor ExtensionsAnimation");
	    	
	    }
	    
	    public class Lighting 
	    {
		    [MenuItem("Project Setup/Assets/Install/Plugins/Lighting/Magic Light Probes")]
		    public static void MagicLightProbes() => Import(
		    "Magic Light Probes", "Eugene B/Editor ExtensionsUtilities");

		    [MenuItem("Project Setup/Assets/Install/Plugins/Lighting/Bakery")]
		    public static void BakeryGPULightmapper() => Import(
		    "Bakery - GPU Lightmapper", "Mr F/Editor ExtensionsDesign");

		    [MenuItem("Project Setup/Assets/Install/Plugins/Lighting/Bakery Previewer")]
		    public static void BakeryRealTimePreview() => Import(
		    "Bakery Real-Time Preview", "Mr F/Editor ExtensionsDesign");
	    }
	    
	    public class Models 
	    {
	    	public class Characters 
	    	{
		    	[MenuItem("Project Setup/Assets/Install/3d Models/Characters/Monsters/Pack 4")]
		    	public static void MonstersPack04() => Import(
		    	"Monsters Pack 04", "NOTFUN/3D ModelsCharactersCreatures");

		    	[MenuItem("Project Setup/Assets/Install/3d Models/Characters/Monsters/Pack 5")]
		    	public static void MonstersPack05() => Import(
		    	"Monsters Pack 05", "NOTFUN/3D ModelsCharactersCreatures");

		    	[MenuItem("Project Setup/Assets/Install/3d Models/Characters/Monsters/Protofactor")]
		    	public static void MONSTERFULLPACKVOL1() => Import(
		    	"MONSTER FULL PACK VOL 1", "PROTOFACTOR INC/3D ModelsCharactersCreatures");

		    	[MenuItem("Project Setup/Assets/Install/3d Models/Characters/Animals/Quirky/Pack 1")]
		    	public static void QuirkyAnimals1() => Import(
		    	"Quirky Series - Animals Mega Pack Vol 1", "Omabuarts Studio/3D ModelsCharactersAnimals");

		    	[MenuItem("Project Setup/Assets/Install/3d Models/Characters/Animals/Quirky/Pack 3")]
		    	public static void QuirkyAnimals3() => Import(
		    	"Quirky Series - Animals Mega Pack Vol 3", "Omabuarts Studio/3D ModelsCharactersAnimals");
	    	}	
	    	
	    	
	    }
	    
	    public class Animations 
	    {
	    	public class Packs 
	    	{
		    	[MenuItem("Project Setup/Assets/Install/Animation/Pack/Misc/Ninja")]
		    	public static void Ninja() => Import(
		    	"Bare Ninja AnimSet", "wemakethegame/Animation");

		    	[MenuItem("Project Setup/Assets/Install/Animation/Pack/Misc/Capoeira")]
		    	public static void CapoeiraAnimSet() => Import(
		    	"Capoeira Anim Set", "wemakethegame/Animation");

		    	[MenuItem("Project Setup/Assets/Install/Animation/Pack/Weapons/Swords/Sword Finishers")]
		    	public static void CruelSwordFinisherSet() => Import(
		    	"Cruel Sword Finisher Set", "wemakethegame/Animation");

		    	[MenuItem("Project Setup/Assets/Install/Animation/Pack/Fantasy/Goblin")]
		    	public static void GoblinSwordShieldAnimSet() => Import(
		    	"Goblin Sword Shield AnimSet", "wemakethegame/Animation");

		    	[MenuItem("Project Setup/Assets/Install/Animation/Pack/Weapons/Swords/Great DualBlade")]
		    	public static void GreatDualBladeAnimSet() => Import(
		    	"Great DualBlade AnimSet", "wemakethegame/Animation");

		    	[MenuItem("Project Setup/Assets/Install/Animation/Pack/Weapons/Spear-Shield")]
		    	public static void InsaneSpearShieldAnimSet() => Import(
		    	"Insane Spear-Shield Anim Set", "wemakethegame/Animation");

		    	[MenuItem("Project Setup/Assets/Install/Animation/Pack/Weapons/Nodachi")]
		    	public static void NodachiAnimSet() => Import(
		    	"Nodachi AnimSet", "wemakethegame/Animation");

		    	[MenuItem("Project Setup/Assets/Install/Animation/Pack/Weapons/Swords/Oriental")]
		    	public static void OrientalSwordAnimSet() => Import(
		    	"Oriental Sword AnimSet", "wemakethegame/Animation");

		    	[MenuItem("Project Setup/Assets/Install/Animation/Pack/Fantasy/Wizard")]
		    	public static void ScriptWizardAnimSet() => Import(
		    	"Script Wizard Anim Set", "wemakethegame/Animation");

		    	[MenuItem("Project Setup/Assets/Install/Animation/Pack/Fantasy/Superhero")]
		    	public static void superheroanimset() => Import(
		    	"superhero animset", "wemakethegame/Animation");

		    	[MenuItem("Project Setup/Assets/Install/Animation/Pack/Fantasy/Werewolf")]
		    	public static void WerewolfAnimSet() => Import(
		    	"Werewolf AnimSet", "wemakethegame/Animation");
		    	
		    	
		    	[MenuItem("Project Setup/Assets/Install/Animation/Pack/Misc/Horse Riding")]
		    	public static void HorseSet() => Import(
		    	"Horse Animset Pro Riding System", "MalberS Animations/3D ModelsCharactersAnimals");
		    	
		    	
		    	
	    	}
	    	
		    [MenuItem("Project Setup/Assets/Install/Animation/Modifying/Final IK")]
		    public static void FinalIK() => Import(
		    "Final IK", "RootMotion/Editor ExtensionsAnimation");
		    [MenuItem("Project Setup/Assets/Install/Animation/Modifying/Motion Matching")]
		    public static void MotionMatchingforUnity() => Import(
		    "Motion Matching for Unity", "Vault Break Studios/Editor ExtensionsAnimation");
		    [MenuItem("Project Setup/Assets/Install/Animation/Modifying/UMotion")]
		    public static void UMotion() => Import(
		    "UMotion Pro - Animation Editor", "Soxware Interactive/Editor ExtensionsAnimation");
		    [MenuItem("Project Setup/Assets/Install/Animation/Modifying/PuppetMaster")]
		    public static void PuppetMaster() => Import(
		    "PuppetMaster", "ScriptingPhysics/Editor ExtensionsAnimation");
		    [MenuItem("Project Setup/Assets/Install/Animation/Modifying/Animancer Pro")]
		    public static void AnimancerPro() => Import(
		    "Animancer Pro", "Kybernetik/ScriptingAnimation");
		    [MenuItem("Project Setup/Assets/Install/Animation/Modifying/Dynamic Bone")]
		    public static void DynamicBone() => Import(
		    "Dynamic Bone", "Will Hong/Editor ExtensionsAnimation");
	    }
	    
	    public class Controllers 
	    {
		    [MenuItem("Project Setup/Assets/Install/System/Controllers/Malbers Animals")]
		    public static void MalbersAnimals() => Import(
		    "Animal Controller Malbers Character Controller", "MalberS Animations/Editor ExtensionsAnimation");
		    	    
		    

		    [MenuItem("Project Setup/Assets/Install/System/Controllers/Corgi Engine")]
		    public static void Corgi() => Import(
		    "Corgi Engine - 2D 25D Platformer", "More Mountains/Complete ProjectsSystems");
		  
		    [MenuItem("Project Setup/Assets/Install/System/Controllers/Kinematic Character Controller")]
		    public static void KinematicCharacterController() => Import(
		    "Kinematic Character Controller", "Philippe St-Amand/ScriptingPhysics");
		    
		    [MenuItem("Project Setup/Assets/Install/System/Controllers/Rival")]
		    public static void RivalDOTSCharacterController() => Import(
		    "Rival - DOTS Character Controller", "Philippe St-Amand/ScriptingPhysics");

	    	
	    	
	    }
	    
	    public class Dialog 
	    {
		    [MenuItem("Project Setup/Assets/Install/System/Dialogue/Love Hate")]
		    public static void LoveHate() => Import(
		    "LoveHate", "Pixel Crushers/ScriptingAI");

		    [MenuItem("Project Setup/Assets/Install/System/Dialogue/Dialog System for Unity")]
		    public static void DialogueSystemforUnity() => Import(
		    "Dialogue System for Unity", "Pixel Crushers/ScriptingAI");
		    
		    
		    [MenuItem("Project Setup/Assets/Install/Dialogue/Storyteller")]
		    public static void Storyteller() => Import(
		    "Storyteller", "Ruchmair/ScriptingIntegration");
		    [MenuItem("Project Setup/Assets/Install/Dialogue/I2 Localization")]
		    public static void LocalizationI2() => Import(
		    "I2 Localization", "Inter Illusion/Editor ExtensionsLanguage");
		    
		    [MenuItem("Project Setup/Assets/Install/Dialogue/Ink")]
		    public static void Ink() => Import(
		    "Ink Integration for Unity", "inkle/ScriptingIntegration");
	    }
	    
	    
	    public class Prototyping 
	    {
	    	
		    [MenuItem("Project Setup/Assets/Install/Prototyping/Textures")]
		    public static void PrototypeTextures() => Import(
		    "Gridbox Prototype Materials", "Ciathyza/Textures Materials");

		    [MenuItem("Project Setup/Assets/Install/Prototyping/UModeler")]
		    public static void UModeler() => Import(
		    "UModeler", "UModeler Inc/Editor ExtensionsModeling");
	    	
		    [MenuItem("Project Setup/Assets/Install/Prototyping/Archimax Pro")]
		    public static void Archimax() => Import(
		    "Archimatix Pro", "Roaring Tide Productions/Editor ExtensionsModeling");	
	    	
	    }
	    
	    
	    
	    
	    
	    public class Environment 
	    {
	    	public class Skyboxes 
	    	{
		    	[MenuItem("Project Setup/Assets/Install/Environment/Skybox/Fantasy")]
		    	public static void FantasySkybox() => Import(
		    	"Fantasy Skybox", "Render Knight/Textures MaterialsSkies");

		    	[MenuItem("Project Setup/Assets/Install/Environment/Skybox/AllSky")]
		    	public static void AllSky() => Import(
		    	"AllSky - 220 Sky Skybox Set", "rpgwhitelock/Textures MaterialsSkies");
	    		
	    		
	    	}
	    	
	    	
	    	
	    }
	    
	    
	    public class Misc 
	    {
		    [MenuItem("Project Setup/Assets/Install/Misc/Performance Tools")]
		    public static void PerformanceTools() => Import(
		    "Performance Tools", "New Game Studio/Editor ExtensionsUtilities");

		    [MenuItem("Project Setup/Assets/Install/Misc/World Streamer 2")]
		    public static void WorldStreamer() => Import(
		    "World Streamer 2", "NatureManufacture/Editor ExtensionsTerrain");
	    	
		    [MenuItem("Project Setup/Assets/Install/Misc/Fast Pool")]
		    public static void FastPool() => Import(
		    "Fast Pool", "Watermelon Assets/Scripting");
		    [MenuItem("Project Setup/Assets/Install/Misc/Inventory Pro")]
		    public static void InventoryPro() => Import(
		    "Inventory Pro", "Sirenix/ScriptingGUI");
        
		    [MenuItem("Project Setup/Assets/Install/Misc/Impostors")]
		    public static void ImpostorsRuntimeOptimization() => Import(
		    "Impostors - Runtime Optimization", "STARasGAMES/Editor ExtensionsUtilities");
		    [MenuItem("Project Setup/Assets/Install/Misc/Editor Enhancers Bundle")]
		    public static void EditorEnhancersBundle() => Import(
		    "Editor Enhancers Bundle", "kubacho lab/Editor ExtensionsUtilities");

		    [MenuItem("Project Setup/Assets/Install/Misc/vTabs 2")]
		    public static void vTabs2() => Import(
		    "vTabs 2", "kubacho lab/Editor ExtensionsUtilities");

		    [MenuItem("Project Setup/Assets/Install/Misc/Inspector Gadget")]
		    public static void InspectorGadgetsPro() => Import(
		    "Inspector Gadgets Pro", "Kybernetik/Editor ExtensionsGUI");

		    [MenuItem("Project Setup/Assets/Install/Misc/Weaver Pro")]
		    public static void WeaverPro() => Import(
		    "Weaver Pro", "Kybernetik/Editor ExtensionsSystem");

		    [MenuItem("Project Setup/Assets/Install/Misc/ColorTools")]
		    public static void ColorTools() => Import(
		    "ColorTools", "LotteMakesStuff/Scripting");


		    [MenuItem("Project Setup/Assets/Install/Misc/InfinityPBR Game Modules")]
		    public static void GameModules() => Import(
		    "Game Modules 3", "Infinity PBR Magic Pig Games/Editor ExtensionsGame ToolkitsRPG Toolkits");
		    
		    
		    [MenuItem("Project Setup/Assets/Install/Misc/Audio Video Options Menu")]
		    public static void AudioVideoOptionsMenu() => Import(
		    "Audio Video Options Menu", "PaulosCreations/ScriptingGUI");
		    [MenuItem("Project Setup/Assets/Install/Misc/Gamelogic Colors")]
		    public static void GamelogicColors() => Import(
		    "Colors", "Gamelogic/Editor ExtensionsUtilities");

		    [MenuItem("Project Setup/Assets/Install/Misc/Gamelogic Extensions")]
		    public static void GamelogicExtensions() => Import(
		    "Gamelogic Extensions", "Gamelogic/Editor ExtensionsUtilities");
		    [MenuItem("Project Setup/Assets/Install/Misc/Ultimate Screenshot Tool")]
		    public static void UltimateScreenshotTool() => Import(
		    "Ultimate Screenshot Tool", "Tangled Reality Studios/Editor ExtensionsUtilities");

		    [MenuItem("Project Setup/Assets/Install/Misc/Screenshot Companion")]
		    public static void ScreenshotCompanion() => Import(
		    "Screenshot Companion", "Topicbird/Editor ExtensionsUtilities");
		    [MenuItem("Project Setup/Assets/Install/Misc/Ultimate Screenshot Creator")]
		    public static void UltimateScreenshotCreator() => Import(
		    "Ultimate Screenshot Creator", "Wild Mage Games/Editor ExtensionsUtilities");
		    [MenuItem("Project Setup/Assets/Install/Misc/DLSS - Upscaling for Unity")]
		    public static void DLSSUpscaling() => Import(
		    "DLSS - Upscaling for Unity", "The Naked Dev/Editor ExtensionsUtilities");
		    [MenuItem("Project Setup/Assets/Install/Misc/More Effective Coroutines PRO")]
		    public static void MoreEffectiveCoroutines() => Import(
		    "More Effective Coroutines PRO", "Trinary Software/ScriptingAnimation");
		    [MenuItem("Project Setup/Assets/Install/Misc/Roslyn C - Runtime Compiler")]
		    public static void RoslynRuntimeCompiler() => Import(
		    "Roslyn C - Runtime Compiler", "Trivial Interactive/ScriptingIntegration");
		    [MenuItem("Project Setup/Assets/Install/Misc/Real Size")]
		    public static void RealSize() => Import(
		    "Real Size Measure and Resize with Real World Units", "Keith Swanger/Editor ExtensionsUtilities");
		    [MenuItem("Project Setup/Assets/Install/Misc/Umbra Boundary Builder")]
		    public static void UmbraBoundaryBuilder() => Import(
		    "Umbra Boundary Builder", "Umbra Evolution/Editor ExtensionsDesign");
		    [MenuItem("Project Setup/Assets/Install/Misc/Moveen")]
		    public static void Moveen() => Import(
		    "Moveen", "Yuri Kravchik/Editor ExtensionsAnimation");
		    [MenuItem("Project Setup/Assets/Install/Misc/TypeSafe")]
		    public static void TypeSafe() => Import(
		    "TypeSafe", "Stompy Robot LTD/Editor ExtensionsUtilities");
		    [MenuItem("Project Setup/Assets/Install/Misc/White Cats Toolbox")]
		    public static void WhiteCatsToolbox() => Import(
		    "White Cats Toolbox", "Yu Yang/ScriptingIntegration");
		    [MenuItem("Project Setup/Assets/Install/Misc/Color Studio")]
		    public static void ColorStudio() => Import(
		    "Color Studio", "Kronnect/Editor ExtensionsPainting");
	    }
	    
	    public class Juice 
	    {
		    [MenuItem("Project Setup/Assets/Install/Systems/Juice/Shapes")]
		    public static void Shapes() => Import(
		    "Shapes", "Freya Holmr/Editor ExtensionsEffects");
	    	
		    [MenuItem("Project Setup/Assets/Install/Systems/Juice/Feel")]
		    public static void Feel() => Import(
		    "Feel", "More Mountains/Editor ExtensionsEffects");
	    	
	    }
	    
	    
	    
	    
	    
	    
	    
	    
	    
	    
	    
	    
	    
        
        
        
        
    }
}