using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;

public class parabola : MonoBehaviour
{
    private BattleCore tarBattleCore;
    private BattleCore attacker;
    private Transform tarTrans;
    private bool isNull;
    private float speed = 5f;
    private float multi;
    private Action<float, BattleCore, parabola> reachFunc;
    private Vector3 tarPY;
    
    private const float min_distance = 0.1f;
    private float distance;
    private Vector3 tarPos;
    private float py;

    public void Init(Vector3 pos, BattleCore attacker_, BattleCore targetBattleCore, float speed_ = 5,
        Action<float, BattleCore, parabola> reach = null, float Multi = 1, Vector3 tarPY_ = default)
    {
        transform.position = pos;
        tarBattleCore = targetBattleCore;
        attacker = attacker_;
        tarTrans = tarBattleCore.animTransform;
        speed = speed_;
        multi = Multi;
        reachFunc = reach;
        tarPY = tarPY_;

        tarPos = tarTrans.position + tarPY;
        distance = Vector3.Distance(transform.position, tarPos);
        py = transform.position.y;

        if (targetBattleCore.dying)
        {
            reachFunc = null;
            PoolManager.RecycleObj(gameObject);
        }
        else
        {
            isNull = false;
            tarBattleCore.DieAction += TarNull;
        }
    }

    void Update()
    {
        if (!isNull)
        {
            tarPos = tarTrans.position + tarPY;
            // if (Vector3.Distance(tarPos, Vector3.zero) > 200) 
            //     isNull = true;
        }

        // 朝向目标, 以计算运动
        transform.LookAt(tarPos);
        // 根据距离衰减 角度
        float angle = Mathf.Min(1, Vector3.Distance(transform.position, tarPos) / distance);
        // 旋转对应的角度（线性插值一定角度，然后每帧绕X轴旋转）
        transform.rotation = transform.rotation * Quaternion.Euler(0, 0, Mathf.Clamp(-angle, -42, 42));
        // 当前距离目标点
        float currentDist = Vector2.Distance(BaseFunc.xz(transform.position), BaseFunc.xz(tarPos));
        // 很接近目标了, 准备结束循环
        if (currentDist < min_distance)
        {
            Arrive();
            return;
        }
        // 平移 (朝向Z轴移动)
        transform.Translate(Vector3.forward * Mathf.Min(speed * Time.deltaTime, currentDist));
        transform.position = new Vector3(transform.position.x, py, transform.position.z);
    }

    private void Arrive()
    {
        if (!isNull)
        {
            if (attacker.gameObject.activeSelf) reachFunc?.Invoke(multi, tarBattleCore, this);
            tarBattleCore.DieAction -= TarNull;
        }
        reachFunc = null;
        PoolManager.RecycleObj(gameObject);
    }
    
    private void TarNull(BattleCore bc_)
    {
        isNull = true;
    }
    
}