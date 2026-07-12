using System;
using Godot;
using Khepri.Extensions;
using Khepri.World.Entities;

namespace Khepri.World.Orbits
{
    /// <summary> An analytic Keplerian orbit around a primary body — a rail. Position is a pure function of time, so it is exact at any timescale and never drifts from the ellipse it was authored on. A zero eccentricity collapses it to a circle, so it strictly supersedes a plain circular orbit. </summary>
    public sealed class StaticOrbit : Orbit
    {
        /// <summary> The body being orbited. Its own position must be current before this orbit is evaluated, so satellites are always added to a system after their primaries. </summary>
        public Body Primary { get; }

        /// <summary> The semi-major axis — half the ellipse's long diameter, in world units. Sets the orbit's size and, with the primary's mass, its period. </summary>
        public Single SemiMajorAxis { get; }

        /// <summary> The eccentricity, from zero (a circle) toward one (an ever more elongated ellipse). </summary>
        public Single Eccentricity { get; }

        /// <summary> The argument of periapsis — the angle, in radians, that orients the ellipse's long axis within the system. </summary>
        public Single ArgumentOfPeriapsis { get; }

        /// <summary> The mean anomaly at the epoch, in radians: where along the orbit the body sits at time zero. </summary>
        public Single MeanAnomalyAtEpoch { get; }

        /// <summary> The mean motion, in radians per simulated second: the rate the mean anomaly advances, derived from the primary's gravity so the rail matches what integration would produce. </summary>
        public Single MeanMotion { get; }


        /// <summary> The cosine and sine of the argument of periapsis, cached so every evaluation rotates the ellipse without recomputing them. </summary>
        private readonly Single _cosArgument;
        private readonly Single _sinArgument;

        /// <summary> The ratio of the semi-minor to the semi-major axis, sqrt(1 - e^2), cached because every evaluation needs it. </summary>
        private readonly Single _semiMinorRatio;


        /// <summary> The number of Newton iterations used to solve Kepler's equation. A handful converges to well within rendering precision for any eccentricity the game authors. </summary>
        private const Int32 KeplerIterations = 5;


        /// <summary> Create a Keplerian orbit around a primary. </summary>
        /// <param name="body"> The body that rides this orbit. </param>
        /// <param name="primary"> The body being orbited. </param>
        /// <param name="semiMajorAxis"> The semi-major axis, in world units. </param>
        /// <param name="eccentricity"> The eccentricity, in [0, 1). </param>
        /// <param name="argumentOfPeriapsis"> The angle orienting the ellipse's long axis, in radians. </param>
        /// <param name="meanAnomalyAtEpoch"> Where along the orbit the body sits at time zero, in radians. </param>
        public StaticOrbit(Body body, Body primary, Single semiMajorAxis, Single eccentricity, Single argumentOfPeriapsis, Single meanAnomalyAtEpoch)
            : base(body)
        {
            Primary = primary;
            SemiMajorAxis = semiMajorAxis;
            Eccentricity = eccentricity;
            ArgumentOfPeriapsis = argumentOfPeriapsis;
            MeanAnomalyAtEpoch = meanAnomalyAtEpoch;
            MeanMotion = OrbitalMechanics.MeanMotion(primary.Mass, semiMajorAxis);

            _cosArgument = Mathf.Cos(argumentOfPeriapsis);
            _sinArgument = Mathf.Sin(argumentOfPeriapsis);
            _semiMinorRatio = Mathf.Sqrt(1f - eccentricity * eccentricity);
        }


        /// <inheritdoc/>
        internal override void Prime(SolarSystem system)
        {
            Advance(system.Time, 0f);
        }


        /// <inheritdoc/>
        internal override void Advance(Double time, Single delta)
        {
            // Where the body would be on a uniformly-swept circle, then Kepler's equation bends that into
            // the eccentric anomaly the real elliptical motion has actually reached by now.
            Single meanAnomaly = (Single)((MeanAnomalyAtEpoch + MeanMotion * time) % Math.Tau);
            Single eccentricAnomaly = SolveEccentricAnomaly(meanAnomaly);

            Single cosE = Mathf.Cos(eccentricAnomaly);
            Single sinE = Mathf.Sin(eccentricAnomaly);

            // Perifocal coordinates, with the primary sitting at the focus of the ellipse.
            Single px = SemiMajorAxis * (cosE - Eccentricity);
            Single py = SemiMajorAxis * _semiMinorRatio * sinE;

            // The eccentric anomaly sweeps faster near periapsis; differentiating the position by it gives
            // the velocity, which is why the body speeds up as it falls in and coasts through apoapsis.
            Single anomalyRate = MeanMotion / (1f - Eccentricity * cosE);
            Single vx = SemiMajorAxis * -sinE * anomalyRate;
            Single vy = SemiMajorAxis * _semiMinorRatio * cosE * anomalyRate;

            Entity.Position = Primary.Position + Rotate(px, py);
            Entity.Velocity = Primary.Velocity + Rotate(vx, vy);
        }


        /// <summary> Rotate a perifocal vector into the system frame by the argument of periapsis. </summary>
        /// <param name="x"> The component along the ellipse's long axis. </param>
        /// <param name="y"> The component along the ellipse's short axis. </param>
        private Vector2 Rotate(Single x, Single y)
        {
            return new Vector2(x * _cosArgument - y * _sinArgument, x * _sinArgument + y * _cosArgument);
        }


        /// <summary> Solve Kepler's equation M = E - e sin E for the eccentric anomaly by Newton's method. </summary>
        /// <param name="meanAnomaly"> The mean anomaly, in radians, and the initial guess. </param>
        /// <returns> The eccentric anomaly, in radians. </returns>
        private Single SolveEccentricAnomaly(Single meanAnomaly)
        {
            Single eccentricAnomaly = meanAnomaly;

            for (Int32 i = 0; i < KeplerIterations; i++)
            {
                Single residual = eccentricAnomaly - Eccentricity * Mathf.Sin(eccentricAnomaly) - meanAnomaly;
                eccentricAnomaly -= residual / (1f - Eccentricity * Mathf.Cos(eccentricAnomaly));
            }

            return eccentricAnomaly;
        }
    }
}
