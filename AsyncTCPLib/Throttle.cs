using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTCPLib
{
    /// <summary>
    /// Transferred from https://gist.github.com/passy/637319 - thanks for this :)
    /// </summary>
    public class Throttle
    {

#region Constants, Members

        /// <summary>
        /// A constant used to specify an infinite number of bytes that can be transferred per second
        /// </summary>
        public const long Infinite = 0;

        /// <summary>
        /// The maximum bytes per second that can be transferred
        /// </summary>
        public long MaximumBytesPerSecond { get; private set; }

        /// <summary>
        /// Number of bytes transferred since last throttle
        /// </summary>
        public long ByteCount { get; private set; }

        /// <summary>
        /// The start time in milliseconds of the last throttle
        /// </summary>
        public long Start { get; private set; }

#endregion

#region Constructor, Functions

        /// <summary>
        /// Create a throttle instance with default MaximumBytesPerSecond = 1024
        /// </summary>
        /// <param name="bytesPerSecond"></param>
        public Throttle(long bytesPerSecond = 1000000)
        {
            this.MaximumBytesPerSecond = bytesPerSecond;
            this.ByteCount = 0;
            this.Start = Math.Abs(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }

        /// <summary>
        /// Get the time in milliseconds to sleep to throttle transfer
        /// </summary>
        /// <param name="bytesTransfer">Number of bytes transferred now </param>
        /// <returns>Number of milliseconds to sleep (0 if no throttle is required)</returns>
        public int ThrottleTime(int bytesTransfer)
        {
            long currentMilliseconds = Math.Abs(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
            long elapsedMilliseconds = currentMilliseconds - this.Start;

            if (elapsedMilliseconds > 0)
            {
                // add transferring bytes to counter
                this.ByteCount += bytesTransfer;

                // current bytes per second
                long bps = this.ByteCount * 1000L / elapsedMilliseconds;

                if (bps > this.MaximumBytesPerSecond)
                {
                    long wakeElapsed = this.ByteCount * 1000L / this.MaximumBytesPerSecond;
                    int toSleep = (int)(wakeElapsed - elapsedMilliseconds);

                    // reset
                    if (elapsedMilliseconds > 1000)
                    {
                        this.ByteCount = 0;
                        this.Start = currentMilliseconds + toSleep; // after sleep
                    }

                    return toSleep;
                }
            }
            return 0;
        }

        /// <summary>
        /// Set the MaximumBytesPerSecond value to a new one
        /// </summary>
        public void SetMaximumBytesPerSecond(int maximum)
        {
            this.MaximumBytesPerSecond = maximum;
        }

#endregion

    }
}
