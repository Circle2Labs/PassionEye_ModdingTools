using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Code.EditorScripts.ModCreator;
using Code.Frameworks.Character.CharacterObjects;
using Code.Tools;
using Microsoft.CSharp;
using Packages.SFB;
using UnityEditor;
using UnityEngine;

namespace Code.Editor.ModEngine
{
	public partial class ModCreator 
	{
		[SerializeField]
		public Tools.Tuple.SerializableTuple<string, byte[]> Assembly;

		private Vector2 advancedScrollPosition;
		
		private Assembly tempAssembly;

		public void DrawAdvanced()
		{
			if (Templates == null || Templates.Count == 0)
				return;
			
			var template = Templates[CurrentTemplate];

			GUILayout.Space(2);
			
			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_ADV_CODEREL"), EditorStyles.boldLabel);
			template.Type = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_ADV_CUSTOMTYPE")}*", template.Type);
			template.Type = Regex.Replace(template.Type, nameTypeFilter, "");
			
			GUILayout.Space(10);

			advancedScrollPosition = GUILayout.BeginScrollView(advancedScrollPosition);
			
			template.Usings = verticalList(template.Usings, GetLocalizedString("MODCREATOR_ADV_USINGS"));
			
			GUILayout.Space(10);
			
			EditorGUILayout.LabelField($"{GetLocalizedString("MODCREATOR_ADV_SOURCE")}*", EditorStyles.boldLabel);
			template.Source = EditorGUILayout.TextArea(template.Source, GUILayout.ExpandHeight(true));

			GUILayout.EndScrollView();
			
			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_ADV_WARN1"));
			EditorGUILayout.LabelField(template.TemplateType == ETemplateType.CharacterObject
				? GetLocalizedString("MODCREATOR_ADV_WARN2")
				: GetLocalizedString("MODCREATOR_ADV_WARN3"));
			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_ADV_WARN4"));
			
			GUILayout.BeginHorizontal();

			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADV_IMPORT")))
			{
				StandaloneFileBrowser.OpenFilePanelAsync(GetLocalizedString("MODCREATOR_ADV_IMPORTSRC"), "", "cs", false, delegate(string[] files)
				{
					if (files.Length == 0 || files[0] == "")
						return;

					if (FileTools.ReadAllTextSafe(files[0], out var source))
					{
						template.Source = source;
						
						Debug.Log("Source Code imported from " + files[0]);

						var from = template.Source.IndexOf(" class ", StringComparison.InvariantCulture);
						var to = template.Source.IndexOf(" : Base", StringComparison.InvariantCulture);

						if (from == -1 || to == -1)
						{
							Debug.LogError("No type found in imported file");
							return;
						}

						from += 7;

						template.Type = template.Source.Substring(from, to - from);
					}
					else
					{
						Debug.LogError($"Failed to read source code from {files[0]}");
					}
				});
			}

			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADV_EXPORT")))
			{
				StandaloneFileBrowser.SaveFilePanelAsync(GetLocalizedString("MODCREATOR_ADV_EXPORTSRC"), "", template.Type + ".cs", "cs", delegate(string file)
				{
					if (string.IsNullOrEmpty(file))
						return;

					if (FileTools.WriteAllTextSafe(file, template.Source))
						Debug.Log("Source Code exported to " + file);
					else
						Debug.LogError("Failed writing Source Code to " + file);
				});
			}

			GUILayout.EndHorizontal();
		}

		public void DrawAdvancedToggle(Template template)
		{
			if (template.TemplateType == ETemplateType.ModdedScene)
				return;
			
			template.Advanced = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_ADVEDIT"), template.Advanced);
		}
		
		public Component AssignCustomComponent(GameObject gameObject, string targetType)
		{
			var type = tempAssembly.GetExportedTypes().SingleOrDefault(t => t.Name == targetType);
			return type == null ? null : gameObject.AddComponent(type);
		}

		public void AdvancedAssignCheck(Template template, ref bool pass)
		{
			if (!template.Advanced)
				return;
			
			if (string.IsNullOrEmpty(template.Type))
			{
				pass = false;
				Debug.LogWarning($"Custom Type is not set for {template.Name}");
			}

			if (Templates.Any(innerTemplate => innerTemplate != template && innerTemplate.Advanced && innerTemplate.Type == template.Type))
			{
				pass = false;
				Debug.LogWarning($"Custom Type is not unique for {template.Name}");
			}
					
			if (string.IsNullOrEmpty(template.Source))
			{
				pass = false;
				Debug.LogWarning($"Source Code is not set for for {template.Name}");
			}
		}
		
		public void AdvancedBuildCheck(Template template, ref bool checkAsm, ref bool pass)
		{
			if (!checkAsm || !template.Advanced || !string.IsNullOrEmpty(Assembly.Item1)) 
				return;
			
			checkAsm = false;
			pass = false;
			Debug.LogWarning("Mod assembly is not built");
		}
		
		public void BuildAssembly(string asmName, string code)
		{
			if (code == "")
				return;

			var parameters = new CompilerParameters
			{
				GenerateExecutable = false,
				GenerateInMemory = true,
				OutputAssembly = asmName,
			};
			
			var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assemblyName in typeof(BaseClothing).Assembly.GetReferencedAssemblies())
			{
				var assembly = allAssemblies.SingleOrDefault(asm => asm.FullName == assemblyName.FullName);
				if (assembly == null || assembly.IsDynamic || assembly.FullName.Contains("mscorlib.dll"))
					continue;

				parameters.ReferencedAssemblies.Add(assembly.Location);
			}

			parameters.ReferencedAssemblies.Add(typeof(BaseClothing).Assembly.Location);
			
			var provider = new CSharpCodeProvider();
			var result = provider.CompileAssemblyFromSource(parameters, code);
			
			result.Errors.Cast<CompilerError>().ToList().ForEach(error => Debug.LogError(error.ErrorText));
			if (result.Errors.Count != 0) 
				return;
				
			tempAssembly = result.CompiledAssembly;
				
			if (!File.Exists(asmName))
			{
				Debug.LogError("Generated assembly not found");
				return;
			}

			if (FileTools.ReadAllBytesSafe(asmName, out var bytes))
			{
				Assembly = new Tools.Tuple.SerializableTuple<string, byte[]>(asmName, bytes);
			}
			else
			{
				Debug.LogError($"Failed to read generated assembly {asmName}");
				return;
			}
		}

		public void CacheAssembly(string asmName, string code)
		{
			if (code == "")
				return;
			
			Directory.CreateDirectory(Path.Combine(ModCreatorPath, "Code/Editor/ModEngine/Cache"));
			
			var asmPath = Path.Combine(ModCreatorPath, "Code/Editor/ModEngine/Cache", asmName);
			var codePath = Path.Combine(ModCreatorPath, "Code/Editor/ModEngine/Cache", asmName + ".cs.source");
				
			if (File.Exists(asmPath))
				File.Delete(asmPath);

			if (File.Exists(codePath))
				File.Delete(codePath);

			if (FileTools.MoveSafe(asmName, asmPath))
				Debug.Log($"Cached assembly to {asmPath}");
			else
				Debug.LogError($"Failed caching assembly to {asmPath}");
				
			if (FileTools.WriteAllTextSafe(codePath, code))
				Debug.Log($"Cached code to {codePath}");
			else
				Debug.LogError($"Failed caching code to {codePath}");
				
			AssetDatabase.ImportAsset(asmPath, ImportAssetOptions.ForceSynchronousImport);
			
			var import = AssetImporter.GetAtPath(asmPath) as PluginImporter;
			import.SetCompatibleWithAnyPlatform(false);
			import.SetCompatibleWithEditor(true);
			import.SaveAndReimport();
		}
		
		public string GetCompleteSource()
		{
			var custom = Templates.Any(template => template.Advanced);
			if (!custom)
				return "";

			var source = $@"using System;
using Code.Frameworks.Character;
using Code.Frameworks.Character.CharacterObjects;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.Character.Structs;

using Code.Frameworks.ModdedScenes;
using Code.Frameworks.ModdedScenes.Flags;

using Code.Frameworks.PhysicsSimulation;

using Code.Frameworks.ForwardKinematics;
using Code.Frameworks.ForwardKinematics.Interfaces;
using Code.Frameworks.ForwardKinematics.Structs;

using Code.Frameworks.Studio;
using Code.Frameworks.Studio.StudioObjects;
using Code.Frameworks.Studio.Enums;
using Code.Frameworks.Studio.Interfaces;

using Code.Interfaces;

{GetCompleteUsings()}
namespace {Manifest.Author}.{Manifest.Name} 
{{
";

			source = Templates.Where(template => template.Advanced && template.Type != "").Aggregate(source, (current, template) => current + template.Source + "\n");
			source += "}";
			
			return source;
		}

		public string GetCompleteUsings()
		{
			var usings = new List<string>();
			usings = Templates.Where(template => template.Advanced).Aggregate(usings, (current, template) => current.Union(template.Usings).ToList());

			var builder = new StringBuilder();
			
			foreach (var usingStr in usings)
				builder.AppendLine($"using {usingStr};");

			return builder.ToString();
		}
	}
}