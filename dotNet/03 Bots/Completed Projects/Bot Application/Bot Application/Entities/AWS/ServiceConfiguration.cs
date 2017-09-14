using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Application.Entities.AWS
{
    [Serializable]
    public class ServiceConfiguration
    {
        private InstanceFamilyType _instanceFamily { get; set; }
        private string _instanceType { get; set; }
        private IEnumerable<string> _instanceTypesAvailable { get; set; }

        /// <summary>
        /// Creates a new <see cref="ServiceConfiguration"/> with the specified <paramref name="instanceType"/> as input.
        /// <paramref name="instanceType"/> can be anything from http://docs.aws.amazon.com/AWSEC2/latest/UserGuide/instance-types.html#AvailableInstanceTypes
        /// E.g. t2.nano | t2.micro or c4.large | c4.xlarge or r3.large | r3.xlarge or d2.xlarge | d2.2xlarge or f1.2xlarge | f1.16xlarge etc.
        /// </summary>
        /// <param name="instanceType">the instanceType</param>
        public ServiceConfiguration(InstanceFamilyType instanceFamily, string instanceType)
        {
            _instanceFamily = instanceFamily;
            _instanceType = instanceType;
            _instanceTypesAvailable = GetInstanceTypes();
        }

        /// <summary>
        /// Ensures that the <see cref="_instanceType"/> provided is actually one that AWS is aware of.
        /// Otherwise, if it can't be found; we just return an empty string.
        /// </summary>
        /// <returns></returns>
        public string SelectedInstanceType
        {
            get
            {
                if (_instanceTypesAvailable != null && _instanceTypesAvailable.Count() > 0)
                {
                    var instanceTypesEnumerator = _instanceTypesAvailable.GetEnumerator();
                    while (instanceTypesEnumerator.MoveNext())
                    {
                        var lcInstanceTypeName = instanceTypesEnumerator.Current.Replace('_', '.').ToLower();
                        var lcInstanceType = _instanceType.ToLower();
                        if (lcInstanceTypeName == _instanceType.ToLower())
                        {
                            return lcInstanceType;
                        }
                    }
                }
                return string.Empty;
            }
            
        }

        /// <summary>
        /// Returns the a list of <see cref="List{string}"/> based on the <see cref="_instanceFamily"/> type that was provided.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetInstanceTypes()
        {
            switch (_instanceFamily)
            {
                case InstanceFamilyType.Storage:
                {
                    return Enum.GetNames(typeof(StorageInstanceType));
                }
                default:
                {
                    return Enum.GetNames(typeof(InstanceFamilyType));
                }
            }
        }
    }
}