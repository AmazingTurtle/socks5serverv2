using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTCPLib
{
    /// <summary>
    /// Nested byte buffer manager. Every buffer manager can hold multiple other buffer manager instances as storage
    /// </summary>
    public class BufferManager
    {

        #region Members

        public BufferManager[] Storage { get; private set; }
        public byte[] Data;

        #endregion

        #region Constructor, Functions

        /// <summary>
        /// Create a new nested buffer manager with storageSize and dataSize
        /// </summary>
        /// <param name="storageSize">Number of nested buffer managers</param>
        /// <param name="dataSize"></param>
        public BufferManager(int storageSize, int dataSize)
        {
            this.Storage = new BufferManager[storageSize];
            this.Data = new byte[dataSize];
        }

        /// <summary>
        /// this[] accessor for BufferManager.Storage
        /// </summary>
        /// <returns>BufferManager.Storage[index]</returns>
        public BufferManager this[int index] { get { return this.Storage[index]; } }

        /// <summary>
        /// Setup all buffer manager in storage with storageSize and dataSize
        /// </summary>
        /// <returns>All initialized buffer manager from storage</returns>
        public BufferManager[] InitSubsequent(int storageSize, int dataSize)
        {
            for (int i = 0; i < this.Storage.Length; i++)
            {
                this.Storage[i] = new BufferManager(storageSize, dataSize);
            }
            return this.Storage;
        }

        #endregion

    }
}
