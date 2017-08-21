using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;


public class PicoVRPose
{
    /// <summary>
    /// 左手系 右手系转换
    /// </summary>
    protected static readonly Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1, 1, -1));
    public Matrix4x4 RightHandedMatrix
    {
        get
        {
            return flipZ * Matrix * flipZ;
        }
    }

    /************************************    Properties  *************************************/
    #region Properties
    public Vector3 Position { get; protected set; }

    public Quaternion Orientation { get; protected set; }

    public Matrix4x4 Matrix { get; protected set; }
    #endregion

    /*************************** Pose Interfaces *****************************/
    #region Interfaces
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="matrix"></param>
    public PicoVRPose(Matrix4x4 matrix)
    {
        Set(matrix);
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    public PicoVRPose(Vector3 position, Quaternion orientation)
    {
        Set(position, orientation);
    }
    
    /// <summary>
    ///构造姿态
    /// </summary>
    public void Set(Vector3 position, Quaternion orientation)
    {
        Position = position;

        Orientation = orientation;

        Matrix = Matrix4x4.TRS(position, orientation, Vector3.one);
    }

    private Quaternion NormalizeQuaternion(ref Quaternion q)
    {
        float sum = 0;
        for (int i = 0; i < 4; ++i)
        {
            sum += q[i] * q[i];
        }
            
        float magnitudeInverse = 1 / Mathf.Sqrt(sum);
        for (int i = 0; i < 4; ++i)
        {
           q[i] *= magnitudeInverse;
        }
        return q;
    }
    /// <summary>
    /// 构造姿态
    /// </summary>
    protected void Set(Matrix4x4 matrix)
    {
        Matrix = matrix;
        Position = matrix.GetColumn(3);
        Orientation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
    }
    #endregion
}
