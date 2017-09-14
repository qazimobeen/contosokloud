using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Application.Entities.AWS
{
    public class ServiceMessage
    {
        public OperationType OperationType { get; }
        public ServiceConfiguration ServiceConfiguration { get; }

        public ServiceMessage(
            OperationType operationType, 
            string instanceType = null, 
            InstanceFamilyType familyType = InstanceFamilyType.None)
        {
            OperationType = operationType;

            if(instanceType != null && familyType != InstanceFamilyType.None)
            {
                ServiceConfiguration = new ServiceConfiguration(familyType, instanceType);
            }
        }

        public int CurrentServiceId
        {
            get
            {
                switch (OperationType)
                {
                    case OperationType.Create:
                    {
                        return 1;
                    }
                    case OperationType.Start:
                    case OperationType.Stop:
                    case OperationType.Restart:
                    {
                        return 2;
                    }
                    case OperationType.Resize:
                    {
                        return 3;
                    }
                    case OperationType.Snapshot:
                    case OperationType.Terminate:
                    {
                        return 5;
                    }
                    default:
                    {
                        return 0;
                    }                        
                }
            }
        }
    }
}