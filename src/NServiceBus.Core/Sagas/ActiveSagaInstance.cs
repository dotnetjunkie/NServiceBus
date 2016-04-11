namespace NServiceBus.Sagas
{
    using System;
    using Saga;

    /// <summary>
    /// Represents a saga instance being processed on the pipeline
    /// </summary>
    public class ActiveSagaInstance
    {
        internal ActiveSagaInstance(Saga saga)
        {
            Instance = saga;
            SagaType = saga.GetType();
        }

        /// <summary>
        /// The type of the saga
        /// </summary>
        public Type SagaType { get; private set; }
        
        /// <summary>
        /// The actual saga instance
        /// </summary>
        public Saga Instance { get; private set; }
        
        /// <summary>
        /// True if this saga was created by this incoming message
        /// </summary>
        public bool IsNew { get; private set; }
                     
        /// <summary>
        /// True if no saga instance could be found for this message
        /// </summary>
        public bool NotFound { get; private set; }

        /// <summary>
        /// Provides a way to update the actual saga entity
        /// </summary>
        /// <param name="sagaEntity">The new entity</param>
        public void AttachNewEntity(IContainSagaData sagaEntity)
        {
            IsNew = true;
            AttachEntity(sagaEntity);
        }

        internal void AttachExistingEntity(IContainSagaData loadedEntity)
        {
            AttachEntity(loadedEntity);
        }

        void AttachEntity(IContainSagaData sagaEntity)
        {
            Instance.Entity = sagaEntity;
        }
        internal void MarkAsNotFound()
        {
            NotFound = true;
        }
    }
}