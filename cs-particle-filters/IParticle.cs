using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParticleFilters
{
    public interface IParticle 
    {
        /// <summary>
        /// Immutable method which turns the updated copy of the particle
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        IParticle GenerateUpdatedCopy(object info = null);

        double ComputeWeight(object info = null);
    }
}
