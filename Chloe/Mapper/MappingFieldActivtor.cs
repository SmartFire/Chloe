﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Mapper
{
    public class MappingFieldActivtor : IObjectActivtor
    {
        Func<IDataReader, int, object> _fn = null;
        int _readerOrdinal;
        public MappingFieldActivtor(Func<IDataReader, int, object> fn, int readerOrdinal)
        {
            this._fn = fn;
            this._readerOrdinal = readerOrdinal;
        }
        public object CreateInstance(IDataReader reader)
        {
            return _fn(reader, _readerOrdinal);
        }
    }
}