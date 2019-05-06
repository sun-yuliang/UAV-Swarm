using System;
using System.Collections.Generic;

namespace CrazyflieLib.Util
{
    public class Caller
    {
        public List<Action<object[]>> callbacks = new List<Action<object[]>>();

        public void add_callback(Action<object[]> cb)
        {
            if (!callbacks.Contains(cb))
                callbacks.Add(cb);
        }

        public void remove_callback(Action<object[]> cb)
        {
            callbacks.Remove(cb);
        }

        public void call(object[] args)
        {
            foreach (var cb in callbacks)
                cb(args);
        }
    }
}
