
using UnityEditor;


public class BaseInspector :Editor
{
    protected virtual bool DrawBaseGUI { get { return true; } }//�����Ƿ���ƻ��� GUI

    private bool isCompiling = false;//��ǰ����״̬
    protected virtual void OnInspectorUpdateInEditor() { }//Update�з��� ������д

    private void OnEnable()
    {
        OnInspectorEnable();//���ý��뷽��
        EditorApplication.update += UpdateEditor;//Editor�µ�Update�¼�,ÿ֡�����,����ʱ���update����
    }
    protected virtual void OnInspectorEnable() { }//����
    /// <summary>
    /// ����ʱע���¼����ý�������
    /// </summary>
    private void OnDisable()
    {
        EditorApplication.update -= UpdateEditor;
        OnInspectorDisable();
    }
    protected virtual void OnInspectorDisable() { }
    /// <summary>
    /// Update�¼�
    /// </summary>

    private void UpdateEditor()
    {
        if (!isCompiling && EditorApplication.isCompiling)//EditorApplication.isCompiling ����unity �Ƿ��ڱ���ű� ,�����ֶ�
        {
            isCompiling = true;
            OnCompileStart();//���øտ�ʼ����ű�����
        }
        else if (isCompiling && !EditorApplication.isCompiling)//ֹͣ����ű�,�����ֶ�
        {
            isCompiling = false;
            OnCompileComplete();//���ñ�����ɷ���
        }
        OnInspectorUpdateInEditor();//ÿ֡����update����
    }

    public override void OnInspectorGUI()
    {
        if (DrawBaseGUI)//�ж��Ƿ�Ҫ����GUI
        {
            base.OnInspectorGUI();
        }
    }

    protected virtual void OnCompileStart() { }
    protected virtual void OnCompileComplete() { }
}