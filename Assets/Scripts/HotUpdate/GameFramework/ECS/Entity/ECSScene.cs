using System.Collections.Generic;


    public class ECSScene : ECSEntity
    {
        private Dictionary<long, ECSEntity> entities;//�洢ʵ�������

        public ECSScene()
        {
            entities = new Dictionary<long, ECSEntity>();//��ʼ��
        }
        /// <summary>
        /// ɾ������
        /// </summary>
        public override void Dispose()
        {
            if (Disposed)//�ж��Ƿ��Ѿ�ɾ����
                return;

            List<long> entityIDList = ListPool<long>.Obtain();//�ӳ��ڻ�ȡ����
            foreach (var entityID in entities.Keys)//��������ʵ��
            {
                entityIDList.Add(entityID);//��ӽ�������
            }
            foreach (var entityID in entityIDList)//����������
            {
                ECSEntity entity = entities[entityID];//��ȡʵ��
                entity.Dispose();//����ʵ�����ٷ���
            }
            ListPool<long>.Release(entityIDList);//���س�

            base.Dispose();//���ø������ٷ���
        }
        /// <summary>
        /// ���ʵ��
        /// </summary>
        /// <param name="entity"></param>
        public void AddEntity(ECSEntity entity)
        {
            if (entity == null)//�п�
                return;

            ECSScene oldScene = entity.Scene;//�õ�Scene
            if (oldScene != null)
            {
                oldScene.RemoveEntity(entity.InstanceID);//ɾ����ӵ�ʵ��
            }

            entities.Add(entity.InstanceID, entity);//��ӽ�����
            entity.SceneID = InstanceID;//��ʵ��ID��ֵ������ID
            UnityLog.Info($"Scene Add Entity, Current Count:{entities.Count}");
        }
        /// <summary>
        /// ɾ��ʵ��
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
 