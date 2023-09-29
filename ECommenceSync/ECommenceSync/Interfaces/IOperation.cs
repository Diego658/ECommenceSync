using System;
using System.Threading.Tasks;

namespace ECommenceSync.Interfaces
{
    public interface IOperation
    {
        public Operations Operation { get;  }
        public OperationModes Mode { get; }
        public OperationDirections Direction { get;  }
        public Guid Identifier { get;  }
        void Start();
        void Stop(TimeSpan timeOut);
        Task Work();

        public OperationStatus Status { get;  }

    }
}
