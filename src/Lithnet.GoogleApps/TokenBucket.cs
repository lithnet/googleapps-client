using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lithnet.GoogleApps
{
    internal class TokenBucket
    {
        private long availableTokens;

        private readonly object consumerLock;

        private readonly object refillLock;

        private long nextRefillTicks;

        private TimeSpan refillInterval;

        private readonly long refillQuantity;

        private readonly string name;

        public long Capacity { get; }

        public TokenBucket(string name, long capacity, TimeSpan refillInterval, long refillAmount)
        {
            this.name = name;
            this.availableTokens = 0;
            this.consumerLock = new object();
            this.refillLock = new object();
            this.Capacity = capacity;
            this.refillQuantity = refillAmount;
            this.refillInterval = refillInterval;
        }

        public bool TryConsume()
        {
            return this.TryConsume(1);
        }

        public bool TryConsume(long tokensRequired)
        {
            if (tokensRequired <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tokensRequired), "Number of tokens required must be greater than zero");
            }

            if (tokensRequired > this.Capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(tokensRequired), "Number of tokens required is greated than the capacity of the bucket");
            }

            lock (this.consumerLock)
            {
                long newTokens = Math.Min(this.Capacity, this.Refill());

                this.availableTokens = Math.Max(0, Math.Min(this.availableTokens + newTokens, this.Capacity));

                if (tokensRequired > this.availableTokens)
                {
                    return false;
                }
                else
                {
                    this.availableTokens -= tokensRequired;
                    return true;
                }
            }
        }

        public void Consume()
        {
            this.Consume(1);
        }

        public void Consume(long numTokens)
        {
            bool logged = false;

            while (true)
            {
                if (this.TryConsume(numTokens))
                {
                    Trace.WriteLine($"{numTokens} tokens taken from bucket {this.name} leaving {this.availableTokens}");
                    break;
                }

                if (!logged)
                {
                    Trace.WriteLine($"{numTokens} tokens not available from bucket {this.name} ({this.availableTokens} remaining)");
                    logged = true;
                }

                Thread.Sleep(50);
            }
        }

        public long Refill()
        {
            lock (this.refillLock)
            {
                long now = DateTime.Now.Ticks;

                if (now < this.nextRefillTicks)
                {
                    return 0;
                }

                this.nextRefillTicks = this.refillInterval.Ticks + now;

                Trace.WriteLine($"Refilling bucket {this.name} with {this.refillQuantity} tokens");

                return this.refillQuantity;
            }
        }
    }
}
