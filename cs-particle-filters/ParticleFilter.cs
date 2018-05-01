using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParticleFilters
{
    public class ParticleFilter
    {
        protected int mParticleCount;
        protected IParticle[] mParticles;

        protected double[] mAccumulatedParticleWeights;
        protected double[] mParticleWeights;


        public double GetMeasurementProb(double measurement_mean, double[] sense_noises, double[] measurement)
        {
            double prob = 1.0;
            
            for (int i = 0; i < measurement.Length; ++i)
            {
                prob *= Gaussian(measurement_mean, sense_noises[i], measurement[i]);
            }

            return prob;
        }

        public double Gaussian(double mu, double sigma, double x)
        {
            double sigma2 = sigma * sigma;
            return (1.0 / System.Math.Sqrt(2 * System.Math.PI * sigma2)) * System.Math.Exp(-System.Math.Pow((mu - x), 2) / sigma2 / 2);
        }

        public delegate IParticle ParticleGenerator();

        public ParticleFilter(int particle_count, ParticleGenerator generator)
        {
            mParticleCount = particle_count;

            mParticles=new IParticle[mParticleCount];
            for (int i = 0; i < mParticleCount; ++i)
            {
                mParticles[i] = generator();
            }

            mAccumulatedParticleWeights = new double[mParticleCount];
            mParticleWeights = new double[mParticleCount];
        }

        public virtual void UpdateParticles(object info = null)
        {
            IParticle[] p = new IParticle[mParticleCount];
            for (int i = 0; i < mParticleCount; ++i)
            {
                p[i] = mParticles[i].GenerateUpdatedCopy(info);
            }
            mParticles = p;
        }

        public virtual void CalculateParticleWeights(object info = null)
        {
            double sum = 0;
            for (int i = 0; i < mParticleCount; ++i)
            {
                double weight = mParticles[i].ComputeWeight(info);
                mParticleWeights[i] = weight;
                sum += weight;
                mAccumulatedParticleWeights[i] = sum;
            }

            if (sum != 0)
            {
                for (int i = 0; i < mParticleCount; ++i)
                {
                    mParticleWeights[i] /= sum;
                    mAccumulatedParticleWeights[i] /= sum;
                }
            }


        }

        public virtual void ResampleParticles()
        {
            ResampleParticles_DefaultRouletteWheel();
        }

        public void ResampleParticles_DefaultRouletteWheel()
        {
            IParticle[] p = new IParticle[mParticleCount];

            double mw = 0;
            for (int i = 0; i < mParticleCount; ++i)
            {
                mw = System.Math.Max(mw, mParticleWeights[i]);
            }

            double beta = 0;
            int index = (int)(mParticleCount * RandomEngine.NextDouble());
            for (int i = 0; i < mParticleCount; ++i)
            {
                beta += mw * 2 * RandomEngine.NextDouble();
                while (beta > mParticleWeights[index])
                {
                    beta -= mParticleWeights[index];
                    index = (index + 1) % mParticleCount;
                }
                p[i] = mParticles[index];
            }

            mParticles = p;
        }

        public void ResampleParticles_MyRouletteWheel()
        {
            IParticle[] p = new IParticle[mParticleCount];

            for (int i = 0; i < mParticleCount; ++i)
            {
                double r = RandomEngine.NextDouble();
                for (int j = 0; j < mParticleCount; ++j)
                {
                    if (mAccumulatedParticleWeights[j] >= r)
                    {
                        p[i] = mParticles[j];
                        break;
                    }
                }
            }

            mParticles = p;
        }

        public IEnumerable<IParticle> Enumerate()
        {
            foreach (IParticle p in mParticles)
            {
                yield return p;
            }
        }
    }
}
