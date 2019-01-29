using System;
using System.Security.Permissions;

namespace Ara3D.DotNetUtilities
{
    // https://stackoverflow.com/questions/2410221/appdomain-and-marshalbyrefobject-life-time-how-to-avoid-remotingexception/54390900#54390900
    public class SingletonRemotingObject<T> : MarshalByRefObject where T : class, new() 
    {
        public static T Instance = new T();

        public SingletonRemotingObject()
        {
            if (Instance != null)
                throw new Exception("Can only have one instance of singleton");            
        }

        // This removes the need for a sponsor to keep the service alive. 
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
            => null;
    }
}
