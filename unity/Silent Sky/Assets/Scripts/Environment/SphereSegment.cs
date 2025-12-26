using System.Collections.Generic;
using UnityEngine;

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Represents a segment of the sphere that maps to one hexagon
    /// Divides the sphere into 19 roughly equal segments
    /// </summary>
    public class SphereSegment
    {
            public int segmentIndex; // 0-18, corresponds to hexagon index
            public float thetaMin, thetaMax; // Azimuth boundaries
            public float phiMin, phiMax; // Polar angle boundaries
        
        // Store center exclusion info for Ring 1 segments
        private float centerPhiMin = -1f; // -1 means no exclusion
        private float centerPhiMax = -1f;
        
        public SphereSegment(int index, float tMin, float tMax, float pMin, float pMax, float centerExcludeMin = -1f, float centerExcludeMax = -1f)
        {
            segmentIndex = index;
            thetaMin = SphericalCoordinateSystem.NormalizeTheta(tMin);
            thetaMax = SphericalCoordinateSystem.NormalizeTheta(tMax);
            phiMin = SphericalCoordinateSystem.ClampPhi(pMin);
            phiMax = SphericalCoordinateSystem.ClampPhi(pMax);
            centerPhiMin = centerExcludeMin >= 0f ? centerExcludeMin : -1f;
            centerPhiMax = centerExcludeMax >= 0f ? centerExcludeMax : -1f;
        }
        
        /// <summary>
        /// Checks if a point (theta, phi) is within this segment
        /// </summary>
        public bool Contains(float theta, float phi)
        {
            theta = SphericalCoordinateSystem.NormalizeTheta(theta);
            phi = SphericalCoordinateSystem.ClampPhi(phi);
            
            // Check phi range
            if (phi < phiMin || phi > phiMax)
            {
                return false;
            }
            
            // Exclude center region if specified (for Ring 1 segments)
            if (centerPhiMin >= 0f && centerPhiMax >= 0f)
            {
                if (phi >= centerPhiMin && phi <= centerPhiMax)
                {
                    return false; // Point is in excluded center region
                }
            }
            
            // Check theta range (handle wrap-around at 0/2π)
            // Special case: if thetaMin == thetaMax after normalization, segment spans full circle (360°)
            // This happens when the segment was created with thetaMax = 2π, which normalizes to 0
            const float epsilon = 0.001f;
            if (Mathf.Abs(thetaMin - thetaMax) < epsilon || 
                (thetaMin == 0f && thetaMax == 0f))
            {
                // Full circle segment - accept all theta values
                return true;
            }
            else if (thetaMin <= thetaMax)
            {
                // Normal case: no wrap-around
                return theta >= thetaMin && theta <= thetaMax;
            }
            else
            {
                // Wrap-around case: segment crosses 0/2π boundary
                return theta >= thetaMin || theta <= thetaMax;
            }
        }
        
        /// <summary>
        /// Creates 19 segments roughly evenly distributed on the sphere
        /// Maps to 19 hexagons: 1 center + 6 ring 1 + 12 ring 2
        /// </summary>
        public static List<SphereSegment> CreateSegments()
        {
            List<SphereSegment> segments = new List<SphereSegment>();
            
            // Center segment (index 0) - small region around equator
            // Phi = π/2 is the equator
            float centerPhiRange = 0.2f;
            segments.Add(new SphereSegment(0, 0f, 2f * Mathf.PI, 
                Mathf.PI / 2f - centerPhiRange, Mathf.PI / 2f + centerPhiRange));
            
            // Ring 1: 6 segments around center (indices 1-6)
            // Band around center, excluding center region
            float ring1LowerPhi = Mathf.PI / 2f - 0.45f;
            float ring1UpperPhi = Mathf.PI / 2f + 0.45f;
            float centerExcludeMin = Mathf.PI / 2f - centerPhiRange;
            float centerExcludeMax = Mathf.PI / 2f + centerPhiRange;
            for (int i = 0; i < 6; i++)
            {
                float thetaStart = (i / 6f) * 2f * Mathf.PI;
                float thetaEnd = ((i + 1) / 6f) * 2f * Mathf.PI;
                // Ring 1: spans from ring1LowerPhi to ring1UpperPhi, but excludes center region
                segments.Add(new SphereSegment(i + 1, thetaStart, thetaEnd, 
                    ring1LowerPhi, ring1UpperPhi, centerExcludeMin, centerExcludeMax));
            }
            
            // Ring 2: 12 segments in outer regions (indices 7-18)
            // Upper polar region: 6 segments (indices 7-12)
            float upperPhiMin = 0f;
            float upperPhiMax = ring1LowerPhi;
            for (int i = 0; i < 6; i++)
            {
                float thetaStart = (i / 6f) * 2f * Mathf.PI;
                float thetaEnd = ((i + 1) / 6f) * 2f * Mathf.PI;
                segments.Add(new SphereSegment(i + 7, thetaStart, thetaEnd, upperPhiMin, upperPhiMax));
            }
            
            // Lower polar region: 6 segments (indices 13-18)
            float lowerPhiMin = ring1UpperPhi;
            float lowerPhiMax = Mathf.PI;
            for (int i = 0; i < 6; i++)
            {
                float thetaStart = (i / 6f) * 2f * Mathf.PI;
                float thetaEnd = ((i + 1) / 6f) * 2f * Mathf.PI;
                segments.Add(new SphereSegment(i + 13, thetaStart, thetaEnd, lowerPhiMin, lowerPhiMax));
            }
            
            return segments;
        }
        
        /// <summary>
        /// Finds which segment contains a given point (theta, phi)
        /// </summary>
        public static int FindSegmentForPoint(float theta, float phi, List<SphereSegment> segments)
        {
            // Normalize inputs first
            float originalTheta = theta;
            float originalPhi = phi;
            theta = SphericalCoordinateSystem.NormalizeTheta(theta);
            phi = SphericalCoordinateSystem.ClampPhi(phi);
            
            // Check center first (it has full theta range and highest priority)
            // Center segment should catch points in the center phi range regardless of theta
            var centerSeg = segments[0];
            bool centerMatches = centerSeg.Contains(theta, phi);
            if (centerMatches)
            {
                return 0;
            }
            
            // Then check all other segments
            for (int i = 1; i < segments.Count; i++)
            {
                if (segments[i].Contains(theta, phi))
                {
                    return segments[i].segmentIndex;
                }
            }
            
            // If no segment found, try to find the closest match for debugging
            // This shouldn't happen with proper partitioning, but let's be helpful
            float minDistance = float.MaxValue;
            int closestSegment = -1;
            System.Text.StringBuilder debugInfo = new System.Text.StringBuilder();
            debugInfo.AppendLine($"No segment found for point (θ={theta:F3}, φ={phi:F3}).");
            debugInfo.AppendLine($"Phi range: [0, π] = [0, {Mathf.PI:F3}]. Point phi={phi:F3} is {(phi < 0 || phi > Mathf.PI ? "OUT OF RANGE" : "in range")}");
            debugInfo.AppendLine("Segment boundaries:");
            
            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                // Calculate approximate distance to segment center
                float segThetaCenter = (seg.thetaMin + seg.thetaMax) * 0.5f;
                if (seg.thetaMin > seg.thetaMax) segThetaCenter = seg.thetaMin; // Handle wrap-around
                float segPhiCenter = (seg.phiMin + seg.phiMax) * 0.5f;
                
                float thetaDiff = Mathf.Abs(theta - segThetaCenter);
                if (thetaDiff > Mathf.PI) thetaDiff = 2f * Mathf.PI - thetaDiff; // Wrap around
                float phiDiff = Mathf.Abs(phi - segPhiCenter);
                float distance = Mathf.Sqrt(thetaDiff * thetaDiff + phiDiff * phiDiff);
                
                bool phiInRange = phi >= seg.phiMin && phi <= seg.phiMax;
                bool thetaInRange = (seg.thetaMin <= seg.thetaMax) ? 
                    (theta >= seg.thetaMin && theta <= seg.thetaMax) : 
                    (theta >= seg.thetaMin || theta <= seg.thetaMax);
                
                debugInfo.AppendLine($"  Seg {seg.segmentIndex}: phi[{seg.phiMin:F3}, {seg.phiMax:F3}] " +
                    $"theta[{seg.thetaMin:F3}, {seg.thetaMax:F3}] - phiInRange={phiInRange}, thetaInRange={thetaInRange}, dist={distance:F3}");
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestSegment = seg.segmentIndex;
                }
            }
            
            Debug.LogWarning(debugInfo.ToString());
            return -1;
        }
    }
}

