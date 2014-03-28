using System;

namespace Mike.Vmware.Connect.Api
{
    [Serializable]
    public class ApiException : ApplicationException
    {
        public ApiException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class InvalidLoginException : ApplicationException
    {
        public InvalidLoginException(string message)
            : base(message)
        {
        }
    }
}