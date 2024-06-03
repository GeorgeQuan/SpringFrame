using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;


public class ECSModule : BaseGameModule
{
    // ECSWorld ���󣬱�ʾʵ�����ϵͳ������
    public ECSWorld World { get; private set; }

    // ��ž���ϵͳ��AwakeSystem�����ֵ�
    private Dictionary<Type, IAwakeSystem> awakeSystemMap;
    // �������ϵͳ��DestroySystem�����ֵ�
    private Dictionary<Type, IDestroySystem> destroySystemMap;

    // ��Ÿ���ϵͳ��UpdateSystem�����ֵ�
    private Dictionary<Type, IUpdateSystem> updateSystemMap;
    // ��������ϵͳ������ʵ����ֵ�
    private Dictionary<IUpdateSystem, List<ECSEntity>> updateSystemRelatedEntityMap;


    // ��ź��ڸ���ϵͳ��LateUpdateSystem�����ֵ�
    private Dictionary<Type, ILateUpdateSystem> lateUpdateSystemMap;
    // �������ڸ���ϵͳ������ʵ����ֵ�
    private Dictionary<ILateUpdateSystem, List<ECSEntity>> lateUpdateSystemRelatedEntityMap;


    // ��Ź̶�����ϵͳ��FixedUpdateSystem�����ֵ�
    private Dictionary<Type, IFixedUpdateSystem> fixedUpdateSystemMap;
    // �����̶�����ϵͳ������ʵ����ֵ�
    private Dictionary<IFixedUpdateSystem, List<ECSEntity>> fixedUpdateSystemRelatedEntityMap;


    // ���ʵ����ֵ䣬��Ϊʵ���Ψһ��ʶ��ֵΪʵ�����
    private Dictionary<long, ECSEntity> entities = new Dictionary<long, ECSEntity>();
    // ���ʵ����Ϣ���������ֵ�
    private Dictionary<Type, List<IEntityMessageHandler>> entityMessageHandlerMap;
    // ���ʵ��Զ�̹��̵��ã�RPC�����������ֵ䣬��ΪRPC���ͣ�ֵΪRPC�������
    private Dictionary<Type, IEntityRpcHandler> entityRpcHandlerMap;

    // OnModuleInit ��������ģ���ʼ��ʱ����
    protected internal override void OnModuleInit()
    {
        // ���û���ĳ�ʼ������
        base.OnModuleInit();
        // ��������ϵͳ�������Ǵ������ļ��������ط����أ�
        LoadAllSystems();
        // ��ʼ��ECSWorld����
        World = new ECSWorld();
    }

    // OnModuleUpdate ��������ģ�����ʱ���ã�����ʱ��deltaTime��
    protected internal override void OnModuleUpdate(float deltaTime)
    {
        // ���û���ĸ��·���
        base.OnModuleUpdate(deltaTime);
        // ��������ϵͳ
        DriveUpdateSystem();
    }

    // OnModuleLateUpdate ��������ģ����ڸ���ʱ���ã�����ʱ��deltaTime��
    protected internal override void OnModuleLateUpdate(float deltaTime)
    {
        // ���û���ĺ��ڸ��·���
        base.OnModuleLateUpdate(deltaTime);
        // �������ڸ���ϵͳ
        DriveLateUpdateSystem();
    }

    protected internal override void OnModuleFixedUpdate(float deltaTime)
    {
        base.OnModuleFixedUpdate(deltaTime);
        DriveFixedUpdateSystem();
    }

    /// <summary>
    /// ģ���ʼ��ʱ����
    /// </summary>
    public void LoadAllSystems()
    {
        awakeSystemMap = new Dictionary<Type, IAwakeSystem>();
        destroySystemMap = new Dictionary<Type, IDestroySystem>();

        updateSystemMap = new Dictionary<Type, IUpdateSystem>();
        updateSystemRelatedEntityMap = new Dictionary<IUpdateSystem, List<ECSEntity>>();

        lateUpdateSystemMap = new Dictionary<Type, ILateUpdateSystem>();
        lateUpdateSystemRelatedEntityMap = new Dictionary<ILateUpdateSystem, List<ECSEntity>>();

        fixedUpdateSystemMap = new Dictionary<Type, IFixedUpdateSystem>();
        fixedUpdateSystemRelatedEntityMap = new Dictionary<IFixedUpdateSystem, List<ECSEntity>>();

        entityMessageHandlerMap = new Dictionary<Type, List<IEntityMessageHandler>>();
        entityRpcHandlerMap = new Dictionary<Type, IEntityRpcHandler>();

        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            // ��������ǳ���ģ�������������
            if (type.IsAbstract)
                continue;

            // ��������Ƿ����ECSSystemAttribute�Զ�������
            if (type.GetCustomAttribute<ECSSystemAttribute>(true) != null)//inherit����Ϊtrueʱ����ʾ����Ҫ����������ֱ�Ӷ�������ԣ���Ҫ������������϶��������
            {
                // ��ʼ��AwakeSystem��ش���
                Type awakeSystemType = typeof(IAwakeSystem);//(typeof)��ȡһ��IAwakeSystem�ӿ����͵�Type����
                // ��鵱ǰ�����Ƿ�ʵ����IAwakeSystem�ӿ�
                if (awakeSystemType.IsAssignableFrom(type))
                {
                    // ���awakeSystemMap�ֵ��Ƿ��Ѿ�������ǰ����
                    if (awakeSystemMap.ContainsKey(type))
                    {
                        // ����Ѵ��ڣ����¼������־��������ǰ����
                        UnityLog.Error($"Duplicated Awake System:{type.FullName}");
                        continue;
                    }

                    // ����IAwakeSystem�ӿڵ�ʵ��
                    IAwakeSystem awakeSystem = Activator.CreateInstance(type) as IAwakeSystem;
                    // �����ͼ����Ӧ��ʵ����ӵ�awakeSystemMap�ֵ���
                    awakeSystemMap.Add(type, awakeSystem);
                }

                // ��ʼ��DestroySystem��ش���
                Type destroySystemType = typeof(IDestroySystem);
                // ��鵱ǰ�����Ƿ�ʵ����IDestroySystem�ӿ�
                if (destroySystemType.IsAssignableFrom(type))
                {
                    // ���destroySystemMap�ֵ��Ƿ��Ѿ�������ǰ����
                    if (destroySystemMap.ContainsKey(type))
                    {
                        // ����Ѵ��ڣ����¼������־��������ǰ����
                        UnityLog.Error($"Duplicated Destroy System:{type.FullName}");
                        continue;
                    }

                    // ����IDestroySystem�ӿڵ�ʵ��
                    IDestroySystem destroySystem = Activator.CreateInstance(type) as IDestroySystem;
                    // �����ͼ����Ӧ��ʵ����ӵ�destroySystemMap�ֵ���
                    destroySystemMap.Add(type, destroySystem);
                }

                // ��ʼ��UpdateSystem��ش���
                Type updateSystemType = typeof(IUpdateSystem);
                // ��鵱ǰ�����Ƿ�ʵ����IUpdateSystem�ӿ�
                if (updateSystemType.IsAssignableFrom(type))
                {
                    // ���updateSystemMap�ֵ��Ƿ��Ѿ�������ǰ����
                    if (updateSystemMap.ContainsKey(type))
                    {
                        // ����Ѵ��ڣ����¼������־��������ǰ����
                        UnityLog.Error($"Duplicated Update System:{type.FullName}");
                        continue;
                    }

                    // ����IUpdateSystem�ӿڵ�ʵ��
                    IUpdateSystem updateSystem = Activator.CreateInstance(type) as IUpdateSystem;
                    // �����ͼ����Ӧ��ʵ����ӵ�updateSystemMap�ֵ���
                    updateSystemMap.Add(type, updateSystem);

                    // ��ʼ�������ϵͳ��ص�ʵ���б�
                    updateSystemRelatedEntityMap.Add(updateSystem, new List<ECSEntity>());
                }

                // LateUpdateSystem
                Type lateUpdateSystemType = typeof(ILateUpdateSystem);
                if (lateUpdateSystemType.IsAssignableFrom(type))
                {
                    if (lateUpdateSystemMap.ContainsKey(type))
                    {
                        UnityLog.Error($"Duplicated Late update System:{type.FullName}");
                        continue;
                    }

                    ILateUpdateSystem lateUpdateSystem = Activator.CreateInstance(type) as ILateUpdateSystem;
                    lateUpdateSystemMap.Add(type, lateUpdateSystem);

                    lateUpdateSystemRelatedEntityMap.Add(lateUpdateSystem, new List<ECSEntity>());
                }

                // FixedUpdateSystem
                Type fixedUpdateSystemType = typeof(IFixedUpdateSystem);
                if (fixedUpdateSystemType.IsAssignableFrom(type))
                {
                    if (fixedUpdateSystemMap.ContainsKey(type))
                    {
                        UnityLog.Error($"Duplicated Late update System:{type.FullName}");
                        continue;
                    }

                    IFixedUpdateSystem fixedUpdateSystem = Activator.CreateInstance(type) as IFixedUpdateSystem;
                    fixedUpdateSystemMap.Add(type, fixedUpdateSystem);

                    fixedUpdateSystemRelatedEntityMap.Add(fixedUpdateSystem, new List<ECSEntity>());
                }
            }

            // ����ĳ�����͵ļ��ϣ�������� type �ǵ�ǰ���ڴ���� Type ����
            if (type.GetCustomAttribute<EntityMessageHandlerAttribute>(true) != null)
            {
                // ��������;��� EntityMessageHandlerAttribute �Զ�������
                Type entityMessageType = typeof(IEntityMessageHandler);
                // ��� type �Ƿ�ʵ���� IEntityMessageHandler �ӿ�
                if (entityMessageType.IsAssignableFrom(type))
                {
                    // ʵ���� type ��Ӧ�Ķ��󣬲����Խ���ת��Ϊ IEntityMessageHandler �ӿ�����
                    IEntityMessageHandler entityMessageHandler = Activator.CreateInstance(type) as IEntityMessageHandler;

                    // ���Դ��ֵ� entityMessageHandlerMap �л�ȡ�뵱ǰ��Ϣ���������Ͷ�Ӧ����Ϣ�����б�
                    // ��������ڣ��򴴽�һ���µ��б���ӵ��ֵ���
                    if (!entityMessageHandlerMap.TryGetValue(entityMessageHandler.MessageType(), out List<IEntityMessageHandler> list))
                    {
                        list = new List<IEntityMessageHandler>();
                        entityMessageHandlerMap.Add(entityMessageHandler.MessageType(), list);
                    }

                    // ����ǰ����Ϣ��������ӵ���Ӧ����Ϣ�����б���
                    list.Add(entityMessageHandler);
                }
            }

            // ���������Ƿ���� EntityRpcHandlerAttribute �Զ�������
            if (type.GetCustomAttribute<EntityRpcHandlerAttribute>(true) != null)
            {
                Type entityRpcType = typeof(IEntityRpcHandler);
                // ��� type �Ƿ�ʵ���� IEntityRpcHandler �ӿ�
                if (entityRpcType.IsAssignableFrom(type))
                {
                    // ʵ���� type ��Ӧ�Ķ��󣬲����Խ���ת��Ϊ IEntityRpcHandler �ӿ�����
                    IEntityRpcHandler entityRpcHandler = Activator.CreateInstance(type) as IEntityRpcHandler;

                    // ���Դ��ֵ� entityRpcHandlerMap �м�鵱ǰ RPC �������� RPC �����Ƿ��Ѵ���
                    if (entityRpcHandlerMap.ContainsKey(entityRpcHandler.RpcType()))
                    {
                        // ����Ѵ��ڣ����¼������־����������ǰѭ����ʣ�ಿ��
                        UnityLog.Error($"Duplicate Entity Rpc, type:{entityRpcHandler.RpcType().FullName}");
                        continue;
                    }

                    // ����ǰ�� RPC ��������ӵ��ֵ��У���Ϊ�� RPC ����
                    entityRpcHandlerMap.Add(entityRpcHandler.RpcType(), entityRpcHandler);
                }
            }
        }
    }

    // ��������ϵͳ�ķ��������ڱ������������и���ϵͳ
    private void DriveUpdateSystem()
    {
        // ��������ϵͳӳ���е����и���ϵͳ
        foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
        {
            // ��ȡ�뵱ǰ����ϵͳ��ص�ʵ���б�
            List<ECSEntity> updateSystemRelatedEntities = updateSystemRelatedEntityMap[updateSystem];
            // ��������ϵͳ��ص�ʵ���б�Ϊ�գ���������ǰѭ����������һ������ϵͳ
            if (updateSystemRelatedEntities.Count == 0)
                continue;

            // �Ӷ�����л�ȡһ��ECSEntity���͵��б�
            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
            // �������ϵͳ��ص�ʵ���б���ӵ��»�ȡ���б��У������޸�ԭʼ�б�
            entityList.AddRangeNonAlloc(updateSystemRelatedEntities);

            // �����»�ȡ��ʵ���б��е�ÿһ��ʵ��
            foreach (var entity in entityList)
            {
                // �����ǰ����ϵͳ����ע��ʵ�壬��������ǰѭ����������һ��ʵ��
                if (!updateSystem.ObservingEntity(entity))
                    continue;

                // ���ø���ϵͳ��Update���������µ�ǰʵ��
                updateSystem.Update(entity);
            }

            // ��ʹ����ϵ��б��ͷŻض����
            ListPool<ECSEntity>.Release(entityList);
        }
    }

    private void DriveLateUpdateSystem()
    {
        foreach (ILateUpdateSystem lateUpdateSystem in lateUpdateSystemMap.Values)
        {
            List<ECSEntity> lateUpdateSystemRelatedEntities = lateUpdateSystemRelatedEntityMap[lateUpdateSystem];
            if (lateUpdateSystemRelatedEntities.Count == 0)
                continue;

            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
            entityList.AddRangeNonAlloc(lateUpdateSystemRelatedEntities);
            foreach (var entity in entityList)
            {
                if (!lateUpdateSystem.ObservingEntity(entity))
                    continue;

                lateUpdateSystem.LateUpdate(entity);
            }

            ListPool<ECSEntity>.Release(entityList);
        }
    }

    private void DriveFixedUpdateSystem()
    {
        foreach (IFixedUpdateSystem fixedUpdateSystem in fixedUpdateSystemMap.Values)
        {
            List<ECSEntity> fixedUpdateSystemRelatedEntities = fixedUpdateSystemRelatedEntityMap[fixedUpdateSystem];
            if (fixedUpdateSystemRelatedEntities.Count == 0)
                continue;

            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
            entityList.AddRangeNonAlloc(fixedUpdateSystemRelatedEntities);
            foreach (var entity in entityList)
            {
                if (!fixedUpdateSystem.ObservingEntity(entity))
                    continue;

                fixedUpdateSystem.FixedUpdate(entity);
            }

            ListPool<ECSEntity>.Release(entityList);
        }
    }

    // ����һ�����ͷ��������ڴ�awakeSystemMap�л�ȡ����ΪC��IAwakeSystem����������ӵ�list��
    private void GetAwakeSystems<C>(List<IAwakeSystem> list) where C : ECSComponent
    {
        foreach (var awakeSystem in awakeSystemMap.Values)
        {
            // ��鵱ǰawakeSystem�������Ƿ��뷺�Ͳ���Cƥ��
            if (awakeSystem.ComponentType() == typeof(C))
            {
                // ���ƥ�䣬�򽫸�awakeSystem��ӵ�list��
                list.Add(awakeSystem);
            }
        }
    }

    // ����һ�����ͷ��������ڻ���ָ�����͵����C
    public void AwakeComponent<C>(C component) where C : ECSComponent
    {
        // ���������������ʵ���ϵͳ�б�
        UpdateSystemEntityList(component.Entity);

        // �Ӷ�����л�ȡһ��IAwakeSystem�б�
        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();

        // ����GetAwakeSystems������ȡ����ΪC��IAwakeSystem������ӵ�list��
        GetAwakeSystems<C>(list);

        bool found = false;
        foreach (var item in list)
        {
            // ���Խ�itemת��ΪAwakeSystem<C>����
            AwakeSystem<C> awakeSystem = item as AwakeSystem<C>;
            if (awakeSystem == null)
                continue;

            // ����awakeSystem��Awake����������ָ�������
            awakeSystem.Awake(component);
            found = true;
        }

        // �ͷ�IAwakeSystem�б������
        ListPool<IAwakeSystem>.Release(list);

        // ���û���ҵ�ƥ���AwakeSystem�����¼������־
        if (!found)
        {
            UnityLog.Warn($"Not found awake system:<{typeof(C).Name}>");
        }
    }

    // ����һ�������������Ͳ����ķ��������ڻ���ָ�����͵����C��������һ������Ĳ���P1
    public void AwakeComponent<C, P1>(C component, P1 p1) where C : ECSComponent
    {
        // ���������������ʵ���ϵͳ�б�
        UpdateSystemEntityList(component.Entity);

        // �Ӷ�����л�ȡһ��IAwakeSystem�б�
        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();

        // ����ECSModuleģ���GetAwakeSystems������ȡ����ΪC��IAwakeSystem������ӵ�list��
        TGameFramework.Instance.GetModule<ECSModule>().GetAwakeSystems<C>(list);

        bool found = false;
        foreach (var item in list)
        {
            // ���Խ�itemת��ΪAwakeSystem<C, P1>����
            AwakeSystem<C, P1> awakeSystem = item as AwakeSystem<C, P1>;
            if (awakeSystem == null)
                continue;

            // ����awakeSystem��Awake��������������Ͳ���p1������ָ�������
            awakeSystem.Awake(component, p1);
            found = true;
        }

        // �ͷ�IAwakeSystem�б������
        ListPool<IAwakeSystem>.Release(list);

        // ���û���ҵ�ƥ���AwakeSystem�����¼������־
        if (!found)
        {
            UnityLog.Warn($"Not found awake system:<{typeof(C).Name}, {typeof(P1).Name}>");
        }
    }

    public void AwakeComponent<C, P1, P2>(C component, P1 p1, P2 p2) where C : ECSComponent
    {
        UpdateSystemEntityList(component.Entity);

        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
        TGameFramework.Instance.GetModule<ECSModule>().GetAwakeSystems<C>(list);

        bool found = false;
        foreach (var item in list)
        {
            AwakeSystem<C, P1, P2> awakeSystem = item as AwakeSystem<C, P1, P2>;
            if (awakeSystem == null)
                continue;

            awakeSystem.Awake(component, p1, p2);
            found = true;
        }

        ListPool<IAwakeSystem>.Release(list);
        if (!found)
        {
            UnityLog.Warn($"Not found awake system:<{typeof(C).Name}, {typeof(P1).Name}, {typeof(P2).Name}>");
        }
    }

    // ����һ�����ͷ��������ڴ�destroySystemMap�л�ȡ����ΪC��IDestroySystem����������ӵ�list��
    private void GetDestroySystems<C>(List<IDestroySystem> list) where C : ECSComponent
    {
        foreach (var destroySystem in destroySystemMap.Values)
        {
            // ��鵱ǰdestroySystem�������Ƿ��뷺�Ͳ���Cƥ��
            if (destroySystem.ComponentType() == typeof(C))
            {
                // ���ƥ�䣬�򽫸�destroySystem��ӵ�list��
                list.Add(destroySystem);
            }
        }
    }

    // ����һ���Ƿ��ͷ��������ڴ�destroySystemMap�л�ȡָ�����͵�IDestroySystem����������ӵ�list��
    private void GetDestroySystems(Type componentType, List<IDestroySystem> list)
    {
        foreach (var destroySystem in destroySystemMap.Values)
        {
            // ��鵱ǰdestroySystem�������Ƿ��봫���componentTypeƥ��
            if (destroySystem.ComponentType() == componentType)
            {
                // ���ƥ�䣬�򽫸�destroySystem��ӵ�list��
                list.Add(destroySystem);
            }
        }
    }

    // ����һ�����ͷ�������������ָ�����͵����C
    public void DestroyComponent<C>(C component) where C : ECSComponent
    {
        // ���������������ʵ���ϵͳ�б�
        UpdateSystemEntityList(component.Entity);

        // �Ӷ�����л�ȡһ��IDestroySystem�б�
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();

        // ����GetDestroySystems������ȡ����ΪC��IDestroySystem������ӵ�list��
        GetDestroySystems<C>(list);

        foreach (var item in list)
        {
            // ���Խ�itemת��ΪDestroySystem<C>����
            DestroySystem<C> destroySystem = item as DestroySystem<C>;
            if (destroySystem == null)
                continue;

            // ����destroySystem��Destroy����������ָ�������
            destroySystem.Destroy(component);

            // ������Ϊ������״̬
            component.Disposed = true;
        }

        // �ͷ�IDestroySystem�б������
        ListPool<IDestroySystem>.Release(list);
    }

    // ����һ���Ƿ��ͷ��������������������͵�ECSComponent���
    public void DestroyComponent(ECSComponent component)
    {
        // ���������������ʵ���ϵͳ�б�
        UpdateSystemEntityList(component.Entity);

        // �Ӷ�����л�ȡһ��IDestroySystem�б�
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();

        // ����GetDestroySystems������ȡָ�����͵�IDestroySystem������ӵ�list��
        GetDestroySystems(component.GetType(), list);

        foreach (var item in list)
        {
            // ����item��Destroy����������ָ�������
            // ���ﲻ��Ҫ����ת������Ϊitem�Ѿ���IDestroySystem���ͣ�����ֱ�ӵ���Destroy����
            item.Destroy(component);

            // ������Ϊ������״̬
            component.Disposed = true;
        }

        // �ͷ�IDestroySystem�б������
        ListPool<IDestroySystem>.Release(list);
    }

    public void DestroyComponent<C, P1>(C component, P1 p1) where C : ECSComponent
    {
        UpdateSystemEntityList(component.Entity);

        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems<C>(list);
        foreach (var item in list)
        {
            DestroySystem<C, P1> destroySystem = item as DestroySystem<C, P1>;
            if (destroySystem == null)
                continue;

            destroySystem.Destroy(component, p1);
            component.Disposed = true;
        }

        ListPool<IDestroySystem>.Release(list);
    }

    public void DestroyComponent<C, P1, P2>(C component, P1 p1, P2 p2) where C : ECSComponent
    {
        UpdateSystemEntityList(component.Entity);

        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems<C>(list);
        foreach (var item in list)
        {
            DestroySystem<C, P1, P2> destroySystem = item as DestroySystem<C, P1, P2>;
            if (destroySystem == null)
                continue;

            destroySystem.Destroy(component, p1, p2);
            component.Disposed = true;
        }

        ListPool<IDestroySystem>.Release(list);
    }

    // ���������ʵ��������ĸ���ϵͳ�б�
    private void UpdateSystemEntityList(ECSEntity entity)
    {
        // �������еļ�ʱ����ϵͳ
        foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
        {
            // ��ȡ��ǰ����ϵͳ��������ʵ���б�
            List<ECSEntity> entityList = updateSystemRelatedEntityMap[updateSystem];

            // ���ʵ���б��в�������ǰʵ��
            if (!entityList.Contains(entity))
            {
                // ��鵱ǰ����ϵͳ�Ƿ����ڹ۲����ʵ��
                if (updateSystem.ObservingEntity(entity))
                {
                    // ������ڹ۲죬��ʵ����ӵ�ʵ���б���
                    entityList.Add(entity);
                }
            }
            else
            {
                // ���ʵ���б����Ѿ�������ǰʵ��
                // ��鵱ǰ����ϵͳ�Ƿ��ٹ۲����ʵ��
                if (!updateSystem.ObservingEntity(entity))
                {
                    // ������ٹ۲죬���ʵ���б����Ƴ���ʵ��
                    entityList.Remove(entity);
                }
            }
        }

        // �������е��ӳٸ���ϵͳ
        foreach (ILateUpdateSystem lateUpdateSystem in lateUpdateSystemMap.Values)
        {
            // ��ȡ��ǰ�ӳٸ���ϵͳ��������ʵ���б�
            List<ECSEntity> entityList = lateUpdateSystemRelatedEntityMap[lateUpdateSystem];

            // �뼴ʱ����ϵͳ���߼����ƣ���鲢�����ӳٸ���ϵͳ��ʵ���б�
            if (!entityList.Contains(entity))
            {
                if (lateUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Add(entity);
                }
            }
            else
            {
                if (!lateUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Remove(entity);
                }
            }
        }

        // �������еĹ̶�����ϵͳ
        foreach (IFixedUpdateSystem fixedUpdateSystem in fixedUpdateSystemMap.Values)
        {
            // ��ȡ��ǰ�̶�����ϵͳ��������ʵ���б�
            List<ECSEntity> entityList = fixedUpdateSystemRelatedEntityMap[fixedUpdateSystem];

            // �뼴ʱ����ϵͳ���߼����ƣ���鲢���¹̶�����ϵͳ��ʵ���б�
            if (!entityList.Contains(entity))
            {
                if (fixedUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Add(entity);
                }
            }
            else
            {
                if (!fixedUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Remove(entity);
                }
            }
        }
    }

    // ���ʵ�嵽ʵ���б��У�ʹ��ʵ���InstanceID��Ϊ��
    public void AddEntity(ECSEntity entity)
    {
        entities.Add(entity.InstanceID, entity);
    }

    // ��ʵ���б����Ƴ�ָ����ʵ��
    public void RemoveEntity(ECSEntity entity)
    {
        if (entity == null)
            return; // ���ʵ��Ϊ�գ���ֱ�ӷ��أ������κβ���

        entities.Remove(entity.InstanceID); // ��ʵ���б����Ƴ���ʵ�����Ŀ
        ECSScene scene = entity.Scene; // ��ȡʵ�������ĳ���
        scene?.RemoveEntity(entity.InstanceID); // ���������Ϊ�գ���ӳ������Ƴ���ʵ��
    }

    // ����ID����ʵ�壬����ECSEntity���͵�ʵ��
    public ECSEntity FindEntity(long id)
    {
        return FindEntity<ECSEntity>(id); // ���÷��ͷ�������ָ����������ΪECSEntity
    }

    // ����ID����ʵ�壬����ָ������T��ʵ��
    public T FindEntity<T>(long id) where T : ECSEntity
    {
        entities.TryGetValue(id, out ECSEntity entity); // ���Դ�ʵ���б��л�ȡָ��ID��ʵ��
        return entity as T; // ��ʵ��ת��Ϊָ������T�����أ����ת��ʧ���򷵻�null
    }

    // ����ʵ��ID���Ҳ����ظ�ʵ����ָ������T�����
    public T FindComponentOfEntity<T>(long entityID) where T : ECSComponent
    {
        return FindEntity(entityID)?.GetComponent<T>(); // ����ʵ�壬����ҵ����ȡ���ϵ�T������������أ����򷵻�null
    }

    // ��ָ��ID��ʵ�巢����ϢM���첽ִ��
    public async Task SendMessageToEntity<M>(long id, M m)
    {
        if (id == 0)
            return; // ���IDΪ0����ֱ�ӷ��أ������κβ���

        ECSEntity entity = FindEntity(id); // ����ID����ʵ��
        if (entity == null)
            return; // ���ʵ�岻���ڣ���ֱ�ӷ��أ������κβ���

        Type messageType = m.GetType(); // ��ȡ��ϢM������
        if (!entityMessageHandlerMap.TryGetValue(messageType, out List<IEntityMessageHandler> list))
            return; // �����Ϣ���Ͷ�Ӧ�Ĵ������б����ڣ���ֱ�ӷ��أ������κβ���

        List<IEntityMessageHandler> entityMessageHandlers = ListPool<IEntityMessageHandler>.Obtain(); // �Ӷ�����л�ȡһ���������б�
        entityMessageHandlers.AddRangeNonAlloc(list); // ���ҵ��Ĵ������б���ӵ��Ӷ�����л�ȡ���б��У������ڴ����
        foreach (IEntityMessageHandler<M> handler in entityMessageHandlers) // �����������б�
        {
            await handler.Post(entity, m); // �첽���ô�������Post����������Ϣ���͸�ʵ��
        }

        ListPool<IEntityMessageHandler>.Release(entityMessageHandlers); // �ͷŴӶ�����л�ȡ�Ĵ������б�
    }

    // �첽����RPC����ָ��ID��ʵ�壬��������Ӧ
    public async Task<Response> SendRpcToEntity<Request, Response>(long entityID, Request request)
        where Response : IEntityRpcResponse, new()
    {
        if (entityID == 0)
            return new Response() { Error = true }; // ���ʵ��IDΪ0����ֱ�ӷ��ش��д������Ӧ

        ECSEntity entity = FindEntity(entityID);
        if (entity == null)
            return new Response() { Error = true }; // ����Ҳ���ʵ�壬��ֱ�ӷ��ش��д������Ӧ

        Type messageType = request.GetType();
        if (!entityRpcHandlerMap.TryGetValue(messageType, out IEntityRpcHandler entityRpcHandler))
            return new Response() { Error = true }; // ����Ҳ������������Ͷ�Ӧ��RPC����������ֱ�ӷ��ش��д������Ӧ

        IEntityRpcHandler<Request, Response> handler = entityRpcHandler as IEntityRpcHandler<Request, Response>;
        if (handler == null)
            return new Response() { Error = true }; // ����޷���RPC������ת��Ϊ��ȷ�ķ������ͣ���ֱ�ӷ��ش��д������Ӧ

        return await handler.Post(entity, request); // �첽����RPC��������Post��������������ʵ�壬��������Ӧ
    }
}

