using System;
using System.Collections.Generic;
using System.Text;

namespace LogikUI.Simulation.Gates
{
    interface IInstance
    {
    }

    abstract class Component<T> where T : IInstance
    {
        public abstract void Propagate(Engine engine, T instance);
    }
}
