using System.Collections.Generic;
using Khepri.Data.Entities;

namespace Khepri.Data
{
    /// <summary> A single solar system within the larger galaxy. </summary>
    public class SolarSystem
    {
        /// <summary> All the entities that currently exist within the system. </summary>
        private HashSet<Entity> _entities = new HashSet<Entity>();
    }
}
