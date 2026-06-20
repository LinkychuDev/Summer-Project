using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class PlayerSpringConnector : MonoBehaviour
{
    
    /*k is spring constant
     m is spring mass
     x0 is current distance
     v0 velocity
     t is game frame or delta time
     xt is new distance from current velocity
     vt is new velocity
     */
    
    public SpringUtils.tDampedSpringMotionParams springParams;

    //angular frequency
    public float frequency = 15f;
    public float dampeningRatio = 0.5f;

    Rigidbody rigidBody;
  
    
    
    private void Awake()
    {
        springParams = new SpringUtils.tDampedSpringMotionParams();
        rigidBody = GetComponent<Rigidbody>();
    }

    
    public  void UpdateSpringVector(float timeStep, ref Vector3 pos, ref Vector3 vel, Vector3 target)
    {
        
        SpringUtils.CalcDampedSpringMotionParams(ref springParams, timeStep, frequency, dampeningRatio);
        SpringUtils.UpdateDampedSpringMotion(
            ref pos.x, ref vel.x, target.x,  springParams);

        SpringUtils.UpdateDampedSpringMotion(
            ref pos.y, ref vel.y, target.y,  springParams);

        SpringUtils.UpdateDampedSpringMotion(
            ref pos.z, ref vel.z, target.z,  springParams);
    }

}