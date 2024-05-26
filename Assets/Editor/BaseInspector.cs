
using UnityEditor;


public class BaseInspector :Editor
{
    protected virtual bool DrawBaseGUI { get { return true; } }//决定是否绘制基础 GUI

    private bool isCompiling = false;//当前编译状态
    protected virtual void OnInspectorUpdateInEditor() { }//Update中方法 可以重写

    private void OnEnable()
    {
        OnInspectorEnable();//调用进入方法
        EditorApplication.update += UpdateEditor;//Editor下的Update事件,每帧会调用,进入时添加update方法
    }
    protected virtual void OnInspectorEnable() { }//进入
    /// <summary>
    /// 禁用时注销事件调用结束方法
    /// </summary>
    private void OnDisable()
    {
        EditorApplication.update -= UpdateEditor;
        OnInspectorDisable();
    }
    protected virtual void OnInspectorDisable() { }
    /// <summary>
    /// Update事件
    /// </summary>

    private void UpdateEditor()
    {
        if (!isCompiling && EditorApplication.isCompiling)//EditorApplication.isCompiling 返回unity 是否在编译脚本 ,更改字段
        {
            isCompiling = true;
            OnCompileStart();//调用刚开始编译脚本方法
        }
        else if (isCompiling && !EditorApplication.isCompiling)//停止编译脚本,更改字段
        {
            isCompiling = false;
            OnCompileComplete();//调用编译完成方法
        }
        OnInspectorUpdateInEditor();//每帧调用update方法
    }

    public override void OnInspectorGUI()
    {
        if (DrawBaseGUI)//判断是否要绘制GUI
        {
            base.OnInspectorGUI();
        }
    }

    protected virtual void OnCompileStart() { }
    protected virtual void OnCompileComplete() { }
}