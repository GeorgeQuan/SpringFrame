using System;
using System.Collections.Generic;
using System.Reflection;

 
    /// <summary>
    /// 作者: Teddy
    /// 时间: 2018/03/02
    /// 功能: 工具类
    /// </summary>
    
	public static partial class Utility
    {
        public static class Types
        {
            public readonly static Assembly GAME_CSHARP_ASSEMBLY = Assembly.Load("Assembly-CSharp");//加载指定名称程序集并保存
            public readonly static Assembly GAME_EDITOR_ASSEMBLY = Assembly.Load("Assembly-CSharp-Editor");

            /// <summary>
            /// 获取所有能从某个类型分配的属性列表
            /// </summary>
            public static List<PropertyInfo> GetAllAssignablePropertiesFromType(Type basePropertyType, Type objType, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            {
                List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
                PropertyInfo[] properties = objType.GetProperties(bindingFlags);
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo propertyInfo = properties[i];
                    if (basePropertyType.IsAssignableFrom(propertyInfo.PropertyType))
                    {
                        propertyInfos.Add(propertyInfo);
                    }
                }
                return propertyInfos;
            }

            /// <summary>
            /// 获取某个类型的所有子类型
            /// </summary>
            /// <param name="baseClass">父类</param>
            /// <param name="assemblies">程序集,如果为null则查找当前程序集</param>
            /// <returns></returns>
            public static List<Type> GetAllSubclasses(Type baseClass, bool allowAbstractClass, params Assembly[] assemblies)
            {
                List<Type> subclasses = new List<Type>();//创建容器用于存储类型
                if (assemblies == null)//如果为传程序集,默认调用此方法的程序集
                {
                    assemblies = new Assembly[] { Assembly.GetCallingAssembly() };//这里返回的是当前程序集,new 进程序集数组内
                }
                foreach (var assembly in assemblies)//遍历程序集数组
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!baseClass.IsAssignableFrom(type))//判断程序集内每一个类型是否是从baseClass派生的
                            continue;

                        if (!allowAbstractClass && type.IsAbstract)//判断类型是否是抽象类型,并且判断是否要保留抽象类型
                            continue;

                        subclasses.Add(type);//添加进容器
                    }
                }
                return subclasses;//返回
            }
        }
    }
 