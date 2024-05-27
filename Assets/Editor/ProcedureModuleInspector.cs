using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(ProcedureModule))]//设置要编辑的类型
public class ProcedureModuleInspector : BaseInspector
{
    private SerializedProperty proceduresProperty;//存储程序名字
    private SerializedProperty defaultProcedureProperty;//默认程序名字

    private List<string> allProcedureTypes;//所有程序名称容器
    /// <summary>
    /// 进入
    /// </summary>
    protected override void OnInspectorEnable()
    {
        base.OnInspectorEnable();
        proceduresProperty = serializedObject.FindProperty("proceduresNames");//从要编辑的对象中找到这个属性,返回值是序列化的属性
        defaultProcedureProperty = serializedObject.FindProperty("defaultProcedureName");

        UpdateProcedures();
    }

    protected override void OnCompileComplete()
    {
        base.OnCompileComplete();
        UpdateProcedures();
    }
    /// <summary>
    ///更新方法
    /// </summary>
    private void UpdateProcedures()
    {
        allProcedureTypes = Utility.Types.GetAllSubclasses(typeof(BaseProcedure), false, Utility.Types.GAME_CSHARP_ASSEMBLY).ConvertAll((Type t) => { return t.FullName; });//获取所有子类型,并获取转换成类型的名字

        //移除不存在的procedure
        for (int i = proceduresProperty.arraySize - 1; i >= 0; i--)//倒序遍历属性
        {
            string procedureTypeName = proceduresProperty.GetArrayElementAtIndex(i).stringValue;//获取元素,获取字符串值
            if (!allProcedureTypes.Contains(procedureTypeName))//判断有没有继承BaseProcedure
            {
                proceduresProperty.DeleteArrayElementAtIndex(i);//删除数组中这个位置的元素
            }
        }
        serializedObject.ApplyModifiedProperties();//将对序列化对象所做的修改应用到实际的对象。
    }
    /// <summary>
    /// 重写绘制GUI方法
    /// </summary>
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginDisabledGroup(Application.isPlaying); // 在游戏运行时禁用组,是否正在运行
        {
            if (allProcedureTypes.Count > 0)//判断模块数量是否大于0
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);//创建垂直布局,GUI.skin.box有背景有边框
                {
                    for (int i = 0; i < allProcedureTypes.Count; i++)//遍历所有程序名字
                    {
                        GUI.changed = false;//显示重置
                        int? index = FindProcedureTypeIndex(allProcedureTypes[i]);//拿到属性下标
                        bool selected = EditorGUILayout.ToggleLeft(allProcedureTypes[i], index.HasValue);//生成Toggle,1文字,2是否选中,返回bool 用户动态点击会改变返回值
                        if (GUI.changed)//判断当前是否有GUI控件的值发生变化
                        {
                            if (selected)//如果选中
                            {
                                AddProcedure(allProcedureTypes[i]);//添加程序
                            }
                            else
                            {
                                RemoveProcedure(index.Value);//移除程序
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUI.EndDisabledGroup();

        if (proceduresProperty.arraySize == 0)//如果没有选择任何程序
        {
            if (allProcedureTypes.Count == 0)//并且项目中没有任何程序
            {
                EditorGUILayout.HelpBox("Can't find any procedure", UnityEditor.MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Please select a procedure at least", UnityEditor.MessageType.Info);
            }
        }
        else
        {
            if (Application.isPlaying)
            {
                //播放中显示当前状态
                EditorGUILayout.LabelField("Current Procedure", TGameFramework.Instance.GetModule<ProcedureModule>().CurrentProcedure?.GetType().FullName);
            }
            else
            {
                //显示默认状态
                List<string> selectedProcedures = new List<string>();
                for (int i = 0; i < proceduresProperty.arraySize; i++)
                {
                    selectedProcedures.Add(proceduresProperty.GetArrayElementAtIndex(i).stringValue);
                }
                selectedProcedures.Sort();
                int defaultProcedureIndex = selectedProcedures.IndexOf(defaultProcedureProperty.stringValue);//下拉列表默认显示默认程序
                defaultProcedureIndex = EditorGUILayout.Popup("Default Procedure", defaultProcedureIndex, selectedProcedures.ToArray());//显示下拉列表,int 为选中的程序
                if (defaultProcedureIndex >= 0)//如果不为空,已经选择了
                {
                    defaultProcedureProperty.stringValue = selectedProcedures[defaultProcedureIndex];//默认程序名字改成选择的
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
    /// <summary>
    /// 属性添加程序
    /// </summary>
    /// <param name="procedureType"></param>
    private void AddProcedure(string procedureType)
    {
        proceduresProperty.InsertArrayElementAtIndex(0);//在下标0的位置插入新元素
        proceduresProperty.GetArrayElementAtIndex(0).stringValue = procedureType;//0位置的字符串改成传进来的字符串
    }
    /// <summary>
    /// 移除程序
    /// </summary>
    /// <param name="index"></param>
    private void RemoveProcedure(int index)
    {
        string procedureType = proceduresProperty.GetArrayElementAtIndex(index).stringValue;
        if (procedureType == defaultProcedureProperty.stringValue)//判断要移除的是否是默认程序
        {
            Debug.LogWarning("Can't remove default procedure");
            return;
        }
        proceduresProperty.DeleteArrayElementAtIndex(index);
    }
    /// <summary>
    /// 查找并返回序列化属性中的下标
    /// </summary>
    /// <param name="procedureType"></param>
    /// <returns></returns>
    private int? FindProcedureTypeIndex(string procedureType)
    {
        for (int i = 0; i < proceduresProperty.arraySize; i++)
        {
            SerializedProperty p = proceduresProperty.GetArrayElementAtIndex(i);
            if (p.stringValue == procedureType)
            {
                return i;
            }
        }
        return null;
    }
}
