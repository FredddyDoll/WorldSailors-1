﻿using Duality;
using Duality.Components;
using Duality.Components.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldSailorsDuality
{
    [RequiredComponent(typeof(RigidBody)), RequiredComponent(typeof(Transform))]
    public class FoilController : Component, Ihudstring,ICmpUpdatable
    {
        public float StatLift { get; set; }
        public float StatDrag { get; set; }
        public float maxSpeed { get; set; } //for drawing purpose
        public string ScreenString { get; set; } //for drawing purpose
        public Vector2 CurrentWorkingPoint { get { return currentWP; } }
        public Vector2 FluidSpeed { get; set; } = new Vector2();
        public MediumController TargetMedium {get;set;}

        [DontSerialize]
        private float liftToDrag = 0;
        [DontSerialize]
        private float angleOfAttack = 0;
        [DontSerialize]
        private Vector2 lastLocalForce = new Vector2();
        [DontSerialize]
        private Vector2 currentWP;

        public float GetLiftToDrag()
        {
            return liftToDrag;
        }

        public float GetAngleOfAttack()
        {
            return angleOfAttack;
        }

        public Vector2 GetLastLocalForce()
        {
            return lastLocalForce;
        }
        
        public string GetHudString()
        {
            return ScreenString + " Lift: " + Math.Round(StatLift, 2).ToString() + " ; Drag: " + Math.Round(StatDrag, 2).ToString();
        }
                
        private Vector2 GetForce(Vector2 workingPoint)
        {
            Vector2 ret = new Vector2();

            Vector2 quadSpeed = workingPoint * workingPoint.Length; //Quadrieren
            ret.X = -quadSpeed.X * StatLift; //Lift
            ret.Y = -quadSpeed.Y * StatDrag; //Drag

            return ret;
        }

        public void OnUpdate()
        {
            if (TargetMedium == null)
                return;

            FluidSpeed = TargetMedium.speed;

            RigidBody body = this.GameObj.GetComponent<RigidBody>();
            Transform pos = this.GameObj.GetComponent<Transform>();

            Vector2 trueSpeed = pos.Vel.Xy - FluidSpeed;
            Vector2 workingPoint = pos.GetLocalVector(trueSpeed);
            Vector2 localForce = GetForce(workingPoint);
            currentWP = workingPoint;


            lastLocalForce = localForce;
            Vector2 dirDrag = workingPoint.Normalized;
            Vector2 dirLift = dirDrag.PerpendicularLeft;
            float lift = Vector2.Dot(localForce, dirLift);
            float drag = Vector2.Dot(localForce, dirDrag);
            liftToDrag = lift / drag;

            angleOfAttack = MathF.RadToDeg(workingPoint.Angle);
            if (angleOfAttack > 180)
                angleOfAttack -= 360;

            if (trueSpeed.Length < 50) //Limit needed after init
            {
                body.ApplyLocalForce(localForce);
            }
        }
    }
}
