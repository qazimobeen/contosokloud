using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Application.Entities.AWS
{
    /// <summary>
    /// Operation typies that AWS understands
    /// </summary>
    [Serializable]
    public enum OperationType
    {
        Create,
        Start,
        Stop,
        Restart,
        Resize,
        Snapshot,
        Terminate
    }
}