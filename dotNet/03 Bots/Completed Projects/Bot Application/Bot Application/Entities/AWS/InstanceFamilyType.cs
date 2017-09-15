using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Bot_Application.Entities.AWS
{
    /// <summary>
    /// Instance Family Types that AWS understands
    /// </summary>
    [Serializable]
    public enum InstanceFamilyType
    {
        None,
        General,
        Computer,
        Memory,
        Storage,
        Accelerated,
    }

    /// <summary>
    /// Storage related Instance Types that AWS understands
    /// </summary>
    [Serializable]
    public enum StorageInstanceType
    {
        d2_xlarge,
        d2_2xlarge,
        d2_4xlarge,
        d2_8xlarge,
        i2_xlarge,
        i2_2xlarge,
        i2_4xlarge,
        i2_8xlarge,
        i3_large,
        i3_xlarge,
        i3_2xlarge,
        i3_4xlarge,
        i3_8xlarge,
        i3_16xlarge
    }
}