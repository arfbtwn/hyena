//
// Bound.cs
//
// Author:
//   nicholas <>
//
// Copyright 2017 nicholas
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Linq.Expressions;

namespace Hyena.Data.Gui
{
    public class Bound<T> : ObjectBinder
    {
        string _last;

        Type _type;

        Lazy<Func<object, T>> _get;

        void Refresh ()
        {
            _get = new Lazy<Func<object, T>> (_Get);
        }

        public Type Type
        {
            get { return _type; }
            set
            {
                if (_type == value) return;

                _type = value;

                Refresh ();
            }
        }

        public override void BindDataItem (object item)
        {
            try
            {
                BoundObject = Get (item);
            }
            catch (Exception e)
            {
                Log.Warning (e);
                base.BindDataItem (item);
            }
        }

        Func<object, T> _Get ()
        {
            _last = Property;
            return _Property (Property);
        }

        public T Get (object obj)
        {
            if (null == obj) return default (T);
            
            Type = obj.GetType ();

            if (_last != Property)
            {
                Refresh ();
            }

            try
            {
                return _get.Value (obj);
            }
            catch
            {
                return default (T);
            }
        }

        protected Func<object, T> _Property (string property)
        {
            var type = Type;
            var value = typeof (T);

            var p = type.GetProperty (property);

            if (null == p)
            {
                throw new ArgumentException (nameof (property));
            }

            if (!value.IsAssignableFrom (p.PropertyType))
            {
                throw new ArgumentException (nameof (T));
            }

            var m = p.GetGetMethod ();

            if (null == m)
            {
                throw new ArgumentException ();
            }

            var arg = Expression.Parameter (typeof (object));
            var targ = Expression.Convert (arg, type);
            var call = Expression.Call (targ, m);
            var cast = Expression.Convert (call, value);
            var lambda = Expression.Lambda (cast, arg);

            return (Func<object, T>) lambda.Compile ();
        }
    }
}
