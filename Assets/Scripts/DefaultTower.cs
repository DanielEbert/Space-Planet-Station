using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultTower : Tower
{
    public bool guidedBullet = false;

    void Update() {
        if (Time.time >= nextCheck) {
            nextCheck = Time.time + .2f;
            target = getClosestTarget();
        }
        
        if (!targetValid())
            target = null;

        if (target != null) {
            //-target.position.normalized not 100 valid anymore
            Vector3 aimPoint = FirstOrderIntercept(turret.position, Vector3.zero, 20, target.position, -target.position.normalized * 5);

            LookAt((Vector2)aimPoint);

            if (Time.time >= nextShotTime) {
                if (Vector3.Angle(turret.up, (aimPoint - turret.position).normalized) < maxShootAngle) {
                    nextShotTime = Time.time + reloadTime;

                    GameObject b = null;

                    if (!guidedBullet) {
                        b = ObjectPool.Instance.pools["Bullet"].get(bulletSpawnPoint.position, 
                            Quaternion.Euler(turret.eulerAngles.x, turret.eulerAngles.y, turret.eulerAngles.z + Random.Range(-bulletSpreadDegrees / 2f, bulletSpreadDegrees / 2f)));
                    } else {
                        Enemy e = target.GetComponent<Enemy>();

                        if (e.OnFutureDamageTaken(3)) {
                            Enemies.Instance.RemoveEnemy(target.gameObject);
                        }

                        b = ObjectPool.Instance.pools["GuidedBullet"].get(bulletSpawnPoint.position, 
                            Quaternion.Euler(turret.eulerAngles.x, turret.eulerAngles.y, turret.eulerAngles.z + Random.Range(-bulletSpreadDegrees / 2f, bulletSpreadDegrees / 2f)));
                        e.lockedOnBullets.Add(b.GetComponent<GuidedBullet>());
                        b.GetComponent<GuidedBullet>().target = target;
                    }

                    b.SetActive(true);
                }
            }
            
        } else {
            RotateToIdle();
        } 
    }
}
