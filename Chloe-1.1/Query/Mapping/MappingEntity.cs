﻿using Chloe.Mapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.Mapping
{

    public abstract class MappingObject
    {
        public abstract Type EntityType { get; }
        public abstract IObjectActivtor CreateObjectActivtor();
    }

    public class MappingField : MappingObject
    {
        Type _type;
        public MappingField(Type type, int readerOrdinal)
        {
            this._type = type;
            this.ReaderOrdinal = readerOrdinal;
        }
        public int ReaderOrdinal { get; private set; }
        public override Type EntityType { get { return this._type; } }

        public override IObjectActivtor CreateObjectActivtor()
        {
            throw new NotImplementedException();
        }
    }

    public class MappingEntity
    {
        public MappingEntity(ConstructorInfo constructor)
        {
            //this.EntityType = constructor.DeclaringType;
            this.Constructor = constructor;
            this.ConstructorParameters = new Dictionary<ParameterInfo, int>();
            this.ConstructorEntityParameters = new Dictionary<ParameterInfo, MappingEntity>();
            this.Members = new Dictionary<MemberInfo, int>();
            this.EntityMembers = new Dictionary<MemberInfo, MappingEntity>();
        }
        //public Type EntityType { get; private set; }
        public ConstructorInfo Constructor { get; private set; }
        public Dictionary<ParameterInfo, int> ConstructorParameters { get; private set; }
        public Dictionary<ParameterInfo, MappingEntity> ConstructorEntityParameters { get; private set; }

        public Dictionary<MemberInfo, int> Members { get; private set; }
        public Dictionary<MemberInfo, MappingEntity> EntityMembers { get; private set; }

        public IObjectActivtor CreateObjectActivtor()
        {
            /*
            * 根据 EntityType 生成 IObjectActivtor
            * 如果 EntityType 是匿名类型的话
           */

            EntityMemberMapper mapper = EntityMemberMapper.GetInstance(this.Constructor.DeclaringType);
            List<IValueSetter> memberSetters = new List<IValueSetter>(this.Members.Count + this.EntityMembers.Count);
            foreach (var kv in this.Members)
            {
                Action<object, IDataReader, int> del = mapper.GetMemberSetter(kv.Key);
                MappingMemberBinder binder = new MappingMemberBinder(del, kv.Value);
                memberSetters.Add(binder);
            }

            foreach (var kv in this.EntityMembers)
            {
                Action<object, object> del = mapper.GetNavigationMemberSetter(kv.Key);
                IObjectActivtor memberActivtor = kv.Value.CreateObjectActivtor();
                NavigationMemberBinder binder = new NavigationMemberBinder(del, memberActivtor);
                memberSetters.Add(binder);
            }

            Func<IDataReader, ReaderOrdinalEnumerator, ObjectActivtorEnumerator, object> instanceCreator = EntityConstructor.GetInstance(this.Constructor).InstanceCreator;

            List<int> readerOrdinals = null;
            List<IObjectActivtor> objectActivtors = null;

            ObjectActivtor ret = new ObjectActivtor(instanceCreator, readerOrdinals, objectActivtors, memberSetters);

            return ret;
        }
    }

}