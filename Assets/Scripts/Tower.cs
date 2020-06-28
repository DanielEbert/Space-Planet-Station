using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public float reloadTime = .3f;
    public float fireRange = 50f;
    public float bulletSpreadDegrees = 3f;
    public float maxShootAngle;
    public float turnRate = 360;

    public Transform bulletSpawnPoint;
    public Transform turret;

    public Transform target;

    protected float nextCheck = 0;
    protected float nextShotTime = 0;

    protected bool targetValid() {
        if (target == null)
            return false;
        if (Vector2.Distance(transform.position, target.position) > fireRange)
            return false;
        return true;
    }

    protected Transform getClosestTarget() {
        Transform cur = null;
        float curMinDist = fireRange;

        foreach (GameObject g in Enemies.Instance.enemies) {
            float dst = Vector2.Distance(transform.position, g.transform.position);
            if (dst <= curMinDist) {
                cur = g.transform;
                curMinDist = dst;
            }
        }
        return cur;
    }

    protected void LookAt(Vector2 targetPos) {
        Vector2 dir = targetPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        turret.rotation = Quaternion.RotateTowards(turret.rotation, Quaternion.AngleAxis(angle - 90, Vector3.forward), turnRate * Time.deltaTime);
    }

    protected bool RotateToIdle() {
        Quaternion newRotation = Quaternion.RotateTowards(turret.localRotation, Quaternion.identity, 2.0f * turnRate * Time.deltaTime);
        turret.localRotation = newRotation;

        if (turret.localRotation == Quaternion.identity)
            return true;
        return false;
    }

    //first-order intercept using absolute target position
    public static Vector3 FirstOrderIntercept(Vector3 shooterPosition, Vector3 shooterVelocity,
                                            float shotSpeed,
                                            Vector3 targetPosition, Vector3 targetVelocity)  {
        Vector3 targetRelativePosition = targetPosition - shooterPosition;
        Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
        float t = FirstOrderInterceptTime(shotSpeed, targetRelativePosition, targetRelativeVelocity);
        return targetPosition + t * (targetRelativeVelocity);
    }

    //first-order intercept using relative target position
    public static float FirstOrderInterceptTime(float shotSpeed, Vector3 targetRelativePosition, 
                                                                Vector3 targetRelativeVelocity) {
        float velocitySquared = targetRelativeVelocity.sqrMagnitude;
        if(velocitySquared < 0.001f)
        return 0f;
    
        float a = velocitySquared - shotSpeed * shotSpeed;
    
        //handle similar velocities
        if (Mathf.Abs(a) < 0.001f)
        {
        float t = -targetRelativePosition.sqrMagnitude / (
            2f * Vector3.Dot(targetRelativeVelocity,targetRelativePosition));
        return Mathf.Max(t, 0f); //don't shoot back in time
        }
    
        float b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
        float c = targetRelativePosition.sqrMagnitude;
        float determinant = b * b - 4f * a * c;
    
        if (determinant > 0f) { //determinant > 0; two intercept paths (most common)
        float	t1 = (-b + Mathf.Sqrt(determinant))/(2f * a),
                t2 = (-b - Mathf.Sqrt(determinant))/(2f * a);
        if (t1 > 0f) {
            if (t2 > 0f)
                return Mathf.Min(t1, t2); //both are positive
            else
                return t1; //only t1 is positive
        } else
            return Mathf.Max(t2, 0f); //don't shoot back in time
        } else if (determinant < 0f) //determinant < 0; no intercept path
        return 0f;
        else //determinant = 0; one intercept path, pretty much never happens
        return Mathf.Max(-b / (2f * a), 0f); //don't shoot back in time
    }
}
