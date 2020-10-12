using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseView : MonoBehaviour
{
    static public int state = 0;//平移模式或者选择模式

    //相机跟随的目标物体
    public Transform target;
    private int MouseWheelSensitivity = 1; //滚轮灵敏度设置
    private int MouseZoomMin = 1; //相机距离最小值
    private int MouseZoomMax = 20; //相机距离最大值

    private float moveSpeed = 10; //相机跟随速度（中键平移时），采用平滑模式时起作用，越大则运动越平滑

    private float xSpeed = 250.0f; //旋转视角时相机x轴转速
    private float ySpeed = 120.0f; //旋转视角时相机y轴转速

    private int yMinLimit = -360;
    private int yMaxLimit = 360;

    private float x = 0.0f; //存储相机的euler角
    private float y = 0.0f; //存储相机的euler角

    private float Distance = 5; //相机和target之间的距离，因为相机的Z轴总是指向target，也就是相机z轴方向上的距离
    private Vector3 targetOnScreenPosition; //目标的屏幕坐标，第三个值为z轴距离
    private Quaternion storeRotation; //存储相机的姿态四元数
    private Vector3 CameraTargetPosition; //target的位置
    private Vector3 initPosition; //平移时用于存储平移的起点位置
    private Vector3 cameraX; //相机的x轴方向向量
    private Vector3 cameraY; //相机的y轴方向向量
    private Vector3 cameraZ; //相机的z轴方向向量

    private Vector3 initScreenPos; //中键刚按下时鼠标的屏幕坐标
    private Vector3 curScreenPos; //当前鼠标的屏幕坐标
    void Start()
    {
        //设置初始相机值
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        CameraTargetPosition = target.position;
        storeRotation = Quaternion.Euler(y + 60, x, 0);
        transform.rotation = storeRotation; //设置相机姿态
        Vector3 position = storeRotation * new Vector3(0.0F, 0.0F, -Distance) + CameraTargetPosition; //四元数表示一个旋转，四元数乘以向量相当于把向量旋转对应角度，然后加上目标物体的位置就是相机位置了
        transform.position = storeRotation * new Vector3(0, 0, -Distance) + CameraTargetPosition; //设置相机位置
    }

    void Update()
    {
        //鼠标右键旋转
        if (Input.GetMouseButton(1))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            storeRotation = Quaternion.Euler(y + 60, x, 0);
            var position = storeRotation * new Vector3(0.0f, 0.0f, -Distance) + CameraTargetPosition;

            transform.rotation = storeRotation;
            transform.position = position;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") != 0) //鼠标滚轮缩放
        {
            if (Distance >= MouseZoomMin && Distance <= MouseZoomMax)
            {
                Distance -= Input.GetAxis("Mouse ScrollWheel") * MouseWheelSensitivity;
            }
            if (Distance < MouseZoomMin)
            {
                Distance = MouseZoomMin;
            }
            if (Distance > MouseZoomMax)
            {
                Distance = MouseZoomMax;
            }
            var rotation = transform.rotation;

            transform.position = storeRotation * new Vector3(0.0F, 0.0F, -Distance) + CameraTargetPosition;
        }

        if (state == 0)
        {

            if (Input.GetMouseButtonDown(0))
            {
                cameraX = transform.right;
                cameraY = transform.up;
                cameraZ = transform.forward;

                initScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, targetOnScreenPosition.z);

                //targetOnScreenPosition.z为目标物体到相机xmidbuttonDownPositiony平面的法线距离
                targetOnScreenPosition = Camera.main.WorldToScreenPoint(CameraTargetPosition);
                initPosition = CameraTargetPosition;
            }

            if (Input.GetMouseButton(0))
            {
                curScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, targetOnScreenPosition.z);
                //控制平移的速度
                target.position = initPosition - 0.01f * ((curScreenPos.x - initScreenPos.x) * cameraX + (curScreenPos.y - initScreenPos.y) * cameraY);

                //重新计算位置
                Vector3 mPosition = storeRotation * new Vector3(0.0F, 0.0F, -Distance) + target.position;
                transform.position = mPosition;

                // //用这个会让相机的平移变得更平滑，但是可能在你buttonup时未使相机移动到应到的位置，导致再进行旋转与缩放操作时出现短暂抖动
                //transform.position=Vector3.Lerp(transform.position,mPosition,Time.deltaTime*moveSpeed);

            }
            if (Input.GetMouseButtonUp(0))
            {
                //平移结束把cameraTargetPosition的位置更新一下，不然会影响缩放与旋转功能
                CameraTargetPosition = target.position;
            }
        }


    }

    //将angle限制在min~max之间
    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

}