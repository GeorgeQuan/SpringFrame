using System.Collections.Generic;


    public class ECSScene : ECSEntity
    {
        private Dictionary<long, ECSEntity> entities;//存储实体的容器

        public ECSScene()
        {
            entities = new Dictionary<long, ECSEntity>();//初始化
        }
        /// <summary>
        /// 删除方法
        /// </summary>
        public override void Dispose()
        {
            if (Disposed)//判断是否已经删除过
                return;

            List<long> entityIDList = ListPool<long>.Obtain();//从池内获取容器
            foreach (var entityID in entities.Keys)//遍历所有实体
            {
                entityIDList.Add(entityID);//添加进新容器
            }
            foreach (var entityID in entityIDList)//遍历新容器
            {
                ECSEntity entity = entities[entityID];//获取实体
                entity.Dispose();//调用实体销毁方法
            }
            ListPool<long>.Release(entityIDList);//返回池

            base.Dispose();//调用父类销毁方法
        }
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="entity"></param>
        public void AddEntity(ECSEntity entity)
        {
            if (entity == null)//判空
                return;

            ECSScene oldScene = entity.Scene;//拿到Scene
            if (oldScene != null)
            {
                oldScene.RemoveEntity(entity.InstanceID);//删除添加的实体
            }

            entities.Add(entity.InstanceID, entity);//添加进容器
            entity.SceneID = InstanceID;//把实例ID赋值给场景ID
            UnityLog.Info($"Scene Add Entity, Current Count:{entities.Count}");
        }
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entityID"></param>
        public void RemoveEntity(long entityID)
        {
            if (entities.Remove(entityID))
            {
                UnityLog.Info($"Scene Remove Entity, Current Count:{entities.Count}");
            }
        }

        public void FindEntities<T>(List<long> list) where T : ECSEntity
        {
            foreach (var item in entities)
            {
                if (item.Value is T)
                {
                    list.Add(item.Key);
                }
            }
        }

        public void FindEntitiesWithComponent<T>(List<long> list) where T : ECSComponent
        {
            foreach (var item in entities)
            {
                if (item.Value.HasComponent<T>())
                {
                    list.Add(item.Key);
                }
            }
        }

        public void GetAllEntities(List<long> list)
        {
            foreach (var item in entities)
            {
                list.Add(item.Key);
            }
        }
    }
 