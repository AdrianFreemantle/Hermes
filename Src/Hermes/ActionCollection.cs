using System;
using System.Collections.Generic;

namespace Hermes
{
    public class ActionCollection 
    {
        readonly List<Action> actions = new List<Action>();
 
        public void AddAction(Action action)
        {
            actions.Add(action);
        }

        public void Clear()
        {
            actions.Clear();
        }

        public void InvokeAll()
        {
            foreach (var action in actions)
            {
                action.Invoke();
            }
        }
    }
}