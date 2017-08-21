using UnityEngine;
using System;

/// <summary>
/// PicoVRSDK 眼镜参数 屏幕参数 畸变参数
/// </summary>
[System.Serializable]
public class PicoVRConfigProfile
{

    /// <summary>
    /// device 属性
    /// </summary>
    public struct Lenses
    {
        public float separation;                //瞳距
        public float offset;                //镜头中心到镜框上边沿的距离
        public float distance;              //镜头到手机屏幕的垂直距离
        public int alignment;
        public const int AlignTop = -1;    // Offset is measured down from top of device.
        public const int AlignCenter = 0;  // Center alignment ignores offset, hence scale is zero.
        public const int AlignBottom = 1;  // Offset is measured up from bottom of device.

    }

    /// <summary>
    /// MaxFOV （Unity Editor 模拟）
    /// </summary>
    public struct MaxFOV
    {
        public float upper;             //最大FOV --上
        public float lower;             //最大FOV --下
        public float inner;             //最大FOV --靠近中心点
        public float outer;             //最大FOV --远离中心点

    }

    /// <summary>
    /// Distortion （Unity Editor 模拟）
    /// </summary>
    public struct Distortion
    {
        public float k1;
        public float k2;
        public float k3;
        public float k4;
        public float k5;
        public float k6;
        public float distort(float r)
        {
            return 0;
        }
        public float distort(float r, float dist)
        {
            float r2 = r * dist * 1000.0f;
            float r3 = k1 * Mathf.Pow(r2, 5.0f) + k2 * Mathf.Pow(r2, 4.0f) + k3 * Mathf.Pow(r2, 3.0f) + k4 * Mathf.Pow(r2, 2.0f) + k5 * r2 + k6;
            return r3 / 1000.0f / dist; ;
        }

        public float diatortInv(float radious)
        {
            return 0;
        }

    }

    /// <summary>
    /// Device
    /// </summary>
    public struct Device
    {
        public Lenses devLenses;
        public MaxFOV devMaxFov;
        public Distortion devDistortion;
        public Distortion devDistortionInv;
    }

    /// <summary>
    /// enum devicetype
    /// </summary>
    public enum DeviceTypes
    {
        General = 0,//无畸变的一组参数，未适配特定头盔   
        Pico1 = 2,//小鸟看看Pico 1   
        Pico1S = 8,
        PicoNeo = 7
		
    };
    public enum DeviceCommand
    {
        SET_PICO_NEO_HMD_BRIGHTNESS = 12,//操作屏幕亮度，包括设置以及获取
        SET_PICO_NEO_HMD_SLEEPDELAY = 13,//操作熄屏，包括设置以及获取
        GET_PICO_NEO_HMD_BRIGHTNESS_FLAG = 14//获取能否调节屏幕亮度属性
    }
    /// <summary>
    /// Pico1
    /// </summary>
    public static readonly Device Pico1 = new Device
    {
        devLenses = { separation = 0.062f, offset = 0.0f, distance = 0.0403196f, alignment = Lenses.AlignCenter },
        devMaxFov = { upper = 40.0f, lower = 40.0f, inner = 40.0f, outer = 40.0f },
        devDistortion =
        {
            k1 = 2.333e-06f,
            k2 = -0.000126f,
            k3 = 0.002978f,
            k4 = -0.02615f,
            k5 = 1.089f,
            k6 = -0.0337f
        },
        devDistortionInv =
        {
            k1 = 1.342e-08f,
            k2 = 1.665e-06f,
            k3 = -0.0002797f,
            k4 = 0.001166f,
            k5 = 0.9945f,
            k6 = 0.004805f
        }
    };

    /// <summary>
    /// General
    /// </summary>
    public static readonly Device General = new Device
    {
        devLenses = { separation = 0.062f, offset = 0.035f, distance = 0.042f, alignment = Lenses.AlignCenter },
        devMaxFov = { upper = 40.0f, lower = 40.0f, inner = 40.0f, outer = 40.0f },
        devDistortion =
        {
            k1 = 2.333e-06f,
            k2 = -0.000126f,
            k3 = 0.002978f,
            k4 = -0.02615f,
            k5 = 1.089f,
            k6 = -0.0337f
        },
        devDistortionInv =
        {
            k1 = 1.342e-08f,
            k2 = 1.665e-06f,
            k3 = -0.0002797f,
            k4 = 0.001166f,
            k5 = 0.9945f,
            k6 = 0.004805f
        }
    };

  

    /// <summary>
    /// FALCON_K26R
    /// </summary>
    public static readonly Device PicoNeo = new Device
    {
        devLenses = { separation = 0.0585f, offset = 0.0f, distance = 0.0403196f, alignment = Lenses.AlignCenter },
        devMaxFov = { upper = 40.0f, lower = 40.0f, inner = 40.0f, outer = 40.0f },
        devDistortion =
        {
            k1 = 2.333e-06f,
            k2 = -0.000126f,
            k3 = 0.002978f,
            k4 = -0.02615f,
            k5 = 1.089f,
            k6 = -0.0337f
        },
        devDistortionInv =
        {
            k1 = 1.342e-08f,
            k2 = 1.665e-06f,
            k3 = -0.0002797f,
            k4 = 0.001166f,
            k5 = 0.9945f,
            k6 = 0.004805f
        }
    };



    /// <summary>
    ///  PICO1S 
    /// </summary>
    public static readonly Device Pico1S = new Device
	{
		devLenses = { separation = 0.062f, offset = 0.0f, distance = 0.0384993f, alignment = Lenses.AlignCenter },
		devMaxFov = { upper = 40.0f, lower = 40.0f, inner = 40.0f, outer = 40.0f },
		devDistortion =
		{
			k1 = 2.333e-06f,
			k2 = -0.000126f,
			k3 = 0.002978f,
			k4 = -0.02615f,
			k5 = 1.089f,
			k6 = -0.0337f
		},
		devDistortionInv =
		{
			k1 = 1.342e-08f,
			k2 = 1.665e-06f,
			k3 = -0.0002797f,
			k4 = 0.001166f,
			k5 = 0.9945f,
			k6 = 0.004805f
		}
	};

    /// <summary>
    /// PicoVRConfigProfile.device
    /// </summary>
    public Device device;
    public static readonly PicoVRConfigProfile Default = new PicoVRConfigProfile
    {
        device = PicoNeo
    };

    /// <summary>
    /// clone
    /// </summary>
    /// <returns></returns>
    public PicoVRConfigProfile Clone()
    {
        return new PicoVRConfigProfile
        {
            device = this.device
        };
    }

    /// <summary>
    /// 根据选择的头盔导入相关的参数 
    /// </summary>
    /// <param name="deviceType"></param>
    /// <returns></returns>
    public static PicoVRConfigProfile GetPicoProfile(DeviceTypes deviceType)
    {
        Device dev;
        switch (deviceType)
        {
            case DeviceTypes.General:
                {
                    dev = General;
                    break;
                }

            case DeviceTypes.Pico1:
                {
                    dev = Pico1;
                    break;
                }                    
            case DeviceTypes.PicoNeo:
                {
                    dev = PicoNeo;
                    break;
                }
			case DeviceTypes.Pico1S:
			{
				dev = Pico1S;
				break;
			}
            default:
                {
                    dev = PicoNeo;
                    break;
                }

        }
        return new PicoVRConfigProfile { device = dev };
    }

    /// <summary>
    /// 根据选择的头盔参数以及设定的最大FOV 求解tan（Unity Editor 模拟）
    /// </summary>
    public float[] GetLeftEyeVisibleTanAngles(float width, float height)
    {
        // Tan-angles from the max FOV.
        float fovLeft = (float)Math.Tan(-this.device.devMaxFov.outer * Math.PI / 180);
        float fovTop = (float)Math.Tan(this.device.devMaxFov.upper * Math.PI / 180);
        float fovRight = (float)Math.Tan(this.device.devMaxFov.inner * Math.PI / 180);
        float fovBottom = (float)Math.Tan(-this.device.devMaxFov.lower * Math.PI / 180);
        float halfWidth = width / 4;
        float halfHeight = height / 2;
        // Viewport center, measured from left lens position.
        float centerX = this.device.devLenses.separation / 2 - halfWidth;
        float centerY = 0.0f;
        float centerZ = this.device.devLenses.distance;
        // Tan-angles of the viewport edges, as seen through the lens.
        float screenLeft = this.device.devDistortion.distort((centerX - halfWidth) / centerZ, this.device.devLenses.distance);
        float screenTop = this.device.devDistortion.distort((centerY + halfHeight) / centerZ, this.device.devLenses.distance);
        float screenRight = this.device.devDistortion.distort((centerX + halfWidth) / centerZ, this.device.devLenses.distance);
        float screenBottom = this.device.devDistortion.distort((centerY - halfWidth) / centerZ, this.device.devLenses.distance);
        // Compare the two sets of tan-angles and take the value closer to zero on each side.
        float left = Math.Max(fovLeft, screenLeft);
        float top = Math.Min(fovTop, screenTop);
        float right = Math.Min(fovRight, screenRight);
        float bottom = Math.Max(fovBottom, screenBottom);
        return new float[] { left, top, right, bottom };
    }

    /// <summary>
    /// （Unity Editor 模拟）
    /// </summary>
    public float[] GetLeftEyeNoLensTanAngles(float width, float height)
    {
        // Tan-angles from the max FOV.
        float fovLeft = this.device.devDistortionInv.distort((float)Math.Tan(-this.device.devMaxFov.outer * Math.PI / 180), this.device.devLenses.distance);
        float fovTop = this.device.devDistortionInv.distort((float)Math.Tan(this.device.devMaxFov.upper * Math.PI / 180), this.device.devLenses.distance);
        float fovRight = this.device.devDistortionInv.distort((float)Math.Tan(this.device.devMaxFov.inner * Math.PI / 180), this.device.devLenses.distance);
        float fovBottom = this.device.devDistortionInv.distort((float)Math.Tan(-this.device.devMaxFov.lower * Math.PI / 180), this.device.devLenses.distance);
        // Viewport size.
        float halfWidth = width / 4;
        float halfHeight = height / 2;
        // Viewport center, measured from left lens position.
        float centerX = this.device.devLenses.separation / 2 - halfWidth;
        float centerY = 0.0f;
        float centerZ = this.device.devLenses.distance;
        // Tan-angles of the viewport edges, as seen through the lens.
        float screenLeft = (centerX - halfWidth) / centerZ;
        float screenTop = (centerY + halfHeight) / centerZ;
        float screenRight = (centerX + halfWidth) / centerZ;
        float screenBottom = (centerY - halfWidth) / centerZ;
        // Compare the two sets of tan-angles and take the value closer to zero on each side.
        float left = Math.Min(fovLeft, screenLeft);
        float top = Math.Min(fovTop, screenTop);
        float right = Math.Min(fovRight, screenRight);
        float bottom = Math.Max(fovBottom, screenBottom);
        return new float[] { left, top, right, bottom };
    }

    /// <summary>
    /// 求解Viewport （Unity Editor 模拟）
    /// </summary>
    public Rect GetLeftEyeVisibleScreenRect(float[] undistortedFrustum, float width, float height)
    {

        float dist = this.device.devLenses.distance;
        float eyeX = (width - this.device.devLenses.separation) / 2;
        float eyeY = height / 2;
        float left = (undistortedFrustum[0] * dist + eyeX) / width;
        float top = (undistortedFrustum[1] * dist + eyeY) / height;
        float right = (undistortedFrustum[2] * dist + eyeX) / width;
        float bottom = (undistortedFrustum[3] * dist + eyeY) / height;
        return new Rect(left, bottom, right - left, top - bottom);
    }

}
