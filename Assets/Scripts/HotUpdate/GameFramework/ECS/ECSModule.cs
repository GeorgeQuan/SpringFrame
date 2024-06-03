using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;


public class ECSModule : BaseGameModule
{
    // ECSWorld 对象，表示实体组件系统的世界
    public ECSWorld World { get; private set; }

    // 存放觉醒系统（AwakeSystem）的字典
    private Dictionary<Type, IAwakeSystem> awakeSystemMap;
    // 存放销毁系统（DestroySystem）的字典
    private Dictionary<Type, IDestroySystem> destroySystemMap;

    // 存放更新系统（UpdateSystem）的字典
    private Dictionary<Type, IUpdateSystem> updateSystemMap;
    // 存放与更新系统关联的实体的字典
    private Dictionary<IUpdateSystem, List<ECSEntity>> updateSystemRelatedEntityMap;


    // 存放后期更新系统（LateUpdateSystem）的字典
    private Dictionary<Type, ILateUpdateSystem> lateUpdateSystemMap;
    // 存放与后期更新系统关联的实体的字典
    private Dictionary<ILateUpdateSystem, List<ECSEntity>> lateUpdateSystemRelatedEntityMap;


    // 存放固定更新系统（FixedUpdateSystem）的字典
    private Dictionary<Type, IFixedUpdateSystem> fixedUpdateSystemMap;
    // 存放与固定更新系统关联的实体的字典
    private Dictionary<IFixedUpdateSystem, List<ECSEntity>> fixedUpdateSystemRelatedEntityMap;


    // 存放实体的字典，键为实体的唯一标识，值为实体对象
    private Dictionary<long, ECSEntity> entities = new Dictionary<long, ECSEntity>();
    // 存放实体消息处理程序的字典
    private Dictionary<Type, List<IEntityMessageHandler>> entityMessageHandlerMap;
    // 存放实体远程过程调用（RPC）处理程序的字典，键为RPC类型，值为RPC处理程序
    private Dictionary<Type, IEntityRpcHandler> entityRpcHandlerMap;

    // OnModuleInit 方法，在模块初始化时调用
    protected internal override void OnModuleInit()
    {
        // 调用基类的初始化方法
        base.OnModuleInit();
        // 加载所有系统（可能是从配置文件或其他地方加载）
        LoadAllSystems();
        // 初始化ECSWorld对象
        World = new ECSWorld();
    }

    // OnModuleUpdate 方法，在模块更新时调用，传入时间差（deltaTime）
    protected internal override void OnModuleUpdate(float deltaTime)
    {
        // 调用基类的更新方法
        base.OnModuleUpdate(deltaTime);
        // 驱动更新系统
        DriveUpdateSystem();
    }

    // OnModuleLateUpdate 方法，在模块后期更新时调用，传入时间差（deltaTime）
    protected internal override void OnModuleLateUpdate(float deltaTime)
    {
        // 调用基类的后期更新方法
        base.OnModuleLateUpdate(deltaTime);
        // 驱动后期更新系统
        DriveLateUpdateSystem();
    }

    protected internal override void OnModuleFixedUpdate(float deltaTime)
    {
        base.OnModuleFixedUpdate(deltaTime);
        DriveFixedUpdateSystem();
    }

    /// <summary>
    /// 模块初始化时调用
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
            // 如果类型是抽象的，则跳过该类型
            if (type.IsAbstract)
                continue;

            // 检查类型是否带有ECSSystemAttribute自定义属性
            if (type.GetCustomAttribute<ECSSystemAttribute>(true) != null)//inherit参数为true时，表示不仅要搜索该类型直接定义的属性，还要搜索其基类链上定义的属性
            {
                // 初始化AwakeSystem相关代码
                Type awakeSystemType = typeof(IAwakeSystem);//(typeof)获取一个IAwakeSystem接口类型的Type变量
                // 检查当前类型是否实现了IAwakeSystem接口
                if (awakeSystemType.IsAssignableFrom(type))
                {
                    // 检查awakeSystemMap字典是否已经包含当前类型
                    if (awakeSystemMap.ContainsKey(type))
                    {
                        // 如果已存在，则记录错误日志并跳过当前类型
                        UnityLog.Error($"Duplicated Awake System:{type.FullName}");
                        continue;
                    }

                    // 创建IAwakeSystem接口的实例
                    IAwakeSystem awakeSystem = Activator.CreateInstance(type) as IAwakeSystem;
                    // 将类型及其对应的实例添加到awakeSystemMap字典中
                    awakeSystemMap.Add(type, awakeSystem);
                }

                // 初始化DestroySystem相关代码
                Type destroySystemType = typeof(IDestroySystem);
                // 检查当前类型是否实现了IDestroySystem接口
                if (destroySystemType.IsAssignableFrom(type))
                {
                    // 检查destroySystemMap字典是否已经包含当前类型
                    if (destroySystemMap.ContainsKey(type))
                    {
                        // 如果已存在，则记录错误日志并跳过当前类型
                        UnityLog.Error($"Duplicated Destroy System:{type.FullName}");
                        continue;
                    }

                    // 创建IDestroySystem接口的实例
                    IDestroySystem destroySystem = Activator.CreateInstance(type) as IDestroySystem;
                    // 将类型及其对应的实例添加到destroySystemMap字典中
                    destroySystemMap.Add(type, destroySystem);
                }

                // 初始化UpdateSystem相关代码
                Type updateSystemType = typeof(IUpdateSystem);
                // 检查当前类型是否实现了IUpdateSystem接口
                if (updateSystemType.IsAssignableFrom(type))
                {
                    // 检查updateSystemMap字典是否已经包含当前类型
                    if (updateSystemMap.ContainsKey(type))
                    {
                        // 如果已存在，则记录错误日志并跳过当前类型
                        UnityLog.Error($"Duplicated Update System:{type.FullName}");
                        continue;
                    }

                    // 创建IUpdateSystem接口的实例
                    IUpdateSystem updateSystem = Activator.CreateInstance(type) as IUpdateSystem;
                    // 将类型及其对应的实例添加到updateSystemMap字典中
                    updateSystemMap.Add(type, updateSystem);

                    // 初始化与更新系统相关的实体列表
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

            // 遍历某个类型的集合，这里假设 type 是当前正在处理的 Type 对象
            if (type.GetCustomAttribute<EntityMessageHandlerAttribute>(true) != null)
            {
                // 如果该类型具有 EntityMessageHandlerAttribute 自定义属性
                Type entityMessageType = typeof(IEntityMessageHandler);
                // 检查 type 是否实现了 IEntityMessageHandler 接口
                if (entityMessageType.IsAssignableFrom(type))
                {
                    // 实例化 type 对应的对象，并尝试将其转换为 IEntityMessageHandler 接口类型
                    IEntityMessageHandler entityMessageHandler = Activator.CreateInstance(type) as IEntityMessageHandler;

                    // 尝试从字典 entityMessageHandlerMap 中获取与当前消息处理器类型对应的消息类型列表
                    // 如果不存在，则创建一个新的列表并添加到字典中
                    if (!entityMessageHandlerMap.TryGetValue(entityMessageHandler.MessageType(), out List<IEntityMessageHandler> list))
                    {
                        list = new List<IEntityMessageHandler>();
                        entityMessageHandlerMap.Add(entityMessageHandler.MessageType(), list);
                    }

                    // 将当前的消息处理器添加到对应的消息类型列表中
                    list.Add(entityMessageHandler);
                }
            }

            // 检查该类型是否具有 EntityRpcHandlerAttribute 自定义属性
            if (type.GetCustomAttribute<EntityRpcHandlerAttribute>(true) != null)
            {
                Type entityRpcType = typeof(IEntityRpcHandler);
                // 检查 type 是否实现了 IEntityRpcHandler 接口
                if (entityRpcType.IsAssignableFrom(type))
                {
                    // 实例化 type 对应的对象，并尝试将其转换为 IEntityRpcHandler 接口类型
                    IEntityRpcHandler entityRpcHandler = Activator.CreateInstance(type) as IEntityRpcHandler;

                    // 尝试从字典 entityRpcHandlerMap 中检查当前 RPC 处理器的 RPC 类型是否已存在
                    if (entityRpcHandlerMap.ContainsKey(entityRpcHandler.RpcType()))
                    {
                        // 如果已存在，则记录错误日志，并跳过当前循环的剩余部分
                        UnityLog.Error($"Duplicate Entity Rpc, type:{entityRpcHandler.RpcType().FullName}");
                        continue;
                    }

                    // 将当前的 RPC 处理器添加到字典中，键为其 RPC 类型
                    entityRpcHandlerMap.Add(entityRpcHandler.RpcType(), entityRpcHandler);
                }
            }
        }
    }

    // 驱动更新系统的方法，用于遍历并更新所有更新系统
    private void DriveUpdateSystem()
    {
        // 遍历更新系统映射中的所有更新系统
        foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
        {
            // 获取与当前更新系统相关的实体列表
            List<ECSEntity> updateSystemRelatedEntities = updateSystemRelatedEntityMap[updateSystem];
            // 如果与更新系统相关的实体列表为空，则跳过当前循环，继续下一个更新系统
            if (updateSystemRelatedEntities.Count == 0)
                continue;

            // 从对象池中获取一个ECSEntity类型的列表
            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
            // 将与更新系统相关的实体列表添加到新获取的列表中，避免修改原始列表
            entityList.AddRangeNonAlloc(updateSystemRelatedEntities);

            // 遍历新获取的实体列表中的每一个实体
            foreach (var entity in entityList)
            {
                // 如果当前更新系统不关注该实体，则跳过当前循环，继续下一个实体
                if (!updateSystem.ObservingEntity(entity))
                    continue;

                // 调用更新系统的Update方法，更新当前实体
                updateSystem.Update(entity);
            }

            // 将使用完毕的列表释放回对象池
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

    // 这是一个泛型方法，用于从awakeSystemMap中获取类型为C的IAwakeSystem，并将其添加到list中
    private void GetAwakeSystems<C>(List<IAwakeSystem> list) where C : ECSComponent
    {
        foreach (var awakeSystem in awakeSystemMap.Values)
        {
            // 检查当前awakeSystem的类型是否与泛型参数C匹配
            if (awakeSystem.ComponentType() == typeof(C))
            {
                // 如果匹配，则将该awakeSystem添加到list中
                list.Add(awakeSystem);
            }
        }
    }

    // 这是一个泛型方法，用于唤醒指定类型的组件C
    public void AwakeComponent<C>(C component) where C : ECSComponent
    {
        // 更新与组件关联的实体的系统列表
        UpdateSystemEntityList(component.Entity);

        // 从对象池中获取一个IAwakeSystem列表
        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();

        // 调用GetAwakeSystems方法获取类型为C的IAwakeSystem，并添加到list中
        GetAwakeSystems<C>(list);

        bool found = false;
        foreach (var item in list)
        {
            // 尝试将item转换为AwakeSystem<C>类型
            AwakeSystem<C> awakeSystem = item as AwakeSystem<C>;
            if (awakeSystem == null)
                continue;

            // 调用awakeSystem的Awake方法，唤醒指定的组件
            awakeSystem.Awake(component);
            found = true;
        }

        // 释放IAwakeSystem列表到对象池
        ListPool<IAwakeSystem>.Release(list);

        // 如果没有找到匹配的AwakeSystem，则记录警告日志
        if (!found)
        {
            UnityLog.Warn($"Not found awake system:<{typeof(C).Name}>");
        }
    }

    // 这是一个带有两个泛型参数的方法，用于唤醒指定类型的组件C，并接受一个额外的参数P1
    public void AwakeComponent<C, P1>(C component, P1 p1) where C : ECSComponent
    {
        // 更新与组件关联的实体的系统列表
        UpdateSystemEntityList(component.Entity);

        // 从对象池中获取一个IAwakeSystem列表
        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();

        // 调用ECSModule模块的GetAwakeSystems方法获取类型为C的IAwakeSystem，并添加到list中
        TGameFramework.Instance.GetModule<ECSModule>().GetAwakeSystems<C>(list);

        bool found = false;
        foreach (var item in list)
        {
            // 尝试将item转换为AwakeSystem<C, P1>类型
            AwakeSystem<C, P1> awakeSystem = item as AwakeSystem<C, P1>;
            if (awakeSystem == null)
                continue;

            // 调用awakeSystem的Awake方法，传入组件和参数p1，唤醒指定的组件
            awakeSystem.Awake(component, p1);
            found = true;
        }

        // 释放IAwakeSystem列表到对象池
        ListPool<IAwakeSystem>.Release(list);

        // 如果没有找到匹配的AwakeSystem，则记录警告日志
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

    // 这是一个泛型方法，用于从destroySystemMap中获取类型为C的IDestroySystem，并将其添加到list中
    private void GetDestroySystems<C>(List<IDestroySystem> list) where C : ECSComponent
    {
        foreach (var destroySystem in destroySystemMap.Values)
        {
            // 检查当前destroySystem的类型是否与泛型参数C匹配
            if (destroySystem.ComponentType() == typeof(C))
            {
                // 如果匹配，则将该destroySystem添加到list中
                list.Add(destroySystem);
            }
        }
    }

    // 这是一个非泛型方法，用于从destroySystemMap中获取指定类型的IDestroySystem，并将其添加到list中
    private void GetDestroySystems(Type componentType, List<IDestroySystem> list)
    {
        foreach (var destroySystem in destroySystemMap.Values)
        {
            // 检查当前destroySystem的类型是否与传入的componentType匹配
            if (destroySystem.ComponentType() == componentType)
            {
                // 如果匹配，则将该destroySystem添加到list中
                list.Add(destroySystem);
            }
        }
    }

    // 这是一个泛型方法，用于销毁指定类型的组件C
    public void DestroyComponent<C>(C component) where C : ECSComponent
    {
        // 更新与组件关联的实体的系统列表
        UpdateSystemEntityList(component.Entity);

        // 从对象池中获取一个IDestroySystem列表
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();

        // 调用GetDestroySystems方法获取类型为C的IDestroySystem，并添加到list中
        GetDestroySystems<C>(list);

        foreach (var item in list)
        {
            // 尝试将item转换为DestroySystem<C>类型
            DestroySystem<C> destroySystem = item as DestroySystem<C>;
            if (destroySystem == null)
                continue;

            // 调用destroySystem的Destroy方法，销毁指定的组件
            destroySystem.Destroy(component);

            // 标记组件为已销毁状态
            component.Disposed = true;
        }

        // 释放IDestroySystem列表到对象池
        ListPool<IDestroySystem>.Release(list);
    }

    // 这是一个非泛型方法，用于销毁任意类型的ECSComponent组件
    public void DestroyComponent(ECSComponent component)
    {
        // 更新与组件关联的实体的系统列表
        UpdateSystemEntityList(component.Entity);

        // 从对象池中获取一个IDestroySystem列表
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();

        // 调用GetDestroySystems方法获取指定类型的IDestroySystem，并添加到list中
        GetDestroySystems(component.GetType(), list);

        foreach (var item in list)
        {
            // 调用item的Destroy方法，销毁指定的组件
            // 这里不需要类型转换，因为item已经是IDestroySystem类型，可以直接调用Destroy方法
            item.Destroy(component);

            // 标记组件为已销毁状态
            component.Disposed = true;
        }

        // 释放IDestroySystem列表到对象池
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

    // 更新与给定实体相关联的更新系统列表
    private void UpdateSystemEntityList(ECSEntity entity)
    {
        // 遍历所有的即时更新系统
        foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
        {
            // 获取当前更新系统所关联的实体列表
            List<ECSEntity> entityList = updateSystemRelatedEntityMap[updateSystem];

            // 如果实体列表中不包含当前实体
            if (!entityList.Contains(entity))
            {
                // 检查当前更新系统是否正在观察这个实体
                if (updateSystem.ObservingEntity(entity))
                {
                    // 如果正在观察，则将实体添加到实体列表中
                    entityList.Add(entity);
                }
            }
            else
            {
                // 如果实体列表中已经包含当前实体
                // 检查当前更新系统是否不再观察这个实体
                if (!updateSystem.ObservingEntity(entity))
                {
                    // 如果不再观察，则从实体列表中移除该实体
                    entityList.Remove(entity);
                }
            }
        }

        // 遍历所有的延迟更新系统
        foreach (ILateUpdateSystem lateUpdateSystem in lateUpdateSystemMap.Values)
        {
            // 获取当前延迟更新系统所关联的实体列表
            List<ECSEntity> entityList = lateUpdateSystemRelatedEntityMap[lateUpdateSystem];

            // 与即时更新系统的逻辑类似，检查并更新延迟更新系统的实体列表
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

        // 遍历所有的固定更新系统
        foreach (IFixedUpdateSystem fixedUpdateSystem in fixedUpdateSystemMap.Values)
        {
            // 获取当前固定更新系统所关联的实体列表
            List<ECSEntity> entityList = fixedUpdateSystemRelatedEntityMap[fixedUpdateSystem];

            // 与即时更新系统的逻辑类似，检查并更新固定更新系统的实体列表
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

    // 添加实体到实体列表中，使用实体的InstanceID作为键
    public void AddEntity(ECSEntity entity)
    {
        entities.Add(entity.InstanceID, entity);
    }

    // 从实体列表中移除指定的实体
    public void RemoveEntity(ECSEntity entity)
    {
        if (entity == null)
            return; // 如果实体为空，则直接返回，不做任何操作

        entities.Remove(entity.InstanceID); // 从实体列表中移除该实体的条目
        ECSScene scene = entity.Scene; // 获取实体所属的场景
        scene?.RemoveEntity(entity.InstanceID); // 如果场景不为空，则从场景中移除该实体
    }

    // 根据ID查找实体，返回ECSEntity类型的实体
    public ECSEntity FindEntity(long id)
    {
        return FindEntity<ECSEntity>(id); // 调用泛型方法，并指定返回类型为ECSEntity
    }

    // 根据ID查找实体，返回指定类型T的实体
    public T FindEntity<T>(long id) where T : ECSEntity
    {
        entities.TryGetValue(id, out ECSEntity entity); // 尝试从实体列表中获取指定ID的实体
        return entity as T; // 将实体转换为指定类型T并返回，如果转换失败则返回null
    }

    // 根据实体ID查找并返回该实体上指定类型T的组件
    public T FindComponentOfEntity<T>(long entityID) where T : ECSComponent
    {
        return FindEntity(entityID)?.GetComponent<T>(); // 查找实体，如果找到则获取其上的T类型组件并返回，否则返回null
    }

    // 向指定ID的实体发送消息M，异步执行
    public async Task SendMessageToEntity<M>(long id, M m)
    {
        if (id == 0)
            return; // 如果ID为0，则直接返回，不做任何操作

        ECSEntity entity = FindEntity(id); // 根据ID查找实体
        if (entity == null)
            return; // 如果实体不存在，则直接返回，不做任何操作

        Type messageType = m.GetType(); // 获取消息M的类型
        if (!entityMessageHandlerMap.TryGetValue(messageType, out List<IEntityMessageHandler> list))
            return; // 如果消息类型对应的处理器列表不存在，则直接返回，不做任何操作

        List<IEntityMessageHandler> entityMessageHandlers = ListPool<IEntityMessageHandler>.Obtain(); // 从对象池中获取一个处理器列表
        entityMessageHandlers.AddRangeNonAlloc(list); // 将找到的处理器列表添加到从对象池中获取的列表中，避免内存分配
        foreach (IEntityMessageHandler<M> handler in entityMessageHandlers) // 遍历处理器列表
        {
            await handler.Post(entity, m); // 异步调用处理器的Post方法，将消息发送给实体
        }

        ListPool<IEntityMessageHandler>.Release(entityMessageHandlers); // 释放从对象池中获取的处理器列表
    }

    // 异步发送RPC请求到指定ID的实体，并返回响应
    public async Task<Response> SendRpcToEntity<Request, Response>(long entityID, Request request)
        where Response : IEntityRpcResponse, new()
    {
        if (entityID == 0)
            return new Response() { Error = true }; // 如果实体ID为0，则直接返回带有错误的响应

        ECSEntity entity = FindEntity(entityID);
        if (entity == null)
            return new Response() { Error = true }; // 如果找不到实体，则直接返回带有错误的响应

        Type messageType = request.GetType();
        if (!entityRpcHandlerMap.TryGetValue(messageType, out IEntityRpcHandler entityRpcHandler))
            return new Response() { Error = true }; // 如果找不到与请求类型对应的RPC处理器，则直接返回带有错误的响应

        IEntityRpcHandler<Request, Response> handler = entityRpcHandler as IEntityRpcHandler<Request, Response>;
        if (handler == null)
            return new Response() { Error = true }; // 如果无法将RPC处理器转换为正确的泛型类型，则直接返回带有错误的响应

        return await handler.Post(entity, request); // 异步调用RPC处理器的Post方法，发送请求到实体，并返回响应
    }
}

