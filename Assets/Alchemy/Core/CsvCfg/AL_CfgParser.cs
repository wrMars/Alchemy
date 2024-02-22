/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Alchemy
{
    /// <summary>
    /// AL通用配置框架
    /// 使用：
    /// 新建配置Vo类文件（继承于AL_BaseCfgVo,并以Vo结尾），比如：DnTestVo；
    /// 在编辑器界面右键选中该类文件，操作Alchemy/配置下的工具脚本，生成基础的的asset类、csv、；也可以选中csv生成对应的Scriptable asset数据
    ///
    /// 配置加载方式：（以DnTestVosData举例）
    /// 1、代码中选个地方先调用AL_CfgLoader.PreLoadAllDatas（该类有多种预加载的方法，可选择一种）预加载，然后可以直接使用 DnTestVosData.DataList
    /// 2、直接使用异步加载 DnTestVosData.LoadAssets
    /// 
    /// </summary>
    public class AL_CfgParser
    {
        private static readonly string _cfgDir = "Assets/DNClient/Config";
        private static string _cfgDirFullPath => AL_PathTools.AssetPath2FullPath(_cfgDir);
        private static readonly string _csvDir = AL_PathTools.Combine(_cfgDir, "CSV");
        private static string _csvDirFullPath => AL_PathTools.AssetPath2FullPath(_csvDir);
        private static readonly string _voDir = AL_PathTools.Combine(_cfgDir, "VO");
        private static string _voDirFullPath => AL_PathTools.AssetPath2FullPath(_voDir);
        private static readonly string _scriptableDir = AL_PathTools.Combine(_cfgDir, "ScriptableData");
        public static string ScriptableAssetPath => _scriptableDir + "/{0}.asset";
        private static string _scriptableDirFullPath => AL_PathTools.AssetPath2FullPath(_scriptableDir);

        //每个项目可能不一样，需要重新设置
#if !DY_MINI_GAME && !UNITY_WEBGL
        private static readonly string _loadAssetDetailFuncStr =
            "        _instance = ResourceManager.Instance.LoadAsset<{0}>(key);\n";
#else
        private static readonly string _loadAssetDetailFuncStr =
            "        _instance = await ResourceManager.Instance.LoadAsset<{0}>(key);\n";
#endif
        
        public static string GetCsvNameByVoName(string voName)
        {
            if (string.IsNullOrEmpty(voName)) return null;
            if (voName.Length >= 2 && voName.Substring(voName.Length - 2).ToLower() == "vo")
            {
                return voName.Substring(0, voName.Length - 2);
            }
            else
            {
                AL_LogTool.LogOnlyEditor($"{voName} 命名没有以Vo结尾");
                return null;
            }
        }
        
        private static string GetVoFileFullPath(string voName)
        {
            return AL_PathTools.AssetPath2FullPath(AL_PathTools.Combine(_voDir, voName + ".cs"));
        }

#if UNITY_EDITOR
        [MenuItem("Alchemy/通用配置/检查&启用框架")]
        private static void UseALCfgFramework()
        {
            if (!Directory.Exists(_csvDirFullPath)) Directory.CreateDirectory(_csvDirFullPath);
            if (!Directory.Exists(_voDirFullPath)) Directory.CreateDirectory(_voDirFullPath);
            if (!Directory.Exists(_scriptableDirFullPath)) Directory.CreateDirectory(_scriptableDirFullPath);
            AL_LogTool.LogOnlyEditor("AL通用配置框架已经准备好");
        }

        [MenuItem("Alchemy/通用配置/重新生成VosData类")]
        private static void ReCreateAllScriptableData()
        {
            UseALCfgFramework();
            var list = AL_CfgVoAttribute.GetAllUserInAssembly();
            foreach (var t in list)
            {
                if (t == typeof(AL_BaseCfgVo)) continue;
                TryCreateScriptableScript(t, out var dataType, out var dataName, true);
            }
        }
        
        private static List<ScriptableObject> _soList = new List<ScriptableObject>();
        [MenuItem("Assets/Alchemy/通用配置/由CSV生成AssetData", priority = 1)]
        private static void CreateCfgData()
        {
            foreach (var obj in _soList)
            {
                Object.DestroyImmediate(obj);
            }
            _soList.Clear();
            foreach (var item in Selection.objects)
            {
                AL_BaseCfgVos vos = ParseCSV(AL_PathTools.AssetPath2FullPath(AssetDatabase.GetAssetPath(item)));
                if (vos != null) _soList.Add(vos);
            }
            Selection.objects = _soList.ToArray();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            AL_LogTool.LogOnlyEditor("由CSV生成AssetData 执行完毕");
        }
        
        [MenuItem("Assets/Alchemy/通用配置/由CSV生成AssetData", true)]
        private static bool ValidateCfgData()
        {
            Func<Object, bool> predicate = (obj) =>
            {
                var path = AssetDatabase.GetAssetPath(obj);
                var a = Path.GetExtension(path).ToLower();
                return a != ".csv";
            };
            return Selection.objects.Length > 0 && !Selection.objects.Any(predicate);
        }
        
        
        [MenuItem("Assets/Alchemy/通用配置/由Vo类生成默认文件", true)]
        private static bool ValidateDepend()
        {
            Func<Object, bool> predicate = (obj) =>
            {
                var name = obj.name;
                var type = AL_CfgVoAttribute.GetUserInAssembly(name);
                return type == null;
            };
            return Selection.objects.Length > 0 && !Selection.objects.Any(predicate);
        }
        
        private static List<Object> _objectList = new List<Object>();
        [MenuItem("Assets/Alchemy/通用配置/由Vo类生成默认文件", priority = 1)]
        private static void CreateDepend()
        {
            foreach (var obj in _objectList)
            {
                Object.DestroyImmediate(obj);
            }
            _objectList.Clear();
            foreach (var item in Selection.objects)
            {
                var type = AL_CfgVoAttribute.GetUserInAssembly(item.name);
                if (type == null) continue;
                var path = TryCreateCsv(type);
                TryCreateScriptableScript(type, out var assetDataType, out var assetDataName, true);
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    _objectList.Add(AssetDatabase.LoadAssetAtPath<Object>(path));
                }
            }
            if (_objectList.Count > 0)
            {
                Selection.objects = _objectList.ToArray();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                AL_LogTool.LogOnlyEditor("由Vo类生成默认文件 完成");
            }
            else
            {
                AL_LogTool.LogOnlyEditor("所选不满足");
            }
        }

        private static void CreateAssetDataClass(string assetDataFilePath, string assetDataName, Type voType)
        {
            string fileContent =$"using System;\nusing Alchemy;\n" + 
                                $"public class {assetDataName}:{typeof(AL_BaseCfgVos)}\n" +
                                "{\n" +
                                $"    public System.Collections.Generic.List<{voType}> Datas = new System.Collections.Generic.List<{voType}>();\n" +
                                $"    public override System.Collections.IList Vos => Datas;\n" +
                                $"    private static {assetDataName} _instance;\n" +
                                $"    public static {assetDataName} Instance => _instance;\n" +
                                $"    public static async void LoadAssets(Func<{assetDataName}, bool> func)\n" +
                                "    {\n" +
                                "        if (_instance != null)\n" +
                                "        {\n" +
                                "            func?.Invoke(_instance);return;\n" +
                                "        }\n" +
                                $"        string key = AL_CfgLoader.GetAssetPath(\"{assetDataName}\");\n" +
                                string.Format(_loadAssetDetailFuncStr, assetDataName) + 
                                "         func?.Invoke(_instance);\n" +
                                "    }\n" +
                                $"    public static System.Collections.Generic.List<{voType}> DataList\n" +
                                "    {\n" +
                                "        get\n" +
                                "        {\n" +
                                "            if (_instance != null) return _instance.Datas;\n" +
                                "            return null;\n" +
                                "        }\n" +
                                "    }\n" +
                                "}";
            AL_IOTool.WriteInFile(assetDataFilePath, fileContent);
            var assetPath = AL_PathTools.FullPath2AssetPath(assetDataFilePath);
            if(!string.IsNullOrEmpty(assetPath)) AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private static string TryCreateCsv(Type voType)
        {
            if (voType == null) return null;
            var csvName = GetCsvNameByVoName(voType.Name);
            if (!Directory.Exists(_csvDirFullPath)) Directory.CreateDirectory(_csvDirFullPath);
            var fullPath = AL_PathTools.AssetPath2FullPath(Path.Combine(_csvDir, $"{csvName}.csv"));
            if (File.Exists(fullPath))
            {
                AL_LogTool.LogOnlyEditor($"{fullPath} 路径下已经有对应文件");
                return null;
            }
            var fieldInfos = voType.GetFields();
            List<string> arrNames = new List<string>();
            foreach (var info in fieldInfos)
            {
                arrNames.Add(info.Name);
            }
            var msg = "#可以像这样一样，单行以 # 开头的，都视为注释、说明，不会影响配置数据\n" + string.Join(",", arrNames);
            AL_IOTool.AppendWriteInFile(fullPath, msg);
            AL_LogTool.LogOnlyEditor($"{fullPath} 创建完毕");
            return fullPath;
        }

        private static bool TryCreateScriptableScript(Type voType, out Type assetDataType, out string assetDataName, bool force = false)
        {
            //ScriptableObject 数据类
            if (!Directory.Exists(_scriptableDir)) Directory.CreateDirectory(_scriptableDir);
            assetDataName = voType.Name + "sData";
            assetDataType = AL_CfgDataAttribute.GetUserInAssembly(assetDataName);
            if (assetDataType == null)
            {
                CreateAssetDataClass(GetVoFileFullPath(assetDataName), assetDataName, voType);
                AL_LogTool.LogOnlyEditor($"没发现scriptable数据类：{assetDataName}，自动创建完成,如需生成.asset数据需要再执行一遍【生成对应配置数据】");
                return false;
            } else if (force)
            {
                CreateAssetDataClass(GetVoFileFullPath(assetDataName), assetDataName, voType);
                AL_LogTool.LogOnlyEditor($"重新创建数据类：{assetDataName}，创建完成,如需生成.asset数据需要再执行一遍【生成对应配置数据】");
            }
            return true;
        }
        
        private static AL_BaseCfgVos ParseCSV(string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
            {
                AL_LogTool.LogOnlyEditor($"没发现文件：{csvFilePath}");
                return null;
            }
            var voName = AL_PathTools.Path2FileName(csvFilePath, out var exten) + "Vo";
            var voType = AL_CfgVoAttribute.GetUserInAssembly(voName);
            if (voType == null)
            {
                AL_LogTool.LogOnlyEditor($"没发现此类：{voName}");
                return null;
            }
            
            //ScriptableObject 数据类
            if (!TryCreateScriptableScript(voType, out var dataType, out var dataName))
            {
                return null;
            }
            
            //.asset文件
            var scriptableDataPath = Path.Combine(_scriptableDir, $"{dataName}.asset");
            AL_BaseCfgVos uObjData;
            if (!AL_PathTools.ExistsAssetFile(scriptableDataPath))
            {
                uObjData = ScriptableObject.CreateInstance(dataType) as AL_BaseCfgVos;
                AssetDatabase.CreateAsset(uObjData, scriptableDataPath);
            }
            else
            {
                uObjData = AssetDatabase.LoadAssetAtPath(scriptableDataPath, dataType) as AL_BaseCfgVos;
            }
            if (uObjData == null)
            {
                AL_LogTool.LogOnlyEditor($"没发现此类型的ScriptableData：{scriptableDataPath},开始重新生成");
                uObjData = ScriptableObject.CreateInstance(dataType) as AL_BaseCfgVos;
                AssetDatabase.CreateAsset(uObjData, scriptableDataPath);
            }
            
            uObjData.Vos.Clear();
            var dataStrs = ParseCsvToStrLines(csvFilePath);
            var attrNames = dataStrs[0];
            for (int i = 1; i < dataStrs.Count; i++)
            {
                var vo = Activator.CreateInstance(voType) as AL_BaseCfgVo;
                vo?.ParseAttr(attrNames, dataStrs[i]);
                uObjData.Vos.Add(vo);
            }
            EditorUtility.SetDirty(uObjData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return uObjData;
        }
        
        private static List<List<string>> ParseCsvToStrLines(string filePath)
        {
            List<List<string>> records = new List<List<string>>();
            try
            {
                AL_IOTool.ReadFileByLines(filePath, str =>
                {
                    string line = str?.Trim();
                    // 忽略注释行和空行
                    if (!string.IsNullOrEmpty(line) && !line.StartsWith("#"))
                    {
                        List<string> values = ParseCSVLine(line);
                        records.Add(values);
                    }
                    return true;
                });
            }
            catch (Exception e)
            {
                AL_LogTool.LogOnlyEditor(e.Message);
                AL_LogTool.LogOnlyEditor("注意检查是否有别的应用占用了对应的CSV文件");
                throw;
            }
            return records;
        }
        
        private static List<string> ParseCSVLine(string line)
        {
            List<string> values = new List<string>();
            bool insideQuote = false;
            string currentValue = "";
            foreach (char c in line)
            {
                if (c == '\"')
                {
                    insideQuote = !insideQuote;
                }
                else if (c == ',' && !insideQuote)
                {
                    values.Add(currentValue.Trim());
                    currentValue = "";
                }
                else
                {
                    currentValue += c;
                }
            }
            values.Add(currentValue.Trim());
            return values;
        }
#endif
    }
}