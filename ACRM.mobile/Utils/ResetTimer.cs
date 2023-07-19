using System;
using System.Threading.Tasks;
using Xamarin.Forms;
namespace ACRM.mobile.Utils
{


    /// <summary>
    /// Timer class that supports reset using xamarin timer
    /// </summary>
    public class ResetTimer
    {
        private readonly Func<Task> callback;
        private double currentTime;
        private double interval;
        private double tickInterval;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetTimer"/> class.
        /// Please note this is not a precision timer as it's tick interval is set proportionate to the interval value
        /// </summary>
        /// <param name="callback">action to call when timer runs out</param>
        public ResetTimer(Func<Task> _callback)
        {
            this.callback = _callback;
        }

        /// <summary>
        /// Gets a value indicating whether Timer is running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Starts or resets the timer. The tick interval is hardcoded at 1/20 of the interval
        /// </summary>
        /// <param name="intervalSeconds">Interval in seconds to delay or reset the timer to</param>
        public void StartOrReset(double intervalSeconds)
        {
            if (intervalSeconds <= 0)
            {
                Task.Run(async () => await callback());
       
                return;
            }

            this.interval = intervalSeconds;
            if (!this.IsRunning)
            {
                this.Start();
            }
            else
            {
                this.currentTime = this.interval;
            }
        }

        /// <summary>
        /// Stops timer at the next tick
        /// </summary>
        public void Stop()
        {
            this.IsRunning = false;
            this.interval = 0;
        }

        private void Start()
        {
            if (!this.IsRunning)
            {
                this.IsRunning = true;
                this.currentTime = this.interval;
                this.tickInterval = this.interval / 20;
                Device.StartTimer(TimeSpan.FromSeconds(this.tickInterval), () =>
                {
                    this.currentTime -= this.tickInterval;
                    if (!this.IsRunning)
                    {
                        return false;
                    }

                    if (this.currentTime <= 0)
                    {
                        Task.Run(async () => await callback());
                        this.IsRunning = false;
                        return false;
                    }

                    return true;
                });
            }
        }
    }
}

