﻿using System;
using System.Collections.Generic;
using System.Linq;
using Duality;
using Duality.Components.Physics;
using Duality.Input;
using Duality.Components;
using Duality.Resources;
using Duality.Drawing;

namespace WorldSailorsDuality
{
    [RequiredComponent(typeof(Transform)),RequiredComponent(typeof(Camera))]
    public class CameraController : Component, ICmpUpdatable
    {
        public Agent TrackedAgent { get; set; }

        [DontSerialize]
        private int BoatCounter = 0;

        public void OnUpdate()
        {
            //switch through Agents
            if (DualityApp.Keyboard.KeyHit(Key.S))
            {
                List<Agent> ln = Scene.Current.FindComponents<Agent>().ToList();
                List<Agent> l = new List<Agent>();
                foreach (Agent a in ln)
                    if (a.Active)
                        l.Add(a);
                BoatCounter %= l.Count;
                TrackedAgent = l[BoatCounter];
                HudRenderer render = GameObj.GetComponent<HudRenderer>();
                if (render != null)
                {
                    render.TrackedAgent = TrackedAgent;
                }
                BoatCounter++;
            }

            if (TrackedAgent == null)
              return;
                        
            Transform t = this.GameObj.GetComponent<Transform>();
            Camera c = this.GameObj.GetComponent<Camera>();

            if (TrackedAgent != null && t != null && TrackedAgent != null) //Camera follows
            {
                float deltaHeight = 0;

                float padAxis = StaticHelpers.ApplyStickDeadZone(DualityApp.Gamepads[0].RightThumbstick.Y);
                deltaHeight += -padAxis * Time.TimeMult * 50;

                if (DualityApp.Keyboard[Key.ShiftLeft])
                    deltaHeight += 50 * Time.TimeMult;
                if (DualityApp.Keyboard[Key.ControlLeft])
                    deltaHeight -= 50 * Time.TimeMult;

                if (c.Perspective == PerspectiveMode.Parallax)
                    deltaHeight += t.Pos.Z;
                else
                { 
                    c.FocusDist *= (float)Math.Pow(c.FocusDist, deltaHeight / 10000f);
                    deltaHeight = t.Pos.Z;
                }
                t.MoveTo(new Vector3(TrackedAgent.GetPosition().X, TrackedAgent.GetPosition().Y, deltaHeight));
            }          
        }
    }
}
