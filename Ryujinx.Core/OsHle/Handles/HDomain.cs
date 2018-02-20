using Ryujinx.Core.OsHle.Utilities;
using System;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Handles
{
    class HDomain : HSession
    {
        private Dictionary<int, object> Objects;

        private IdPool ObjIds;

        public HDomain(HSession Session) : base(Session)
        {
            Objects = new Dictionary<int, object>();

            ObjIds = new IdPool();
        }

        public int GenerateObjectId(object Obj)
        {
            int Id = ObjIds.GenerateId();

            if (Id == -1)
            {
                throw new InvalidOperationException();
            }

            Objects.Add(Id, Obj);

            return Id;
        }

        public void DeleteObject(int Id)
        {
            if (Objects.TryGetValue(Id, out object Obj))
            {
                if (Obj is IDisposable DisposableObj)
                {
                    DisposableObj.Dispose();
                }

                ObjIds.DeleteId(Id);
                Objects.Remove(Id);
            }
        }

        public object GetObject(int Id)
        {
            if (Objects.TryGetValue(Id, out object Obj))
            {
                return Obj;
            }

            return null;
        }
    }
}